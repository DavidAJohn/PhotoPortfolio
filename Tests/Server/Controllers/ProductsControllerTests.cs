using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using NSubstitute.ReturnsExtensions;
using PhotoPortfolio.Server.Contracts;
using PhotoPortfolio.Server.Controllers;
using PhotoPortfolio.Shared.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PhotoPortfolio.Tests.Server.Controllers;

public class ProductsControllerTests : BaseApiController
{
    private readonly ProductsController _sut;
    private readonly IProductRepository _repository = Substitute.For<IProductRepository>();

    public ProductsControllerTests()
    {
        _sut = new ProductsController(_repository);
    }

    [Fact]
    public async Task GetProducts_ShouldReturnProducts_WhenProductsExist()
    {
        // Arrange
        _repository.GetAllAsync()
                   .Returns(new List<Product>());

        // Act
        var result = (OkObjectResult)await _sut.GetProducts();

        // Assert
        result.StatusCode.Should().Be(200);
        result.Value.Should().BeOfType<List<Product>>();
    }

    [Fact]
    public async Task GetProducts_ShouldReturnNotFound_WhenProductsDoNotExist()
    {
        // Arrange
        _repository.GetAllAsync()
                   .ReturnsNull();

        // Act
        var result = (NotFoundResult)await _sut.GetProducts();

        // Assert
        result.StatusCode.Should().Be(404);
    }
}
