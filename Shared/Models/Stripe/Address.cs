namespace PhotoPortfolio.Shared.Models.Stripe;

public class Address
{
    public string City { get; set; }
    public string Country { get; set; }
    public string Line1 { get; set; }
    public string Line2 { get; set; } = string.Empty;
    public string PostalCode { get; set; }
    public string State { get; set; } = string.Empty;
}
