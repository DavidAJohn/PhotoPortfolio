using Microsoft.AspNetCore.Mvc;
using PhotoPortfolio.Shared.Models;
using Stripe;
using Stripe.Checkout;
using PhotoPortfolioStripe = PhotoPortfolio.Shared.Models.Stripe;

namespace PhotoPortfolio.Server.Controllers;

public class PaymentsController : BaseApiController
{
    private readonly ILogger<PaymentsController> _logger;
    private readonly IConfigurationService _configService;
    private readonly IPaymentService _paymentService;
    private readonly IOrderService _orderService;
    private readonly IQuoteService _quoteService;

    public PaymentsController(ILogger<PaymentsController> logger, IConfigurationService configService, IPaymentService paymentService, IOrderService orderService, IQuoteService quoteService)
    {
        _logger = logger;
        _configService = configService;
        _paymentService = paymentService;
        _orderService = orderService;
        _quoteService = quoteService;
    }

    [HttpPost("session")]
    public async Task<IActionResult> CreateCheckoutSession(OrderBasketDto orderBasketDto)
    {
        // check that the basket is still consistent with a quote from Prodigi
        OrderBasketDto updatedBasket = await _quoteService.GetBasketQuote(orderBasketDto, IsUserAdmin());

        // update the order in the database, in case values have been changed
        var updateCostsResponse = await _orderService.UpdateOrderCosts(updatedBasket);

        if (updateCostsResponse == false)
        {
            _logger.LogError("Problem updating order costs in OrderId : {orderId}", orderBasketDto.OrderId);
            return BadRequest();
        }
        
        // then supply it to the payment service
        var session = await _paymentService.CreateCheckoutSession(updatedBasket);

        if (session == null) return BadRequest();
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
        var config = _configService.GetConfiguration();
        string WhSecret = config["Stripe:WhSecret"];

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

    private bool IsUserAdmin()
    {
        var config = _configService.GetConfiguration();
        var adminUserName = config["AdminUserName"];

        if (User?.Identity is not null && User.Identity.IsAuthenticated)
        {
            if (!string.IsNullOrWhiteSpace(adminUserName))
            {
                if (adminUserName != User.Identity.Name)
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
            else
            {
                return true;
            }
        }
        else
        {
            return false;
        }
    }
}
