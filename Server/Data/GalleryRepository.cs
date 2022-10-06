using LinqKit;
using MongoDB.Bson;
using PhotoPortfolio.Shared.Entities;
using System.Linq.Expressions;

namespace PhotoPortfolio.Server.Data;

public class GalleryRepository : BaseRepository<Gallery>, IGalleryRepository
{
    private readonly static string _collectionName = "galleries";
    private readonly MongoContext _context;
    private readonly IPhotoRepository _photoRepository;

    public GalleryRepository(MongoContext context, IPhotoRepository photoRepository) : base(context, _collectionName)
    {
        _context = context;
        _photoRepository = photoRepository;
    }

    public async Task<Gallery> GetGalleryWithPhotos(string id, bool includePrivate = false)
    {
        if (ObjectId.TryParse(id, out _)) //check that id is a valid 24 character hex string
        {
            var galleries = _context.Database.GetCollection<Gallery>(_collectionName);

            Expression<Func<Gallery, bool>> predicate = PredicateBuilder.New<Gallery>(g => g.Id == id);

            if (!includePrivate)
            {
                predicate = predicate.And(g => g.Public == true);
            }

            var gallery = await galleries.Find(predicate).FirstOrDefaultAsync();

            if (gallery is null) return null!;

            // now get the photos for that gallery, using its sort parameters
            PhotoSpecificationParams photoParams = new()
            {
                GalleryId = gallery.Id,
                SortBy = gallery.SortBy,
                SortOrder = gallery.SortOrder
            };

            gallery.Photos = await _photoRepository.GetFilteredPhotosAsync(photoParams);

            return gallery;
        }

        return null!;
    }

    public async Task<List<Gallery>> GetPublicGalleries()
    {
        var collection = _context.Database.GetCollection<Gallery>(_collectionName);
        var galleries = await collection.Find(g => g.Public == true).ToListAsync();

        return galleries;
    }
}
