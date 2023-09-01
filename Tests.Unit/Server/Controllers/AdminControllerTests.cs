using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using PhotoPortfolio.Server.Contracts;
using PhotoPortfolio.Server.Controllers;
using PhotoPortfolio.Shared.Entities;
using PhotoPortfolio.Shared.Helpers;
using PhotoPortfolio.Shared.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace PhotoPortfolio.Tests.Unit.Server.Controllers;

public class AdminControllerTests : BaseApiController
{
    private readonly AdminController _sut;
    private readonly ILogger<AdminController> _logger = Substitute.For<ILogger<AdminController>>();
    private readonly IOrderService _orderService = Substitute.For<IOrderService>();
    private readonly IPhotoRepository _photoRepository = Substitute.For<IPhotoRepository>();
    private readonly IGalleryRepository _galleryRepository = Substitute.For<IGalleryRepository>();
    private readonly IProductRepository _productRepository = Substitute.For<IProductRepository>();
    private readonly IPreferencesRepository _preferencesRepository = Substitute.For<IPreferencesRepository>();
    private readonly IUploadService _uploadService = Substitute.For<IUploadService>();
    private readonly IConfigurationService _configService = Substitute.For<IConfigurationService>();

    public AdminControllerTests()
    {
        _sut = new AdminController(_galleryRepository, _photoRepository, _productRepository, _preferencesRepository, _orderService, _logger, _uploadService, _configService);
    }

    // GALLERIES
    //
    [Fact]
    public async Task GetAllGalleries_ShouldReturnGalleries_WhenGalleriesExist()
    {
        // Arrange
        var galleries = new List<Gallery>
        {
            new Gallery
            {
                Id = Guid.NewGuid().ToString(),
                Name = "Test Gallery",
                Description = "Test Gallery Description",
                Photos = new List<Photo>
                {
                    new Photo
                    {
                        Id = Guid.NewGuid().ToString(),
                        GalleryId = Guid.NewGuid().ToString(),
                        Title = "Test",
                        FileName = "test.jpg"
                    }
                },
                Public = true
            }
        };

        _galleryRepository.GetAllAsync().Returns(galleries);

        // Act
        var result = (OkObjectResult)await _sut.GetAllGalleries();

        // Assert
        result.StatusCode.Should().Be(200);
        result.Value.Should().BeOfType<List<Gallery>>();

        var galleriesResult = result.Value as List<Gallery>;
        galleriesResult.Should().NotBeNull();
        galleriesResult!.Count.Should().Be(1);
        galleriesResult.First().Name.Should().Be("Test Gallery");
        galleriesResult.First().Description.Should().Be("Test Gallery Description");
        galleriesResult.First().Photos.Count.Should().Be(1);
    }

    [Fact]
    public async Task GetAllGalleries_ShouldReturnNotFound_WhenNoGalleriesExist()
    {
        // Arrange
        var galleryId = Guid.NewGuid().ToString();

        _galleryRepository.GetAllAsync().ReturnsNull();

        // Act
        var result = (NotFoundResult)await _sut.GetAllGalleries();

        // Assert
        result.StatusCode.Should().Be(404);
    }

    [Fact]
    public async Task GetGalleryById_ShouldReturnGallery_WhenGalleryIdExists()
    {
        // Arrange
        var galleryId = Guid.NewGuid().ToString();
        var gallery = new Gallery
        {
            Id = galleryId,
            Name = "Test Gallery",
            Description = "Test Gallery Description",
            Photos = new List<Photo>
            {
                new Photo
                {
                    Id = Guid.NewGuid().ToString(),
                    GalleryId = galleryId,
                    Title = "Test",
                    FileName = "test.jpg"
                }
            },
            Public = true
        };

        _galleryRepository.GetGalleryWithPhotos(galleryId, true).Returns(gallery);

        // Act
        var result = (OkObjectResult)await _sut.GetGalleryById(galleryId);

        // Assert
        result.StatusCode.Should().Be(200);
        result.Value.Should().BeOfType<Gallery>();

        var galleryResult = result.Value as Gallery;
        galleryResult.Should().NotBeNull();
        galleryResult!.Id.Should().Be(galleryId);
        galleryResult.Name.Should().Be("Test Gallery");
        galleryResult.Description.Should().Be("Test Gallery Description");
        galleryResult.Photos.Count.Should().Be(1);
    }

    [Fact]
    public async Task GetGalleryById_ShouldReturnNotFound_WhenGalleryIdDoesNotExist()
    {
        // Arrange
        var galleryId = Guid.NewGuid().ToString();

        _galleryRepository.GetGalleryWithPhotos(galleryId, true).ReturnsNull();

        // Act
        var result = (NotFoundResult)await _sut.GetGalleryById(galleryId);

        // Assert
        result.StatusCode.Should().Be(404);
    }

    [Fact]
    public async Task AddGallery_ShouldAddGallery_WhenAddGalleryRequestIsValid()
    {
        // Arrange
        var gallery = new Gallery
        {
            Id = Guid.NewGuid().ToString(),
            Name = "Test Gallery",
            Description = "Test Gallery Description",
            ParentGallery = null,
            Photos = new List<Photo>
            {
                new Photo
                {
                    Id = Guid.NewGuid().ToString(),
                    GalleryId = Guid.NewGuid().ToString(),
                    Title = "Test",
                    FileName = "test.jpg"
                }
            },
            Public = true
        };

        _galleryRepository.AddAsync(gallery).Returns(gallery);

        // Act
        var result = (CreatedAtActionResult)await _sut.AddGallery(gallery);

        // Assert
        result.StatusCode.Should().Be(201);
        result.Value.Should().BeOfType<Gallery>();
        result.RouteValues!["id"].Should().BeEquivalentTo(gallery.Id);

        var galleryResult = result.Value as Gallery;
        galleryResult.Should().NotBeNull();
        galleryResult!.Id.Should().Be(gallery.Id);
        galleryResult.Name.Should().Be("Test Gallery");
        galleryResult.Photos.Count.Should().Be(1);
    }

    [Fact]
    public async Task AddGallery_ShouldReturnBadRequest_WhenAddGalleryRequestIsInvalid()
    {
        // Arrange
        _galleryRepository.AddAsync(Arg.Any<Gallery>()).ReturnsNull();

        // Act
        var result = (BadRequestResult)await _sut.AddGallery(new Gallery());

        // Assert
        result.StatusCode.Should().Be(400);
    }

    [Fact]
    public async Task UpdateGallery_ShouldReturnNoContent_WhenGalleryWasUpdated()
    {
        // Arrange
        var galleryId = Guid.NewGuid().ToString();
        var gallery = new Gallery
        {
            Id = galleryId,
            Name = "Test Gallery",
            Description = "Test Gallery Description",
            Photos = new List<Photo>
            {
                new Photo
                {
                    Id = Guid.NewGuid().ToString(),
                    GalleryId = galleryId,
                    Title = "Test",
                    FileName = "test.jpg"
                }
            },
            Public = true
        };

        _galleryRepository.GetSingleAsync(Arg.Any<Expression<Func<Gallery, bool>>>()).Returns(gallery);

        // Act
        var result = (NoContentResult)await _sut.UpdateGallery(galleryId, gallery);

        // Assert
        result.StatusCode.Should().Be(204);
    }

    [Fact]
    public async Task UpdateGallery_ShouldReturnNotFound_WhenGalleryWasNotFound()
    {
        // Arrange
        _galleryRepository.GetSingleAsync(Arg.Any<Expression<Func<Gallery, bool>>>()).ReturnsNull();

        // Act
        var result = (NotFoundResult)await _sut.UpdateGallery(Guid.NewGuid().ToString(), new Gallery());

        // Assert
        result.StatusCode.Should().Be(404);
    }

    [Fact]
    public async Task DeleteGallery_ShouldReturnNoContent_WhenGalleryWasDeleted()
    {
        // Arrange
        _galleryRepository.GetSingleAsync(Arg.Any<Expression<Func<Gallery, bool>>>()).Returns(new Gallery());

        // Act
        var result = (NoContentResult)await _sut.DeleteGallery(Guid.NewGuid().ToString());

        // Assert
        result.StatusCode.Should().Be(204);
    }

    [Fact]
    public async Task DeleteGallery_ShouldReturnNotFound_WhenGalleryWasNotFound()
    {
        // Arrange
        _galleryRepository.GetSingleAsync(Arg.Any<Expression<Func<Gallery, bool>>>()).ReturnsNull();

        // Act
        var result = (NotFoundResult)await _sut.DeleteGallery(Guid.NewGuid().ToString());

        // Assert
        result.StatusCode.Should().Be(404);
    }

    // UPLOADS
    //

    [Fact]
    public async Task UploadFiles_ShouldReturnCreatedResultAndListOfUploadResults_WhenFilesUploadedSuccessfully()
    {
        // Arrange
        var inMemoryConfig = new Dictionary<string, string?> {
            {"AzureUpload:AzureStorageConnectionString", "AzureStorageConnectionString"},
            {"AzureUpload:AzureContainerUri", "AzureContainerUri"},
            {"AzureUpload:AzureStorageContainerName", "AzureStorageContainerName"}
        };

        IConfiguration configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemoryConfig)
            .Build();

        _configService.GetConfiguration().Returns(configuration);

        var files = Substitute.For<List<IFormFile>>();
        files.Add(new FormFile(new MemoryStream(Encoding.UTF8.GetBytes("This is a test file")), 0, 100, "Test", "test.jpg"));

        var uploadResults = new List<UploadResult>()
        {
            new UploadResult()
            {
                Uploaded = true,
                ErrorCode = 0,
                FileName= "test.jpg",
                StoredFileName = "test_vxcvx.jpg",
                AzureUri = "https://test.blob.core.windows.net/test/test_vxcvx.jpg",
                Title = "Test",
                Subject = "Test",
                ErrorMessages = new List<string>(),
                Metadata = new PhotoMetadata()
            }
        };

        _uploadService.UploadFiles(Arg.Any<List<IFormFile>>()).ReturnsForAnyArgs(uploadResults);

        // Act
        var result = (CreatedResult)await _sut.UploadFiles(files);

        // Assert
        result.StatusCode.Should().Be(201);
        result.Value.Should().BeOfType<List<UploadResult>>();

        var results = result.Value as List<UploadResult>;
        results.Should().NotBeNull();
        results!.Count.Should().Be(1);
        results.First().Uploaded.Should().BeTrue();
        results.First().FileName.Should().Be("test.jpg");
    }

    [Fact]
    public async Task UploadFiles_ShouldReturnListOfUploadResults_WhenAzureConnectionStringIsNull()
    {
        // Arrange
        var inMemoryConfig = new Dictionary<string, string?> {
            {"AzureUpload:AzureStorageContainerName", "AzureStorageContainerName"},
            {"AzureUpload:AzureContainerUri", "AzureContainerUri"}
        };

        IConfiguration configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemoryConfig)
            .Build();

        _configService.GetConfiguration().Returns(configuration);

        var files = new List<IFormFile>()
        {
            new FormFile(new MemoryStream(Encoding.UTF8.GetBytes("This is a test file")), 0, 0, "Data", "test.txt")
        };

        var uploadResults = new List<UploadResult>()
        {
            new UploadResult()
            {
                Uploaded = true,
                ErrorCode = 0,
            }
        };

        _uploadService.UploadFiles(files).Returns(uploadResults);

        // Act
        var result = (BadRequestObjectResult)await _sut.UploadFiles(files);

        // Assert
        result.StatusCode.Should().Be(400);

    }

    [Fact]
    public async Task UploadFiles_ShouldReturnListOfUploadResults_WhenAzureContainerNameIsNull()
    {
        // Arrange
        var inMemoryConfig = new Dictionary<string, string?> {
            {"AzureUpload:AzureStorageConnectionString", "AzureStorageConnectionString"},
            {"AzureUpload:AzureContainerUri", "AzureContainerUri"}
        };

        IConfiguration configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemoryConfig)
            .Build();

        _configService.GetConfiguration().Returns(configuration);

        var files = new List<IFormFile>()
        {
            new FormFile(new MemoryStream(Encoding.UTF8.GetBytes("This is a test file")), 0, 0, "Data", "test.txt")
        };

        var uploadResults = new List<UploadResult>()
        {
            new UploadResult()
            {
                Uploaded = true,
                ErrorCode = 0,
            }
        };

        _uploadService.UploadFiles(files).Returns(uploadResults);

        // Act
        var result = (BadRequestObjectResult)await _sut.UploadFiles(files);

        // Assert
        result.StatusCode.Should().Be(400);

    }

    [Fact]
    public async Task UploadFiles_ShouldReturnListOfUploadResults_WhenAzureContainerUriIsNull()
    {
        // Arrange
        var inMemoryConfig = new Dictionary<string, string?> {
            {"AzureUpload:AzureStorageConnectionString", "AzureStorageConnectionString"},
            {"AzureUpload:AzureStorageContainerName", "AzureStorageContainerName"}
        };

        IConfiguration configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemoryConfig)
            .Build();

        _configService.GetConfiguration().Returns(configuration);

        var files = new List<IFormFile>()
        {
            new FormFile(new MemoryStream(Encoding.UTF8.GetBytes("This is a test file")), 0, 0, "Data", "test.txt")
        };

        var uploadResults = new List<UploadResult>()
        {
            new UploadResult()
            {
                Uploaded = true,
                ErrorCode = 0,
            }
        };

        _uploadService.UploadFiles(files).Returns(uploadResults);

        // Act
        var result = (BadRequestObjectResult)await _sut.UploadFiles(files);

        // Assert
        result.StatusCode.Should().Be(400);

    }

    // PHOTOS
    //
    [Fact]
    public async Task GetPhotos_ShouldReturnPhotos_WhenParamsSupplied()
    {
        // Arrange
        var photoParams = new PhotoSpecificationParams
        {
            GalleryId = "1",
            Title = "Test",
            SortBy = "Custom",
            SortOrder = "asc",
        };

        _photoRepository.GetFilteredPhotosAsync(photoParams)
                        .Returns(new List<Photo>());

        // Act
        var result = (OkObjectResult)await _sut.GetPhotos(photoParams);

        // Assert
        result.StatusCode.Should().Be(200);
        result.Value.Should().BeOfType<List<Photo>>();
    }

    [Fact]
    public async Task GetPhotos_ShouldReturnPhotos_WhenParamsAreNull()
    {
        // Arrange
        var photoParams = new PhotoSpecificationParams
        {
            GalleryId = null,
            Title = null,
            SortBy = null,
            SortOrder = null
        };

        _photoRepository.GetAllAsync()
                        .Returns(new List<Photo>());

        // Act
        var result = (OkObjectResult)await _sut.GetPhotos(photoParams);

        // Assert
        result.StatusCode.Should().Be(200);
        result.Value.Should().BeOfType<List<Photo>>();
    }

    [Fact]
    public async Task GetPhotos_ShouldReturnNotFound_WhenNoPhotosExist()
    {
        // Arrange
        var photoParams = new PhotoSpecificationParams
        {
            GalleryId = "1",
            Title = "Test",
            SortBy = "Custom",
            SortOrder = "asc",
        };

        _photoRepository.GetFilteredPhotosAsync(photoParams)
                        .ReturnsNull();

        // Act
        var result = (NotFoundResult)await _sut.GetPhotos(photoParams);

        // Assert
        result.StatusCode.Should().Be(404);
    }

    [Fact]
    public async Task AddPhoto_ShouldAddPhoto_WhenAddPhotoRequestIsValid()
    {
        // Arrange
        var photo = new Photo
        {
            Id = Guid.NewGuid().ToString(),
            GalleryId = Guid.NewGuid().ToString(),
            Title = "Test",
            FileName = "test.jpg"
        };

        _photoRepository.AddAsync(photo).Returns(photo);

        // Act
        var result = (CreatedAtActionResult)await _sut.AddPhoto(photo);

        // Assert
        result.StatusCode.Should().Be(201);
        result.Value.Should().BeOfType<Photo>();
        result.RouteValues!["id"].Should().BeEquivalentTo(photo.Id);

        var photoResult = result.Value as Photo;
        photoResult.Should().NotBeNull();
        photoResult!.Id.Should().Be(photo.Id);
        photoResult.Title.Should().Be("Test");
    }

    [Fact]
    public async Task AddPhoto_ShouldReturnBadRequest_WhenAddPhotoRequestIsInvalid()
    {
          // Arrange
        _photoRepository.AddAsync(Arg.Any<Photo>()).ReturnsNull();

        // Act
        var result = (BadRequestResult)await _sut.AddPhoto(new Photo());

        // Assert
        result.StatusCode.Should().Be(400);
    }

    [Fact]
    public async Task UpdatePhoto_ShouldReturnNoContent_WhenPhotoWasUpdated()
    {
        // Arrange
        var photoId = Guid.NewGuid().ToString();
        var photo = new Photo
        {
            Id = photoId,
            GalleryId = Guid.NewGuid().ToString(),
            Title = "Test",
            FileName = "test.jpg"
        };

        _photoRepository.GetSingleAsync(Arg.Any<Expression<Func<Photo, bool>>>()).Returns(photo);

        // Act
        var result = (NoContentResult)await _sut.UpdatePhoto(photoId, photo);

        // Assert
        result.StatusCode.Should().Be(204);
    }

    [Fact]
    public async Task UpdatePhoto_ShouldReturnNotFound_WhenPhotoWasNotFound()
    {
        // Arrange
        _photoRepository.GetSingleAsync(Arg.Any<Expression<Func<Photo, bool>>>()).ReturnsNull();

        // Act
        var result = (NotFoundResult)await _sut.UpdatePhoto(Guid.NewGuid().ToString(), new Photo());

        // Assert
        result.StatusCode.Should().Be(404);
    }

    [Fact]
    public async Task DeletePhoto_ShouldReturnNoContent_WhenPhotoWasDeleted()
    {
        // Arrange
        _photoRepository.GetSingleAsync(Arg.Any<Expression<Func<Photo, bool>>>()).Returns(new Photo());

        // Act
        var result = (NoContentResult)await _sut.DeletePhoto(Guid.NewGuid().ToString());

        // Assert
        result.StatusCode.Should().Be(204);
    }

    [Fact]
    public async Task DeletePhoto_ShouldReturnNotFound_WhenPhotoWasNotFound()
    {
        // Arrange
        _photoRepository.GetSingleAsync(Arg.Any<Expression<Func<Photo, bool>>>()).ReturnsNull();

        // Act
        var result = (NotFoundResult)await _sut.DeletePhoto(Guid.NewGuid().ToString());

        // Assert
        result.StatusCode.Should().Be(404);
    }

    // PRODUCTS
    //

    [Fact]
    public async Task GetAllProducts_ShouldReturnProducts_WhenProductsExist()
    {
        // Arrange
        _productRepository.GetAllAsync().Returns(new List<Product>());

        // Act
        var result = (OkObjectResult)await _sut.GetAllProducts();

        // Assert
        result.StatusCode.Should().Be(200);
        result.Value.Should().BeOfType<List<Product>>();
    }

    [Fact]
    public async Task GetAllProducts_ShouldReturnNotFound_WhenProductsDoNotExist()
    {
        // Arrange
        _productRepository.GetAllAsync().ReturnsNull();

        // Act
        var result = (NotFoundResult)await _sut.GetAllProducts();

        // Assert
        result.StatusCode.Should().Be(404);
    }

    [Fact]
    public async Task GetProductById_ShouldReturnProduct_WhenProductIdExists()
    {
        // Arrange
        var product = new Product
        {
            Id = Guid.NewGuid().ToString()
        };

        _productRepository.GetSingleAsync(Arg.Any<Expression<Func<Product, bool>>>()).Returns(product);

        // Act
        var result = (OkObjectResult)await _sut.GetProductById(product.Id);

        // Assert
        result.StatusCode.Should().Be(200);
        result.Value.Should().BeOfType<Product>();
    }

    [Fact]
    public async Task GetProductById_ShouldReturnNotFound_WhenProductIdDoesNotExist()
    {
        // Arrange
        var product = new Product
        {
            Id = Guid.NewGuid().ToString()
        };

        _productRepository.GetSingleAsync(Arg.Any<Expression<Func<Product, bool>>>()).ReturnsNull();

        // Act
        var result = (NotFoundResult)await _sut.GetProductById(product.Id);

        // Assert
        result.StatusCode.Should().Be(404);
    }

    [Fact]
    public async Task AddProduct_ShouldReturnCreatedProduct_WhenProductIsAdded()
    {
          // Arrange
        var product = new Product
        {
            Id = Guid.NewGuid().ToString(),
        };

        _productRepository.AddAsync(product).Returns(product);

        // Act
        var result = (CreatedAtActionResult)await _sut.AddProduct(product);

        // Assert
        result.StatusCode.Should().Be(201);
        result.Value.Should().BeOfType<Product>();
        result.RouteValues!["id"].Should().BeEquivalentTo(product.Id);

        var productResult = result.Value as Product;
        productResult.Should().NotBeNull();
        productResult!.Id.Should().Be(product.Id);
    }

    [Fact]
    public async Task AddProduct_ShouldReturnBadRequest_WhenAddProductRequestIsInvalid()
    {
        // Arrange
        _productRepository.AddAsync(Arg.Any<Product>()).ReturnsNull();

        // Act
        var result = (BadRequestResult)await _sut.AddProduct(new Product());

        // Assert
        result.StatusCode.Should().Be(400);
    }

    [Fact]
    public async Task UpdateProduct_ShouldReturnNoContent_WhenProductWasUpdated()
    {
        // Arrange
        var productId = Guid.NewGuid().ToString();
        var product = new Product
        {
            Id = productId,
        };

        _productRepository.GetSingleAsync(Arg.Any<Expression<Func<Product, bool>>>()).Returns(product);

        // Act
        var result = (NoContentResult)await _sut.UpdateProduct(product);

        // Assert
        result.StatusCode.Should().Be(204);
    }

    [Fact]
    public async Task UpdateProduct_ShouldReturnNotFound_WhenProductWasNotFound()
    {
        // Arrange
        _productRepository.GetSingleAsync(Arg.Any<Expression<Func<Product, bool>>>()).ReturnsNull();

        // Act
        var result = (NotFoundResult)await _sut.UpdateProduct(new Product());

        // Assert
        result.StatusCode.Should().Be(404);
    }

    // SITE PREFERENCES
    //

    [Fact]
    public async Task GetSitePreferences_ShouldReturnSitePreferences_WhenSitePreferencesExist()
    {
        // Arrange
        var sitePrefsId = Guid.NewGuid().ToString();

        var inMemoryConfig = new Dictionary<string, string?> {
            { "SitePreferencesId", sitePrefsId }
        };

        IConfiguration configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemoryConfig)
            .Build();

        _configService.GetConfiguration().Returns(configuration);

        _preferencesRepository.GetSingleAsync(Arg.Any<Expression<Func<Preferences, bool>>>())
                              .Returns(new Preferences());

        // Act
        var result = (OkObjectResult)await _sut.GetSitePreferences();

        // Assert
        result.StatusCode.Should().Be(200);
        result.Value.Should().BeOfType<Preferences>();
    }

    [Fact]
    public async Task GetSitePreferences_ShouldReturnNotFound_WhenSitePreferencesDoNotExist()
    {
        // Arrange
        var sitePrefsId = Guid.NewGuid().ToString();

        var inMemoryConfig = new Dictionary<string, string?> {
            { "SitePreferencesId", sitePrefsId }
        };

        IConfiguration configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemoryConfig)
            .Build();

        _configService.GetConfiguration().Returns(configuration);

        _preferencesRepository.GetSingleAsync(Arg.Any<Expression<Func<Preferences, bool>>>())
                              .ReturnsNull();

        // Act
        var result = (NotFoundResult)await _sut.GetSitePreferences();

        // Assert
        result.StatusCode.Should().Be(404);
    }

    [Fact]
    public async Task UpdateSitePrefences_ShouldReturnNoContent_WhenSitePreferencesUpdated()
    {
        // Arrange
        var preferences = new Preferences()
        {
            SiteName = "SiteName",
            Metadata = new Dictionary<string, string>()
            {
                { "Key", "Value" }
            }
        };

        _preferencesRepository.UpdateAsync(preferences).Returns(new Preferences());

        // Act
        var result = (NoContentResult)await _sut.UpdateSitePrefences(preferences);

        // Assert
        result.StatusCode.Should().Be(204);
    }

    [Fact]
    public async Task UpdateSitePrefences_ShouldReturnBadRequest_WhenSitePreferencesNotUpdated()
    {
        // Arrange
        var preferences = new Preferences();
        _preferencesRepository.UpdateAsync(preferences)
                              .ReturnsNull();

        // Act
        var result = (BadRequestResult)await _sut.UpdateSitePrefences(preferences);

        // Assert
        result.StatusCode.Should().Be(400);
    }

    // ORDERS
    //

    [Fact]
    public async Task GetOrders_ShouldReturnListOfOrderDetailsDto_WhenParamsAreNotNullAndOrdersExist()
    {
        // Arrange
        var orderSpecParams = new OrderSpecificationParams()
        {
            Status = "Status",
            CustomerEmail = "CustomerEmail",
            InLastNumberOfDays = 365,
            SortBy = "SortBy",
            SortOrder = "SortOrder",
            ExcludePaymentIncomplete = false
        };

        var orderDetailsDto = new List<OrderDetailsDto>()
        {
            new OrderDetailsDto()
            {
                Id = Guid.NewGuid().ToString()
            }
        };

        _orderService.GetOrderDetails(orderSpecParams).Returns(orderDetailsDto);

        // Act
        var result = (OkObjectResult)await _sut.GetOrders(orderSpecParams);

        // Assert
        result.StatusCode.Should().Be(200);
        result.Value.Should().BeOfType<List<OrderDetailsDto>>();
    }

    [Fact]
    public async Task GetOrders_ShouldReturnListOfOrderDetailsDto_WhenParamsAreNullAndOrdersExist()
    {
        // Arrange
        var orderSpecParams = new OrderSpecificationParams()
        {
            Status = "Status",
            CustomerEmail = "CustomerEmail",
            InLastNumberOfDays = 365,
            SortBy = "SortBy",
            SortOrder = "SortOrder",
            ExcludePaymentIncomplete = false
        };

        var orderDetailsDto = new List<OrderDetailsDto>()
        {
            new OrderDetailsDto()
            {
                Id = Guid.NewGuid().ToString()
            }
        };

        _orderService.GetOrderDetails(Arg.Any<OrderSpecificationParams>()).Returns(orderDetailsDto);

        // Act
        var result = (OkObjectResult)await _sut.GetOrders(orderSpecParams);

        // Assert
        result.StatusCode.Should().Be(200);
        result.Value.Should().BeOfType<List<OrderDetailsDto>>();
    }

    [Fact]
    public async Task GetOrders_ShouldReturnNotFound_WhenNoOrdersExist()
    {
        // Arrange
        var orderSpecParams = new OrderSpecificationParams()
        {
            Status = "Status",
            CustomerEmail = "CustomerEmail",
            InLastNumberOfDays = 365,
            SortBy = "SortBy",
            SortOrder = "SortOrder",
            ExcludePaymentIncomplete = false
        };

        _orderService.GetOrderDetails(Arg.Any<OrderSpecificationParams>()).ReturnsNull();

        // Act
        var result = (NotFoundResult)await _sut.GetOrders(orderSpecParams);

        // Assert
        result.StatusCode.Should().Be(404);
    }
}
