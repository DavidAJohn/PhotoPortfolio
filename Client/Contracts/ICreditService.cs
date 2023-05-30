using PhotoPortfolio.Shared.Models;

namespace PhotoPortfolio.Client.Contracts;

public interface ICreditService
{
    Task<List<PhotoCredit>> GetPhotoCredits();
}
