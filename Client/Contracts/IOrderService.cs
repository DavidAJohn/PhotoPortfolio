using PhotoPortfolio.Shared.Models;

namespace PhotoPortfolio.Client.Contracts;

public interface IOrderService
{
    Task<string> CreateCheckoutSession(OrderBasketDto orderBasketDto, bool userIsAdmin);
    Task<string> CreateInitialOrder(OrderBasketDto orderBasketDto);
    Task<CheckoutSessionResponse> GetOrderFromCheckoutSession(string sessionId);
    Task<OrderDetailsDto> GetOrderDetailsFromId(string orderId);
    Task<bool> ShouldApproveOrder(string orderId);
}
