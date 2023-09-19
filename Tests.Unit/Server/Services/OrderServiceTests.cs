using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using PhotoPortfolio.Server.Contracts;
using PhotoPortfolio.Server.Messaging;
using PhotoPortfolio.Server.Services;
using PhotoPortfolio.Shared.Entities;
using PhotoPortfolio.Shared.Helpers;
using PhotoPortfolio.Shared.Models;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using PhotoPortfolioStripe = PhotoPortfolio.Shared.Models.Stripe;
using Prodigi = PhotoPortfolio.Shared.Models.Prodigi;

namespace PhotoPortfolio.Tests.Unit.Server.Services;

public class OrderServiceTests
{
    private readonly OrderService _sut;
    private readonly IOrderRepository _orderRepository = Substitute.For<IOrderRepository>();
    private readonly IPreferencesRepository _preferencesRepository = Substitute.For<IPreferencesRepository>();
    private readonly IConfigurationService _configService = Substitute.For<IConfigurationService>();
    private readonly IMessageSender _messageSender = Substitute.For<IMessageSender>();
    private readonly ILogger<OrderService> _logger = Substitute.For<ILogger<OrderService>>();

    public OrderServiceTests()
    { 
        _sut = new OrderService(_orderRepository, _preferencesRepository, _configService, _messageSender, _logger);
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
                        Id = Guid.NewGuid().ToString(),
                        PhotoId = Guid.NewGuid().ToString(),
                        ImageUri = "https://localhost/images/1.jpg",
                        ImageTitle = "Test",
                    },
                    Total = 100m,
                }
            },
            ShippingMethod = "Standard",
            ShippingCost = 5m
        };
    }

    private static Order CreateInProgressOrder(string orderId, OrderBasketDto orderBasketDto)
    {
        return new Order()
        {
            Id = orderId,
            Name = "Test",
            EmailAddress = "test@test.com",
            Items = orderBasketDto.BasketItems,
            ShippingMethod = string.IsNullOrWhiteSpace(orderBasketDto.ShippingMethod) ? "" : orderBasketDto.ShippingMethod,
            OrderCreated = BsonDateTime.Create(DateTime.UtcNow),
            PaymentCompleted = BsonDateTime.Create(DateTime.UtcNow),
            ItemsCost = (BsonDecimal128)100m,
            ShippingCost = (BsonDecimal128)orderBasketDto.ShippingCost,
            TotalCost = (BsonDecimal128)105m,
            Address = new Prodigi.Orders.Address
            {
                Line1 = "Test",
                Line2 = "Test",
                PostalOrZipCode = "Test",
                CountryCode = "Test",
                TownOrCity = "Test",
                StateOrCounty = "Test",
            },
            Status = OrderStatus.InProgress
        };
    }

    private static Order CreateIncompleteOrder(string orderId, OrderBasketDto orderBasketDto)
    {
        return new Order()
        {
            Id = orderId,
            Name = "Test",
            EmailAddress = "test@test.com",
            Items = orderBasketDto.BasketItems,
            ShippingMethod = string.IsNullOrWhiteSpace(orderBasketDto.ShippingMethod) ? "" : orderBasketDto.ShippingMethod,
            OrderCreated = BsonDateTime.Create(DateTime.UtcNow),
            PaymentCompleted = null!,
            ItemsCost = (BsonDecimal128)100m,
            ShippingCost = (BsonDecimal128)orderBasketDto.ShippingCost,
            TotalCost = (BsonDecimal128)105m,
            Address = new Prodigi.Orders.Address
            {
                Line1 = "Test",
                Line2 = "Test",
                PostalOrZipCode = "Test",
                CountryCode = "Test",
                TownOrCity = "Test",
                StateOrCounty = "Test",
            },
            Status = OrderStatus.PaymentIncomplete
        };
    }

    [Fact]
    public async Task CreatOrder_ShouldReturnOrderIdString_WhenOrderIsCreated()
    {
        // Arrange
        var orderBasketDto = CreateOrderBasketDto();

        var orderId = Guid.NewGuid().ToString();
        var newOrder = CreateInProgressOrder(orderId, orderBasketDto);

        _orderRepository.AddAsync(Arg.Any<Order>()).Returns(newOrder);

        // Act
        var result = await _sut.CreatOrder(orderBasketDto);

        // Assert
        result.Should().NotBeNullOrEmpty();
        result.Should().BeOfType<string>();
        result.Should().Be(orderId);
    }

    [Fact]
    public async Task CreateOrder_ShouldReturnEmptyString_WhenOrderCreationFailed()
    {
        // Arrange
        var orderBasketDto = CreateOrderBasketDto();

        _orderRepository.AddAsync(Arg.Any<Order>()).ReturnsNull();

        // Act
        var result = await _sut.CreatOrder(orderBasketDto);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task CreateOrder_ShouldReturnEmptyString_WhenExceptionIsThrown()
    {
        // Arrange
        var orderBasketDto = CreateOrderBasketDto();

        var exception = new Exception("Order creation failed");
        _orderRepository.AddAsync(Arg.Any<Order>()).Throws(exception);

        // Act
        var result = await _sut.CreatOrder(orderBasketDto);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task UpdateOrder_ShouldReturnTrue_WhenOrderIsUpdated()
    {
        // Arrange
        var orderBasketDto = CreateOrderBasketDto();
        var orderId = Guid.NewGuid().ToString();
        var order = CreateInProgressOrder(orderId, orderBasketDto);

        var customer = new PhotoPortfolioStripe.Customer()
        {
            Name = "Test",
            EmailAddress = "test@test.com"
        };

        var stripeAddress = new PhotoPortfolioStripe.Address()
        {
            City = "Test",
            Country = "Test",
            Line1 = "Test",
            Line2 = "Test",
            PostalCode = "Test",
            State = "Test"
        };

        var shippingDetails = new PhotoPortfolioStripe.ShippingDetails()
        {
            Address = stripeAddress,
            Name = "Test",
            Phone = "Test",
            TrackingNumber = "Test"
        };

        _orderRepository.GetSingleAsync(Arg.Any<Expression<Func<Order, bool>>>()).Returns(order);
        _orderRepository.UpdateAsync(Arg.Any<Order>()).Returns(order);

        // Act
        var result = await _sut.UpdateOrder(orderId, customer, shippingDetails, orderBasketDto.ShippingMethod, "pi_test");

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task UpdateOrder_ShouldReturnFalse_WhenExceptionIsThrown()
    {
        // Arrange
        var orderBasketDto = CreateOrderBasketDto();
        var orderId = Guid.NewGuid().ToString();
        var order = CreateInProgressOrder(orderId, orderBasketDto);

        var exception = new Exception("Order update failed");
        _orderRepository.GetSingleAsync(Arg.Any<Expression<Func<Order, bool>>>()).Returns(order);
        _orderRepository.UpdateAsync(Arg.Any<Order>()).Throws(exception);

        var customer = new PhotoPortfolioStripe.Customer()
        {
            Name = "Test",
            EmailAddress = "test@test.com"
        };

        var stripeAddress = new PhotoPortfolioStripe.Address()
        {
            City = "Test",
            Country = "Test",
            Line1 = "Test",
            Line2 = "Test",
            PostalCode = "Test",
            State = "Test"
        };

        var shippingDetails = new PhotoPortfolioStripe.ShippingDetails()
        {
            Address = stripeAddress,
            Name = "Test",
            Phone = "Test",
            TrackingNumber = "Test"
        };

        // Act
        var result = await _sut.UpdateOrder(orderId, customer, shippingDetails, orderBasketDto.ShippingMethod, "pi_test");

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task UpdateOrder_ShouldReturnFalse_WhenOrderNotFound()
    {
        // Arrange
        var orderBasketDto = CreateOrderBasketDto();
        var orderId = Guid.NewGuid().ToString();

        _orderRepository.GetSingleAsync(Arg.Any<Expression<Func<Order, bool>>>()).ReturnsNull();

        var customer = new PhotoPortfolioStripe.Customer()
        {
            Name = "Test",
            EmailAddress = "test@test.com"
        };

        var stripeAddress = new PhotoPortfolioStripe.Address()
        {
            City = "Test",
            Country = "Test",
            Line1 = "Test",
            Line2 = "Test",
            PostalCode = "Test",
            State = "Test"
        };

        var shippingDetails = new PhotoPortfolioStripe.ShippingDetails()
        {
            Address = stripeAddress,
            Name = "Test",
            Phone = "Test",
            TrackingNumber = "Test"
        };

        // Act
        var result = await _sut.UpdateOrder(orderId, customer, shippingDetails, orderBasketDto.ShippingMethod, "pi_test");

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task UpdateOrderCosts_ShouldReturnTrue_WhenCostsUpdatedSuccessfully()
    {
        // Arrange
        var orderBasketDto = CreateOrderBasketDto();
        var orderId = Guid.NewGuid().ToString();
        var order = CreateInProgressOrder(orderId, orderBasketDto);

        _orderRepository.GetSingleAsync(Arg.Any<Expression<Func<Order, bool>>>()).Returns(order);
        _orderRepository.UpdateAsync(Arg.Any<Order>()).Returns(order);

        // Act
        var result = await _sut.UpdateOrderCosts(orderBasketDto);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task UpdateOrderCosts_ShouldReturnFalse_WhenCostsWereNotUpdated()
    {
        // Arrange
        var orderBasketDto = CreateOrderBasketDto();
        var orderId = Guid.NewGuid().ToString();
        var order = CreateInProgressOrder(orderId, orderBasketDto);

        _orderRepository.GetSingleAsync(Arg.Any<Expression<Func<Order, bool>>>()).Returns(order);
        _orderRepository.UpdateAsync(Arg.Any<Order>()).ReturnsNull();

        // Act
        var result = await _sut.UpdateOrderCosts(orderBasketDto);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task UpdateOrderCosts_ShouldReturnFalse_WhenOrderNotFound()
    {
        // Arrange
        var orderBasketDto = CreateOrderBasketDto();

        _orderRepository.GetSingleAsync(Arg.Any<Expression<Func<Order, bool>>>()).ReturnsNull();

        // Act
        var result = await _sut.UpdateOrderCosts(orderBasketDto);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task UpdateOrderCosts_ShouldReturnFalse_WhenExceptionIsThrown()
    {
        // Arrange
        var orderBasketDto = CreateOrderBasketDto();
        var orderId = Guid.NewGuid().ToString();
        var order = CreateInProgressOrder(orderId, orderBasketDto);

        var exception = new Exception("Order update failed");
        _orderRepository.GetSingleAsync(Arg.Any<Expression<Func<Order, bool>>>()).Returns(order);
        _orderRepository.UpdateAsync(Arg.Any<Order>()).Throws(exception);

        // Act
        var result = await _sut.UpdateOrderCosts(orderBasketDto);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task GetOrderDetailsFromId_ShouldReturnOrderDetailsDto_WhenOrderExists()
    {
        // Arrange
        var orderBasketDto = CreateOrderBasketDto();
        var orderId = Guid.NewGuid().ToString();
        var order = CreateInProgressOrder(orderId, orderBasketDto);

        _orderRepository.GetSingleAsync(Arg.Any<Expression<Func<Order, bool>>>()).Returns(order);

        // Act
        var result = await _sut.GetOrderDetailsFromId(orderId);

        // Assert
        result.Should().BeOfType<OrderDetailsDto>();
        result.Id.Should().Be(orderId);
    }

    [Fact]
    public async Task GetOrderDetailsFromId_ShouldReturnNull_WhenOrderDoesNotExist()
    {
        // Arrange
        _orderRepository.GetSingleAsync(Arg.Any<Expression<Func<Order, bool>>>()).ReturnsNull();

        // Act
        var result = await _sut.GetOrderDetailsFromId(Guid.NewGuid().ToString());

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetOrderDetailsFromId_ShouldReturnNull_WhenExceptionIsThrown()
    {
        // Arrange
        var exception = new Exception("Getting order details failed");
        _orderRepository.GetSingleAsync(Arg.Any<Expression<Func<Order, bool>>>()).Throws(exception);

        // Act
        var result = await _sut.GetOrderDetailsFromId(Guid.NewGuid().ToString());

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetOrderDetails_ShouldReturnListOfOrderDetailsDto_WhenParamsAreNotNullAndOrdersExist()
    {
        // Arrange
        var orderSpecParams = new OrderSpecificationParams()
        {
            Status = "Status",
            CustomerEmail = "CustomerEmail",
            InLastNumberOfDays = 365,
            SortBy = "SortBy",
            SortOrder = "SortOrder",
            ExcludePaymentIncomplete = false
        };

        var orderBasketDto = CreateOrderBasketDto();
        var orderId = Guid.NewGuid().ToString();
        var newOrder = CreateInProgressOrder(orderId, orderBasketDto);

        var orders = new List<Order>
        {
            newOrder
        };

        _orderRepository.GetFilteredOrdersAsync(Arg.Any<OrderSpecificationParams>()).Returns(orders);

        // Act
        var result = await _sut.GetOrderDetails(orderSpecParams);

        // Assert
        result.Should().BeOfType<List<OrderDetailsDto>>();
        result.Count.Should().Be(1);
    }

    [Fact]
    public async Task GetOrderDetails_ShouldReturnListOfOrderDetailsDto_WhenParamsAreNullAndOrdersExist()
    {
        // Arrange
        var orderBasketDto = CreateOrderBasketDto();
        var orderId = Guid.NewGuid().ToString();
        var newOrder = CreateInProgressOrder(orderId, orderBasketDto);

        var orders = new List<Order>
        {
            newOrder
        };

        _orderRepository.GetAllAsync().Returns(orders);

        // Act
        var result = await _sut.GetOrderDetails(null!);

        // Assert
        result.Should().BeOfType<List<OrderDetailsDto>>();
        result.Count.Should().Be(1);
    }

    [Fact]
    public async Task GetOrderDetails_ShouldReturnNull_WhenNoOrdersAreFound()
    {
        // Arrange
        var orderSpecParams = new OrderSpecificationParams()
        {
            Status = "Status",
            CustomerEmail = "CustomerEmail",
            InLastNumberOfDays = 365,
            SortBy = "SortBy",
            SortOrder = "SortOrder",
            ExcludePaymentIncomplete = false
        };

        _orderRepository.GetAllAsync().ReturnsNull();
        _orderRepository.GetFilteredOrdersAsync(Arg.Any<OrderSpecificationParams>()).ReturnsNull();

        // Act
        var result = await _sut.GetOrderDetails(orderSpecParams);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetOrderDetails_ShouldReturnEmptyList_WhenNoOrdersExist()
    {
        // Arrange
        var orderSpecParams = new OrderSpecificationParams()
        {
            Status = "Status",
            CustomerEmail = "CustomerEmail",
            InLastNumberOfDays = 365,
            SortBy = "SortBy",
            SortOrder = "SortOrder",
            ExcludePaymentIncomplete = false
        };

        _orderRepository.GetFilteredOrdersAsync(Arg.Any<OrderSpecificationParams>()).Returns(new List<Order>());

        // Act
        var result = await _sut.GetOrderDetails(orderSpecParams);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetOrderDetails_ShouldReturnListOfOrderDetailsDto_WhenOrderPaymentIsIncomplete()
    {
        // Arrange
        var orderSpecParams = new OrderSpecificationParams()
        {
            Status = "Status",
            CustomerEmail = "CustomerEmail",
            InLastNumberOfDays = 365,
            SortBy = "SortBy",
            SortOrder = "SortOrder",
            ExcludePaymentIncomplete = false
        };

        var orderBasketDto = CreateOrderBasketDto();
        var orderId = Guid.NewGuid().ToString();
        var newOrder = CreateIncompleteOrder(orderId, orderBasketDto);

        var orders = new List<Order>
        {
            newOrder
        };

        _orderRepository.GetFilteredOrdersAsync(Arg.Any<OrderSpecificationParams>()).Returns(orders);

        // Act
        var result = await _sut.GetOrderDetails(orderSpecParams);

        // Assert
        result.Should().BeOfType<List<OrderDetailsDto>>();
        result.Count.Should().Be(1);
    }

    [Fact]
    public async Task ShouldApproveOrder_ShouldReturnTrue_WhenOrderTotalIsBelowApprovalLimitAndAutoApproveIsApproveBelow()
    {
        // Arrange
        var sitePrefsId = Guid.NewGuid().ToString();

        var inMemoryConfig = new Dictionary<string, string?> {
            { "SitePreferencesId", sitePrefsId }
        };

        IConfiguration configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemoryConfig)
            .Build();

        _configService.GetConfiguration().Returns(configuration);

        var preferences = new Preferences()
        {
            SiteName = "SiteName",
            Metadata = new Dictionary<string, string>()
            {
                { "OrdersSentToProdigiAutomatically", "AutoApproveBelow" },
                { "OrderAutoApproveLimit", "999" }
            }
        };

        _preferencesRepository.GetSingleAsync(Arg.Any<Expression<Func<Preferences, bool>>>()).Returns(preferences);

        var orderBasketDto = CreateOrderBasketDto();
        var orderId = Guid.NewGuid().ToString();
        var newOrder = CreateInProgressOrder(orderId, orderBasketDto);
        _orderRepository.GetSingleAsync(Arg.Any<Expression<Func<Order, bool>>>()).Returns(newOrder);

        // Act
        var result = await _sut.ShouldApproveOrder(orderId);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task ShouldApproveOrder_ShouldReturnFalse_WhenOrderTotalIsAboveApprovalLimitAndAutoApproveIsApproveBelow()
    {
        // Arrange
        var sitePrefsId = Guid.NewGuid().ToString();

        var inMemoryConfig = new Dictionary<string, string?> {
            { "SitePreferencesId", sitePrefsId }
        };

        IConfiguration configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemoryConfig)
            .Build();

        _configService.GetConfiguration().Returns(configuration);

        var preferences = new Preferences()
        {
            SiteName = "SiteName",
            Metadata = new Dictionary<string, string>()
            {
                { "OrdersSentToProdigiAutomatically", "AutoApproveBelow" },
                { "OrderAutoApproveLimit", "1" }
            }
        };

        _preferencesRepository.GetSingleAsync(Arg.Any<Expression<Func<Preferences, bool>>>()).Returns(preferences);

        var orderBasketDto = CreateOrderBasketDto();
        var orderId = Guid.NewGuid().ToString();
        var newOrder = CreateInProgressOrder(orderId, orderBasketDto);
        _orderRepository.GetSingleAsync(Arg.Any<Expression<Func<Order, bool>>>()).Returns(newOrder);

        // Act
        var result = await _sut.ShouldApproveOrder(orderId);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task ShouldApproveOrder_ShouldReturnFalse_WhenManuallyApproveAllIsSetInPrefs()
    {
        // Arrange
        var sitePrefsId = Guid.NewGuid().ToString();

        var inMemoryConfig = new Dictionary<string, string?> {
            { "SitePreferencesId", sitePrefsId }
        };

        IConfiguration configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemoryConfig)
            .Build();

        _configService.GetConfiguration().Returns(configuration);

        var preferences = new Preferences()
        {
            SiteName = "SiteName",
            Metadata = new Dictionary<string, string>()
            {
                { "OrdersSentToProdigiAutomatically", "ManuallyApproveAll" },
                { "OrderAutoApproveLimit", "999" }
            }
        };

        _preferencesRepository.GetSingleAsync(Arg.Any<Expression<Func<Preferences, bool>>>()).Returns(preferences);

        var orderBasketDto = CreateOrderBasketDto();
        var orderId = Guid.NewGuid().ToString();
        var newOrder = CreateInProgressOrder(orderId, orderBasketDto);
        _orderRepository.GetSingleAsync(Arg.Any<Expression<Func<Order, bool>>>()).Returns(newOrder);

        // Act
        var result = await _sut.ShouldApproveOrder(orderId);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task ShouldApproveOrder_ShouldReturnTrue_WhenAutoApproveAllIsSetInPrefs()
    {
        // Arrange
        var sitePrefsId = Guid.NewGuid().ToString();

        var inMemoryConfig = new Dictionary<string, string?> {
            { "SitePreferencesId", sitePrefsId }
        };

        IConfiguration configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemoryConfig)
            .Build();

        _configService.GetConfiguration().Returns(configuration);

        var preferences = new Preferences()
        {
            SiteName = "SiteName",
            Metadata = new Dictionary<string, string>()
            {
                { "OrdersSentToProdigiAutomatically", "AutoApproveAll" },
                { "OrderAutoApproveLimit", "999" }
            }
        };

        _preferencesRepository.GetSingleAsync(Arg.Any<Expression<Func<Preferences, bool>>>()).Returns(preferences);

        var orderBasketDto = CreateOrderBasketDto();
        var orderId = Guid.NewGuid().ToString();
        var newOrder = CreateInProgressOrder(orderId, orderBasketDto);
        _orderRepository.GetSingleAsync(Arg.Any<Expression<Func<Order, bool>>>()).Returns(newOrder);

        // Act
        var result = await _sut.ShouldApproveOrder(orderId);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task ShouldApproveOrder_ShouldReturnFalse_WhenOrderIsNotFound()
    {
        // Arrange
        var sitePrefsId = Guid.NewGuid().ToString();

        var inMemoryConfig = new Dictionary<string, string?> {
            { "SitePreferencesId", sitePrefsId }
        };

        IConfiguration configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemoryConfig)
            .Build();

        _configService.GetConfiguration().Returns(configuration);

        _preferencesRepository.GetSingleAsync(Arg.Any<Expression<Func<Preferences, bool>>>()).ReturnsNull();

        var orderBasketDto = CreateOrderBasketDto();
        var orderId = Guid.NewGuid().ToString();
        var newOrder = CreateInProgressOrder(orderId, orderBasketDto);
        _orderRepository.GetSingleAsync(Arg.Any<Expression<Func<Order, bool>>>()).Returns(newOrder);

        // Act
        var result = await _sut.ShouldApproveOrder(orderId);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task ShouldApproveOrder_ShouldReturnFalse_WhenSitePrefsIdIsEmptyOrNull()
    {
        // Arrange
        var inMemoryConfig = new Dictionary<string, string?> {
            { "SitePreferencesId", "" }
        };

        IConfiguration configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemoryConfig)
            .Build();

        _configService.GetConfiguration().Returns(configuration);

        // Act
        var result = await _sut.ShouldApproveOrder(Guid.NewGuid().ToString());

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task ApproveOrder_ShouldReturnTrue_WhenOrderIsApproved()
    {
        // Arrange
        var orderBasketDto = CreateOrderBasketDto();
        var orderId = Guid.NewGuid().ToString();
        var newOrder = CreateInProgressOrder(orderId, orderBasketDto);

        _orderRepository.GetSingleAsync(Arg.Any<Expression<Func<Order, bool>>>()).Returns(newOrder);
        _orderRepository.UpdateAsync(Arg.Any<Order>()).Returns(newOrder);
        _messageSender.SendOrderApprovedMessageAsync(Arg.Any<OrderDetailsDto>()).Returns(true);

        // Act
        var result = await _sut.ApproveOrder(orderId);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task ApproveOrder_ShouldReturnFalse_WhenOrderIsNotFound()
    {
        // Arrange
        _orderRepository.GetSingleAsync(Arg.Any<Expression<Func<Order, bool>>>()).ReturnsNull();

        // Act
        var result = await _sut.ApproveOrder(Guid.NewGuid().ToString());

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task ApproveOrder_ShouldReturnFalse_WhenOrderWasNotUpdated()
    {
        // Arrange
        var orderBasketDto = CreateOrderBasketDto();
        var orderId = Guid.NewGuid().ToString();
        var newOrder = CreateInProgressOrder(orderId, orderBasketDto);

        _orderRepository.GetSingleAsync(Arg.Any<Expression<Func<Order, bool>>>()).Returns(newOrder);
        _orderRepository.UpdateAsync(Arg.Any<Order>()).ReturnsNull();

        // Act
        var result = await _sut.ApproveOrder(orderId);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task ApproveOrder_ShouldReturnFalse_WhenMessageToServiceBusWasNotSent()
    {
        // Arrange
        var orderBasketDto = CreateOrderBasketDto();
        var orderId = Guid.NewGuid().ToString();
        var newOrder = CreateInProgressOrder(orderId, orderBasketDto);

        _orderRepository.GetSingleAsync(Arg.Any<Expression<Func<Order, bool>>>()).Returns(newOrder);
        _orderRepository.UpdateAsync(Arg.Any<Order>()).Returns(newOrder);
        _messageSender.SendOrderApprovedMessageAsync(Arg.Any<OrderDetailsDto>()).Returns(false);

        // Act
        var result = await _sut.ApproveOrder(orderId);

        // Assert
        result.Should().BeFalse();
    }
}
