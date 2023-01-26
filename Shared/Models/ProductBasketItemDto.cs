namespace PhotoPortfolio.Shared.Models;

public class ProductBasketItemDto : PhotoProduct
{
    public string ImageUri { get; set; } = string.Empty;
    public string ImageTitle { get; set; } = string.Empty;
}
