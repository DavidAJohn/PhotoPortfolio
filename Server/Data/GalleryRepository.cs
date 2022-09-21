using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using PhotoPortfolio.Shared.Entities;

namespace PhotoPortfolio.Server.Data;

public class GalleryRepository : BaseRepository<Gallery>, IGalleryRepository
{
    private readonly static string _collectionName = "galleries";
    private readonly MongoContext _context;

    public GalleryRepository(MongoContext context) : base(context, _collectionName)
    {
        _context = context;
    }

    public async Task<Gallery> GetGalleryWithPhotos(string id)
    {
        if (ObjectId.TryParse(id, out _)) //check that id is a valid 24 character hex string
        {
            var galleries = _context.Database.GetCollection<Gallery>(_collectionName);
            var filter = Builders<Gallery>.Filter.Where(g => g.Id == id);

            var bsonDoc = await galleries
                .Aggregate()
                .Match(filter)
                .Lookup("photos", "string", "string", "Photos")
                .FirstOrDefaultAsync();

            if (bsonDoc is null) return null!;

            var gallery = BsonSerializer.Deserialize<Gallery>(bsonDoc);

            return gallery;
        }

        return null!;
    }
}
