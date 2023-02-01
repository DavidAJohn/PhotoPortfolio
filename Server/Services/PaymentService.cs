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

    public async Task<Session> CreateCheckoutSession(List<BasketItem> basketItems)
    {
        StripeConfiguration.ApiKey = _config["Stripe:SecretKey"];

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
                    Images = new List<string> { item.Product.ImageUri }
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
            SuccessUrl = "https://localhost:7200/checkout/success",
            CancelUrl = "https://localhost:7200/checkout"
        };

        var service = new SessionService();
        Session session = service.Create(options);
        return session;
    }
}
