using Stripe;

namespace PhotoPortfolio.Server.Helpers;

public class StripeClientFactory : IStripeClientFactory
{
    private readonly IConfigurationService _configService;

    public StripeClientFactory(IConfigurationService configService)
    {
        _configService = configService;
    }

    public StripeClient Create()
    {
        var config = _configService.GetConfiguration();
        var secretKey = config.GetValue<string>("Stripe:SecretKey");
        var apiBaseValue = config.GetValue<string>("Stripe:ApiBase");

        // StripeClient's ApiBase property only needs to be specified when testing, to point to a mock server
        // The default value if omitted is https://api.stripe.com
        // More details: https://github.com/stripe/stripe-dotnet/wiki/Advanced-client-usage#using-custom-base-urls

        if (string.IsNullOrWhiteSpace(apiBaseValue))
        {
            var stripeClient = new StripeClient(
                secretKey
            );

            return stripeClient;
        }
        else
        {
            var stripeClient = new StripeClient(
                secretKey,
                apiBase: apiBaseValue
            );

            return stripeClient;
        }
    }
}
