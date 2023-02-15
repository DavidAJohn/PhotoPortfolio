using LinqKit;
using Microsoft.AspNetCore.Mvc;
using PhotoPortfolio.Shared.Models.Prodigi.Quotes;
using Stripe;
using Stripe.Checkout;
using PhotoPortfolioStripe = PhotoPortfolio.Shared.Models.Stripe;

namespace PhotoPortfolio.Server.Controllers;

public class PaymentsController : BaseApiController
{
    private readonly ILogger<AdminController> _logger;
    private readonly IConfiguration _config;
    private readonly IPaymentService _paymentService;
    private readonly IOrderService _orderService;
    private readonly IQuoteService _quoteService;

    public PaymentsController(ILogger<AdminController> logger, IConfiguration config, IPaymentService paymentService, IOrderService orderService, IQuoteService quoteService)
    {
        _logger = logger;
        _config = config;
        _paymentService = paymentService;
        _orderService = orderService;
        _quoteService = quoteService;
    }

    [HttpPost("session")]
    public async Task<IActionResult> CreateCheckoutSession(List<BasketItem> basketItems)
    {
        // check the price of each basket item
        foreach (var item in basketItems)
        {
            var quote = await GetItemQuote(item);

            if (quote != item.Total)
            {
                _logger.LogWarning("Basket item price differed from quoted price. Basket: {basket} - Quoted: {quote}", item.Total, quote);
            }
            
            // if quote received is not 0, then use it
            if (quote != 0) item.Total = quote;
        }

        // then supply it to the payment service
        var session = await _paymentService.CreateCheckoutSession(basketItems);
        var url = session.Url;

        return Ok(url);
    }

    [HttpPost("webhook")]
    public async Task<ActionResult> StripeWebhook()
    {
        string WhSecret = _config["Stripe:WhSecret"];

        try
        {
            var json = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();
            var stripeEvent = EventUtility.ConstructEvent(json, Request.Headers["Stripe-Signature"], WhSecret);

            if (stripeEvent.Type == Events.CheckoutSessionCompleted)
            {
                Session session = (Session)stripeEvent.Data.Object;

                _logger.LogInformation("STRIPE --> Checkout Session Completed: {Id}", session.Id);

                var options = new SessionGetOptions();
                options.AddExpand("line_items"); // line items are not included by default, so must be requested explicitly
                var service = new SessionService();
                Session expandedSession = service.Get(session.Id, options);
                StripeList<LineItem> stripeLineItems = expandedSession.LineItems;

                var data = new List<LineItem>();
                stripeLineItems.ForEach(item => data.Add(new LineItem
                {
                    Id = item.Id,
                    Object =item.Object,
                    AmountDiscount = item.AmountDiscount,
                    AmountSubtotal = item.AmountSubtotal,
                    AmountTax = item.AmountTax,
                    AmountTotal = item.AmountTotal,
                    Currency = item.Currency,
                    Description = item.Description,
                    Discounts = item.Discounts,
                    Quantity = item.Quantity,
                    Taxes = item.Taxes,
                }));

                var lineItems = new PhotoPortfolioStripe.LineItems()
                {
                    Object = stripeLineItems.Object,
                    Data = data,
                    HasMore = stripeLineItems.HasMore,
                    Url = stripeLineItems.Url
                };

                var customer = new PhotoPortfolioStripe.Customer() // PhotoPortfolio.Shared.Models.Stripe
                {
                    Name = session.CustomerDetails.Name,
                    EmailAddress = session.CustomerDetails.Email
                };

                _logger.LogInformation("STRIPE --> Order received from: {name} ({email})", session.CustomerDetails.Name, session.CustomerDetails.Email);

                var stripeShippingDetails = session.ShippingDetails;
                var shippingDetails = new PhotoPortfolioStripe.ShippingDetails()
                {
                    Address = new()
                    {
                        City = stripeShippingDetails.Address.City,
                        Country = stripeShippingDetails.Address.Country,
                        Line1 = stripeShippingDetails.Address.Line1,
                        Line2 = stripeShippingDetails.Address.Line2,
                        PostalCode = stripeShippingDetails.Address.PostalCode,
                        State = stripeShippingDetails.Address.State,
                    },
                    Carrier = stripeShippingDetails.Carrier,
                    Name = stripeShippingDetails.Name,
                    Phone = stripeShippingDetails.Phone,
                    TrackingNumber = stripeShippingDetails.TrackingNumber
                };

                // pass details to order service to save in the db
                var response = await _orderService.PlaceOrder(customer, lineItems, shippingDetails);
                if (response)
                {
                    _logger.LogInformation("Order added to database successfully: {Id}", session.Id);
                    session = null!;
                }
                else
                {
                    _logger.LogInformation("Order could NOT be added to database: {Id}", session.Id);
                }
            }
            else if (stripeEvent.Type == Events.PaymentIntentCreated)
            {
                var paymentIntent = stripeEvent.Data.Object as PaymentIntent;
                _logger.LogInformation("STRIPE --> Payment Intent Created: {Id}", paymentIntent?.Id);
                paymentIntent = null!;
            }
            else if (stripeEvent.Type == Events.PaymentIntentSucceeded)
            {
                var paymentIntent = stripeEvent.Data.Object as PaymentIntent;
                _logger.LogInformation("STRIPE --> Payment Intent Succeeded: {Id}", paymentIntent?.Id);
                paymentIntent = null!;
            }
            else if (stripeEvent.Type == Events.ChargeSucceeded)
            {
                var charge = stripeEvent.Data.Object as Charge;
                _logger.LogInformation("STRIPE --> Charge Suceeded: {Id}", charge?.Id);
                charge = null!;
            }
            else if (stripeEvent.Type == Events.CheckoutSessionExpired)
            {
                Session session = (Session)stripeEvent.Data.Object;
                _logger.LogInformation("STRIPE --> Checkout Session Expired: {Id}", session.Id);
                session = null!;
            }
            else
            {
                // Unexpected event type
                Session session = (Session)stripeEvent.Data.Object;
                _logger.LogInformation("STRIPE --> Unexpected event type for {Id}: {ev}", session.Id, stripeEvent.Type);
                session = null!;
            }

            return Ok(); // confirms to Stripe that the response has been received
        }
        catch (StripeException e)
        {
            _logger.LogError("STRIPE --> Stripe exception: {e}", e.Message);
            return BadRequest();
        }
        catch (Exception e)
        {
            _logger.LogError("STRIPE --> Exception: {e}", e.Message);
            return BadRequest();
        }
    }

    private async Task<decimal> GetItemQuote(BasketItem basketItem)
    {
        List<CreateQuoteItemDto> items = new();
        List<Dictionary<string, string>> assetList = new();
        Dictionary<string, string> assets = new();

        assets.Add("printArea", "default");
        assetList.Add(assets);

        items.Add(new CreateQuoteItemDto
        {
            Sku = basketItem.Product.ProdigiSku,
            Copies = 1,
            Assets = assetList
        });

        CreateQuoteDto quote = new CreateQuoteDto()
        {
            ShippingMethod = basketItem.ShippingMethod,
            DestinationCountryCode = "GB",
            CurrencyCode = "GBP",
            Items = items
        };

        var quoteResponse = await _quoteService.GetQuote(quote);

        if (quoteResponse is not null)
        {
            _logger.LogInformation("Prodigi --> Quote received");

            var quotes = quoteResponse.Quotes;
            var quoteReturned = quotes.FirstOrDefault();

            if (quoteReturned is not null)
            {
                if (!string.IsNullOrWhiteSpace(quoteReturned.CostSummary.TotalCost.Amount))
                {
                    decimal totalCost = decimal.Parse(quoteReturned.CostSummary.TotalCost.Amount);
                    return totalCost;
                }

                return 0;
            }
            else
            {
                return 0;
            }
        }
        else
        {
            _logger.LogWarning("Prodigi --> Quote NOT received");
            return 0;
        }
    }
}
