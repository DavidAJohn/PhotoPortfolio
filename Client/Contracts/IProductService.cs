using PhotoPortfolio.Shared.Entities;

namespace PhotoPortfolio.Client.Contracts
{
    public interface IProductService
    {
        Task<List<Product>> GetProductsForPhotoAsync(string photoId);
    }
}
