using PhotoPortfolio.Shared.Entities;
using System.Linq.Expressions;

namespace PhotoPortfolio.Server.Data;

public class BaseRepository<T> : IBaseRepository<T> where T : BaseEntity
{
    private readonly MongoContext _context;
    private readonly IMongoCollection<T> _collection;

    public BaseRepository(MongoContext context, string collectionName)
    {
        _context = context;
        _collection = _context.Database.GetCollection<T>(collectionName);
    }

    public async Task<List<T>> GetAllAsync()
    {
        return await _collection.Find(_ => true).ToListAsync();
    }

    public async Task<T> GetSingleAsync(Expression<Func<T, bool>> predicate)
    {
        var filter = Builders<T>.Filter.Where(predicate);
        return (await _collection.FindAsync(filter)).FirstOrDefault();
    }

    public async Task<T> AddAsync(T obj)
    {
        await _collection.InsertOneAsync(obj);
        return obj;
    }

    public async Task<T> UpdateAsync(T obj)
    {
        return await _collection.FindOneAndReplaceAsync(x => x.Id == obj.Id, obj);
    }

    public async Task DeleteAsync(string id)
    {
        await _collection.DeleteOneAsync(x => x.Id == id);
    }
}
