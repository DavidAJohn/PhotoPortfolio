using PhotoPortfolio.Shared.Models;
using PhotoPortfolio.Shared.Helpers;
using PhotoPortfolioStripe = PhotoPortfolio.Shared.Models.Stripe;

namespace PhotoPortfolio.Server.Contracts;

public interface IOrderService
{
    Task<string> CreatOrder(OrderBasketDto orderBasketDto);

    Task<bool> UpdateOrder(
        string orderId,
        PhotoPortfolioStripe.Customer customer,
        PhotoPortfolioStripe.ShippingDetails shippingDetails,
        string shippingMethod,
        string paymentIntentId
    );

    Task<bool> UpdateOrderCosts(OrderBasketDto orderBasketDto);

    Task<OrderDetailsDto> GetOrderDetailsFromId(string orderId);
    Task<List<OrderDetailsDto>> GetOrderDetails(OrderSpecificationParams? orderParams = null!);
    Task<bool> ShouldApproveOrder(string orderId);
}
