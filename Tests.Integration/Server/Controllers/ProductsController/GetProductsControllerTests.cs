using MongoDB.Bson;
using PhotoPortfolio.Server.Contracts;
using PhotoPortfolio.Server.Data;
using PhotoPortfolio.Shared.Entities;
using System.Net;
using System.Net.Http.Json;

namespace PhotoPortfolio.Tests.Integration.Server.Controllers.ProductsController;

public class GetProductsControllerTests : IClassFixture<PhotoApiFactory>
{
    private readonly PhotoApiFactory _apiFactory;
    private readonly HttpClient _client;
    private readonly IProductRepository _productRepository;

    public GetProductsControllerTests(PhotoApiFactory apiFactory)
    {
        _apiFactory = apiFactory;
        _client = _apiFactory.CreateClient();
        _client.BaseAddress = new Uri("https://localhost/api/");
        _client.DefaultRequestHeaders.Add("Accept", "application/json");

        _productRepository = new ProductRepository(
            new MongoContext(_apiFactory.ConnectionString, PhotoApiFactory.TestDbName));
    }

    [Fact]
    public async Task GetProducts_ReturnsListOfProducts_WhenProductsExist()
    {
        // Arrange
        var product = new Product
        {
            Id = ObjectId.GenerateNewId().ToString()
        };
        var createdProduct = await _productRepository.AddAsync(product);

        // Act
        var response = await _client.GetAsync($"products");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<List<Product>>();
        result.Should().HaveCount(1);
        result!.First().Id.Should().Be(createdProduct.Id);

        // Clean up
        await _productRepository.DeleteAsync(createdProduct.Id!);
    }

    [Fact]
    public async Task GetProducts_ReturnsNotFound_WhenNoProductsExist()
    {
        // Act
        var response = await _client.GetAsync($"products");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
