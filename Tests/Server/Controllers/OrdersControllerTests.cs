using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using PhotoPortfolio.Server.Contracts;
using PhotoPortfolio.Server.Controllers;
using PhotoPortfolio.Shared.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PhotoPortfolio.Tests.Server.Controllers;

public class OrdersControllerTests : BaseApiController
{
    private readonly OrdersController _sut;
    private readonly IOrderService _service = Substitute.For<IOrderService>();
    private readonly ILogger<OrdersController> _logger = Substitute.For<ILogger<OrdersController>>();

    public OrdersControllerTests()
    {
        _sut = new OrdersController(_logger, _service);
    }

    [Fact]
    public async Task CreateOrder_ShouldCreateOrder_WhenCreateOrderRequestIsValid()
    {
        // Arrange
        var order = new OrderBasketDto
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
            ShippingMethod = "Standard"
        };

        var orderId = Guid.NewGuid().ToString();
        _service.CreatOrder(order).Returns(orderId);

        // Act
        var result = (OkObjectResult)await _sut.CreateOrder(order);

        // Assert
        result.StatusCode.Should().Be(200);
        result.Value.Should().BeOfType<string>();
        result.Value.Should().Be(orderId);
    }

    [Fact]
    public async Task CreateOrder_ShouldReturnBadRequest_WhenCreateOrderRequestIsInvalid()
    {
        // Arrange
        _service.CreatOrder(Arg.Any<OrderBasketDto>()).ReturnsNull();

        // Act
        var result = (BadRequestResult)await _sut.CreateOrder(Arg.Any<OrderBasketDto>());

        // Assert
        result.StatusCode.Should().Be(400);
    }

    [Fact]
    public async Task GetOrderDetailsFromId_ShouldReturnOrderDetailsDto_WhenOrderIdExists()
    {
        // Arrange
        var id = Guid.NewGuid().ToString();
        _service.GetOrderDetailsFromId(id).Returns(new OrderDetailsDto());

        // Act
        var result = (OkObjectResult)await _sut.GetOrderDetailsFromId(id);

        // Assert
        result.StatusCode.Should().Be(200);
        result.Value.Should().BeOfType<OrderDetailsDto>();
    }

    [Fact]
    public async Task GetOrderDetailsFromId_ShouldReturnNotFound_WhenOrderIdDoesNotExist()
    {
        // Arrange
        var id = Guid.NewGuid().ToString();
        _service.GetOrderDetailsFromId(id).ReturnsNull();

        // Act
        var result = (NotFoundResult)await _sut.GetOrderDetailsFromId(id);

        // Assert
        result.StatusCode.Should().Be(404);
    }

    [Fact]
    public async Task ShouldApproveOrderFromId_ReturnsOkTrue_WhenOrderShouldBeApproved()
    {
        // Arrange
        var id = Guid.NewGuid().ToString();
        _service.ShouldApproveOrder(id).Returns(true);

        // Act
        var result = (OkObjectResult)await _sut.ShouldApproveOrderFromId(id);

        // Assert
        result.StatusCode.Should().Be(200);
        result.Value.Should().BeOfType<bool>();
        result.Value.Should().Be(true);
    }

    [Fact]
    public async Task ShouldApproveOrderFromId_ReturnsBadRequest_WhenOrderShouldNotBeApproved()
    {
        // Arrange
        var id = Guid.NewGuid().ToString();
        _service.ShouldApproveOrder(id).Returns(false);

        // Act
        var result = (BadRequestResult)await _sut.ShouldApproveOrderFromId(id);

        // Assert
        result.StatusCode.Should().Be(400);
    }
}
