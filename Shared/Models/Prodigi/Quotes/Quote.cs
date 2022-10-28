namespace PhotoPortfolio.Shared.Models.Prodigi.Quotes;

public class Quote
{
    public string ShipmentMethod { get; set; } = string.Empty;
    public CostSummary? CostSummary { get; set; }
    public List<Shipment>? Shipments { get; set; }
    public List<Item>? Items { get; set; }
}
