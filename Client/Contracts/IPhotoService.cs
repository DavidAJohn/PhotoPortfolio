using PhotoPortfolio.Shared.Entities;

namespace PhotoPortfolio.Client.Contracts
{
    public interface IPhotoService
    {
        Task<Photo> GetPhotoByIdAsync(string id);
    }
}
