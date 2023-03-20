using PhotoPortfolio.Shared.Entities;

namespace PhotoPortfolio.Server.Contracts;

public interface IOrderRepository : IBaseRepository<Order>
{
    Task<List<Order>> GetFilteredOrdersAsync(OrderSpecificationParams orderParams);
}
