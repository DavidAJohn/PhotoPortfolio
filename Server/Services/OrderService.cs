using PhotoPortfolio.Shared.Entities;
using PhotoPortfolioStripe = PhotoPortfolio.Shared.Models.Stripe;
using Prodigi = PhotoPortfolio.Shared.Models.Prodigi.Orders;

namespace PhotoPortfolio.Server.Services;

public class OrderService : IOrderService
{
    private readonly IOrderRepository _orderRepository;

    public OrderService(IOrderRepository orderRepository)
    {
        _orderRepository = orderRepository;
    }

    public async Task<bool> PlaceOrder(
        PhotoPortfolioStripe.Customer customer,
        PhotoPortfolioStripe.LineItems lineItems,
        PhotoPortfolioStripe.ShippingDetails shippingDetails,
        string shippingMethod
        )
    {
        var address = new Prodigi.Address()
        {
            Line1 = shippingDetails.Address.Line1,
            Line2 =  string.IsNullOrWhiteSpace(shippingDetails.Address.Line2) ? "" : shippingDetails.Address.Line2,
            PostalOrZipCode = shippingDetails.Address.PostalCode,
            CountryCode = shippingDetails.Address.Country,
            TownOrCity = shippingDetails.Address.City,
            StateOrCounty = string.IsNullOrWhiteSpace(shippingDetails.Address.State) ? "" : shippingDetails.Address.State
        };

        var stripeDetails = new PhotoPortfolioStripe.StripeOrder()
        {
            LineItems = new()
            {
                Object = lineItems.Object,
                Data = lineItems.Data,
                HasMore = lineItems.HasMore,
                Url = lineItems.Url
            },
            ShippingDetails = shippingDetails
        };

        var order = new Order()
        {
            Name = customer.Name,
            EmailAddress = customer.EmailAddress,
            Address = address,
            StripeDetails = stripeDetails,
            ShippingMethod = string.IsNullOrWhiteSpace(shippingMethod) ? "Standard" : shippingMethod
        };

        // save to db
        var response = await _orderRepository.AddAsync(order);

        if (response != null) return true;

        return false;
    }
}
