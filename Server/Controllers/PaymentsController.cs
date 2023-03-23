using Microsoft.AspNetCore.Mvc;
using PhotoPortfolio.Shared.Models;
using PhotoPortfolio.Shared.Models.Prodigi.Quotes;
using Stripe;
using Stripe.Checkout;
using PhotoPortfolioStripe = PhotoPortfolio.Shared.Models.Stripe;

namespace PhotoPortfolio.Server.Controllers;

public class PaymentsController : BaseApiController
{
    private readonly ILogger<PaymentsController> _logger;
    private readonly IConfiguration _config;
    private readonly IPaymentService _paymentService;
    private readonly IOrderService _orderService;
    private readonly IQuoteService _quoteService;

    public PaymentsController(ILogger<PaymentsController> logger, IConfiguration config, IPaymentService paymentService, IOrderService orderService, IQuoteService quoteService)
    {
        _logger = logger;
        _config = config;
        _paymentService = paymentService;
        _orderService = orderService;
        _quoteService = quoteService;
    }

    [HttpPost("session")]
    public async Task<IActionResult> CreateCheckoutSession(OrderBasketDto orderBasketDto)
    {
        // check the price of each basket item
        foreach (var item in orderBasketDto.BasketItems)
        {
            var quote = await GetItemQuote(item, orderBasketDto.ShippingMethod);

            if (quote != item.Total)
            {
                _logger.LogWarning("Basket item price differed from quoted price. Basket: {basket} - Quoted: {quote}", item.Total, quote);
            }

            // if quote received is not 0, then use it
            if (quote != 0) item.Total = quote;
        }

        // then supply it to the payment service
        var session = await _paymentService.CreateCheckoutSession(orderBasketDto);
        var url = session.Url;

        return Ok(url);
    }

    [HttpGet("session/{id}")]
    public async Task<IActionResult> GetCheckoutSessionFromId(string id)
    {
        var checkoutSessionResponse = await _paymentService.GetOrderFromCheckoutSessionId(id);

        if (checkoutSessionResponse == null) return BadRequest();

        return Ok(checkoutSessionResponse);
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
                
                var customer = new PhotoPortfolioStripe.Customer() // PhotoPortfolio.Shared.Models.Stripe
                {
                    Name = session.CustomerDetails.Name,
                    EmailAddress = session.CustomerDetails.Email
                };

                if (string.IsNullOrWhiteSpace(session.CustomerDetails.Name) || string.IsNullOrWhiteSpace(session.CustomerDetails.Email))
                {
                    // if the customer name or email address can't be retrieved from Stripe's response, this needs to be logged
                    _logger.LogWarning("Customer name or email could NOT be retrieved from Stripe metadata : {Id}", session.Id);
                }
                else
                {
                    _logger.LogInformation("STRIPE --> Order received from: {name} ({email})", session.CustomerDetails.Name, session.CustomerDetails.Email);
                }
                    
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

                var sessionMetadata = session.Metadata;
                var shippingMethod = sessionMetadata.FirstOrDefault(x => x.Key == "shipping_method").Value ?? "";
                var orderId = sessionMetadata.FirstOrDefault(x => x.Key == "order_id").Value ?? "";
                var paymentIntentId = session.PaymentIntentId;

                if (string.IsNullOrEmpty(orderId))
                {
                    // if the order_id can't be retrieved from Stripe's response, this needs to be logged
                    _logger.LogWarning("OrderId from Stripe metadata could not be retrieved: {Id}", session.Id);
                }

                // pass details to order service to update the db
                var response = await _orderService.UpdateOrder(orderId, customer, shippingDetails, shippingMethod, paymentIntentId);

                if (response)
                {
                    _logger.LogInformation("Order details from Stripe added to database successfully: {Id}", session.Id);
                    session = null!;
                }
                else
                {
                    _logger.LogInformation("Order details from Stripe could NOT be added to database: {Id}", session.Id);
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

    private async Task<decimal> GetItemQuote(BasketItem basketItem, string shippingMethod)
    {
        List<CreateQuoteItemDto> items = new();
        List<Dictionary<string, string>> assetList = new();
        Dictionary<string, string> assets = new()
        {
            { "printArea", "default" }
        };
        assetList.Add(assets);

        items.Add(new CreateQuoteItemDto
        {
            Sku = basketItem.Product.ProdigiSku,
            Copies = 1,
            Assets = assetList
        });

        CreateQuoteDto quote = new CreateQuoteDto()
        {
            ShippingMethod = shippingMethod,
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
