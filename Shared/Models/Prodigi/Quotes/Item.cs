using PhotoPortfolio.Shared.Models.Prodigi.Common;

namespace PhotoPortfolio.Shared.Models.Prodigi.Quotes;

public class Item
{
    public string Id { get; set; } = string.Empty;
    public string Sku { get; set; } = string.Empty;
    public int Copies { get; set; }
    public Cost? UnitCost { get; set; }
    public List<ProdigiAttribute>? Attributes { get; set; }
    public List<ProdigiAttribute>? Assets { get; set; }
}
