using PhotoPortfolio.Shared.Entities;

namespace PhotoPortfolio.Shared.Models;

public class PhotoProduct : Product
{
    public PhotoProduct()
    {
    }

    public PhotoProduct(Product product)
    {
        Id = product.Id;
    }

    public string CustomDescription { get; set; } = string.Empty;
    public string FurtherDetails { get; set; } = string.Empty;
    public string MockupImageUri { get; set; } = string.Empty;
    public int MarkupPercentage { get; set; } = 0;
}
