namespace PhotoPortfolio.Shared.Entities.Prodigi;

public class Variant
{
    public List<Attribute> Attributes { get; set; } = null!;
    public string[] ShipsTo { get; set; } = null!;
    public List<PrintAreaDimensions> PrintAreaSizes { get; set; } = null!;
}
