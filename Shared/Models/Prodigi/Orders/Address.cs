namespace PhotoPortfolio.Shared.Models.Prodigi.Orders;

public class Address
{
    public string Line1 { get; set; }
    public string Line2 { get; set; } = string.Empty;
    public string PostalOrZipCode { get; set; }
    public string CountryCode { get; set; }
    public string TownOrCity { get; set; }
    public string StateOrCounty { get; set; } = string.Empty;
}
