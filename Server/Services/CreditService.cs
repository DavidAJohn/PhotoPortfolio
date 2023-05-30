using PhotoPortfolio.Shared.Models;

namespace PhotoPortfolio.Server.Services;

public class CreditService : ICreditService
{
    private readonly MongoContext _context;
    private readonly IMongoCollection<PhotoCredit> _collection;

    public CreditService(MongoContext context)
    {
        _context = context;
        _collection = _context.Database.GetCollection<PhotoCredit>("credits");
    }

    public async Task<List<PhotoCredit>> GetPhotoCredits()
    {
        return await _collection.Find(_ => true)
            .SortBy(pc => pc.Order)
            .ToListAsync();
    }
}
