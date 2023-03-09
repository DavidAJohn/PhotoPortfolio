using PhotoPortfolio.Shared.Models;
using Stripe.Checkout;

namespace PhotoPortfolio.Server.Contracts;

public interface IPaymentService
{
    Task<Session> CreateCheckoutSession(OrderBasketDto orderBasketDto);
    Task<CheckoutSessionResponse> GetOrderFromCheckoutSessionId(string sessionId);
}
