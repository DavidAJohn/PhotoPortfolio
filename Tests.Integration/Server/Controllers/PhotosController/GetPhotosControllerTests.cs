using MongoDB.Bson;
using PhotoPortfolio.Server.Contracts;
using PhotoPortfolio.Server.Data;
using PhotoPortfolio.Shared.Entities;
using PhotoPortfolio.Shared.Helpers;
using PhotoPortfolio.Shared.Models;
using System.Net;
using System.Net.Http.Json;

namespace PhotoPortfolio.Tests.Integration.Server.Controllers.PhotosController;

public class GetPhotosControllerTests : IClassFixture<PhotoApiFactory>
{
    private readonly PhotoApiFactory _apiFactory;
    private readonly HttpClient _client;
    private readonly IPhotoRepository _photoRepository;

    public GetPhotosControllerTests(PhotoApiFactory apiFactory)
    {
        _apiFactory = apiFactory;
        _client = _apiFactory.CreateClient();
        _client.BaseAddress = new Uri("https://localhost/api/");
        _client.DefaultRequestHeaders.Add("Accept", "application/json");
        
        _photoRepository = new PhotoRepository(
            new MongoContext(_apiFactory.ConnectionString, PhotoApiFactory.TestDbName));
    }

    [Fact]
    public async Task GetPhotoById_ReturnsPhoto_WhenPhotoExists()
    {
        // Arrange
        var photo = CreatePhoto();
        var createdPhoto = await _photoRepository.AddAsync(photo);

        // Act
        var response = await _client.GetAsync($"photos/{createdPhoto.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<Photo>();
        result!.Id.Should().Be(createdPhoto.Id);
        result.Should().BeEquivalentTo(createdPhoto, options => options
                            .ExcludingMissingMembers()
                            .Excluding(p => p.DateAdded));

        // Clean up
        await _photoRepository.DeleteAsync(createdPhoto.Id!);
    }

    [Fact]
    public async Task GetPhotoById_ReturnsNotFound_WhenPhotoDoesNotExist()
    {
        // Act
        var response = await _client.GetAsync($"photos/{Guid.NewGuid}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetPhotos_ReturnsListOfPhotos_WhenPhotosExist_WithParams()
    {
        // Arrange
        var photo = CreatePhoto();
        var createdPhoto = await _photoRepository.AddAsync(photo);

        var photoParams = new PhotoSpecificationParams
        {
            GalleryId = "1",
            Title = "Test",
            SortBy = "Custom",
            SortOrder = "asc",
        };

        // Act
        var queryStringParams = new Dictionary<string, string>
        {
            { "GalleryId", photoParams.GalleryId }
        };
        var url = QueryHelpers.AddQueryString("photos", queryStringParams);
        var response = await _client.GetAsync(url);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<List<Photo>>();
        result!.First().Id.Should().Be(createdPhoto.Id);
        result!.First().Should().BeEquivalentTo(createdPhoto, options => options
                            .ExcludingMissingMembers()
                            .Excluding(p => p.DateAdded));

        // Clean up
        await _photoRepository.DeleteAsync(createdPhoto.Id!);
    }

    [Fact]
    public async Task GetPhotos_ReturnsListOfPhotos_WhenPhotosExist_WithSortingParams()
    {
        // Arrange
        var photo = CreatePhoto();
        var createdPhoto = await _photoRepository.AddAsync(photo);

        var photoParams = new PhotoSpecificationParams
        {
            GalleryId = "1",
            Title = "Test",
            SortBy = "Custom",
            SortOrder = "asc",
        };

        // Act
        var queryStringParams = new Dictionary<string, string>
        {
            { "sortBy", photoParams.SortBy },
            { "sortOrder", photoParams.SortOrder }
        };
        var url = QueryHelpers.AddQueryString("photos", queryStringParams);
        var response = await _client.GetAsync(url);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<List<Photo>>();
        result!.First().Id.Should().Be(createdPhoto.Id);
        result!.First().Should().BeEquivalentTo(createdPhoto, options => options
                            .ExcludingMissingMembers()
                            .Excluding(p => p.DateAdded));

        // Clean up
        await _photoRepository.DeleteAsync(createdPhoto.Id!);
    }

    [Fact]
    public async Task GetPhotos_ReturnsListOfPhotos_WhenPhotosExist_NoParams()
    {
        // Arrange
        var photo = CreatePhoto();
        var createdPhoto = await _photoRepository.AddAsync(photo);

        // Act
        var response = await _client.GetAsync("photos");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<List<Photo>>();
        result!.First().Id.Should().Be(createdPhoto.Id);

        // Clean up
        await _photoRepository.DeleteAsync(createdPhoto.Id!);
    }

    [Fact]
    public async Task GetPhotos_ReturnsNotFound_WhenNoPhotosExist()
    {
        // Act
        var response = await _client.GetAsync("photos");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    private Photo CreatePhoto()
    {
        var id = ObjectId.GenerateNewId().ToString();
        var photo = new Photo
        {
            Id = id,
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
                    Id = ObjectId.GenerateNewId().ToString(),
                }
            }
        };

        return photo;
    }
}
