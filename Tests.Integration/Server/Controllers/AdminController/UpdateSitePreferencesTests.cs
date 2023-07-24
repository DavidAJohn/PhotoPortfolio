using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using PhotoPortfolio.Server.Contracts;
using PhotoPortfolio.Server.Data;
using PhotoPortfolio.Server.Services;
using PhotoPortfolio.Shared.Entities;
using System.Net.Http.Json;
using System.Net;

namespace PhotoPortfolio.Tests.Integration.Server.Controllers.AdminController;

public class UpdateSitePreferencesTests : IClassFixture<PhotoApiFactory>
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

    public UpdateSitePreferencesTests(PhotoApiFactory apiFactory)
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
    public async Task UpdateSitePreferences_ReturnsNoContent_WhenPreferencesUpdated()
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

        var updatedPrefs = new Preferences()
        {
            Id = _sitePreferencesId,
            SiteName = "Updated Site Name", // updated
            Metadata = new Dictionary<string, string>()
            {
                { "OrdersSentToProdigiAutomatically", "AutoApproveBelow" },
                { "OrderAutoApproveLimit", "10" },
                { "ExcludeIncompleteFromOrderSearch", "false" }
            },
        };

        // Act
        var updateResponse = await _client.PutAsync($"admin/preferences", JsonContent.Create(updatedPrefs));
        var getUpdatedResponse = await _client.GetAsync($"admin/preferences");

        // Assert
        updateResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        getUpdatedResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await getUpdatedResponse.Content.ReadFromJsonAsync<Preferences>();
        result.Should().BeEquivalentTo(updatedPrefs);

        // Clean up
        await _preferencesRepository.DeleteAsync(_sitePreferencesId);
    }

    [Fact]
    public async Task UpdateSitePreferences_ReturnsBadRequest_WhenPreferencesCouldNotBeUpdated()
    {
        // Arrange
        var prefsId = ObjectId.GenerateNewId().ToString();
        var preferences = new Preferences()
        {
            Id = prefsId,
            SiteName = "Test Site Name",
            Metadata = new Dictionary<string, string>()
            {
                { "OrdersSentToProdigiAutomatically", "AutoApproveBelow" },
                { "OrderAutoApproveLimit", "10" },
                { "ExcludeIncompleteFromOrderSearch", "false" }
            },
        };

        await _preferencesRepository.AddAsync(preferences);

        var updatedPrefs = new Preferences()
        {
            Id = ObjectId.GenerateNewId().ToString(),
            SiteName = "Updated Site Name",
            Metadata = new Dictionary<string, string>()
            {
                { "OrdersSentToProdigiAutomatically", "AutoApproveBelow" },
                { "OrderAutoApproveLimit", "10" },
                { "ExcludeIncompleteFromOrderSearch", "false" }
            },
        };

        // Act
        var updateResponse = await _client.PutAsync($"admin/preferences", JsonContent.Create(updatedPrefs));

        // Assert
        updateResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        // Clean up
        await _preferencesRepository.DeleteAsync(_sitePreferencesId);
    }
}
