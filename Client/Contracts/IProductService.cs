using PhotoPortfolio.Shared.Entities;
using PhotoPortfolio.Shared.Models.Prodigi.Products;

namespace PhotoPortfolio.Client.Contracts;

public interface IProductService
{
    Task<List<Product>> GetProductsForPhotoAsync(string photoId);
    Task<ProductDetails> GetProductDetailsAsync(string sku);
}
