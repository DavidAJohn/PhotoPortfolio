using PhotoPortfolio.Shared.Models;
using PhotoPortfolioStripe = PhotoPortfolio.Shared.Models.Stripe;

namespace PhotoPortfolio.Server.Contracts;

public interface IOrderService
{
    Task<string> CreatOrder(List<BasketItem> lineItems, string shippingMethod);

    Task<bool> UpdateOrder(
        string orderId,
        PhotoPortfolioStripe.Customer customer,
        PhotoPortfolioStripe.LineItems lineItems, 
        PhotoPortfolioStripe.ShippingDetails shippingDetails,
        string shippingMethod
    );

    Task<OrderDetailsDto> GetOrderDetailsFromId(string orderId);
}
