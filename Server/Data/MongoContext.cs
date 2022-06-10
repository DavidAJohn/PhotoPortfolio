namespace PhotoPortfolio.Server.Data;

public class MongoContext
{
    private readonly MongoClient _client;
    private readonly IMongoDatabase _database;
    private readonly IConfiguration _config;

    public MongoContext(IConfiguration config)
    {
        _config = config;
        _client = new MongoClient(_config["MongoConnection:ConnectionString"]);
        _database = _client.GetDatabase(_config["MongoConnection:DatabaseName"]);
    }

    public IMongoClient Client => _client;

    public IMongoDatabase Database => _database;
}
