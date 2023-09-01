using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using PhotoPortfolio.Server.Contracts;
using PhotoPortfolio.Server.Services;
using PhotoPortfolio.Shared.Models;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PhotoPortfolio.Tests.Unit.Server.Services;

public class UploadServiceTests
{
    private readonly UploadService _sut;
    private readonly IConfigurationService _configService = Substitute.For<IConfigurationService>();
    private readonly ILogger<UploadService> _logger = Substitute.For<ILogger<UploadService>>();

    public UploadServiceTests()
    {
        _sut = new UploadService(_logger, _configService);
    }

    [Fact]
    public async Task UploadFiles_ShouldReturn_ListOfUploadResultsWithErrors_WhenFileExtensionIsInvalid()
    {
        // Arrange
        var inMemoryConfig = new Dictionary<string, string?> {
            {"AzureUpload:AzureStorageConnectionString", "AzureStorageConnectionString"},
            {"AzureUpload:AzureContainerName", "AzureContainerName"},
            {"AzureUpload:FileUploadTypesAllowed", ".jpeg,.jpg,.png"},
            {"AzureUpload:MaxFileNameLength", "75"},
        };

        IConfiguration configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemoryConfig)
            .Build();

        _configService.GetConfiguration().Returns(configuration);

        var files = new List<IFormFile>()
        {
            new FormFile(new MemoryStream(Encoding.UTF8.GetBytes("This is a test file")), 0, 0, "Data", "test.txt")
        };

        // Act
        var result = await _sut.UploadFiles(files);

        // Assert
        result.Should().BeOfType<List<UploadResult>>();
        result.Should().HaveCount(1);
        result.First().FileName.Should().Be("test.txt");
        result.First().Uploaded.Should().BeFalse();
        result.First().ErrorCode.Should().Be(UploadErrorCode.BasicFileCheckError);
        result.Should().Contain(x => x.ErrorMessages!.Contains("Upload of 'test.txt' with file type '.txt' is not allowed"));
    }

    [Fact]
    public async Task UploadFiles_ShouldReturn_ListOfUploadResultsWithErrors_WhenFileExtensionIsMissing()
    {
        // Arrange
        var inMemoryConfig = new Dictionary<string, string?> {
            {"AzureUpload:AzureStorageConnectionString", "AzureStorageConnectionString"},
            {"AzureUpload:AzureContainerName", "AzureContainerName"},
            {"AzureUpload:FileUploadTypesAllowed", ".jpeg,.jpg,.png"},
        };

        IConfiguration configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemoryConfig)
            .Build();

        _configService.GetConfiguration().Returns(configuration);

        var files = new List<IFormFile>()
        {
            new FormFile(new MemoryStream(Encoding.UTF8.GetBytes("This is a test file")), 0, 0, "Data", "test")
        };

        // Act
        var result = await _sut.UploadFiles(files);

        // Assert
        result.Should().BeOfType<List<UploadResult>>();
        result.Should().HaveCount(1);
        result.First().FileName.Should().Be("test");
        result.First().Uploaded.Should().BeFalse();
        result.First().ErrorCode.Should().Be(UploadErrorCode.BasicFileCheckError);
        result.Should().Contain(x => x.ErrorMessages!.Contains("'test' does not appear to have a file extension"));
    }

    [Fact]
    public async Task UploadFiles_ShouldReturn_ListOfUploadResultsWithErrors_WhenFileIsAboveBytesSizeLimit()
    {
        // Arrange
        var inMemoryConfig = new Dictionary<string, string?> {
            {"AzureUpload:AzureStorageConnectionString", "AzureStorageConnectionString"},
            {"AzureUpload:AzureContainerName", "AzureContainerName"},
            {"AzureUpload:FileUploadTypesAllowed", ".jpeg,.jpg,.png"},
            {"AzureUpload:AzureUpload:MaxFileUploadSize", "0"},
        };

        IConfiguration configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemoryConfig)
            .Build();

        _configService.GetConfiguration().Returns(configuration);

        var files = new List<IFormFile>()
        {
            new FormFile(new MemoryStream(Encoding.UTF8.GetBytes("This is a test file")), 0, 1, "Data", "test.jpg")
        };

        // Act
        var result = await _sut.UploadFiles(files);

        // Assert
        result.Should().BeOfType<List<UploadResult>>();
        result.Should().HaveCount(1);
        result.First().FileName.Should().Be("test.jpg");
        result.First().Uploaded.Should().BeFalse();
        result.First().ErrorCode.Should().Be(UploadErrorCode.BasicFileCheckError);
        result.Should().Contain(x => x.ErrorMessages!.Contains("The size of 'test.jpg' (1 bytes) is larger than the current file size limit"));
    }

    [Fact]
    public async Task UploadFiles_ShouldReturn_ListOfUploadResultsWithErrors_WhenFileNameLengthIsAboveLimit()
    {
        // Arrange
        var inMemoryConfig = new Dictionary<string, string?> {
            {"AzureUpload:AzureStorageConnectionString", "AzureStorageConnectionString"},
            {"AzureUpload:AzureContainerName", "AzureContainerName"},
            {"AzureUpload:FileUploadTypesAllowed", ".jpeg,.jpg,.png"},
            {"AzureUpload:MaxFileNameLength", "1"},
        };

        IConfiguration configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemoryConfig)
            .Build();

        _configService.GetConfiguration().Returns(configuration);

        var files = new List<IFormFile>()
        {
            new FormFile(new MemoryStream(Encoding.UTF8.GetBytes("This is a test file")), 0, 1, "Data", "test.jpg")
        };

        // Act
        var result = await _sut.UploadFiles(files);

        // Assert
        result.Should().BeOfType<List<UploadResult>>();
        result.Should().HaveCount(1);
        result.First().FileName.Should().Be("test.jpg");
        result.First().Uploaded.Should().BeFalse();
        result.First().ErrorCode.Should().Be(UploadErrorCode.BasicFileCheckError);
        result.Should().Contain(x => x.ErrorMessages!.Contains("The name of 'test.jpg' was too long: 8 characters"));
    }
}
