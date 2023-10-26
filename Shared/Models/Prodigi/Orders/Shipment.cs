using PhotoPortfolio.Shared.Models.Prodigi.Common;

namespace PhotoPortfolio.Shared.Models.Prodigi.Orders;

public class Shipment
{
    public string Id { get; set; }
    public string DispatchDate { get; set; }
    public Carrier Carrier { get; set; }
    public Dictionary<string, string>? Tracking { get; set; }
    public List<ShipmentItem> Items { get; set; }
    public FulfillmentLocation FulfillmentLocation { get; set; }
}
