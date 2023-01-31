using Stripe.Checkout;

namespace PhotoPortfolio.Server.Contracts;

public interface IPaymentService
{
    Task<Session> CreateCheckoutSession(List<BasketItem> basketItems);
}
