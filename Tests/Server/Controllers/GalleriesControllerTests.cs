using Microsoft.AspNetCore.Mvc;
using PhotoPortfolio.Server.Contracts;
using PhotoPortfolio.Server.Controllers;
using PhotoPortfolio.Shared.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PhotoPortfolio.Tests.Server.Controllers;

public class GalleriesControllerTests : BaseApiController
{
    private readonly GalleriesController _sut;
    private readonly IGalleryRepository _repository = Substitute.For<IGalleryRepository>();

    public GalleriesControllerTests()
    {
        _sut = new GalleriesController(_repository);
    }

    [Fact]
    public async Task GetPublicGalleries_ShouldReturnGalleries_WhenGalleriesExist()
    {
        // Arrange
        _repository.GetPublicGalleries()
                   .Returns(new List<Gallery>());

        // Act
        var result = (OkObjectResult)await _sut.GetPublicGalleries();

        // Assert
        result.StatusCode.Should().Be(200);
        result.Value.Should().BeOfType<List<Gallery>>();
    }

    [Fact]
    public async Task GetPublicGalleries_ShouldReturnNotFound_WhenNoGalleriesExist()
    {
        // Arrange
        _repository.GetPublicGalleries()
                   .ReturnsNull();

        // Act
        var result = (NotFoundResult)await _sut.GetPublicGalleries();

        // Assert
        result.StatusCode.Should().Be(404);
    }

    [Fact]
    public async Task GetGalleryById_ShouldReturnGallery_WhenGalleryExists()
    {
        // Arrange
        var id = Guid.NewGuid().ToString();
        var gallery = new Gallery
        {
            Id = id,
            Description = "Test",
            Public = true,
            Photos = new List<Photo>()
        };

        _repository.GetGalleryWithPhotos(id)
                   .Returns(gallery);

        // Act
        var result = (OkObjectResult)await _sut.GetGalleryById(id);

        // Assert
        result.StatusCode.Should().Be(200);
        result.Value.Should().BeOfType<Gallery>();
    }

    [Fact]
    public async Task GetGalleryById_ShouldReturnNotFound_WhenGalleryDoesNotExist()
    {
        // Arrange
        var id = Guid.NewGuid().ToString();

        _repository.GetGalleryWithPhotos(id)
                   .ReturnsNull();

        // Act
        var result = (NotFoundResult)await _sut.GetGalleryById(id);

        // Assert
        result.StatusCode.Should().Be(404);
    }
}
