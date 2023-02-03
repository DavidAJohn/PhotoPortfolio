using PhotoPortfolio.Shared.Models.Prodigi.Common;

namespace PhotoPortfolio.Shared.Models.Prodigi.Orders;

public class ChargeItem
{
    public string Id { get; set; }
    public string Description { get; set; }
    public string ItemSku { get; set; }
    public string ShipmentId { get; set; }
    public string ItemId { get; set; }
    public string MerchantItemReference { get; set; }
    public Cost Cost { get; set; }
}
