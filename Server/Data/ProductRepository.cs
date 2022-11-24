using PhotoPortfolio.Shared.Entities;

namespace PhotoPortfolio.Server.Data;

public class ProductRepository : BaseRepository<Product>, IProductRepository
{
    private readonly MongoContext _context;
    private readonly static string _collectionName = "products";

    public ProductRepository(MongoContext context) : base(context, _collectionName)
    {
        _context = context;
    }
}
