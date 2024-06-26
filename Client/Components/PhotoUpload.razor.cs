﻿using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using PhotoPortfolio.Shared.Entities;
using PhotoPortfolio.Shared.Models;
using System.Net.Http.Headers;
using PhotoPortfolio.Client.Helpers;

namespace PhotoPortfolio.Client.Components;

public partial class PhotoUpload
{
    [Parameter]
    public ImageUploadType UploadType { get; set; } = ImageUploadType.Gallery;

    [Parameter]
    public string? GalleryId { get; set; }

    [Parameter]
    public Product? Product { get; set; }

    [Parameter]
    public EventCallback<string> PhotoUpdate { get; set; }

    [Parameter]
    public EventCallback<Product> ProductUpdate { get; set; }

    private static string DefaultDragClass = "relative rounded-lg border-2 border-dashed pa-4 mt-4 mud-width-full mud-height-full ";
    private string DragClass = DefaultDragClass;
    private List<IBrowserFile> validatedFiles = new();
    private List<UploadResult> uploadResults = new();
    private List<string> fileNames = new();
    private List<string> uploadFailures = new();

    private bool uploadsEnabled = true;

    private int maxFileUploadSize;
    private string fileExtensionsAllowed = string.Empty;
    private string fileContentTypesAllowed = string.Empty;
    private string[]? extensionsArray;
    private string[]? contentTypesArray;
    private int fileNameTruncateAt;
    private int fileUploadCount;

    protected override void OnInitialized()
    {
        maxFileUploadSize = int.TryParse(config["Uploads:MaxFileUploadSize"], out _)
            ? int.Parse(config["Uploads:MaxFileUploadSize"])
            : 2097152; // 2Mb default

        fileNameTruncateAt = int.TryParse(config["Uploads:FileNameTruncateAt"], out _)
            ? int.Parse(config["Uploads:FileNameTruncateAt"])
            : 48;

        fileUploadCount = int.TryParse(config["Uploads:FileUploadCount"], out _)
            ? int.Parse(config["Uploads:FileUploadCount"])
            : 1;

        fileExtensionsAllowed = config["Uploads:FileExtensionsAllowed"];
        fileContentTypesAllowed = config["Uploads:FileContentTypesAllowed"];

        // uploadable file & content types must be provided in wwwroot/appsettings: 
        // eg. ".jpg,.png" & "image/jpeg,image/png"
        if (string.IsNullOrEmpty(fileExtensionsAllowed) || string.IsNullOrEmpty(fileContentTypesAllowed))
        {
            uploadsEnabled = false;
        }
        else
        {
            extensionsArray = fileExtensionsAllowed.Split(",");
            contentTypesArray = fileContentTypesAllowed.Split(",");
        }
    }

    private void OnInputFileChanged(InputFileChangeEventArgs e)
    {
        ClearDragClass();

        if (e.FileCount <= fileUploadCount)
        {
            var files = e.GetMultipleFiles();

            foreach (var file in files)
            {
                if (contentTypesArray is null || !contentTypesArray.Contains(file.ContentType))
                {
                    if (extensionsArray is not null)
                    {
                        Snackbar.Add($"Only these file types can be uploaded: '{string.Join(", ", extensionsArray)}' - '{file.Name}' was excluded", Severity.Warning);
                    }
                    else
                    {
                        Snackbar.Add($"'{file.Name}' cannot be uploaded because of its file type", Severity.Warning);
                    }
                }
                else
                {
                    if (file.Size > maxFileUploadSize)
                    {
                        Snackbar.Add($"'{file.Name}' is too large. The limit is {maxFileUploadSize / 1048576}Mb", Severity.Warning);
                    }
                    else
                    {
                        validatedFiles.Add(file);
                        fileNames.Add(file.Name.Truncate(fileNameTruncateAt));
                    }
                }
            }
        }
        else
        {
            Snackbar.Add($"Only {fileUploadCount} {(fileUploadCount == 1 ? "photo" : "photos")} at a time can be uploaded", Severity.Error);
        }
    }

    private async Task Clear()
    {
        fileNames.Clear();
        validatedFiles.Clear();
        uploadFailures.Clear();
        ClearDragClass();
        await Task.Delay(100);
    }

    private void RemoveFile(MudChip chip)
    {
        fileNames.Remove(chip.Text);

        var index = validatedFiles.FindIndex(x => x.Name.Truncate(fileNameTruncateAt) == chip.Text);

        if (index != -1)
        {
            validatedFiles.RemoveAt(index);
        }

        uploadFailures.Remove(chip.Text);
    }

    private void RemoveFile(string filename)
    {
        fileNames.Remove(filename);

        var index = validatedFiles.FindIndex(x => x.Name == filename);

        if (index != -1)
        {
            validatedFiles.RemoveAt(index);
        }

        uploadFailures.Remove(filename);
    }

    private void HighlightFile(string filename)
    {
        // effectively generates a re-render of the file list in the ui, 
        // by removing and re-adding the file name, thus changing the fileNames list
        fileNames.Remove(filename.Truncate(fileNameTruncateAt));
        fileNames.Add(filename.Truncate(fileNameTruncateAt));
    }

    private async Task Upload()
    {
        var upload = false;
        using var content = new MultipartFormDataContent();
        uploadFailures.Clear();

        foreach (var file in validatedFiles)
        {
            try
            {
                var fileContent = new StreamContent(file.OpenReadStream(maxFileUploadSize));

                fileContent.Headers.ContentType = new MediaTypeHeaderValue(file.ContentType);

                content.Add(
                    content: fileContent,
                    name: "\"files\"",
                    fileName: file.Name);

                upload = true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);

                uploadResults.Add(new()
                {
                    FileName = file.Name,
                    ErrorCode = UploadErrorCode.UnexpectedError,
                    Uploaded = false
                });
            }
        }

        if (upload)
        {
            var uploadResults = await adminService.UploadPhotos(content);

            if (uploadResults is not null)
            {
                foreach (var result in uploadResults)
                {
                    if (result.Uploaded)
                    {
                        switch (UploadType)
                        {
                            case ImageUploadType.Gallery:
                                await SaveGalleryUpload(result);
                                break;

                            case ImageUploadType.Product:
                                await SaveProductUpload(result);
                                break;

                            default:
                                break;
                        };
                    }
                    else
                    {
                        uploadFailures.Add(result.FileName?.Truncate(fileNameTruncateAt)); //add to list of failed uploads to allow ui to highlight it
                        HighlightFile(result.FileName); // effectively forces a re-render of the file list in the ui
                        Snackbar.Add($"There was a problem uploading '{result.FileName}'", Severity.Error);
                    }
                }
            }
            else
            {
                Snackbar.Add($"There was a problem uploading these files", Severity.Error);
            }
        }
        else
        {
            Snackbar.Add("There was a problem uploading these files", Severity.Error);
        }
    }

    private async Task SaveGalleryUpload(UploadResult result)
    {
        // add each successful upload to the photo collection in the database

        Photo photo = new Photo()
        {
            Title = result.Title ?? "",
            Caption = result.Subject,
            FileName = result.FileName ?? "",
            Uri = result.AzureUri,
            GalleryId = GalleryId,
            Metadata = result.Metadata
        };

        var newPhoto = await adminService.AddPhotoAsync(photo);

        if (newPhoto)
        {
            await PhotoUpdate.InvokeAsync(GalleryId);
            RemoveFile(photo.FileName); // successful uploads will disappear from the upload list
            uploadFailures.Remove(photo.FileName); // in case of previous failure
            Console.WriteLine($"'{result.FileName}' was uploaded to Azure and added to the database successfully");
            Snackbar.Add($"'{result.FileName}' has been added to the gallery", Severity.Success);
        }
        else
        {
            uploadFailures.Add(photo.FileName.Truncate(fileNameTruncateAt)); //add to list of failed uploads to allow ui to highlight it
            HighlightFile(photo.FileName); // effectively forces a re-render of the file list in the ui
            Console.WriteLine($"'{result.FileName}' was uploaded to Azure, but NOT added to the database");
            Snackbar.Add($"There was a problem adding '{result.FileName}' to the database", Severity.Error);
        }
    }

    private async Task SaveProductUpload(UploadResult result)
    {
        // add each successful upload to the ProdigiImageAssetUris property of the supplied product

        Product?.ProdigiImageAssetUris.Add(result.AzureUri);
        await ProductUpdate.InvokeAsync(Product); // now return the updated product via an event callback

        RemoveFile(result.FileName); // successful uploads will disappear from the upload list
        uploadFailures.Remove(result.FileName); // in case of previous failure
        Console.WriteLine($"'{result.FileName}' was uploaded to Azure");
        Snackbar.Add($"'{result.FileName}' has been added to the product", Severity.Success);
    }

    private void SetDragClass()
    {
        DragClass = $"{DefaultDragClass} mud-border-primary";
    }

    private void ClearDragClass()
    {
        DragClass = DefaultDragClass;
    }
}
