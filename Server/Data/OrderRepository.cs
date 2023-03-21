using PhotoPortfolio.Shared.Entities;
using PhotoPortfolio.Shared.Helpers;
using System.Linq.Expressions;

namespace PhotoPortfolio.Server.Data;

public class OrderRepository : BaseRepository<Order>, IOrderRepository
{
    private readonly static string _collectionName = "orders";
    private readonly MongoContext _context;

    public OrderRepository(MongoContext context) : base(context, _collectionName)
    {
        _context = context;
    }

    public Expression<Func<Order, object>> SortByPredicate { get; set; } = null!;

    public async Task<List<Order>> GetFilteredOrdersAsync(OrderSpecificationParams orderParams)
    {
        var _orders = _context.Database.GetCollection<Order>(_collectionName);
        var orderPredicate = OrderSpecification.CreateOrderSpecificationPredicate(orderParams);

        var filter = Builders<Order>.Filter.Where(orderPredicate);

        if (orderParams.SortBy is not null)
        {
            SortByPredicate = orderParams.SortBy.ToLower() switch
            {
                "ordercreated" => o => o.OrderCreated,
                _ => o => o.OrderCreated
            };
        }

        if (orderParams.SortOrder is not null)
        {
            if (orderParams.SortOrder.ToLower() == "desc")
            {
                return await _orders
                    .Find(filter)
                    .SortByDescending(SortByPredicate)
                    .ToListAsync();
            }

            return await _orders
                .Find(filter)
                .SortBy(SortByPredicate)
                .ToListAsync();
        }

        return await _orders
            .Find(filter)
            .SortBy(o => o.OrderCreated)
            .ToListAsync();
    }
}
