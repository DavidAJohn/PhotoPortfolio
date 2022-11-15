﻿using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using MetadataExtractor;
using MetadataExtractor.Formats.Exif;
using MetadataExtractor.Formats.Iptc;
using MetadataExtractor.Formats.Jpeg;
using MetadataExtractor.Formats.Png;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PhotoPortfolio.Shared.Entities;
using PhotoPortfolio.Shared.Models;
using System.Web;

namespace PhotoPortfolio.Server.Controllers;

[Authorize]
public class AdminController : BaseApiController
{
    private readonly IGalleryRepository _repository;
    private readonly IConfiguration _config;
    private readonly ILogger<AdminController> _logger;

    public AdminController(IGalleryRepository repository, IConfiguration config, ILogger<AdminController> logger)
    {
        _repository = repository;
        _config = config;
        _logger = logger;
    }

    [HttpGet("galleries")]
    public async Task<List<Gallery>> GetAllGalleries()
    {
        return await _repository.GetAllAsync();
    }

    [HttpGet("galleries/{id:length(24)}")]
    public async Task<IActionResult> GetGalleryById(string id)
    {
        var gallery = await _repository.GetGalleryWithPhotos(id, true);

        if (gallery == null)
        {
            return NotFound();
        }

        return Ok(gallery);
    }

    [HttpPost("galleries")]
    public async Task<IActionResult> AddGallery(Gallery gallery)
    {
        await _repository.AddAsync(gallery);

        return CreatedAtAction(nameof(GetAllGalleries), new { id = gallery.Id }, gallery);
    }

    [HttpPut("galleries/{id:length(24)}")]
    public async Task<IActionResult> UpdateGallery(string id, Gallery gallery)
    {
        var galleryToUpdate = await _repository.GetSingleAsync(x => x.Id == id);

        if (galleryToUpdate is null)
        {
            return NotFound();
        }

        gallery.Id = galleryToUpdate.Id;

        await _repository.UpdateAsync(gallery);

        return NoContent();
    }

    [HttpDelete("galleries/{id:length(24)}")]
    public async Task<IActionResult> DeleteGallery(string id)
    {
        var galleryToDelete = await _repository.GetSingleAsync(x => x.Id == id);

        if (galleryToDelete is null)
        {
            return NotFound();
        }

        await _repository.DeleteAsync(id);

        return NoContent();
    }

    [HttpPost("uploads")]
    public async Task<ActionResult<IList<UploadResult>>> UploadFiles([FromForm] IEnumerable<IFormFile> files)
    {
        var azureConnectionString = _config["AzureUpload:AzureStorageConnectionString"];
        var azureContainerUri = _config["AzureUpload:AzureContainerUri"];
        var azureContainerName = _config["AzureUpload:AzureContainerName"];
        var permittedFileExtensions = _config["AzureUpload:FileUploadTypesAllowed"];
        var fileSizeLimit = _config.GetValue<long>("AzureUpload:MaxFileUploadSize");

        var imageTitle = "";
        var imageSubject = "";

        List<UploadResult> uploadResults = new();

        foreach (var file in files)
        {
            var uploadResult = new UploadResult();
            var untrustedFileName = file.FileName;
            uploadResult.FileName = untrustedFileName;
            string trustedFileNameForStorage = "";

            var fileExtension = Path.GetExtension(file.FileName.ToLowerInvariant());

            // check the file type is allowed
            if (!permittedFileExtensions.Contains(fileExtension) && fileExtension != "")
            {
                _logger.LogError("The file type '" + fileExtension + "' is not allowed");
                return BadRequest("The file type '" + fileExtension + "' is not allowed");
            }

            // check the file size (in bytes)
            if (file.Length > fileSizeLimit)
            {
                _logger.LogError("The file size of " + file.Length + " bytes was too large");
                return BadRequest("The file size of " + file.Length + " bytes was too large");
            }

            // check the file name length isn't excessive
            if (file.FileName.Length > 75)
            {
                _logger.LogError("The file name is too long: " + file.FileName.Length + " characters");
                return BadRequest("The file name is too long: " + file.FileName.Length + " characters");
            }

            // check container name isn't empty
            if (azureContainerName.Length == 0)
            {
                _logger.LogError("Azure container name is empty");
                return BadRequest("Azure container name is empty");
            }

            if (file.Length > 0)
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

                    // generate a unique upload file name
                    // [original_filename_without_extension]_[8_random_chars].[original_filename_extension]
                    // eg. filename_xgh38tye.jpg
                    trustedFileNameForStorage = HttpUtility.HtmlEncode(Path.GetFileNameWithoutExtension(file.FileName)) +
                        "_" + Path.GetRandomFileName().Substring(0, 8) + Path.GetExtension(file.FileName);

                    var blob = azureContainer.GetBlobClient(trustedFileNameForStorage);
                    //await blob.DeleteIfExistsAsync(DeleteSnapshotsOption.IncludeSnapshots);

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

                    using (var fileStream = file.OpenReadStream())
                    {
                        await blob.UploadAsync(fileStream, blobHttpHeader);
                    }

                    // extract image metadata - need to use a new stream
                    using (var fileStream = file.OpenReadStream())
                    {
                        IEnumerable<MetadataExtractor.Directory> directories = ImageMetadataReader.ReadMetadata(fileStream);

                        var imageWidth = 0;
                        var imageHeight = 0;

                        if (fileExtension == ".jpeg" || fileExtension == ".jpg")
                        {
                            var jpegDirectory = directories.OfType<JpegDirectory>().FirstOrDefault();

                            if (jpegDirectory != null)
                            {
                                imageWidth = jpegDirectory.GetImageWidth();
                                imageHeight = jpegDirectory.GetImageHeight();
                            }
                        }

                        if (fileExtension == ".png")
                        {
                            var pngDirectory = directories.OfType<PngDirectory>().FirstOrDefault();

                            if (pngDirectory != null)
                            {
                                imageWidth = pngDirectory.GetInt32(ExifDirectoryBase.TagImageWidth);
                                imageHeight = pngDirectory.GetInt32(ExifDirectoryBase.TagImageHeight);
                            }
                        }

                        var ifdoDirectory = directories.OfType<ExifIfd0Directory>().FirstOrDefault();
                        var make = ifdoDirectory?.GetDescription(ExifDirectoryBase.TagMake);
                        var model = ifdoDirectory?.GetDescription(ExifDirectoryBase.TagModel);

                        imageTitle = ifdoDirectory?.GetDescription(ExifDirectoryBase.TagWinTitle);
                        imageSubject = ifdoDirectory?.GetDescription(ExifDirectoryBase.TagWinSubject);

                        var subIfdDirectory = directories.OfType<ExifSubIfdDirectory>().FirstOrDefault();
                        var dateTaken = subIfdDirectory?.GetDescription(ExifDirectoryBase.TagDateTimeOriginal);
                        var aperture = subIfdDirectory?.GetDescription(ExifDirectoryBase.TagAperture);
                        var shutterSpeed = subIfdDirectory?.GetDescription(ExifDirectoryBase.TagShutterSpeed);
                        var iso = subIfdDirectory?.GetDescription(ExifDirectoryBase.TagIsoEquivalent);
                        var focalLength = subIfdDirectory?.GetDescription(ExifDirectoryBase.TagFocalLength);

                        var iptcDirectory = directories.OfType<IptcDirectory>().FirstOrDefault();
                        var tags = iptcDirectory?.GetKeywords();

                        // add the extracted metadata to the UploadResult object for this file
                        uploadResult.Metadata = new PhotoMetadata()
                        {
                            Width = imageWidth,
                            Height = imageHeight,
                            Camera = make + " " + model,
                            ShutterSpeed = shutterSpeed,
                            Aperture = aperture,
                            Iso = iso,
                            FocalLength = focalLength,
                            DateTaken = dateTaken,
                            Tags = tags?.ToList()
                        };
                    }

                    // update the UploadRequest object with data from Azure
                    uploadResult.Uploaded = true;
                    uploadResult.StoredFileName = blob.Name;
                    uploadResult.AzureUri = blob.Uri.ToString();
                    uploadResult.ErrorCode = 0;
                    uploadResult.Title = imageTitle;
                    uploadResult.Subject = imageSubject;

                    // ... and add it to the List which will be returned
                    uploadResults.Add(uploadResult);
                }
                catch (Exception ex)
                {
                    _logger.LogError("The file '{fileName}' could not be uploaded: {message}", file.Name, ex.Message);

                    uploadResult.Uploaded = false;
                    uploadResult.ErrorCode = 1;

                    uploadResults.Add(uploadResult);
                }
            }
        }

        return new CreatedResult(azureContainerUri, uploadResults);
    }
}
