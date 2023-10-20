using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using PhotoPortfolio.Server.Contracts;
using PhotoPortfolio.Server.Data;
using PhotoPortfolio.Server.Messaging;
using PhotoPortfolio.Server.Services;
using PhotoPortfolio.Shared.Models;
using PhotoPortfolio.Tests.Integration.Infrastructure;
using System.Net;

namespace PhotoPortfolio.Tests.Integration.Server.Controllers.PaymentsController;

public class StripeWebhookTests : IClassFixture<PhotoApiFactory>
{
    private readonly PhotoApiFactory _apiFactory;
    private readonly HttpClient _client;
    private readonly MongoContext _mongoContext;
    private readonly IConfiguration _configuration;
    private readonly IConfigurationService _configService;
    private readonly IOrderService _orderService;
    private readonly IOrderRepository _orderRepository;
    private readonly ILogger<OrderService> _orderServiceLogger;
    private readonly IPreferencesRepository _preferencesRepository;
    private readonly IMessageSender _messageSender;
    private readonly IHttpClientFactory _httpClientFactory;

    public StripeWebhookTests(PhotoApiFactory apiFactory)
    {
        _apiFactory = apiFactory;
        _client = _apiFactory.CreateClient();
        _client.BaseAddress = new Uri("https://localhost/api/");

        _configuration = _apiFactory.Services.GetRequiredService<IConfiguration>();
        _configService = new ConfigurationService(_configuration);
        _mongoContext = new MongoContext(_apiFactory.ConnectionString, PhotoApiFactory.TestDbName);
        _preferencesRepository = new PreferencesRepository(_mongoContext);
        _orderServiceLogger = new Logger<OrderService>(new LoggerFactory());
        _messageSender = _apiFactory.Services.GetRequiredService<IMessageSender>();
        _orderRepository = new OrderRepository(_mongoContext);
        _httpClientFactory = _apiFactory.Services.GetRequiredService<IHttpClientFactory>();
        _orderService = new OrderService(_orderRepository, _preferencesRepository, _configService, _messageSender, _orderServiceLogger, _httpClientFactory);
    }

    private static OrderBasketDto CreateOrderBasketDto()
    {
        return new OrderBasketDto
        {
            BasketItems = new List<BasketItem>
            {
                new BasketItem
                {
                    Quantity = 1,
                    Product = new ProductBasketItemDto
                    {
                        Id = ObjectId.GenerateNewId().ToString(),
                        PhotoId = ObjectId.GenerateNewId().ToString(),
                        ImageUri = "https://localhost/images/1.jpg",
                        ImageTitle = "Test",
                    },
                    Total = 100m,
                }
            },
            ShippingMethod = "Standard",
            ShippingCost = 5.25m
        };
    }

    [Fact]
    public async Task StripeWebhook_ReturnsOkAndUpdatesOrder_WhenCheckoutSessionCompletes()
    {
        // Arrange
        var orderBasketDto = CreateOrderBasketDto();
        var orderId = await _orderService.CreatOrder(orderBasketDto);

        var config = _configService.GetConfiguration();
        var stripeWebhookSecret = config["Stripe:WhSecret"];

        var webhookEvent = StripeWebhookHelperMethods.CreateCheckoutSessionCompleteEvent(orderId);
        var webhookContent = StripeWebhookHelperMethods.CreateWebhookContent(webhookEvent, stripeWebhookSecret!);

        // Act
        var response = await _client.PostAsync("payments/webhook", webhookContent);
        var updatedOrder = await _orderService.GetOrderDetailsFromId(orderId);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        updatedOrder.Id.Should().Be(orderId);
        updatedOrder.Status.Should().Be(OrderStatus.AwaitingApproval.ToString());

        var sessionDataObj = (Stripe.Checkout.Session)webhookEvent.Data.Object;
        updatedOrder.Id.Should().Be(sessionDataObj.Metadata["order_id"]);
        updatedOrder.Name.Should().Be(sessionDataObj.CustomerDetails.Name);
        updatedOrder.EmailAddress.Should().Be(sessionDataObj.CustomerDetails.Email);
        updatedOrder.Address.Line1.Should().Be(sessionDataObj.ShippingDetails.Address.Line1);
        updatedOrder.Address.Line2.Should().Be(sessionDataObj.ShippingDetails.Address.Line2);
        updatedOrder.Address.TownOrCity.Should().Be(sessionDataObj.ShippingDetails.Address.City);
        updatedOrder.Address.PostalOrZipCode.Should().Be(sessionDataObj.ShippingDetails.Address.PostalCode);
        updatedOrder.Address.CountryCode.Should().Be(sessionDataObj.ShippingDetails.Address.Country);
        updatedOrder.Address.StateOrCounty.Should().Be(sessionDataObj.ShippingDetails.Address.State);

        // Clean up
        await _orderRepository.DeleteAsync(orderId);
    }

    [Fact]
    public async Task StripeWebhook_ReturnsOk_WhenPaymentIntentCreatedEventReceived()
    {
        // Arrange
        var config = _configService.GetConfiguration();
        var stripeWebhookSecret = config["Stripe:WhSecret"];

        var webhookEvent = StripeWebhookHelperMethods.CreatePaymentIntentCreatedEvent();
        var webhookContent = StripeWebhookHelperMethods.CreateWebhookContent(webhookEvent, stripeWebhookSecret!);

        // Act
        var response = await _client.PostAsync("payments/webhook", webhookContent);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task StripeWebhook_ReturnsOk_WhenPaymentIntentSucceededEventReceived()
    {
        // Arrange
        var config = _configService.GetConfiguration();
        var stripeWebhookSecret = config["Stripe:WhSecret"];

        var webhookEvent = StripeWebhookHelperMethods.CreatePaymentIntentSucceededEvent();
        var webhookContent = StripeWebhookHelperMethods.CreateWebhookContent(webhookEvent, stripeWebhookSecret!);

        // Act
        var response = await _client.PostAsync("payments/webhook", webhookContent);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task StripeWebhook_ReturnsOk_WhenChargeSucceededEventReceived()
    {
        // Arrange
        var config = _configService.GetConfiguration();
        var stripeWebhookSecret = config["Stripe:WhSecret"];

        var webhookEvent = StripeWebhookHelperMethods.CreateChargeSucceededEvent();
        var webhookContent = StripeWebhookHelperMethods.CreateWebhookContent(webhookEvent, stripeWebhookSecret!);

        // Act
        var response = await _client.PostAsync("payments/webhook", webhookContent);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
