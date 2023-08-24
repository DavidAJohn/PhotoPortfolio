using Blazored.Modal;
using Blazored.Modal.Services;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using PhotoPortfolio.Client.Components;
using PhotoPortfolio.Shared.Entities;
using PhotoPortfolio.Shared.Models;

namespace PhotoPortfolio.Client.Pages.Admin;

public partial class EditGallery
{
    [Parameter]
    public string? Id { get; set; }

    [CascadingParameter]
    public IModalService Modal { get; set; } = null!;

    private Gallery gallery = null!;
    Gallery model = new Gallery();
    private string errorMessage = "";

    private string displayAs = "Masonry Grid";

    private List<BreadcrumbCrumb> BreadcrumbCrumbs = new();

    protected override async Task OnInitializedAsync()
    {
        await GetGalleryDetails();
    }

    protected override void OnInitialized()
    {
        BreadcrumbCrumbs.Add(new BreadcrumbCrumb { Id = 1, Name = "Galleries", Uri = "/admin/galleries", Enabled = true });
    }

    private async Task GetGalleryDetails()
    {
        if (!string.IsNullOrEmpty(Id))
        {
            try
            {
                gallery = await adminService.GetGalleryByIdAsync(Id);

                if (gallery is null)
                {
                    errorMessage = "Could not retrieve gallery details";
                }
                else
                {
                    model = gallery;
                    errorMessage = "";
                    BreadcrumbCrumbs.Add(new BreadcrumbCrumb { Id = 2, Name = gallery.Name + " Gallery", Uri = "", Enabled = false });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                errorMessage = "Could not retrieve gallery details";
            }
        }
        else
        {
            errorMessage = "Could not retrieve gallery details";
        }
    }

    private async Task OnValidSubmit()
    {
        var galleryUpdated = await adminService.UpdateGalleryAsync(gallery);

        if (galleryUpdated)
        {
            Snackbar.Add($"The gallery has now been updated", Severity.Success);
        }
        else
        {
            Snackbar.Add($"Sorry, there was a problem updating the gallery", Severity.Error);
        }
    }

    private async Task HeaderImageChange(Gallery updatedGallery)
    {
        gallery = updatedGallery;
        await InvokeAsync(StateHasChanged);
    }

    private async Task ShowPhoto(string photoId)
    {
        var parameters = new ModalParameters();
        parameters.Add(nameof(LightboxModal.PhotoId), photoId);

        LightboxConfirmOptions editImageOptions = new LightboxConfirmOptions()
        {
            ShowButton = true,
            ButtonText = "Edit image details",
            ButtonAction = LightboxConfirmAction.Navigate,
            NavigateUri = "/admin/photos/edit/" + photoId
        };

        var headerImageText = "Use as cover image";

        LightboxConfirmOptions headerImageOptions = new LightboxConfirmOptions()
        {
            ShowButton = true,
            ButtonText = headerImageText,
            ButtonAction = LightboxConfirmAction.CloseOnly
        };

        List<LightboxConfirmOptions> confirmOptions = new();
        confirmOptions.Add(editImageOptions);
        confirmOptions.Add(headerImageOptions);

        parameters.Add(nameof(LightboxModal.LightboxConfirmOptions), confirmOptions);

        var options = new ModalOptions()
        {
            Animation = ModalAnimation.FadeInOut(0.5),
            Class = "custom-modal-container"
        };

        var lightboxModal = Modal.Show<LightboxModal>("", parameters, options);
        var result = await lightboxModal.Result;

        if (!result.Cancelled)
        {
            var data = (ModalConfirmResult)result.Data;
            var photo = data.Photo;
            var button = data.Button;

            if (button.ButtonText == headerImageText)
            {
                await UpdateHeaderImage(photo);
            }
        }
    }

    private async Task UpdateHeaderImage(Photo photo)
    {
        if (photo.Uri != null)
        {
            gallery.HeaderImage = photo.Uri;

            var response = await adminService.UpdateGalleryAsync(gallery);

            if (response)
            {
                Snackbar.Add($"The header image has been changed", Severity.Success);
            }
            else
            {
                Snackbar.Add($"The header image could NOT be updated", Severity.Error);
            }
        }
        else
        {
            Snackbar.Add($"The header image could NOT be updated", Severity.Error);
        }
    }

    public async Task GetGalleryPhotos(string galleryId)
    {
        if (!string.IsNullOrEmpty(galleryId))
        {
            try
            {
                gallery = await adminService.GetGalleryByIdAsync(galleryId);

                if (gallery is null)
                {
                    errorMessage = "Could not retrieve gallery details";
                }
                else
                {
                    model = gallery;
                    errorMessage = "";
                    await InvokeAsync(StateHasChanged);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                errorMessage = "Could not retrieve gallery details";
            }
        }
        else
        {
            errorMessage = "Could not retrieve gallery details";
        }
    }
}
