using PhotoPortfolio.Shared.Models.Prodigi.Common;

namespace PhotoPortfolio.Shared.Models.Prodigi.Orders;

public class Item
{
    public string Id { get; set; } = string.Empty;
    public string MerchantReference { get; set; } = string.Empty;
    public string Sku { get; set; }
    public int Copies { get; set; } = 1;
    public string Sizing { get; set; }
    public Cost? RecipientCost { get; set; }
    public Dictionary<string, string>? Attributes { get; set; }
    public List<Dictionary<string, string>> Assets { get; set; }
}
