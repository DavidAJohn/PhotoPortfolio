using Azure.Storage.Blobs.Models;
using Azure.Storage.Blobs;
using MetadataExtractor.Formats.Exif;
using MetadataExtractor.Formats.Iptc;
using MetadataExtractor.Formats.Jpeg;
using MetadataExtractor.Formats.Png;
using MetadataExtractor;
using PhotoPortfolio.Shared.Models;
using System.Web;
using SerilogTimings;

namespace PhotoPortfolio.Server.Services;

public class UploadService : IUploadService
{
    private readonly ILogger<UploadService> _logger;
    private readonly IConfiguration _config;

    public UploadService(ILogger<UploadService> logger, IConfiguration config)
    {
        _logger = logger;
        _config = config;
    }

    public async Task<List<UploadResult>> UploadFiles(IEnumerable<IFormFile> files)
    {
        var azureConnectionString = _config["AzureUpload:AzureStorageConnectionString"];
        var azureContainerName = _config["AzureUpload:AzureContainerName"];
        var permittedFileExtensions = _config["AzureUpload:FileUploadTypesAllowed"];
        var fileSizeLimit = _config.GetValue<long>("AzureUpload:MaxFileUploadSize");
        var fileNameLengthLimit = _config.GetValue<int>("AzureUpload:MaxFileNameLength");

        List<UploadResult> uploadResults = new();

        foreach (var file in files)
        {
            var uploadResult = new UploadResult();
            var untrustedFileName = file.FileName;
            uploadResult.FileName = untrustedFileName;
            string trustedFileNameForStorage = "";

            var imageTitle = "";
            var imageSubject = "";

            var fileExtension = Path.GetExtension(file.FileName.ToLowerInvariant());

            List<string> basicFileChecks = BasicFileChecks(file, permittedFileExtensions, fileSizeLimit, fileNameLengthLimit, fileExtension);

            if (basicFileChecks is not null)
            {
                uploadResult.Uploaded = false;
                uploadResult.FileName = file.FileName;
                uploadResult.ErrorCode = 2; // basic file checks failed
                uploadResult.ErrorMessages = basicFileChecks;

                uploadResults.Add(uploadResult);
            }
            else
            {
                try
                {
                    var azureContainer = new BlobContainerClient(azureConnectionString, azureContainerName);

                    // in case the container doesn't exist
                    var createResponse = await azureContainer.CreateIfNotExistsAsync();

                    if (createResponse != null && createResponse.GetRawResponse().Status == 201)
                    {
                        await azureContainer.SetAccessPolicyAsync(PublicAccessType.Blob);
                    }

                    // generate a safe and unique upload file name:
                    // [original_filename_without_extension]_[8_random_chars].[original_filename_extension]
                    // eg. filename_xgh38tye.jpg
                    trustedFileNameForStorage = string.Concat(
                        HttpUtility.HtmlEncode(Path.GetFileNameWithoutExtension(file.FileName)),
                        "_",
                        Path.GetRandomFileName().AsSpan(0, 8),
                        Path.GetExtension(file.FileName));

                    var blob = azureContainer.GetBlobClient(trustedFileNameForStorage);

                    // set the content type (which may or may not have been provided by the client)
                    var blobHttpHeader = new BlobHttpHeaders();

                    if (file.ContentType != null)
                    {
                        blobHttpHeader.ContentType = file.ContentType;
                    }
                    else
                    {
                        blobHttpHeader.ContentType = fileExtension switch
                        {
                            ".jpg" => "image/jpeg",
                            ".jpeg" => "image/jpeg",
                            ".png" => "image/png",
                            _ => null
                        };
                    }

                    using (var op = Operation.Begin("Upload of '{filename}' to Azure Storage", file.FileName)) // SerilogTimings
                    using (var fileStream = file.OpenReadStream())
                    {
                        await blob.UploadAsync(fileStream, blobHttpHeader);
                        op.Complete();
                    }

                    _logger.LogInformation("'{originalName}' was uploaded to Azure as '{uploadName}'", file.FileName, trustedFileNameForStorage);

                    // extract image metadata - using a new stream
                    using (var op = Operation.Begin("Extraction of metadata from '{filename}'", file.FileName)) // SerilogTimings
                    using (var fileStream = file.OpenReadStream())
                    {
                        uploadResult.Metadata = new PhotoMetadata();

                        // For details of how to use MetadataExtractor: 
                        // https://github.com/drewnoakes/metadata-extractor-dotnet

                        IEnumerable<MetadataExtractor.Directory> directories = ImageMetadataReader.ReadMetadata(fileStream);

                        // extract width and height metadata from a file type-specific directory
                        if (fileExtension == ".jpeg" || fileExtension == ".jpg")
                        {
                            var jpegDirectory = directories.OfType<JpegDirectory>().FirstOrDefault();

                            if (jpegDirectory != null)
                            {
                                uploadResult.Metadata.Width = jpegDirectory.GetImageWidth();
                                uploadResult.Metadata.Height = jpegDirectory.GetImageHeight();
                            }
                        }

                        if (fileExtension == ".png")
                        {
                            var pngDirectory = directories.OfType<PngDirectory>().FirstOrDefault();

                            if (pngDirectory != null)
                            {
                                var imageWidthString = pngDirectory.GetDescription(PngDirectory.TagImageWidth);
                                var imageHeightString = pngDirectory.GetDescription(PngDirectory.TagImageHeight);

                                uploadResult.Metadata.Width = string.IsNullOrEmpty(imageWidthString) ? 0 : int.Parse(imageWidthString);
                                uploadResult.Metadata.Height = string.IsNullOrEmpty(imageHeightString) ? 0 : int.Parse(imageHeightString);
                            }
                        }

                        if (uploadResult.Metadata.Width != 0 && uploadResult.Metadata.Height != 0)
                        {
                            _logger.LogInformation("Extracted width & height ({imageWidth}x{imageHeight}) photo metadata for '{fileName}'", uploadResult.Metadata.Width, uploadResult.Metadata.Height, file.FileName);
                        }
                        else
                        {
                            _logger.LogWarning("Failed to extract width & height photo metadata for '{fileName}'", file.FileName);
                        }

                        // extract metadata from the 'Exif IFD0' directory
                        var ifdoDirectory = directories.OfType<ExifIfd0Directory>().FirstOrDefault();

                        if (ifdoDirectory != null)
                        {
                            uploadResult.Metadata.CameraMake = ifdoDirectory?.GetDescription(ExifDirectoryBase.TagMake) ?? "";
                            uploadResult.Metadata.CameraModel = ifdoDirectory?.GetDescription(ExifDirectoryBase.TagModel) ?? "";

                            imageTitle = ifdoDirectory?.GetDescription(ExifDirectoryBase.TagWinTitle) ?? "";
                            imageSubject = ifdoDirectory?.GetDescription(ExifDirectoryBase.TagWinSubject) ?? "";
                        }

                        // extract metadata from the 'Exif SubIFD' directory
                        var subIfdDirectory = directories.OfType<ExifSubIfdDirectory>().FirstOrDefault();

                        if (subIfdDirectory != null)
                        {
                            uploadResult.Metadata.DateTaken = subIfdDirectory?.GetDescription(ExifDirectoryBase.TagDateTimeOriginal) ?? "";
                            uploadResult.Metadata.Aperture = subIfdDirectory?.GetDescription(ExifDirectoryBase.TagAperture) ?? "";
                            uploadResult.Metadata.ShutterSpeed = subIfdDirectory?.GetDescription(ExifDirectoryBase.TagShutterSpeed) ?? "";
                            uploadResult.Metadata.Iso = subIfdDirectory?.GetDescription(ExifDirectoryBase.TagIsoEquivalent) ?? "";
                            uploadResult.Metadata.FocalLength = subIfdDirectory?.GetDescription(ExifDirectoryBase.TagFocalLength) ?? "";

                            uploadResult.Metadata.LensMake = subIfdDirectory?.GetDescription(ExifDirectoryBase.TagLensMake) ?? "";
                            uploadResult.Metadata.LensModel = subIfdDirectory?.GetDescription(ExifDirectoryBase.TagLensModel) ?? "";
                        }

                        // extract metadata from the 'IPTC' directory
                        var iptcDirectory = directories.OfType<IptcDirectory>().FirstOrDefault();

                        if (iptcDirectory != null)
                        {
                            uploadResult.Metadata.Tags = iptcDirectory?.GetKeywords()?.ToList();
                        }

                        op.Complete();
                    }

                    _logger.LogInformation("Extracted photo metadata for '{fileName}' without errors", file.FileName);

                    // also update the uploadResult object with data from Azure
                    uploadResult.Uploaded = true;
                    uploadResult.StoredFileName = blob.Name;
                    uploadResult.AzureUri = blob.Uri.ToString();
                    uploadResult.ErrorCode = 0;

                    uploadResult.Title = imageTitle == "" ? file.FileName : imageTitle; // use file name if title is unavailable
                    uploadResult.Subject = imageSubject;

                    // ... and finally, add it to the List which will be returned
                    uploadResults.Add(uploadResult);

                    _logger.LogInformation("Azure Storage info and photo metadata for '{fileName}' was retrieved and added to the List of UploadResults", file.FileName);
                }
                catch (ImageProcessingException ex)
                {
                    var metadata = uploadResult.Metadata;
                    _logger.LogError("Photo metadata for '{fileName}' could not be extracted - {message}. Metadata : {@Metadata}", file.FileName, ex.Message, metadata);

                    uploadResult.Uploaded = false;
                    uploadResult.FileName = file.FileName;
                    uploadResult.ErrorCode = 1;
                    uploadResult.ErrorMessages = new List<string>() { ex.Message };
                    uploadResults.Add(uploadResult);
                }
                catch (Exception ex)
                {
                    var metadata = uploadResult.Metadata;
                    _logger.LogError("The file '{fileName}' could not be uploaded to Azure, or photo metadata could not be extracted without error(s): {message}. Metadata : {@Metadata}", file.FileName, ex.Message, metadata);

                    uploadResult.Uploaded = false;
                    uploadResult.FileName = file.FileName;
                    uploadResult.ErrorCode = 1;
                    uploadResult.ErrorMessages = new List<string>() { ex.Message };
                    uploadResults.Add(uploadResult);
                }
            }
        }

        return uploadResults;
    }

    private List<string> BasicFileChecks(IFormFile file, string permittedFileExtensions, long fileSizeLimit, int fileNameLengthLimit, string fileExtension = "unknown")
    {
        var filecheckErrors = new List<string>();

        // check the file has an extension
        if (string.IsNullOrWhiteSpace(fileExtension))
        {
            _logger.LogError("'{fileName}' does not appear to have a file extension", file.FileName);
            filecheckErrors.Add($"'{file.FileName}' does not appear to have a file extension");
        }

        // check the file type is allowed
        if (!permittedFileExtensions.Contains(fileExtension))
        {
            _logger.LogError("Upload of '{fileName}' with file type '{fileExtension}' was not allowed", file.FileName, fileExtension);
            filecheckErrors.Add($"Upload of '{file.FileName}' with file type '{fileExtension}' is not allowed");
        }

        // check file isn't 0 bytes
        if (file.Length < 1)
        {
            _logger.LogError("The file '{fileName}' had a file size of 0 bytes", file.FileName);
            filecheckErrors.Add($"'{file.FileName}' has a file size of 0 bytes");
        }

        // check the file size (in bytes) isn't above the limit
        if (file.Length > fileSizeLimit)
        {
            _logger.LogError("The size of '{fileName}' ({fileSize} bytes) was larger than the current file size limit ({sizeLimit} bytes)", file.FileName, file.Length, fileSizeLimit);
            filecheckErrors.Add($"The size of '{file.FileName}' ({file.Length} bytes) is larger than the current file size limit");
        }

        // check the file name length isn't above the limit
        if (file.FileName.Length > fileNameLengthLimit)
        {
            _logger.LogError("The name of '{fileName}' was too long at {fileNameLength} characters. The current limit is {fileNameLengthLimit} characters", file.FileName, file.FileName.Length, fileNameLengthLimit);
            filecheckErrors.Add($"The name of '{file.FileName}' was too long: {file.FileName.Length}  characters");
        }

        if (filecheckErrors.Count == 0) return null!;

        return filecheckErrors;
    }
}
