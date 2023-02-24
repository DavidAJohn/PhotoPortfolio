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

    public async Task<string> CreatOrder(List<BasketItem> lineItems, string shippingMethod)
    {
        var order = new Order()
        {
            Items = lineItems,
            ShippingMethod = string.IsNullOrWhiteSpace(shippingMethod) ? "" : shippingMethod
        };

        // save to db
        var newOrder = await _orderRepository.AddAsync(order);

        if (newOrder is null) return "";

        return newOrder.Id;
    }

    public async Task<bool> UpdateOrder(
        string orderId,
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

        // get existing order details
        var existingOrder = await _orderRepository.GetSingleAsync(o => o.Id == orderId);

        if (existingOrder != null)
        {
            var order = new Order()
            {
                Id = orderId,
                Name = customer.Name,
                EmailAddress = customer.EmailAddress,
                Items = existingOrder.Items,
                Address = address,
                StripeDetails = stripeDetails,
                ShippingMethod = existingOrder.ShippingMethod
            };

            // update order with new details sent from Stripe
            var response = await _orderRepository.UpdateAsync(order);

            if (response != null) return true;

            return false;
        };

        return false;
    }
}
