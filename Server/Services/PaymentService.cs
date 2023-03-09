using PhotoPortfolio.Shared.Models;
using Stripe;
using Stripe.Checkout;

namespace PhotoPortfolio.Server.Services;

public class PaymentService : IPaymentService
{
    private readonly IConfiguration _config;

    public PaymentService(IConfiguration config)
    {
        _config = config;
    }

    public async Task<Session> CreateCheckoutSession(OrderBasketDto orderBasketDto)
    {
        StripeConfiguration.ApiKey = _config["Stripe:SecretKey"];

        var basketItems = orderBasketDto.BasketItems;
        var lineItems = new List<SessionLineItemOptions>();

        basketItems.ForEach(item => lineItems.Add(new SessionLineItemOptions
        {
            PriceData = new SessionLineItemPriceDataOptions
            {
                UnitAmountDecimal = item.Total * 100,
                Currency = "gbp",
                ProductData = new SessionLineItemPriceDataProductDataOptions
                {
                    Name = item.Product.ImageTitle + " " + "(" + item.Product.CustomDescription + ")",
                    Images = new List<string> { item.Product.ImageUri },
                    Metadata = new Dictionary<string, string>()
                    {
                        { "product_id", item.Product.Id },
                        { "sku", item.Product.ProdigiSku }
                    }
                }
            },
            Quantity = item.Quantity
        })); 

        var options = new SessionCreateOptions
        {
            ShippingAddressCollection = new SessionShippingAddressCollectionOptions
            {
                AllowedCountries = new List<string>() { "GB", "US" }
            },
            PaymentMethodTypes = new List<string>
            {
                "card"
            },
            LineItems = lineItems,
            Mode = "payment",
            SuccessUrl = "https://localhost:7200/checkout/success?session_id={CHECKOUT_SESSION_ID}",
            CancelUrl = "https://localhost:7200/checkout",
            Metadata = new Dictionary<string, string>
            {
                { "shipping_method", orderBasketDto.ShippingMethod },
                { "order_id", orderBasketDto.OrderId }
            }
        };

        var service = new SessionService();
        Session session = service.Create(options);
        return session;
    }

    public async Task<CheckoutSessionResponse> GetOrderFromCheckoutSessionId(string sessionId)
    {
        StripeConfiguration.ApiKey = _config["Stripe:SecretKey"];

        var sessionService = new SessionService();
        Session session = sessionService.Get(sessionId);

        var sessionMetadata = session.Metadata;
        var orderId = sessionMetadata.FirstOrDefault(x => x.Key == "order_id").Value ?? "";
        var customerName = session.CustomerDetails.Name;

        var checkoutResponse = new CheckoutSessionResponse()
        {
            OrderId = orderId,
            CustomerName = customerName
        };

        return checkoutResponse;
    }
}
