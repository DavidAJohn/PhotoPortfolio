using PhotoPortfolio.Shared.Models;

namespace PhotoPortfolio.Client.Contracts;

public interface IOrderService
{
    Task<string> CreateCheckoutSession(OrderBasketDto orderBasketDto);
}
