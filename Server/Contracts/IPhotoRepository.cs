using PhotoPortfolio.Shared.Entities;
using PhotoPortfolio.Shared.Helpers;

namespace PhotoPortfolio.Server.Contracts;

public interface IPhotoRepository : IBaseRepository<Photo>
{
    Task<List<Photo>> GetFilteredPhotosAsync(PhotoSpecificationParams photoParams);
}
