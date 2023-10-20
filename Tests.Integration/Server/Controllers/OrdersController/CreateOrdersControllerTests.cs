using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using PhotoPortfolio.Server.Contracts;
using PhotoPortfolio.Server.Data;
using PhotoPortfolio.Server.Messaging;
using PhotoPortfolio.Server.Services;
using PhotoPortfolio.Shared.Models;
using System.Net;
using System.Net.Http.Json;

namespace PhotoPortfolio.Tests.Integration.Server.Controllers.OrdersController;

public class CreateOrdersControllerTests : IClassFixture<PhotoApiFactory>
{
    private readonly PhotoApiFactory _apiFactory;
    private readonly HttpClient _client;
    private readonly MongoContext _mongoContext;
    private readonly IPreferencesRepository _preferencesRepository;
    private readonly IOrderRepository _orderRepository;
    private readonly IConfiguration _configuration;
    private readonly IConfigurationService _configService;
    private readonly IOrderService _orderService;
    private readonly ILogger<OrderService> _orderServiceLogger;
    private readonly IMessageSender _messageSender;
    private readonly ILogger<CreateOrdersControllerTests> _logger;
    private readonly IHttpClientFactory _httpClientFactory;

    public CreateOrdersControllerTests(PhotoApiFactory apiFactory)
    {
        _apiFactory = apiFactory;
        _client = _apiFactory.CreateClient();
        _client.BaseAddress = new Uri("https://localhost/api/");
        _client.DefaultRequestHeaders.Add("Accept", "application/json");

        _mongoContext = new MongoContext(_apiFactory.ConnectionString, PhotoApiFactory.TestDbName);

        _preferencesRepository = new PreferencesRepository(_mongoContext);
        _orderRepository = new OrderRepository(_mongoContext);
        _configuration = _apiFactory.Services.GetRequiredService<IConfiguration>();
        _configService = new ConfigurationService(_configuration);
        _logger = new Logger<CreateOrdersControllerTests>(new LoggerFactory());
        _orderServiceLogger = new Logger<OrderService>(new LoggerFactory());
        _messageSender = _apiFactory.Services.GetRequiredService<IMessageSender>();
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
    public async Task CreateOrder_ReturnsOkAndOrderId_WhenOrderCreated()
    {
        // Arrange
        var orderBasketDto = CreateOrderBasketDto();

        // Act
        var response = await _client.PostAsJsonAsync("orders", orderBasketDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<string>();
        result!.Should().NotBeNullOrEmpty();

        // Clean up
        await _orderRepository.DeleteAsync(result!);
    }
}
