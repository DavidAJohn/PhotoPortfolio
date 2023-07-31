using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using PhotoPortfolio.Server.Contracts;
using PhotoPortfolio.Server.Data;
using PhotoPortfolio.Server.Services;
using PhotoPortfolio.Shared.Helpers;
using PhotoPortfolio.Shared.Models;

namespace PhotoPortfolio.Tests.Integration.Server.Services;

public class OrderServiceTests : IClassFixture<PhotoApiFactory>
{
    private readonly PhotoApiFactory _apiFactory;
    private readonly HttpClient _client;
    private readonly MongoContext _mongoContext;
    private readonly IOrderRepository _orderRepository;
    private readonly IPreferencesRepository _preferencesRepository;
    private readonly IConfiguration _configuration;
    private readonly IConfigurationService _configService;
    private readonly ILogger<OrderService> _logger;

    public OrderServiceTests(PhotoApiFactory apiFactory)
    {
        _apiFactory = apiFactory;
        _client = _apiFactory.CreateClient();
        _client.BaseAddress = new Uri("https://localhost/api/");
        _client.DefaultRequestHeaders.Add("Accept", "application/json");

        _mongoContext = new MongoContext(_apiFactory.ConnectionString, PhotoApiFactory.TestDbName);
        _configuration = _apiFactory.Services.GetRequiredService<IConfiguration>();
        _configService = new ConfigurationService(_configuration);
        _preferencesRepository = new PreferencesRepository(_mongoContext);
        _logger = new Logger<OrderService>(new LoggerFactory());
        _orderRepository = new OrderRepository(_mongoContext);
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
    public async Task GetOrderDetails_ReturnsListOfOrderDetailsDto_WhenParamsNotNullAndOrdersExist()
    {
        // Arrange
        var orderService = new OrderService(_orderRepository, _preferencesRepository, _configService, _logger);

        var orderSpecParams = new OrderSpecificationParams()
        {
            InLastNumberOfDays = 365,
            SortBy = "OrderCreated",
            SortOrder = "Desc",
            ExcludePaymentIncomplete = false
        };

        var orderBasketDto = CreateOrderBasketDto();

        var orderId = await orderService.CreatOrder(orderBasketDto);

        // Act
        var result = await orderService.GetOrderDetails(orderSpecParams);

        // Assert
        orderId.Should().NotBeNull();

        result.Should().NotBeNull();
        result.Count.Should().Be(1);

        // Clean up
        await _orderRepository.DeleteAsync(orderId);
    }

    [Fact]
    public async Task GetOrderDetails_ReturnsListOfOrderDetailsDto_WhenSortOrderNotSpecifiedAndOrdersExist()
    {
        // Arrange
        var orderService = new OrderService(_orderRepository, _preferencesRepository, _configService, _logger);

        var orderSpecParams = new OrderSpecificationParams()
        {
            InLastNumberOfDays = 365,
            SortBy = "OrderCreated",
            //SortOrder = "Desc",
            ExcludePaymentIncomplete = false
        };

        var orderBasketDto = CreateOrderBasketDto();

        var orderId = await orderService.CreatOrder(orderBasketDto);

        // Act
        var result = await orderService.GetOrderDetails(orderSpecParams);

        // Assert
        orderId.Should().NotBeNull();

        result.Should().NotBeNull();
        result.Count.Should().Be(1);

        // Clean up
        await _orderRepository.DeleteAsync(orderId);
    }

    [Fact]
    public async Task GetOrderDetails_ReturnsListOfOrderDetailsDto_WhenSortOrderIsNotDescAndOrdersExist()
    {
        // Arrange
        var orderService = new OrderService(_orderRepository, _preferencesRepository, _configService, _logger);

        var orderSpecParams = new OrderSpecificationParams()
        {
            InLastNumberOfDays = 365,
            SortBy = "OrderCreated",
            SortOrder = "Asc",
            ExcludePaymentIncomplete = false
        };

        var orderBasketDto = CreateOrderBasketDto();

        var orderId = await orderService.CreatOrder(orderBasketDto);

        // Act
        var result = await orderService.GetOrderDetails(orderSpecParams);

        // Assert
        orderId.Should().NotBeNull();

        result.Should().NotBeNull();
        result.Count.Should().Be(1);

        // Clean up
        await _orderRepository.DeleteAsync(orderId);
    }
}
