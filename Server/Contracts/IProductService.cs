using PhotoPortfolio.Shared.Models.Prodigi.Products;

namespace PhotoPortfolio.Server.Contracts;

public interface IProductService
{
    Task<ProductDetails> GetProductDetails(string sku);
}
