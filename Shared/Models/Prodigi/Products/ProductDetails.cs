namespace PhotoPortfolio.Shared.Models.Prodigi.Products;

public class ProductDetails
{
    public string Sku { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public ProductDimensions ProductDimensions { get; set; } = null!;
    public Dictionary<string, string>? Attributes { get; set; } = null!;
    public PrintArea PrintAreas { get; set; } = null!;
    public List<Variant> Variants { get; set; } = null!;
}
