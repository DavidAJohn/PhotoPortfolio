using PhotoPortfolio.Shared.Entities;

namespace PhotoPortfolio.Client.Contracts;

public interface IGalleryService
{
    Task<List<Gallery>> GetGalleriesAsync();
    Task<Gallery> GetGalleryByIdAsync(string id);
}
