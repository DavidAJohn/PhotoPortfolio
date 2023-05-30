using PhotoPortfolio.Shared.Models;

namespace PhotoPortfolio.Server.Contracts;

public interface ICreditService
{
    Task<List<PhotoCredit>> GetPhotoCredits();
}
