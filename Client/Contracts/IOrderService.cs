using PhotoPortfolio.Shared.Models;

namespace PhotoPortfolio.Client.Contracts;

public interface IOrderService
{
    Task<string> CreateCheckoutSession(OrderBasketDto orderBasketDto);
    Task<string> CreateInitialOrder(OrderBasketDto orderBasketDto);
    Task<CheckoutSessionResponse> GetOrderFromCheckoutSession(string sessionId);
    Task<OrderDetailsDto> GetOrderDetailsFromId(string orderId);
}
