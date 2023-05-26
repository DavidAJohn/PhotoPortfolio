using PhotoPortfolio.Shared.Entities;

namespace PhotoPortfolio.Shared.Models;

public class PhotoCredit : BaseEntity
{
    public int Order { get; set; }
    public string CreatorName { get; set; }
    public string CreatorUnsplashProfileUri { get; set; }
    public string UnsplashImageUri { get; set; }
    public string PhotoPortfolioImageUri { get; set; }
}
