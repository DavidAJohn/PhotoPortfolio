using PhotoPortfolio.Shared.Entities;

namespace PhotoPortfolio.Server.Contracts;

public interface IPhotoRepository : IBaseRepository<Photo>
{
    Task<List<Photo>> GetFilteredPhotosAsync(PhotoSpecificationParams photoParams);
}
