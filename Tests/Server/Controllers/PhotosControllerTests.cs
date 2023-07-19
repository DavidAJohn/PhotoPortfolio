using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using NSubstitute.ReturnsExtensions;
using PhotoPortfolio.Server.Contracts;
using PhotoPortfolio.Server.Controllers;
using PhotoPortfolio.Shared.Entities;
using PhotoPortfolio.Shared.Helpers;
using PhotoPortfolio.Shared.Models;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace PhotoPortfolio.Tests.Server.Controllers;

public class PhotosControllerTests : BaseApiController
{
    private readonly PhotosController _sut;
    private readonly IPhotoRepository _repository = Substitute.For<IPhotoRepository>();

    public PhotosControllerTests()
    {
        _sut = new PhotosController(_repository);
    }

    [Fact]
    public async Task GetPhotos_WithParams_ReturnsPhotos()
    {
        // Arrange
        var photoParams = new PhotoSpecificationParams
        {
            GalleryId = "1",
            Title = "Test",
            SortBy = "Custom",
            SortOrder = "asc",
        };

        var photo = new Photo
        {
            Id = Guid.NewGuid().ToString(),
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
                    Id = Guid.NewGuid().ToString(),
                }
            }
        };

        var photos = new List<Photo> { photo };
        _repository.GetFilteredPhotosAsync(photoParams)
                   .Returns(photos);

        // Act
        var result = (OkObjectResult)await _sut.GetPhotos(photoParams);

        // Assert
        result.StatusCode.Should().Be(200);
        result.Value.Should().BeOfType<List<Photo>>();
    }

    [Fact]
    public async Task GetPhotos_WithNullParams_ReturnsPhotos()
    {
        // Arrange
        var photoParams = new PhotoSpecificationParams 
        { 
            GalleryId = null, 
            Title = null, 
            SortBy = null, 
            SortOrder = null 
        };

        var photo = new Photo
        {
            Id = Guid.NewGuid().ToString(),
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
                    Id = Guid.NewGuid().ToString(),
                }
            }
        };

        var photos = new List<Photo> { photo };

        _repository.GetAllAsync()
                   .Returns(photos);

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

        _repository.GetFilteredPhotosAsync(photoParams)
                   .ReturnsNull();

        // Act
        var result = (NotFoundResult)await _sut.GetPhotos(photoParams);

        // Assert
        result.StatusCode.Should().Be(404);
    }

    [Fact]
    public async Task GetPhotoById_WithValidId_ReturnsPhoto()
    {
        // Arrange
        var id = Guid.NewGuid().ToString();
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
                    Id = Guid.NewGuid().ToString(),
                }
            }
        };

        _repository.GetSingleAsync(Arg.Any<Expression<Func<Photo, bool>>>())
                   .Returns(photo);

        // Act
        var result = (OkObjectResult)await _sut.GetPhotoById(id);

        // Assert
        result.StatusCode.Should().Be(200);
        result.Value.Should().BeOfType<Photo>();
    }

    [Fact]
    public async Task GetPhotoById_ShouldReturnNotFound_WhenPhotoDoesNotExist()
    {
          // Arrange
        var id = Guid.NewGuid().ToString();

        _repository.GetSingleAsync(Arg.Any<Expression<Func<Photo, bool>>>())
                   .ReturnsNull();

        // Act
        var result = (NotFoundResult)await _sut.GetPhotoById(id);

        // Assert
        result.StatusCode.Should().Be(404);
    }
}
