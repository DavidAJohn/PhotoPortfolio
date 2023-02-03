using PhotoPortfolio.Shared.Models.Prodigi.Common;

namespace PhotoPortfolio.Shared.Models.Prodigi.Quotes;

public class Item
{
    public string Id { get; set; } = string.Empty;
    public string Sku { get; set; } = string.Empty;
    public int Copies { get; set; }
    public Cost? UnitCost { get; set; }
    public Dictionary<string, string>? Attributes { get; set; }
    public List<Dictionary<string, string>>? Assets { get; set; }
    public Cost? TaxUnitCost { get; set; }
}
