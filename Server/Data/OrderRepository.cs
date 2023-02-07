using PhotoPortfolio.Shared.Entities;

namespace PhotoPortfolio.Server.Data;

public class OrderRepository : BaseRepository<Order>, IOrderRepository
{
    private readonly static string _collectionName = "orders";

    public OrderRepository(MongoContext context) : base(context, _collectionName)
    {
    }
}
