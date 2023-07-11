using PhotoPortfolio.Shared.Models;

namespace PhotoPortfolio.Server.Contracts;

public interface IUploadService
{
    Task<List<UploadResult>> UploadFiles(IEnumerable<IFormFile> files);
}
