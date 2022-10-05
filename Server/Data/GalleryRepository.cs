using LinqKit;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using PhotoPortfolio.Shared.Entities;
using System.Linq.Expressions;

namespace PhotoPortfolio.Server.Data;

public class GalleryRepository : BaseRepository<Gallery>, IGalleryRepository
{
    private readonly static string _collectionName = "galleries";
    private readonly MongoContext _context;

    public GalleryRepository(MongoContext context) : base(context, _collectionName)
    {
        _context = context;
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

            var filter = Builders<Gallery>.Filter
                .Where(predicate);

            var bsonDoc = await galleries
                .Aggregate()
                .Match(filter) 
                .Lookup("photos", "GalleryId", "Id", "Photos") // just joins the two collections/tables without filtering
                .FirstOrDefaultAsync();

            if (bsonDoc is null) return null!;

            var gallery = BsonSerializer.Deserialize<Gallery>(bsonDoc);
            gallery.Photos = gallery.Photos.Where(p => p.GalleryId == id).ToList();

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
