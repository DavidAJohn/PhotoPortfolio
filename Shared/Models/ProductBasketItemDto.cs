using PhotoPortfolio.Shared.Entities;

namespace PhotoPortfolio.Shared.Models;

public class ProductBasketItemDto : PhotoProduct
{
    public ProductBasketItemDto()
    {
    }

    public ProductBasketItemDto(Product product) : base(product)
    {
    }

    public string PhotoId { get; set; } = string.Empty;
    public string ImageUri { get; set; } = string.Empty;
    public string ImageTitle { get; set; } = string.Empty;
    public List<ProductOptionSelected>? Options { get; set; } = null!;
}
