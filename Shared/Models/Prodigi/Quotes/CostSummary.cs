using PhotoPortfolio.Shared.Models.Prodigi.Common;

namespace PhotoPortfolio.Shared.Models.Prodigi.Quotes;

public class CostSummary
{
    public Cost? Items { get; set; }
    public Cost? Shipping { get; set; }
    public Cost? TotalCost { get; set; }
    public Cost? TotalTax { get; set; }
}
