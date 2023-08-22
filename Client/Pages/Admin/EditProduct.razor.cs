using Microsoft.AspNetCore.Components;
using MudBlazor;
using PhotoPortfolio.Client.Components;
using PhotoPortfolio.Shared.Entities;
using PhotoPortfolio.Shared.Models;
using ProdigiProducts = PhotoPortfolio.Shared.Models.Prodigi.Products;

namespace PhotoPortfolio.Client.Pages.Admin;

public partial class EditProduct
{
    [Parameter]
    public string? Id { get; set; }

    Product product = new();
    private string errorMessage = "";
    private Product model = new();
    private bool quoteRequested = false;
    decimal wholesalePrice = 0.0m;

    private List<BreadcrumbCrumb> BreadcrumbCrumbs = new();

    protected override async Task OnInitializedAsync()
    {
        await GetProductById();
    }

    protected override void OnInitialized()
    {
        BreadcrumbCrumbs.Add(new BreadcrumbCrumb { Id = 1, Name = "Products", Uri = "/admin/products", Enabled = true });
    }

    private async Task GetProductById()
    {
        if (!string.IsNullOrEmpty(Id))
        {
            try
            {
                product = await adminService.GetProductByIdAsync(Id);

                if (product is not null)
                {
                    model = product;
                    errorMessage = "";
                    BreadcrumbCrumbs.Add(new BreadcrumbCrumb { Id = 2, Name = product.ProdigiSku, Uri = "", Enabled = false });
                }
                else
                {
                    errorMessage = "Could not find details for this product";
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                errorMessage = "Could not find this product";
            }
        }
        else
        {
            errorMessage = "Could not find this product";
        }
    }

    private async Task UpdateProduct()
    {
        var productUpdated = await adminService.UpdateProductAsync(model);

        if (productUpdated)
        {
            Snackbar.Add("This product has now been updated", Severity.Success);
        }
        else
        {
            Snackbar.Add("Sorry, there was a problem updating this product", Severity.Error);
        }
    }

    private async void ImageAssetsAdded(Product returnedProduct)
    {
        var productUpdated = await adminService.UpdateProductAsync(returnedProduct);

        if (productUpdated)
        {
            //Snackbar.Add("The image(s) were successfully uploaded and added to the product", Severity.Success);
        }
        else
        {
            Snackbar.Add("The image(s) could NOT be added to the product", Severity.Error);
        }
    }

    private async void DeleteImage(string imageAssetUri)
    {
        // first, get confirmation via a dialog
        var parameters = new DialogParameters();
        parameters.Add("ContentText", "Do you really want to delete this image? This process cannot be undone.");
        parameters.Add("ButtonText", "Delete");
        parameters.Add("Color", Color.Error);

        var options = new DialogOptions() { CloseButton = true, MaxWidth = MaxWidth.ExtraSmall };

        var dialog = DialogService.Show<ConfirmDialog>("Delete", parameters, options);
        var result = await dialog.Result;

        // user confirmed, proceed with the deletion
        if (!result.Cancelled) // user must have clicked confirm button
        {
            var imageToRemove = product.ProdigiImageAssetUris?.SingleOrDefault(i => i == imageAssetUri);

            if (imageToRemove is not null)
            {
                var imageRemoved = product.ProdigiImageAssetUris.Remove(imageToRemove);
                var productUpdated = await adminService.UpdateProductAsync(product);

                if (productUpdated)
                {
                    model = product;
                    await InvokeAsync(StateHasChanged);
                    Snackbar.Add($"That image was deleted from the product", Severity.Success);
                }
                else
                {
                    Snackbar.Add("The image could NOT be deleted", Severity.Error);
                }
            }
        }
    }

    private async Task GetQuote(string sku)
    {
        quoteRequested = true;

        var productDetails = await GetProductDetails(sku);

        if (productDetails is not null)
        {
            model.ProdigiDescription = productDetails.Description;
        }

        var quoteResponse = await quoteService.GetQuote(sku);

        if (quoteResponse is not null)
        {
            var quotes = quoteResponse.Quotes;
            var quoteReturned = quotes.FirstOrDefault();

            if (quoteReturned is not null && quoteReturned.CostSummary is not null)
            {
                if (!string.IsNullOrWhiteSpace(quoteReturned.CostSummary.TotalCost!.Amount))
                {
                    wholesalePrice = decimal.Parse(quoteReturned.CostSummary.TotalCost.Amount);
                    quoteRequested = false;
                }
            }
        }
        else
        {
            errorMessage = "Sorry, there was a problem getting a quote for this product";
        }
    }

    private async Task<ProdigiProducts.ProductDetails> GetProductDetails(string sku)
    {
        var productDetails = await productService.GetProductDetailsAsync(sku);

        if (productDetails is not null) return productDetails;
        else
        {
            errorMessage = "Sorry, there was a problem getting details for this product";
            return null!;
        }
    }
}
