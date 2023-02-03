using PhotoPortfolio.Shared.Models.Prodigi.Common;

namespace PhotoPortfolio.Shared.Models.Prodigi.Orders;

public class Shipment
{
    public string Id { get; set; }
    public string Carrier { get; set; } = string.Empty;
    public string Tracking { get; set; } = string.Empty;
    public string DispatchDate { get; set; }
    public List<ShipmentItem> ShipmentItems { get; set; }
    public FulfillmentLocation FulfillmentLocation { get; set; }
}
