using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using PhotoPortfolio.Server.Contracts;
using PhotoPortfolio.Server.Controllers;
using PhotoPortfolio.Shared.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PhotoPortfolio.Tests.Unit.Server.Controllers;

public class PaymentsControllerTests : BaseApiController
{
    private readonly PaymentsController _sut;
    private readonly ILogger<PaymentsController> _logger = Substitute.For<ILogger<PaymentsController>>();
    private readonly IConfigurationService _configService = Substitute.For<IConfigurationService>();
    private readonly IPaymentService _paymentService = Substitute.For<IPaymentService>();
    private readonly IOrderService _orderService = Substitute.For<IOrderService>();
    private readonly IQuoteService _quoteService = Substitute.For<IQuoteService>();

    public PaymentsControllerTests()
    {
        _sut = new PaymentsController(_logger, _configService, _paymentService, _orderService, _quoteService);
    }

    [Fact]
    public async Task CreateCheckoutSession_ShouldReturnOk_WhenCheckoutSessionIsCreated()
    {
        // Arrange
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
            ShippingMethod = "Standard"
        };

        _quoteService.GetBasketQuote(orderBasket, false).Returns(orderBasket);
        _orderService.UpdateOrderCosts(orderBasket).Returns(true);
        _paymentService.CreateCheckoutSession(orderBasket).Returns(new Stripe.Checkout.Session());

        // Act
        var result = (OkObjectResult)await _sut.CreateCheckoutSession(orderBasket);

        // Assert
        result.StatusCode.Should().Be(200);
    }

    [Fact]
    public async Task CreateCheckoutSession_ShouldReturnBadRequest_WhenBasketValuesAreInconsitentWithQuote()
    {
        // Arrange
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
            ShippingMethod = "Standard"
        };

        _quoteService.GetBasketQuote(orderBasket, false).Returns(orderBasket);
        _orderService.UpdateOrderCosts(orderBasket).Returns(false);
        _paymentService.CreateCheckoutSession(orderBasket).ReturnsNull();

        // Act
        var result = (BadRequestResult)await _sut.CreateCheckoutSession(orderBasket);

        // Assert
        result.StatusCode.Should().Be(400);
    }

    [Fact]
    public async Task GetCheckoutSessionFromId_ShouldReturnCheckoutSessionResponse_WhenCheckoutSessionIsFound()
    {
        // Arrange
        var id = Guid.NewGuid().ToString();
        var CheckoutSessionResponse = new CheckoutSessionResponse()
        {
            OrderId = id,
            CustomerName = "Test"
        };

        _paymentService.GetOrderFromCheckoutSessionId(Arg.Any<string>()).Returns(CheckoutSessionResponse);

        // Act
        var result = (OkObjectResult)await _sut.GetCheckoutSessionFromId(id);

        // Assert
        result.StatusCode.Should().Be(200);
        result.Value.Should().BeEquivalentTo(CheckoutSessionResponse);
    }

    [Fact]
    public async Task GetCheckoutSessionFromId_ShouldReturnBadRequest_WhenCheckoutSessionIsNotFound()
    {
        // Arrange
        _paymentService.GetOrderFromCheckoutSessionId(Arg.Any<string>()).ReturnsNull();

        // Act
        var result = (BadRequestResult)await _sut.GetCheckoutSessionFromId(Guid.NewGuid().ToString());

        // Assert
        result.StatusCode.Should().Be(400);
    }
}
