using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PhotoPortfolio.Server.Contracts;
using PhotoPortfolio.Server.Helpers;
using PhotoPortfolio.Server.Services;
using PhotoPortfolio.Shared.Models;

namespace PhotoPortfolio.Tests.Integration.Server.Services;

public class PaymentServiceTests : IClassFixture<PhotoApiFactory>
{
    private readonly PhotoApiFactory _apiFactory;
    private readonly HttpClient _client;
    private readonly IConfiguration _configuration;
    private readonly IConfigurationService _configService;
    private readonly IStripeClientFactory _stripeClientFactory;
    private readonly ILogger<PaymentService> _logger;
    private readonly IHttpContextAccessor _httpContext;

    public PaymentServiceTests(PhotoApiFactory apiFactory)
    {
        _apiFactory = apiFactory;
        _client = _apiFactory.CreateClient();
        _client.BaseAddress = new Uri("https://localhost/api/");
        _client.DefaultRequestHeaders.Add("Accept", "application/json");

        _configuration = _apiFactory.Services.GetRequiredService<IConfiguration>();
        _configService = new ConfigurationService(_configuration);
        _stripeClientFactory = new StripeClientFactory(_configService);
        _logger = new Logger<PaymentService>(new LoggerFactory());

        _httpContext = new HttpContextAccessor
        {
            HttpContext = new DefaultHttpContext()
        };
        _httpContext.HttpContext!.Request.Scheme = "https";
        _httpContext.HttpContext.Request.Host = new HostString("localhost");
    }

    [Fact]
    public async Task CreateCheckoutSession_ReturnsStripeCheckoutSession_WhenBasketAndStripeConfigAreValid()
    {
        // Arrange
        var paymentService = new PaymentService(_logger, _stripeClientFactory, _httpContext);

        var orderBasket = new OrderBasketDto
        {
            BasketItems = new List<BasketItem>
            {
                new BasketItem
                {
                    Quantity = 1,
                    Product = new ProductBasketItemDto
                    {
                        Id = Guid.NewGuid().ToString(),
                        PhotoId = Guid.NewGuid().ToString(),
                        ImageUri = "https://localhost/images/1.jpg",
                        ImageTitle = "Test",
                    },
                    Total = 100,
                }
            },
            ShippingCost = 5.25m,
            ShippingMethod = "Standard"
        };

        // Act
        var response = await paymentService.CreateCheckoutSession(orderBasket);

        // Assert
        response.Should().NotBeNull();
        response.Should().BeOfType<Stripe.Checkout.Session>();
        response.Url.Should().NotBeNullOrEmpty();
        response.Url.Should().StartWith("https://checkout.stripe.com/");
        response.PaymentMethodTypes.Should().Contain("card");
    }

    [Fact]
    public async Task CreateCheckoutSession_ReturnsNull_WhenStripeSecretKeyIsInvalid()
    {
        // Arrange
        _configuration["Stripe:SecretKey"] = "bad_key";
        var paymentService = new PaymentService(_logger, _stripeClientFactory, _httpContext);

        var orderBasket = new OrderBasketDto
        {
            BasketItems = new List<BasketItem>
            {
                new BasketItem
                {
                    Quantity = 1,
                    Product = new ProductBasketItemDto
                    {
                        Id = Guid.NewGuid().ToString(),
                        PhotoId = Guid.NewGuid().ToString(),
                        ImageUri = "https://localhost/images/1.jpg",
                        ImageTitle = "Test",
                    },
                    Total = 100,
                }
            },
            ShippingCost = 5.25m,
            ShippingMethod = "Standard"
        };

        // Act
        var response = await paymentService.CreateCheckoutSession(orderBasket);

        // Assert
        response.Should().BeNull();
    }

    [Fact]
    public async Task GetOrderFromCheckoutSessionId_ReturnsCheckoutSessionResponse_WhenSessionIdIsValid()
    {
        // Arrange
        var paymentService = new PaymentService(_logger, _stripeClientFactory, _httpContext);

        var orderBasket = new OrderBasketDto
        {
            BasketItems = new List<BasketItem>
            {
                new BasketItem
                {
                    Quantity = 1,
                    Product = new ProductBasketItemDto
                    {
                        Id = Guid.NewGuid().ToString(),
                        PhotoId = Guid.NewGuid().ToString(),
                        ImageUri = "https://localhost/images/1.jpg",
                        ImageTitle = "Test",
                    },
                    Total = 100,
                }
            },
            ShippingCost = 5.25m,
            ShippingMethod = "Standard"
        };

        var session = await paymentService.CreateCheckoutSession(orderBasket);

        // Act
        var response = await paymentService.GetOrderFromCheckoutSessionId(session.Id);

        // Assert
        response.Should().NotBeNull();
        response.Should().BeOfType<CheckoutSessionResponse>();
    }

    [Fact]
    public async Task GetOrderFromCheckoutSessionId_ReturnsNull_WhenSessionIdIsNotValid()
    {
        // Arrange
        var paymentService = new PaymentService(_logger, _stripeClientFactory, _httpContext);

        // Act
        var response = await paymentService.GetOrderFromCheckoutSessionId("");

        // Assert
        response.Should().BeNull();
    }
}
