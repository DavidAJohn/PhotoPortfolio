using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using PhotoPortfolio.Server.Contracts;
using PhotoPortfolio.Server.Data;
using PhotoPortfolio.Server.Services;
using PhotoPortfolio.Shared.Entities;
using System.Net;
using System.Net.Http.Json;

namespace PhotoPortfolio.Tests.Integration.Server.Controllers.AdminController;

public class GetSitePreferencesTests : IClassFixture<PhotoApiFactory>
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
    private readonly string _sitePreferencesId;

    public GetSitePreferencesTests(PhotoApiFactory apiFactory)
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
        _configService = new ConfigurationService(_configuration);
        _orderService = new OrderService(_orderRepository, _preferencesRepository, _configService, _orderServiceLogger);
        _uploadService = new UploadService(_uploadServiceLogger, _configService);
    }

    [Fact]
    public async Task GetSitePreferences_ReturnsPreferences_WhenPreferencesExist()
    {
        // Arrange
        var preferences = new Preferences()
        {
            Id = _sitePreferencesId,
            SiteName = "Test Site Name",
            Metadata = new Dictionary<string, string>()
            {
                { "OrdersSentToProdigiAutomatically", "AutoApproveBelow" },
                { "OrderAutoApproveLimit", "10" },
                { "ExcludeIncompleteFromOrderSearch", "false" }
            },
        };

        await _preferencesRepository.AddAsync(preferences);

        // Act
        var response = await _client.GetAsync("admin/preferences");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<Preferences>();
        result.Should().BeEquivalentTo(preferences);

        // Clean up
        await _preferencesRepository.DeleteAsync(_sitePreferencesId);
    }

    [Fact]
    public async Task GetSitePreferences_ReturnsNotFound_WhenNoPreferencesExist()
    {
        // Act
        var response = await _client.GetAsync($"admin/preferences");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
