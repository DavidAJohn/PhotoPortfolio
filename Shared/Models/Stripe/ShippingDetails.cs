namespace PhotoPortfolio.Shared.Models.Stripe;

public class ShippingDetails
{
    public Address Address { get; set; }
    public string Carrier { get; set; } = string.Empty;
    public string Name { get; set; }
    public string Phone { get; set; } = string.Empty;
    public string TrackingNumber { get; set; } = string.Empty;
}
