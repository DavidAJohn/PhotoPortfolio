namespace PhotoPortfolio.Shared.Models.Prodigi.Quotes;

public class Shipment
{
    public Carrier Carrier { get; set; } = null!;
    public FulfillmentLocation FulfillmentLocation { get; set; } = null!;
    public Cost Cost { get; set; } = null!;
    public List<string> Items { get; set; } = null!;
}
