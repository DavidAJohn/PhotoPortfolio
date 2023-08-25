using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using Serilog;

namespace PhotoPortfolio.Server.Data;

public class SeedData
{
    public static async Task SeedDatabaseCollections(MongoContext context, IConfiguration config)
    {
        var db = context.Client.GetDatabase(config.GetValue<string>("MongoConnection:DatabaseName"));

        // galleries
        var galleriesCollection = db.GetCollection<BsonDocument>("galleries");
        var galleries = galleriesCollection.Find(_ => true).ToList();

        if (galleries.Count == 0)
        {
            await InsertDocuments(galleriesCollection, "galleries.json");
        }

        // photos
        var photosCollection = db.GetCollection<BsonDocument>("photos");
        var photos = photosCollection.Find(_ => true).ToList();

        if (photos.Count == 0)
        {
            await InsertDocuments(photosCollection, "photos.json");
        }

        // products
        var productsCollection = db.GetCollection<BsonDocument>("products");
        var products = productsCollection.Find(_ => true).ToList();

        if (products.Count == 0)
        {
            await InsertDocuments(productsCollection, "products.json");
        }

        // preferences
        var prefsCollection = db.GetCollection<BsonDocument>("preferences");
        var prefs = prefsCollection.Find(_ => true).ToList();

        if (prefs.Count == 0)
        {
            await InsertDocuments(prefsCollection, "preferences.json");
        }

        // photo credits
        var creditsCollection = db.GetCollection<BsonDocument>("credits");
        var credits = creditsCollection.Find(_ => true).ToList();

        if (credits.Count == 0)
        {
            await InsertDocuments(creditsCollection, "credits.json");
        }
    }

    private static async Task InsertDocuments(IMongoCollection<BsonDocument> collection, string jsonFileName)
    {
        try
        {
            var json = await File.ReadAllTextAsync($"./Data/SeedData/{jsonFileName}");
            var bsonDocuments = BsonSerializer.Deserialize<BsonDocument[]>(json);

            if (bsonDocuments.Length > 0)
            {
                await collection.InsertManyAsync(bsonDocuments);
            }
        }
        catch (Exception)
        {
            Log.Error("An error occurred while seeding initial data in the '{collectionName}' collection", 
                collection.CollectionNamespace.CollectionName);
        }
    }
}
