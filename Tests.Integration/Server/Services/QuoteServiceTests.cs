using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using PhotoPortfolio.Server.Contracts;
using PhotoPortfolio.Server.Data;
using PhotoPortfolio.Server.Services;
using PhotoPortfolio.Shared.Entities;
using PhotoPortfolio.Shared.Models;

namespace PhotoPortfolio.Tests.Integration.Server.Services;

public class QuoteServiceTests : IClassFixture<PhotoApiFactory>
{
    private readonly PhotoApiFactory _apiFactory;
    private readonly HttpClient _client;
    private readonly MongoContext _mongoContext;
    private readonly IConfiguration _configuration;
    private readonly IConfigurationService _configService;
    private readonly ILogger<QuoteService> _logger;
    private readonly IPhotoRepository _photoRepository;
    private readonly IHttpClientFactory _httpClientFactory;

    private readonly string _photoId = ObjectId.GenerateNewId().ToString();
    private readonly string _productId = ObjectId.GenerateNewId().ToString();

    private readonly string _prodigiWireMockUri;

    public QuoteServiceTests(PhotoApiFactory apiFactory)
    {
        _apiFactory = apiFactory;
        _client = _apiFactory.CreateClient();
        _client.BaseAddress = new Uri(_apiFactory.ProdigiPrintApiUrl + "/");
        _client.DefaultRequestHeaders.Add("Accept", "application/json");

        _mongoContext = new MongoContext(_apiFactory.ConnectionString, PhotoApiFactory.TestDbName);
        _configuration = _apiFactory.Services.GetRequiredService<IConfiguration>();
        _configService = new ConfigurationService(_configuration);
        _logger = new Logger<QuoteService>(new LoggerFactory());
        _photoRepository = new PhotoRepository(_mongoContext);
        _httpClientFactory = _apiFactory.Services.GetRequiredService<IHttpClientFactory>();

        _prodigiWireMockUri = _apiFactory.ProdigiPrintApiUrl + "/";
    }

    private OrderBasketDto CreateOrderBasketDto()
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
                        Id = _productId,
                        PhotoId = _photoId,
                        ImageUri = "https://localhost/images/1.jpg",
                        ImageTitle = "Test",
                        ProdigiSku = "ECO-CAN-16x24",
                        Options = new List<ProductOptionSelected>
                        {
                            new ProductOptionSelected
                            {
                                OptionLabel = "wrap",
                                OptionRef = "MirrorWrap",
                            }
                        }
                    },
                    Total = 100m,
                }
            },
            ShippingMethod = "Standard",
            ShippingCost = 5.25m
        };
    }

    private Photo CreatePhoto()
    {
        var photo = new Photo
        {
            Id = _photoId,
            Title = "Test",
            Caption = "Caption",
            FileName = "Test.jpg",
            Uri = "https://localhost/images/Test.jpg",
            GalleryId = "1",
            GallerySortOrder = 1,
            DateAdded = DateTime.UtcNow,
            Metadata = new PhotoMetadata
            {
                Tags = new List<string> { "Test" },
                Height = 100,
                Width = 100
            },
            Products = new List<PhotoProduct>
            {
                new PhotoProduct
                {
                    Id = _productId,
                    ProdigiSku = "ECO-CAN-16x24",
                    MarkupPercentage = 100,
                }
            }
        };

        return photo;
    }

    [Fact]
    public async Task GetBasketQuote_ReturnsOrderBasketDto_WhenIsAdminIsFalseQuoteIsReceived()
    {
        // Arrange
        _configuration["Prodigi:ApiKey"] = "00000000-0000-0000-0000-created";
        _configuration["Prodigi:ApiUri"] = _prodigiWireMockUri;
        var quoteService = new QuoteService(_httpClientFactory, _configService, _logger, _photoRepository);

        var photo = CreatePhoto();
        var createdPhoto = await _photoRepository.AddAsync(photo);

        var orderBasketDto = CreateOrderBasketDto();

        // Act
        var orderBasketReturned = await quoteService.GetBasketQuote(orderBasketDto, false);

        // Assert
        orderBasketReturned.Should().NotBeNull();
        orderBasketReturned.Should().BeOfType<OrderBasketDto>();
        orderBasketReturned.BasketItems.Should().NotBeEmpty();

        // Clean up
        await _photoRepository.DeleteAsync(createdPhoto.Id!);
    }

    [Fact]
    public async Task GetBasketQuote_ReturnsOrderBasketDto_WhenIsAdminIsTrueQuoteIsReceived()
    {
        // Arrange
        _configuration["Prodigi:ApiKey"] = "00000000-0000-0000-0000-created";
        _configuration["Prodigi:ApiUri"] = _prodigiWireMockUri;
        var quoteService = new QuoteService(_httpClientFactory, _configService, _logger, _photoRepository);

        var photo = CreatePhoto();
        var createdPhoto = await _photoRepository.AddAsync(photo);

        var orderBasketDto = CreateOrderBasketDto();

        // Act
        var orderBasketReturned = await quoteService.GetBasketQuote(orderBasketDto, true);

        // Assert
        orderBasketReturned.Should().NotBeNull();
        orderBasketReturned.Should().BeOfType<OrderBasketDto>();
        orderBasketReturned.BasketItems.Should().NotBeEmpty();
        // also check the markup percentage hasn't been applied
        // orderBasketReturned.BasketItems.First().Product.MarkupPercentage.Should().Be(0);

        // Clean up
        await _photoRepository.DeleteAsync(createdPhoto.Id!);
    }

    [Fact]
    public async Task GetBasketQuote_ReturnsOrderBasketDto_WhenQuoteIsReceivedAsCreatedWithIssues()
    {
        // Arrange
        _configuration["Prodigi:ApiKey"] = "00000000-0000-0000-0000-createdwithissues"; // returns a quote with issues from the mock
        _configuration["Prodigi:ApiUri"] = _prodigiWireMockUri;
        var quoteService = new QuoteService(_httpClientFactory, _configService, _logger, _photoRepository);

        var photo = CreatePhoto();
        var createdPhoto = await _photoRepository.AddAsync(photo);

        var orderBasketDto = CreateOrderBasketDto();

        // Act
        var orderBasketReturned = await quoteService.GetBasketQuote(orderBasketDto, false);

        // Assert
        orderBasketReturned.Should().NotBeNull();
        orderBasketReturned.Should().BeOfType<OrderBasketDto>();
        orderBasketReturned.BasketItems.Should().NotBeEmpty();

        // Clean up
        await _photoRepository.DeleteAsync(createdPhoto.Id!);
    }

    [Fact]
    public async Task GetBasketQuote_ShouldReturnNull_WhenQuoteResponseStatusCodeIsNot200OK()
    {
        // Arrange
        _configuration["Prodigi:ApiKey"] = "00000000-0000-0000-0000-badrequest"; // returns a 400 Bad Request from the mock
        _configuration["Prodigi:ApiUri"] = _prodigiWireMockUri;
        var quoteService = new QuoteService(_httpClientFactory, _configService, _logger, _photoRepository);

        var photo = CreatePhoto();
        var createdPhoto = await _photoRepository.AddAsync(photo);

        var orderBasketDto = CreateOrderBasketDto();

        // Act
        var orderBasketReturned = await quoteService.GetBasketQuote(orderBasketDto, false);

        // Assert
        orderBasketReturned.Should().BeNull();

        // Clean up
        await _photoRepository.DeleteAsync(createdPhoto.Id!);
    }

    [Fact]
    public async Task GetBasketQuote_ShouldReturnNull_WhenAnExceptionIsThrown()
    {
        // Arrange
        _configuration["Prodigi:ApiKey"] = "00000000-0000-0000-0000-unexpected-quote-structure"; // should return a 200 Ok response with an unexpected response structure
        _configuration["Prodigi:ApiUri"] = _prodigiWireMockUri;

        var quoteService = new QuoteService(_httpClientFactory, _configService, _logger, _photoRepository);

        var photo = CreatePhoto();
        var createdPhoto = await _photoRepository.AddAsync(photo);

        var orderBasketDto = CreateOrderBasketDto();

        // Act
        var orderBasketReturned = await quoteService.GetBasketQuote(orderBasketDto, false);

        // Assert
        orderBasketReturned.Should().BeNull();

        // Clean up
        await _photoRepository.DeleteAsync(createdPhoto.Id!);
    }

    [Fact]
    public async Task GetBasketQuote_ShouldReturnNull_WhenResponseOutcomeIsUnexpected()
    {
        // Arrange
        _configuration["Prodigi:ApiKey"] = "00000000-0000-0000-0000-unexpected-outcome"; // should return a 200 Ok response with an unexpected response structure
        _configuration["Prodigi:ApiUri"] = _prodigiWireMockUri;

        var quoteService = new QuoteService(_httpClientFactory, _configService, _logger, _photoRepository);

        var photo = CreatePhoto();
        var createdPhoto = await _photoRepository.AddAsync(photo);

        var orderBasketDto = CreateOrderBasketDto();

        // Act
        var orderBasketReturned = await quoteService.GetBasketQuote(orderBasketDto, false);

        // Assert
        orderBasketReturned.Should().BeNull();

        // Clean up
        await _photoRepository.DeleteAsync(createdPhoto.Id!);
    }
}
