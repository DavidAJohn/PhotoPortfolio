using PhotoPortfolio.Shared.Entities;

namespace PhotoPortfolio.Server.Contracts;

public interface IProductRepository : IBaseRepository<Product>
{
    Task<List<Product>> GetProductsForPhotoAsync(string photoId);
}
