using MudBlazor;
using PhotoPortfolio.Shared.Models.Prodigi.Products;
using PhotoPortfolio.Shared.Models;
using PhotoPortfolio.Shared.Entities;

namespace PhotoPortfolio.Client.Pages.Admin;

public partial class AddProduct
{
    private string errorMessage = "";
    private string productId = "";
    private Product product = null!;
    private Product model = new();
    decimal wholesalePrice = 0.0m;

    private List<BreadcrumbCrumb> BreadcrumbCrumbs = new();

    protected override void OnInitialized()
    {
        BreadcrumbCrumbs.Add(new BreadcrumbCrumb { Id = 1, Name = "Products", Uri = "/admin/products", Enabled = true });
        BreadcrumbCrumbs.Add(new BreadcrumbCrumb { Id = 2, Name = "Add New Product", Uri = "", Enabled = false });
    }

    private async Task CreateProduct()
    {
        var productCreated = await adminService.AddProductAsync(model);

        if (productCreated is not null)
        {
            product = productCreated;
            Snackbar.Add($"This product has now been created", Severity.Success);
        }
        else
        {
            Snackbar.Add($"Sorry, there was a problem creating this product", Severity.Error);
        }
    }

    private async void ImageAssetsAdded(Product returnedProduct)
    {
        var productUpdated = await adminService.UpdateProductAsync(returnedProduct);

        if (productUpdated)
        {
            Console.WriteLine("Image assets were successfully added to the product");
        }
        else
        {
            Console.WriteLine("Image assets could NOT be added to the product");
        }
    }

    private async Task GetQuote(string sku)
    {
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
                }
            }
        }
        else
        {
            errorMessage = "Sorry, there was a problem getting a quote for this product";
        }
    }

    private async Task<ProductDetails> GetProductDetails(string sku)
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
