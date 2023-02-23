namespace PhotoPortfolio.Shared.Models.Stripe;

public class StripeOrder
{
    public LineItems LineItems { get; set; }
    public ShippingDetails ShippingDetails { get; set; }
}
