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

            Session session = (Session)stripeEvent.Data.Object;

            switch (stripeEvent.Type)
            {
                case Events.CheckoutSessionCompleted:
                    _logger.LogInformation("Checkout Session Completed: {Id}", session.Id);

                    Console.WriteLine("Order received from: " + session.CustomerEmail);

                    var options = new SessionGetOptions();
                    options.AddExpand("line_items");

                    var service = new SessionService();
                    Session sessionWithLineItems = service.Get(session.Id, options);
                    StripeList<LineItem> lineItems = sessionWithLineItems.LineItems;

                    //var shippingDetails = session.ShippingDetails;

                    //await _orderService.PlaceOrder();
                    session = null!;
                    break;

                case Events.CheckoutSessionExpired:
                    _logger.LogInformation("Checkout Session Expired: {Id}", session.Id);

                    break;

                default:
                    _logger.LogError("Unhandled Stripe event type for {Id}: {ev}", session.Id, stripeEvent.Type);
                    break;
            }

            return new EmptyResult(); // confirms to Stripe that the response has been received
        }
        catch (StripeException e)
        {
            _logger.LogError("Stripe exception: {e}", e.Message);
            return BadRequest();
        }
        catch (Exception e)
        {
            _logger.LogError("Exception: {e}", e.Message);
            return BadRequest();
        }
        
    }
}
