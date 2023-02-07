using PhotoPortfolio.Shared.Entities;
using PhotoPortfolioStripe = PhotoPortfolio.Shared.Models.Stripe;
using Prodigi = PhotoPortfolio.Shared.Models.Prodigi.Orders;

namespace PhotoPortfolio.Server.Services;

public class OrderService : IOrderService
{
    public async Task<bool> PlaceOrder(
        PhotoPortfolioStripe.Customer customer,
        PhotoPortfolioStripe.LineItems lineItems,
        PhotoPortfolioStripe.ShippingDetails shippingDetails
        )
    {
        var address = new Prodigi.Address()
        {
            Line1 = shippingDetails.Address.Line1,
            Line2 = shippingDetails.Address.Line2 ??= "",
            PostalOrZipCode = shippingDetails.Address.PostalCode,
            CountryCode = shippingDetails.Address.Country,
            TownOrCity = shippingDetails.Address.City,
            StateOrCounty = shippingDetails.Address.State ??= ""
        };

        var stripeDetails = new PhotoPortfolioStripe.StripeOrder()
        {
            LineItems = new()
            {
                Object = lineItems.Object,
                Data = lineItems.Data,
                HasMore = lineItems.HasMore,
                Url= lineItems.Url
            },
            ShippingDetails = shippingDetails,
            Customer = customer,
        };

        var order = new Order()
        {
            Name = customer.Name,
            EmailAddress = customer.EmailAddress,
            Address = address,
            StripeDetails = stripeDetails
        };

        // TO DO: save to db

        return true;
    }
}
