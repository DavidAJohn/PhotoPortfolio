using Microsoft.AspNetCore.Mvc;
using Stripe;
using Stripe.Checkout;

namespace PhotoPortfolio.Server.Controllers;

public class PaymentsController : BaseApiController
{
    private readonly ILogger<AdminController> _logger;
    private readonly IConfiguration _config;
    private readonly IPaymentService _paymentService;

    public PaymentsController(ILogger<AdminController> logger, IConfiguration config, IPaymentService paymentService)
    {
        _logger = logger;
        _config = config;
        _paymentService = paymentService;
    }

    [HttpPost("session")]
    public async Task<IActionResult> CreateCheckoutSession(List<BasketItem> basketItems)
    {
        // TO DO: create a quote service (similar to client) to check the price of each basket item

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
                StripeList<LineItem> lineItems = expandedSession.LineItems;

                var customerEmail = session.CustomerDetails.Email;
                Console.WriteLine("STRIPE --> Order received from: " + session.CustomerDetails.Name + "(" + session.CustomerDetails.Email + ")");

                var shippingDetails = session.ShippingDetails;

                //await _orderService.PlaceOrder(customerEmail, lineItems, shippingDetails);
                session = null!;
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
}
