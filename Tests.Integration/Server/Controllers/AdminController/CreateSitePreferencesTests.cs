using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using PhotoPortfolio.Server.Contracts;
using PhotoPortfolio.Server.Data;
using PhotoPortfolio.Server.Services;
using PhotoPortfolio.Shared.Entities;
using System.Net.Http.Json;
using System.Net;
using PhotoPortfolio.Server.Messaging;
using Microsoft.Extensions.DependencyInjection;

namespace PhotoPortfolio.Tests.Integration.Server.Controllers.AdminController;

public class CreateSitePreferencesTests : IClassFixture<PhotoApiFactory>
{
    private readonly PhotoApiFactory _apiFactory;
    private readonly HttpClient _client;
    private readonly MongoContext _mongoContext;
    private readonly IGalleryRepository _galleryRepository;
    private readonly IPhotoRepository _photoRepository;
    private readonly IProductRepository _productRepository;
    private readonly IPreferencesRepository _preferencesRepository;
    private readonly IOrderRepository _orderRepository;
    private readonly IOrderService _orderService;
    private readonly ILogger<OrderService> _orderServiceLogger;
    private readonly ILogger<UploadService> _uploadServiceLogger;
    private readonly IUploadService _uploadService;
    private readonly IConfiguration _configuration;
    private readonly IConfigurationService _configService;
    private readonly IMessageSender _messageSender;
    private readonly string _sitePreferencesId;
    private readonly IHttpClientFactory _httpClientFactory;

    public CreateSitePreferencesTests(PhotoApiFactory apiFactory)
    {
        _apiFactory = apiFactory;
        _client = _apiFactory.CreateClient();
        _client.BaseAddress = new Uri("https://localhost/api/");
        _client.DefaultRequestHeaders.Add("Accept", "application/json");

        _sitePreferencesId = PhotoApiFactory.SitePreferencesId;

        _mongoContext = new MongoContext(_apiFactory.ConnectionString, PhotoApiFactory.TestDbName);

        _photoRepository = new PhotoRepository(_mongoContext);
        _galleryRepository = new GalleryRepository(_mongoContext, _photoRepository);
        _productRepository = new ProductRepository(_mongoContext);
        _preferencesRepository = new PreferencesRepository(_mongoContext);
        _orderRepository = new OrderRepository(_mongoContext);
        _configuration = _apiFactory.Services.GetRequiredService<IConfiguration>();
        _configService = new ConfigurationService(_configuration);
        _orderServiceLogger = new Logger<OrderService>(new LoggerFactory());
        _uploadServiceLogger = new Logger<UploadService>(new LoggerFactory());
        _messageSender = _apiFactory.Services.GetRequiredService<IMessageSender>();
        _httpClientFactory = _apiFactory.Services.GetRequiredService<IHttpClientFactory>();
        _orderService = new OrderService(_orderRepository, _preferencesRepository, _configService, _messageSender, _orderServiceLogger, _httpClientFactory);
        _uploadService = new UploadService(_uploadServiceLogger, _configService);
    }

    [Fact]
    public async Task CreateSitePreferences_ReturnsPreferences_WhenPreferencesCreated()
    {
        // Act
        var response = await _client.PostAsync("admin/preferences", null!);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<Preferences>();
        result!.Id.Should().Be(_sitePreferencesId);

        // Clean up
        await _preferencesRepository.DeleteAsync(_sitePreferencesId);
    }
}
