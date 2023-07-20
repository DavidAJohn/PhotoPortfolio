using MongoDB.Bson;
using PhotoPortfolio.Server.Contracts;
using PhotoPortfolio.Server.Data;
using PhotoPortfolio.Shared.Entities;
using PhotoPortfolio.Shared.Models;
using System.Net;
using System.Net.Http.Json;

namespace PhotoPortfolio.Tests.Integration.Server.Controllers.GalleriesController;

public class GetGalleriesControllerTests : IClassFixture<PhotoApiFactory>
{
    private readonly PhotoApiFactory _apiFactory;
    private readonly HttpClient _client;
    private readonly IGalleryRepository _galleryRepository;
    private readonly IPhotoRepository _photoRepository;

    public GetGalleriesControllerTests(PhotoApiFactory apiFactory)
    {
        _apiFactory = apiFactory;
        _client = _apiFactory.CreateClient();
        _client.BaseAddress = new Uri("https://localhost/api/");
        _client.DefaultRequestHeaders.Add("Accept", "application/json");

        _photoRepository = new PhotoRepository(
            new MongoContext(_apiFactory.ConnectionString, PhotoApiFactory.TestDbName));

        _galleryRepository = new GalleryRepository(
            new MongoContext(_apiFactory.ConnectionString, PhotoApiFactory.TestDbName), _photoRepository);
    }

    [Fact]
    public async Task GetGalleryById_ReturnsGalleryWithPhoto_WhenGalleryExists()
    {
        // Arrange
        var gallery = CreateGallery();
        var createdGallery = await _galleryRepository.AddAsync(gallery);

        var photo = CreatePhoto();
        photo.GalleryId = createdGallery.Id;
        var createdPhoto = await _photoRepository.AddAsync(photo);

        // Act
        var response = await _client.GetAsync($"galleries/{createdGallery.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<Gallery>();
        result!.Id.Should().Be(createdGallery.Id);
        result.Photos.Should().HaveCount(1);
        result.Photos.First().Id.Should().Be(createdPhoto.Id);

        // Clean up
        await _photoRepository.DeleteAsync(createdPhoto.Id!);
        await _galleryRepository.DeleteAsync(createdGallery.Id!);
    }

    [Fact]
    public async Task GetGalleryById_ReturnsNotFound_WhenGalleryDoesNotExist()
    {
        // Act
        var response = await _client.GetAsync($"galleries/{Guid.NewGuid}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetPublicGalleries_ReturnsOnlyPublicGalleries_WhenPublicGalleriesExist()
    {
        // Arrange
        var gallery = CreateGallery();
        gallery.DisplayInGalleryList = true;
        var createdGallery = await _galleryRepository.AddAsync(gallery);

        var nonPublicGallery = CreateGallery();
        nonPublicGallery.Public = false;
        var createdNonPublicGallery = await _galleryRepository.AddAsync(nonPublicGallery);

        // Act
        var response = await _client.GetAsync($"galleries");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<List<Gallery>>();
        result.Should().HaveCount(1);
        result!.First().Id.Should().Be(createdGallery.Id);

        // Clean up
        await _galleryRepository.DeleteAsync(createdGallery.Id!);
        await _galleryRepository.DeleteAsync(createdNonPublicGallery.Id!);
    }

    [Fact]
    public async Task GetPublicGalleries_ReturnsNotFound_WhenPublicGalleriesDoNotExist()
    {
        // Arrange
        var nonPublicGallery = CreateGallery();
        nonPublicGallery.Public = false;
        var createdNonPublicGallery = await _galleryRepository.AddAsync(nonPublicGallery);

        // Act
        var response = await _client.GetAsync($"galleries");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);

        // Clean up
        await _galleryRepository.DeleteAsync(createdNonPublicGallery.Id!);
    }

    private static Photo CreatePhoto()
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

    private static Gallery CreateGallery()
    {
        var gallery = new Gallery
        {
            Id = ObjectId.GenerateNewId().ToString(),
            Description = "Test",
            Public = true,
            Photos = new List<Photo>()
        };

        return gallery;
    }
}
