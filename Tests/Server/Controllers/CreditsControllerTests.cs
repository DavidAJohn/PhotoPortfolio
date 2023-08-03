using Microsoft.AspNetCore.Mvc;
using PhotoPortfolio.Server.Contracts;
using PhotoPortfolio.Server.Controllers;
using PhotoPortfolio.Shared.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PhotoPortfolio.Tests.Server.Controllers;

public class CreditsControllerTests : BaseApiController
{
    private readonly CreditsController _sut;
    private readonly ICreditService _service = Substitute.For<ICreditService>();

    public CreditsControllerTests()
    {
        _sut = new CreditsController(_service);
    }

    [Fact]
    public async Task GetPhotoCredits_ShouldReturnPhotoCredits_WhenPhotoCreditsExist()
    {
        // Arrange
        var credits = new List<PhotoCredit>
        {
            new PhotoCredit
            {
                Id = Guid.NewGuid().ToString(),
                Order = 1,
                CreatorName = "Test",
                CreatorUnsplashProfileUri = "https://unsplash.com/@test",
                UnsplashImageUri = "https://unsplash.com/photos/1.jpg",
                PhotoPortfolioImageUri = "https://localhost/images/1.jpg",
            }
        };

        _service.GetPhotoCredits().Returns(credits);

        // Act
        var result = (OkObjectResult)await _sut.GetPhotoCredits();

        // Assert
        result.StatusCode.Should().Be(200);
        result.Value.Should().BeOfType<List<PhotoCredit>>();
        result.Value.Should().BeEquivalentTo(credits);
    }

    [Fact]
    public async Task GetPhotoCredits_ShouldReturnNotFound_WhenPhotoCreditsDoNotExist()
    {
        // Arrange
        _service.GetPhotoCredits().ReturnsNull();

        // Act
        var result = (NotFoundResult)await _sut.GetPhotoCredits();

        // Assert
        result.StatusCode.Should().Be(404);
    }
}
