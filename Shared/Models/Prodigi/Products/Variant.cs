namespace PhotoPortfolio.Shared.Models.Prodigi.Products;

public class Variant
{
    public Dictionary<string, string>? Attributes { get; set; } = null!;
    public string[] ShipsTo { get; set; } = null!;
    public PrintAreaSize PrintAreaSizes { get; set; } = null!;
}
