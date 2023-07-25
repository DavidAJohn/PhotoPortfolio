using Stripe;

namespace PhotoPortfolio.Server.Contracts;

public interface IStripeClientFactory
{
    StripeClient Create();
}
