using PhotoPortfolio.Shared.Entities;
using PhotoPortfolio.Shared.Helpers;

namespace PhotoPortfolio.Server.Contracts;

public interface IOrderRepository : IBaseRepository<Order>
{
    Task<List<Order>> GetFilteredOrdersAsync(OrderSpecificationParams orderParams);
}
