namespace PhotoPortfolio.Client.Contracts;

public interface IOrderService
{
    Task<string> CreateCheckoutSession(List<BasketItem> basketItems);
}
