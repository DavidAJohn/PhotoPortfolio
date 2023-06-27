using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components;
using PhotoPortfolio.Client.Shared;
using PhotoPortfolio.Shared.Models;
using PhotoPortfolio.Shared.Models.Prodigi.Quotes;

namespace PhotoPortfolio.Client.Pages;

public partial class Checkout
{
    [CascadingParameter]
    public BasketState basketState { get; set; }

    [CascadingParameter]
    private Task<AuthenticationState>? authenticationState { get; set; }

    private List<BasketItem> basketItems { get; set; }

    private List<Quote> quotes { get; set; }
    private bool multipleDeliveryOptions = false;

    private string SubmitSpinnerHidden = "hidden";
    private string DeliverySpinnerVisible = "invisible";
    private bool ShowErrors = false;
    private string ErrorMessage = "";

    private List<DropdownItem> deliveryDropdownOptions = new();
    private string selectedDeliveryOption = "Standard";

    protected override async Task OnInitializedAsync()
    {
        await Task.Run(() => Thread.Sleep(1)); // forces the page to wait before getting the basket
        basketItems = basketState.Basket.BasketItems;

        await GetBasketQuotes();
    }

    private async Task GetBasketQuotes(string deliveryOption = "")
    {
        var quoteResponse = await quoteService.GetQuote(null!, basketItems, deliveryOption);

        if (quoteResponse is not null && quoteResponse.Outcome.ToLowerInvariant() == "created")
        {
            quotes = quoteResponse.Quotes;

            CheckForMultipleDeliveryOptions();

            if (multipleDeliveryOptions)
            {
                var deliveryOptions = GetDeliveryOptions();
                AddDeliveryOptions(deliveryOptions);
            }

            var quote = GetDeliveryOption("Standard");

            if (quote != null)
            {
                basketState.Basket.ShippingCost = decimal.Parse(quote.CostSummary!.Shipping!.Amount);
            }
        }
    }

    private void AddDeliveryOptions(List<string> deliveryOptions)
    {
        deliveryDropdownOptions.Clear();

        foreach (var option in deliveryOptions)
        {
            deliveryDropdownOptions.Add(new DropdownItem
            {
                OptionName = $"{option} Delivery",
                OptionRef = option
            });
        }
    }

    private async Task DeleteItem(BasketItem item)
    {
        basketState.Basket.BasketItems.Remove(basketState.Basket.BasketItems.FirstOrDefault(
                i => i.Product.CustomDescription == item.Product.CustomDescription &&
                i.Quantity == item.Quantity
            ));

        basketState.BasketItemCount--;

        if (basketState.BasketItemCount > 0)
        {
            await GetBasketQuotes(selectedDeliveryOption);
        }
        else
        {
            basketState.Basket.ShippingCost = 0;
        }

        await basketState.SaveChangesAsync(); // save to local storage
    }

    private async Task PlaceOrder()
    {
        SubmitSpinnerHidden = "";

        OrderBasketDto orderBasketDto = new OrderBasketDto()
        {
            BasketItems = basketItems,
            ShippingMethod = selectedDeliveryOption,
            ShippingCost = basketState.Basket.ShippingCost
        };

        // save initial order to db with items and shipping method
        var orderId = await orderService.CreateInitialOrder(orderBasketDto);

        if (!string.IsNullOrEmpty(orderId))
        {
            orderBasketDto.OrderId = orderId;

            var isUserAuthenticated = await IsUserAuthenticated();
            var checkoutUrl = await orderService.CreateCheckoutSession(orderBasketDto, isUserAuthenticated);

            if (!string.IsNullOrEmpty(checkoutUrl))
            {
                navigationManager.NavigateTo(checkoutUrl);
            }
            else
            {
                ShowErrors = true;
                ErrorMessage = "Sorry, there was a problem placing your order. Please try again.";
                SubmitSpinnerHidden = "hidden";
            }
        }
        else
        {
            ShowErrors = true;
            ErrorMessage = "Sorry, there was a problem placing your order. Please try again.";
            SubmitSpinnerHidden = "hidden";
        }
    }

    private async Task SelectedDeliveryOption(DropdownItem deliveryOption)
    {
        DeliverySpinnerVisible = "visible";

        var quote = GetDeliveryOption(deliveryOption.OptionRef);

        if (quote != null)
        {
            await UpdateBasketCosts(quote);
        }

        DeliverySpinnerVisible = "invisible";

        selectedDeliveryOption = deliveryOption.OptionRef;
    }

    private async Task UpdateBasketCosts(Quote quote)
    {
        if (quote is not null && quote.CostSummary is not null)
        {
            decimal shippingCost = 0m;

            if (!string.IsNullOrWhiteSpace(quote.CostSummary.Shipping!.Amount))
            {
                shippingCost = decimal.Parse(quote.CostSummary.Shipping.Amount);
            }

            basketState.Basket.ShippingCost = shippingCost;

            // also confirm the basket item costs are still correct
            var quoteItems = quote.Items;

            foreach (BasketItem item in basketItems)
            {
                var unitCost = decimal.Parse(quoteItems.FirstOrDefault(i => i.Sku == item.Product.ProdigiSku).UnitCost.Amount);
                var taxUnitCost = decimal.Parse(quoteItems.FirstOrDefault(i => i.Sku == item.Product.ProdigiSku).TaxUnitCost.Amount);

                var productId = item.Product.Id;
                var photoId = item.Product.PhotoId;
                var photo = await photoService.GetPhotoByIdAsync(photoId);

                if (photo is not null)
                {
                    if (photo.Products is not null)
                    {
                        var product = photo.Products.FirstOrDefault(p => p.Id == productId) ?? null!;

                        if (product is not null)
                        {
                            int markupPercentage = await GetMarkupPercentage(product);
                            decimal markupMultiplier = ((decimal)markupPercentage / 100) + 1;
                            item.Total = (unitCost + taxUnitCost) * markupMultiplier;

                            await basketState.SaveChangesAsync();
                        }
                    }
                }
            }
        }
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
                    markupPercentage = 0;
                }

                return markupPercentage;
            }
        }

        return markupPercentage;
    }

    private async Task<bool> IsUserAuthenticated()
    {
        if (authenticationState is not null)
        {
            var authState = await authenticationState;
            var user = authState?.User;

            if (user?.Identity is not null && user.Identity.IsAuthenticated)
            {
                return true;
            }

            return false;
        }

        return false;
    }

    // iterate through quotes checking if the shipments carrier and shipments cost amount are the same
    // if they are not the same then set multipleDeliveryOptions to true
    private void CheckForMultipleDeliveryOptions()
    {
        if (quotes is not null && quotes.Count > 1)
        {
            var firstQuote = quotes.FirstOrDefault();

            if (firstQuote is not null)
            {
                var firstCarrier = firstQuote.Shipments.FirstOrDefault().Carrier;
                var firstShipmentCost = firstQuote.Shipments.FirstOrDefault().Cost.Amount;

                foreach (var quote in quotes)
                {
                    if (quote.Shipments.FirstOrDefault().Carrier.Service != firstCarrier.Service 
                        || quote.Shipments.FirstOrDefault().Cost.Amount != firstShipmentCost)
                    {
                        multipleDeliveryOptions = true;
                        break;
                    }
                }
            }
        }
    }

    private List<string> GetDeliveryOptions()
    {
        List<string> deliveryOptions = new();

        if (quotes is not null)
        {
            foreach (var quote in quotes)
            {
                deliveryOptions.Add(quote.ShipmentMethod);
            }
        }

        return deliveryOptions;
    }

    // get a specific delivery method from quotes
    private Quote GetDeliveryOption(string deliveryMethod)
    {
        if (quotes is not null)
        {
            foreach (var quote in quotes)
            {
                if (quote.ShipmentMethod == deliveryMethod)
                {
                    return quote;
                }
            }
        }

        return null!;
    }
}
