using PhotoPortfolioStripe = PhotoPortfolio.Shared.Models.Stripe;

namespace PhotoPortfolio.Server.Contracts;

public interface IOrderService
{
    Task<bool> PlaceOrder(
        PhotoPortfolioStripe.Customer customer,
        PhotoPortfolioStripe.LineItems lineItems, 
        PhotoPortfolioStripe.ShippingDetails shippingDetails);
}
