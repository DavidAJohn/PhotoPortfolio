namespace PhotoPortfolio.Shared.Entities.Prodigi.Products;

public class ProductDetails
{
    public string Sku { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public ProductDimensions ProductDimensions { get; set; } = null!;
    public List<Attribute> Attributes { get; set; } = null!;
    public List<bool> PrintAreas { get; set; } = null!;
    public List<Variant> Variants { get; set; } = null!;
}
