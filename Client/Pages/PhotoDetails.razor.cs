using Humanizer;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using MudBlazor;
using PhotoPortfolio.Client.Shared;
using PhotoPortfolio.Shared.Entities;
using PhotoPortfolio.Shared.Models;
using PhotoPortfolio.Shared.Models.Prodigi.Quotes;

namespace PhotoPortfolio.Client.Pages;

public partial class PhotoDetails
{
    [Parameter]
    public string Id { get; set; } = string.Empty;

    [CascadingParameter]
    public BasketState basketState { get; set; }

    [CascadingParameter]
    private Task<AuthenticationState>? authenticationState { get; set; }

    private Photo photo = null!;
    private string errorMessage = "";

    private List<DropdownItem> dropdownOptions = new();

    private List<PhotoProduct> products = new();
    private PhotoProduct product { get; set; }
    private Dictionary<string, string[]> productOptions = new();
    private int productOptionsRequired = 0;
    private int productOptionsChosen = 0;

    private string initialInstructionsText = "Choose an option above to see further details, including prices.";

    private bool optionSelected = false;

    private string productPrice = "";
    private bool quoteReceived = false;
    private string quoteError = "";
    private bool awaitingQuote = false;

    protected override async Task OnInitializedAsync()
    {
        await GetPhotoById();
        await sessionStorage.RemoveItemAsync("product_options");
    }

    private async Task GetPhotoById()
    {
        try
        {
            photo = await photoService.GetPhotoByIdAsync(Id);

            if (photo is not null)
            {
                if (photo.Products is not null && photo.Products.Count > 0)
                {
                    products = photo.Products;

                    foreach (PhotoProduct product in products)
                    {
                        dropdownOptions.Add(new DropdownItem
                        {
                            OptionName = product.CustomDescription,
                            OptionRef = product.ProdigiSku
                        });
                    }
                }

                errorMessage = "";
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

    private async Task SelectedOption(DropdownItem selectedOption)
    {
        string sku = selectedOption.OptionRef;
        productPrice = "";

        if (products is not null)
        {
            product = products.FirstOrDefault(p => p.ProdigiSku == sku, null!);

            if (product is not null)
            {
                optionSelected = true;
                awaitingQuote = true;

                // get a quote
                await GetProductQuote(product);

                awaitingQuote = false;
            }
            else
            {
                awaitingQuote = false;
                initialInstructionsText = "Sorry, details for this product could not be found";
            }
        }

        await CreateProductOptions(sku);
    }

    private async Task CreateProductOptions(string sku)
    {
        productOptions.Clear();

        var productDetails = await productService.GetProductDetailsAsync(sku);

        if (productDetails.Attributes is not null)
        {
            foreach (var attribute in productDetails.Attributes)
            {
                if (attribute.Value.Length > 1)
                {
                    productOptions.Add(attribute.Key, attribute.Value);
                    productOptionsRequired++;
                }
            }
        }
    }

    private async Task GetProductQuote(PhotoProduct product, string deliveryOption = "Standard")
    {
        var quoteResponse = await quoteService.GetQuote(product.ProdigiSku, null!, deliveryOption);

        if (quoteResponse is not null)
        {
            var quotes = quoteResponse.Quotes;
            var quoteReturned = quotes.FirstOrDefault();

            if (quoteReturned is not null && quoteReturned.CostSummary is not null)
            {
                if (AnyNulls(quoteReturned.CostSummary) == false)
                {
                    decimal itemsCost = 0m;
                    decimal shippingCost = 0m;
                    decimal salesTaxCost = 0m;

                    if (!string.IsNullOrWhiteSpace(quoteReturned.CostSummary.Items!.Amount))
                    {
                        itemsCost = decimal.Parse(quoteReturned.CostSummary.Items.Amount);
                        //Console.WriteLine("Items Cost: " + itemsCost.ToString("0.00"));
                    }

                    if (!string.IsNullOrWhiteSpace(quoteReturned.CostSummary.Shipping!.Amount))
                    {
                        shippingCost = decimal.Parse(quoteReturned.CostSummary.Shipping.Amount);
                        //Console.WriteLine("Shipping Cost: " + shippingCost.ToString());
                    }

                    if (!string.IsNullOrWhiteSpace(quoteReturned.CostSummary.TotalTax!.Amount))
                    {
                        salesTaxCost = decimal.Parse(quoteReturned.CostSummary.TotalTax.Amount);
                        //Console.WriteLine("Sales Tax Cost: " + salesTaxCost.ToString("0.00"));
                    }

                    decimal salesTaxMultiplier = ((100 / ((itemsCost + shippingCost) / salesTaxCost)) / 100) + 1;
                    //Console.WriteLine("Sales Tax Multiplier: " + salesTaxMultiplier.ToString("0.00"));

                    int markupPercentage = await GetMarkupPercentage(product);
                    //Console.WriteLine($"Markup Percentage: {markupPercentage}%");
                    decimal totalCost = 0m;

                    if (markupPercentage == 0)
                    {
                        totalCost = (itemsCost * salesTaxMultiplier);
                    }
                    else
                    {
                        decimal markupMultiplier = ((decimal)markupPercentage / 100) + 1;
                        //Console.WriteLine($"Markup Multiplier: {markupMultiplier}");
                        totalCost = (itemsCost * salesTaxMultiplier) * markupMultiplier;
                    }

                    //Console.WriteLine("Total Cost: " + totalCost.ToString("0.00"));

                    quoteReceived = true;
                    quoteError = "";
                    productPrice = "£" + totalCost.ToString("0.00");
                }
                else
                {
                    quoteError = "Unfortunately, we could not retrieve a price for this item at the moment";
                }
            }
            else
            {
                quoteError = "Unfortunately, we could not retrieve a price for this item at the moment";
            }
        }
        else
        {
            quoteError = "Unfortunately, we could not retrieve a price for this item at the moment";
        }
    }

    private async Task AddItemToBasket()
    {
        if (productOptionsRequired == productOptionsChosen)
        {
            var selectedProductOptions = await GetProductOptionsFromSession();

            // remove product options from session storage
            await sessionStorage.RemoveItemAsync("product_options");

            var productToAdd = new ProductBasketItemDto(product)
            {
                Id = product.Id,
                ProdigiSku = product.ProdigiSku,
                ProdigiDescription = product.ProdigiDescription,
                CustomDescription = product.CustomDescription,
                FurtherDetails = product.FurtherDetails,
                MockupImageUri = product.MockupImageUri,
                PhotoId = photo.Id,
                ImageUri = photo.Uri,
                ImageTitle = photo.Title,
                Options = selectedProductOptions
            };

            var tempPrice = productPrice.Remove(0, 1);
            decimal totalPrice = decimal.Parse(tempPrice);
            var item = new BasketItem { Product = productToAdd, Quantity = 1, Total = totalPrice };

            basketState.Basket.BasketItems.Add(item);
            await basketState.SaveChangesAsync(); // save to local storage
            basketState.BasketItemCount++;

            snackbar.Add("A new item has been added to your basket", Severity.Success);
        }
        else
        {
            snackbar.Add("Please select your product options", Severity.Error);
        }
    }

    private bool AnyNulls(CostSummary costSummary)
    {
        foreach (object prop in costSummary.GetType().GetProperties())
        {
            if (prop is null)
            {
                return true;
            }
        }

        return false;
    }

    private async Task<int> GetMarkupPercentage(PhotoProduct product)
    {
        int markupPercentage = product.MarkupPercentage;

        if (authenticationState is not null)
        {
            var authState = await authenticationState;
            var user = authState?.User;

            if (user is not null)
            {
                if (user.Identity is not null && user.Identity.IsAuthenticated)
                {
                    // Console.WriteLine($"User '{authUser}' is authenticated - no markup % applied");
                    // This application uses Azure AD with one admin user, but additionally perhaps a role could be checked
                    markupPercentage = 0;
                }

                return markupPercentage;
            }
        }

        return markupPercentage;
    }

    private static List<DropdownItem> CreateDropdownItemList(string[] options)
    {
        var dropdownItems = new List<DropdownItem>();

        foreach (var option in options)
        {
            var dropdownItem = new DropdownItem
            {
                OptionName = option.Humanize(LetterCasing.Title),
                OptionRef = option,
            };

            dropdownItems.Add(dropdownItem);
        }

        return dropdownItems;
    }

    private async Task SelectedProductOption(ProductOptionSelected selectedOption)
    {
        var productOptions = await GetProductOptionsFromSession();

        if (productOptions is null)
        {
            productOptions = new List<ProductOptionSelected>();
        }

        var existingOption = productOptions.FirstOrDefault(x => x.OptionLabel == selectedOption.OptionLabel);

        if (existingOption is not null)
        {
            productOptions.Remove(existingOption);
        }

        productOptions.Add(selectedOption);
        productOptionsChosen++;

        await sessionStorage.SetItemAsync("product_options", productOptions);
    }

    private async Task<List<ProductOptionSelected>> GetProductOptionsFromSession()
    {
        return await sessionStorage.GetItemAsync<List<ProductOptionSelected>>("product_options");
    }
}
