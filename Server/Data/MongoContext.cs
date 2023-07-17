namespace PhotoPortfolio.Server.Data;

public class MongoContext
{
    private readonly MongoClient _client;
    private readonly IMongoDatabase _database;

    public MongoContext(string? connectionString, string? databaseName)
    {
        _client = new MongoClient(connectionString);
        _database = _client.GetDatabase(databaseName);
    }

    public IMongoClient Client => _client;

    public IMongoDatabase Database => _database;
}
