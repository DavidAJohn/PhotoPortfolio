using PhotoPortfolio.Shared.Entities;

namespace PhotoPortfolio.Server.Data;

public class GalleryRepository : BaseRepository<Gallery>, IGalleryRepository
{
    private readonly static string _collectionName = "galleries";

    public GalleryRepository(MongoContext context) : base(context, _collectionName)
    {
    }
}
