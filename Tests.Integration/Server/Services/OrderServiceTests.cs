using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using PhotoPortfolio.Server.Contracts;
using PhotoPortfolio.Server.Data;
using PhotoPortfolio.Server.Messaging;
using PhotoPortfolio.Server.Services;
using PhotoPortfolio.Shared.Helpers;
using PhotoPortfolio.Shared.Models;
using PhotoPortfolio.Shared.Models.Stripe;

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
    private readonly IMessageSender _messageSender;
    private readonly ILogger<OrderService> _logger;
    private readonly ILogger<MessageSender> _messageSenderLogger;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly string _prodigiWireMockUri;

    public OrderServiceTests(PhotoApiFactory apiFactory)
    {
        _apiFactory = apiFactory;
        _client = _apiFactory.CreateClient();
        _client.BaseAddress = new Uri("https://localhost/api/");
        _client.DefaultRequestHeaders.Add("Accept", "application/json");
        _prodigiWireMockUri = _apiFactory.ProdigiPrintApiUrl + "/";

        _mongoContext = new MongoContext(_apiFactory.ConnectionString, PhotoApiFactory.TestDbName);
        _configuration = _apiFactory.Services.GetRequiredService<IConfiguration>();
        _configService = new ConfigurationService(_configuration);
        _preferencesRepository = new PreferencesRepository(_mongoContext);
        _logger = new Logger<OrderService>(new LoggerFactory());
        _orderRepository = new OrderRepository(_mongoContext);
        _messageSenderLogger = new Logger<MessageSender>(new LoggerFactory());
        _messageSender = new MessageSender(_apiFactory.Services.GetRequiredService<IAzureClientFactory<ServiceBusSender>>(), _configuration, _messageSenderLogger);
        _httpClientFactory = _apiFactory.Services.GetRequiredService<IHttpClientFactory>();
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

    private static Customer CreateStripeCustomer()
    {
        return new Customer
        {
            Name = "Test Customer",
            EmailAddress = "test@test.com"
        };
    }

    private static ShippingDetails CreateStripeShippingDetails()
    {
        return new ShippingDetails
        {
            Name = "Test Customer",
            Address = new Address
            {
                Line1 = "Test Address",
                Line2 = "Test Address",
                City = "Test City",
                State = "Test State",
                PostalCode = "TE3T 1NG",
                Country = "Test Country"
            }
        };
    }

    [Fact]
    public async Task GetOrderDetails_ReturnsListOfOrderDetailsDto_WhenParamsNotNullAndOrdersExist()
    {
        // Arrange
        var orderService = new OrderService(_orderRepository, _preferencesRepository, _configService, _messageSender, _logger, _httpClientFactory);

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
        var orderService = new OrderService(_orderRepository, _preferencesRepository, _configService, _messageSender, _logger, _httpClientFactory);

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
        var orderService = new OrderService(_orderRepository, _preferencesRepository, _configService, _messageSender, _logger, _httpClientFactory);

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

    [Fact]
    public async Task GetOrderDetails_ReturnsListOfOrderDetailsDto_WhenSortByNotSpecifiedAndOrdersExist()
    {
        // Arrange
        var orderService = new OrderService(_orderRepository, _preferencesRepository, _configService, _messageSender, _logger, _httpClientFactory);

        var orderSpecParams = new OrderSpecificationParams()
        {
            InLastNumberOfDays = 365,
            //SortBy = "OrderCreated",
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
    public async Task GetOrderDetails_ReturnsListOfOrderDetailsDto_WhenOrderSpecIsNullAndOrdersExist()
    {
        // Arrange
        var orderService = new OrderService(_orderRepository, _preferencesRepository, _configService, _messageSender, _logger, _httpClientFactory);

        var orderBasketDto = CreateOrderBasketDto();

        var orderId = await orderService.CreatOrder(orderBasketDto);

        // Act
        var result = await orderService.GetOrderDetails(null! as OrderSpecificationParams);

        // Assert
        orderId.Should().NotBeNull();

        result.Should().NotBeNull();
        result.Count.Should().Be(1);

        // Clean up
        await _orderRepository.DeleteAsync(orderId);
    }

    [Fact]
    public async Task GetOrderDetails_ReturnsNull_WhenNoOrdersExist()
    {
        // Arrange
        var orderService = new OrderService(_orderRepository, _preferencesRepository, _configService, _messageSender, _logger, _httpClientFactory);

        // Act
        var result = await orderService.GetOrderDetails(null! as OrderSpecificationParams);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task CreateProdigiOrder_ShouldReturnTrue_WhenOrderIsCreated()
    {
        // Arrange
        _configuration["Prodigi:ApiKey"] = "00000000-0000-0000-0000-created"; // should return a "Created" response
        _configuration["Prodigi:ApiUri"] = _prodigiWireMockUri;

        var orderService = new OrderService(_orderRepository, _preferencesRepository, _configService, _messageSender, _logger, _httpClientFactory);

        var orderBasketDto = CreateOrderBasketDto();
        var orderId = await orderService.CreatOrder(orderBasketDto);

        var stripeCustomer = CreateStripeCustomer();
        var stripeShippingDetails = CreateStripeShippingDetails();
        var shippingMethod = "Standard";
        var paymentIntentId = "pi_12345";

        await orderService.UpdateOrder(orderId, stripeCustomer, stripeShippingDetails, shippingMethod, paymentIntentId);

        var orderDetailsDto = await orderService.GetOrderDetailsFromId(orderId);

        // Act
        var result = await orderService.CreateProdigiOrder(orderDetailsDto);
        var updatedOrderDetailsDto = await orderService.GetOrderDetailsFromId(orderId);

        // Assert
        result.Should().BeTrue();
        updatedOrderDetailsDto.ProdigiDetails.Should().NotBeNull();
        updatedOrderDetailsDto.ProdigiDetails!.Order!.IdempotencyKey.Should().NotBeNullOrEmpty();
        updatedOrderDetailsDto.ProdigiDetails!.Order!.IdempotencyKey.Should().Be(orderId);
        updatedOrderDetailsDto.ProdigiDetails!.Order!.Status.Issues.Should().BeEmpty();
        updatedOrderDetailsDto.ProdigiDetails!.Order!.Metadata.Should().NotBeNullOrEmpty();
        updatedOrderDetailsDto.ProdigiDetails!.Order!.Metadata.Values.Should().Contain(paymentIntentId);
        updatedOrderDetailsDto.ProdigiDetails!.Order!.CallbackUrl.Should().Be($"{_configuration["ApplicationBaseUri"]}/api/callbacks/{orderId}");

        // Clean up
        await _orderRepository.DeleteAsync(orderId);
    }

    [Fact]
    public async Task CreateProdigiOrder_ShouldReturnTrueWithIssues_WhenOrderIsCreatedWithIssues()
    {
        // Arrange
        _configuration["Prodigi:ApiKey"] = "00000000-0000-0000-0000-createdwithissues"; // should return a "CreatedWithIssues" response
        _configuration["Prodigi:ApiUri"] = _prodigiWireMockUri;

        var orderService = new OrderService(_orderRepository, _preferencesRepository, _configService, _messageSender, _logger, _httpClientFactory);

        var orderBasketDto = CreateOrderBasketDto();
        var orderId = await orderService.CreatOrder(orderBasketDto);

        var stripeCustomer = CreateStripeCustomer();
        var stripeShippingDetails = CreateStripeShippingDetails();
        var shippingMethod = "Standard";
        var paymentIntentId = "pi_12345";

        await orderService.UpdateOrder(orderId, stripeCustomer, stripeShippingDetails, shippingMethod, paymentIntentId);

        var orderDetailsDto = await orderService.GetOrderDetailsFromId(orderId);

        // Act
        var result = await orderService.CreateProdigiOrder(orderDetailsDto);
        var updatedOrderDetailsDto = await orderService.GetOrderDetailsFromId(orderId);

        // Assert
        result.Should().BeTrue();
        updatedOrderDetailsDto.ProdigiDetails.Should().NotBeNull();
        updatedOrderDetailsDto.ProdigiDetails!.Order!.IdempotencyKey.Should().NotBeNullOrEmpty();
        updatedOrderDetailsDto.ProdigiDetails!.Order!.IdempotencyKey.Should().Be(orderId);
        updatedOrderDetailsDto.ProdigiDetails!.Order!.Status.Issues.Should().NotBeNullOrEmpty();
        updatedOrderDetailsDto.ProdigiDetails!.Order!.Metadata.Should().NotBeNullOrEmpty();
        updatedOrderDetailsDto.ProdigiDetails!.Order!.Metadata.Values.Should().Contain(paymentIntentId);

        // Clean up
        await _orderRepository.DeleteAsync(orderId);
    }

    [Fact]
    public async Task CreateProdigiOrder_ShouldReturnTrue_WhenOrderAlreadyExists()
    {
        // Arrange
        _configuration["Prodigi:ApiKey"] = "00000000-0000-0000-0000-created"; // should return a "Created" response
        _configuration["Prodigi:ApiUri"] = _prodigiWireMockUri;

        var orderServiceCreated = new OrderService(_orderRepository, _preferencesRepository, _configService, _messageSender, _logger, _httpClientFactory);

        var orderBasketDto = CreateOrderBasketDto();
        var orderId = await orderServiceCreated.CreatOrder(orderBasketDto);

        var stripeCustomer = CreateStripeCustomer();
        var stripeShippingDetails = CreateStripeShippingDetails();

        await orderServiceCreated.UpdateOrder(orderId, stripeCustomer, stripeShippingDetails, "Standard", "pi_12345");

        var orderDetailsDto = await orderServiceCreated.GetOrderDetailsFromId(orderId);

        var createdResult = await orderServiceCreated.CreateProdigiOrder(orderDetailsDto);
        var createdOrderDetailsDto = await orderServiceCreated.GetOrderDetailsFromId(orderId);

        _configuration["Prodigi:ApiKey"] = "00000000-0000-0000-0000-alreadyexists"; // should return an "AlreadyExists" response

        var orderServiceAlreadyExists = new OrderService(_orderRepository, _preferencesRepository, _configService, _messageSender, _logger, _httpClientFactory);

        // Act
        var existsResult = await orderServiceAlreadyExists.CreateProdigiOrder(orderDetailsDto);
        var updatedOrderDetailsDto = await orderServiceAlreadyExists.GetOrderDetailsFromId(orderId);

        // Assert
        createdResult.Should().BeTrue();
        existsResult.Should().BeTrue();
        createdOrderDetailsDto.ProdigiDetails.Should().NotBeNull();
        updatedOrderDetailsDto.ProdigiDetails.Should().NotBeNull();
        updatedOrderDetailsDto.ProdigiDetails!.Order!.IdempotencyKey.Should().Be(orderId); // idempotency key/order id should not have changed
        updatedOrderDetailsDto.ProdigiDetails!.Order!.Status.Issues.Should().BeEmpty(); // issues should still be empty
        updatedOrderDetailsDto.ProdigiDetails!.Order!.Status.Stage.Should().BeEquivalentTo(createdOrderDetailsDto.ProdigiDetails!.Order!.Status.Stage); // order status stage should not have changed

        // Clean up
        await _orderRepository.DeleteAsync(orderId);
    }

    [Fact]
    public async Task CreateProdigiOrder_ShouldReturnFalse_WhenResponseIsNot200OK()
    {
        // Arrange
        _configuration["Prodigi:ApiKey"] = "00000000-0000-0000-0000-badrequest"; // should return a 400 Bad Request response
        _configuration["Prodigi:ApiUri"] = _prodigiWireMockUri;

        var orderService = new OrderService(_orderRepository, _preferencesRepository, _configService, _messageSender, _logger, _httpClientFactory);

        var orderBasketDto = CreateOrderBasketDto();
        var orderId = await orderService.CreatOrder(orderBasketDto);

        var stripeCustomer = CreateStripeCustomer();
        var stripeShippingDetails = CreateStripeShippingDetails();

        await orderService.UpdateOrder(orderId, stripeCustomer, stripeShippingDetails, "Standard", "pi_12345");

        var orderDetailsDto = await orderService.GetOrderDetailsFromId(orderId);

        // Act
        var result = await orderService.CreateProdigiOrder(orderDetailsDto);

        // Assert
        result.Should().BeFalse();

        // Clean up
        await _orderRepository.DeleteAsync(orderId);
    }

    [Fact]
    public async Task CreateProdigiOrder_ShouldReturnFalse_WhenAnExceptionIsThrown()
    {
        // Arrange
        _configuration["Prodigi:ApiKey"] = "00000000-0000-0000-0000-unexpectedorderstructure"; // should return a 200 Ok response with an unexpected response structure
        _configuration["Prodigi:ApiUri"] = _prodigiWireMockUri;

        var orderService = new OrderService(_orderRepository, _preferencesRepository, _configService, _messageSender, _logger, _httpClientFactory);

        var orderBasketDto = CreateOrderBasketDto();
        var orderId = await orderService.CreatOrder(orderBasketDto);

        var stripeCustomer = CreateStripeCustomer();
        var stripeShippingDetails = CreateStripeShippingDetails();

        await orderService.UpdateOrder(orderId, stripeCustomer, stripeShippingDetails, "Standard", "pi_12345");

        var orderDetailsDto = await orderService.GetOrderDetailsFromId(orderId);

        // Act
        var result = await orderService.CreateProdigiOrder(orderDetailsDto);

        // Assert
        result.Should().BeFalse();

        // Clean up
        await _orderRepository.DeleteAsync(orderId);
    }

    [Fact]
    public async Task CreateProdigiOrder_ShouldReturnFalse_WhenResponseOutcomeIsUnexpected()
    {
        // Arrange
        _configuration["Prodigi:ApiKey"] = "00000000-0000-0000-0000-unexpectedoutcome"; // should return a 200 Ok response, but with an unexpected outcome
        _configuration["Prodigi:ApiUri"] = _prodigiWireMockUri;

        var orderService = new OrderService(_orderRepository, _preferencesRepository, _configService, _messageSender, _logger, _httpClientFactory);

        var orderBasketDto = CreateOrderBasketDto();
        var orderId = await orderService.CreatOrder(orderBasketDto);

        var stripeCustomer = CreateStripeCustomer();
        var stripeShippingDetails = CreateStripeShippingDetails();

        await orderService.UpdateOrder(orderId, stripeCustomer, stripeShippingDetails, "Standard", "pi_12345");

        var orderDetailsDto = await orderService.GetOrderDetailsFromId(orderId);

        // Act
        var result = await orderService.CreateProdigiOrder(orderDetailsDto);

        // Assert
        result.Should().BeFalse();

        // Clean up
        await _orderRepository.DeleteAsync(orderId);
    }
}
