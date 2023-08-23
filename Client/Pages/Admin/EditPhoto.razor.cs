using Microsoft.AspNetCore.Components;
using MudBlazor;
using PhotoPortfolio.Client.Components;
using PhotoPortfolio.Shared.Entities;
using PhotoPortfolio.Shared.Models;
using ProdigiProducts = PhotoPortfolio.Shared.Models.Prodigi.Products;
using ProdigiQuotes = PhotoPortfolio.Shared.Models.Prodigi.Quotes;

namespace PhotoPortfolio.Client.Pages.Admin;

public partial class EditPhoto
{
    [Parameter]
    public string? Id { get; set; }

    Photo photo = new Photo();
    private string errorMessage = "";

    private bool addDialogVisible = false;
    private bool editDialogVisible = false;
    private int rating;
    private DialogOptions dialogOptions = new() { FullWidth = true };
    private List<Product> products = null!;
    private bool dropdownOpen = false;
    private List<DropdownItem> dropdownOptions = new();

    CreatePhotoProductDto model = new();
    Product selectedProduct = new();
    PhotoProduct editedProduct { get; set; }
    int editedMarkupPercentage = 100;
    private string AddProductFormId = "addProductFormId";
    private string EditProductFormId = "editProductFormId";
    private bool addFormIsValid => selectedProduct.Id != null ? true : false;
    decimal wholesalePrice = 0.0m;
    decimal addWholesalePrice = 0.0m;

    private List<BreadcrumbCrumb> BreadcrumbCrumbs = new();

    protected override async Task OnInitializedAsync()
    {
        await GetPhotoById();
    }

    protected override void OnInitialized()
    {
        BreadcrumbCrumbs.Add(new BreadcrumbCrumb { Id = 1, Name = "Galleries", Uri = "/admin/galleries", Enabled = true });
    }

    private async Task GetPhotoById()
    {
        if (!string.IsNullOrEmpty(Id))
        {
            try
            {
                photo = await photoService.GetPhotoByIdAsync(Id);

                if (photo is not null)
                {
                    errorMessage = "";
                    BreadcrumbCrumbs.Add(new BreadcrumbCrumb { Id = 2, Name = "Edit Gallery", Uri = $"/admin/galleries/edit/{photo.GalleryId}", Enabled = true });
                    BreadcrumbCrumbs.Add(new BreadcrumbCrumb { Id = 3, Name = photo.Title ?? photo.FileName, Uri = "", Enabled = false });
                }
                else
                {
                    errorMessage = "Could not find details for this photo";
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                errorMessage = "Could not find this photo";
            }
        }
        else
        {
            errorMessage = "Could not find this photo";
        }
    }

    private async Task GetAvailableProducts()
    {
        try
        {
            var allProducts = await adminService.GetProductsAsync();

            if (photo.Products is not null && photo.Products.Count > 0)
            {
                // exclude any products already added to the photo
                products = allProducts.Where(p => photo.Products.All(pp => pp.ProdigiSku != p.ProdigiSku)).ToList();
            }
            else
            {
                products = allProducts;
            }

            dropdownOptions.Clear();

            // convert the products into a list of DropdownItems
            foreach (Product product in products)
            {
                dropdownOptions.Add(new DropdownItem
                {
                    OptionName = product.ProdigiSku + " - " + product.ProdigiDescription,
                    OptionRef = product.ProdigiSku
                });
            }

            errorMessage = "";
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            errorMessage = "Could not retrieve list of products";
        }
    }

    private async Task OpenAddProductDialog()
    {
        addDialogVisible = true;

        // clear the form
        selectedProduct = new();
        model = new();

        await GetAvailableProducts();
    }

    private async Task OpenEditProductDialog(PhotoProduct product)
    {
        editDialogVisible = true;

        editedProduct = product;
        editedMarkupPercentage = product.MarkupPercentage;

        var productDetails = await GetProductDetails(product.ProdigiSku);

        if (productDetails is not null)
        {
            editedProduct.ProdigiDescription = productDetails.Description;
        }

        var productQuote = await GetProductQuote(product);

        if (productQuote is not null && productQuote.CostSummary is not null)
        {
            if (!string.IsNullOrWhiteSpace(productQuote.CostSummary.TotalCost!.Amount))
            {
                wholesalePrice = decimal.Parse(productQuote.CostSummary.TotalCost.Amount);
            }
        }
    }

    void Close()
    {
        addDialogVisible = false;
        editDialogVisible = false;
    }

    private async void SelectedOption(DropdownItem selectedOption)
    {
        string sku = selectedOption.OptionRef;
        selectedProduct = products.FirstOrDefault(p => p.ProdigiSku == sku) ?? null!;

        model.ProdigiDescription = selectedProduct.ProdigiDescription;
        model.DefaultMarkupPercentage = selectedProduct.DefaultMarkupPercentage;

        var quoteResponse = await quoteService.GetQuote(selectedProduct.ProdigiSku);

        if (quoteResponse is not null)
        {
            var quotes = quoteResponse.Quotes;
            var quoteReturned = quotes.FirstOrDefault();

            if (quoteReturned is not null && quoteReturned.CostSummary is not null)
            {
                if (!string.IsNullOrWhiteSpace(quoteReturned.CostSummary.TotalCost!.Amount))
                {
                    addWholesalePrice = decimal.Parse(quoteReturned.CostSummary.TotalCost.Amount);
                }
            }
        }
    }

    private async Task OnValidSubmit()
    {
        var productToAdd = new PhotoProduct(selectedProduct)
        {
            ProdigiSku = selectedProduct.ProdigiSku,
            ProdigiDescription = selectedProduct.ProdigiDescription,
            ProdigiImageAssetUris = selectedProduct.ProdigiImageAssetUris,
            CustomDescription = model.CustomDescription!,
            FurtherDetails = model.FurtherDetails!,
            MockupImageUri = "",
            MarkupPercentage = model.MarkupPercentage
        };

        var productToAddList = new List<PhotoProduct>()
        {
            productToAdd
        };

        photo.Products = photo.Products?.Concat(productToAddList).ToList();

        var photoUpdated = await adminService.UpdatePhotoAsync(photo);

        if (photoUpdated)
        {
            addDialogVisible = false;
            Snackbar.Add($"'{productToAdd!.ProdigiSku}' has now been added to this photo", Severity.Success);
        }
        else
        {
            Snackbar.Add("Sorry, there was a problem adding this product to the photo", Severity.Error);
        }
    }

    private async Task DeleteProduct(PhotoProduct product)
    {
        var parameters = new DialogParameters();
        parameters.Add("ContentText", "Are you sure you want to remove this product from this photo?");
        parameters.Add("ButtonText", "Yes, Delete It");
        parameters.Add("Color", Color.Error);

        var options = new DialogOptions() { CloseButton = true, MaxWidth = MaxWidth.ExtraSmall };

        var dialog = DialogService.Show<ConfirmDialog>("Delete", parameters, options);
        var result = await dialog.Result;

        if (!result.Cancelled)
        {
            photo.Products?.Remove(product);

            var photoUpdated = await adminService.UpdatePhotoAsync(photo);

            if (photoUpdated)
            {
                addDialogVisible = false;
                Snackbar.Add($"'{product.ProdigiSku}' has now been removed from this photo", Severity.Success);
            }
            else
            {
                Snackbar.Add("Sorry, there was a problem removing this product from the photo", Severity.Error);
            }
        }
    }

    private async Task EditProduct(PhotoProduct editedProduct, int markupPercentage)
    {
        var productToUpdate = photo.Products.FirstOrDefault(p => p.ProdigiSku == editedProduct.ProdigiSku) ?? null!;
        productToUpdate.CustomDescription = editedProduct.CustomDescription;
        productToUpdate.MarkupPercentage = markupPercentage;

        if (string.IsNullOrEmpty(productToUpdate.Id))
        {
            productToUpdate.Id = null!; // photo update does not work if Id == "" - it must be null
        }

        var photoUpdated = await adminService.UpdatePhotoAsync(photo);

        if (photoUpdated)
        {
            editDialogVisible = false;
            Snackbar.Add($"'{editedProduct.ProdigiSku}' has now been updated for this photo", Severity.Success);
        }
        else
        {
            Snackbar.Add("Sorry, there was a problem editing this product", Severity.Error);
        }
    }

    private async Task<ProdigiQuotes.Quote> GetProductQuote(PhotoProduct product, string deliveryOption = "Standard")
    {
        wholesalePrice = 0.0m;

        var quoteResponse = await quoteService.GetQuote(product.ProdigiSku);

        if (quoteResponse is not null)
        {
            var quotes = quoteResponse.Quotes;
            var quoteReturned = quotes.FirstOrDefault();

            if (quoteReturned is not null) return quoteReturned;
            else
            {
                errorMessage = "Sorry, there was a problem getting a quote for this product";
                return null!;
            }
        }
        else
        {
            errorMessage = "Sorry, there was a problem getting a quote for this product";
            return null!;
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
