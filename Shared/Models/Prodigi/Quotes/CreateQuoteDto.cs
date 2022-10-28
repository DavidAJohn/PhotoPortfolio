namespace PhotoPortfolio.Shared.Models.Prodigi.Quotes;

public class CreateQuoteDto
{
    public string ShippingMethod { get; set; } = "Standard";
    public string DestinationCountryCode { get; set; }
    public string? CurrencyCode { get; set; }
    public List<Item> Items { get; set; }
}
