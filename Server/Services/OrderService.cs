using MongoDB.Bson;
using PhotoPortfolio.Shared.Entities;
using PhotoPortfolio.Shared.Helpers;
using PhotoPortfolio.Shared.Models;
using PhotoPortfolioStripe = PhotoPortfolio.Shared.Models.Stripe;
using Prodigi = PhotoPortfolio.Shared.Models.Prodigi.Orders;

namespace PhotoPortfolio.Server.Services;

public class OrderService : IOrderService
{
    private readonly IOrderRepository _orderRepository;
    private readonly IPreferencesRepository _preferencesRepository;
    private readonly IConfiguration _config;

    public OrderService(IOrderRepository orderRepository, IPreferencesRepository preferencesRepository, IConfiguration config)
    {
        _orderRepository = orderRepository;
        _preferencesRepository = preferencesRepository;
        _config = config;
    }

    public async Task<string> CreatOrder(OrderBasketDto orderBasketDto)
    {
        var lineItems = orderBasketDto.BasketItems;
        var shippingMethod = orderBasketDto.ShippingMethod;
        var shippingCost = orderBasketDto.ShippingCost;

        decimal itemsCost = 0;

        foreach (var item in lineItems)
        {
            itemsCost += item.Total;
        }

        decimal totalCost = itemsCost + shippingCost;

        var order = new Order()
        {
            Items = lineItems,
            ShippingMethod = string.IsNullOrWhiteSpace(shippingMethod) ? "" : shippingMethod,
            OrderCreated = BsonDateTime.Create(DateTime.UtcNow),
            ItemsCost = (BsonDecimal128)itemsCost,
            ShippingCost = (BsonDecimal128)shippingCost,
            TotalCost = (BsonDecimal128)totalCost
        };

        // save to db
        var newOrder = await _orderRepository.AddAsync(order);

        if (newOrder is null) return "";

        return newOrder.Id;
    }

    public async Task<bool> UpdateOrder(
        string orderId,
        PhotoPortfolioStripe.Customer customer,
        PhotoPortfolioStripe.ShippingDetails shippingDetails,
        string shippingMethod,
        string paymentIntentId
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

        // get existing order details
        var existingOrder = await _orderRepository.GetSingleAsync(o => o.Id == orderId);

        if (existingOrder != null)
        {
            var order = new Order()
            {
                Id = orderId,
                Name = customer.Name,
                EmailAddress = customer.EmailAddress,
                OrderCreated = existingOrder.OrderCreated,
                PaymentCompleted = BsonDateTime.Create(DateTime.UtcNow),
                Items = existingOrder.Items,
                ItemsCost = existingOrder.ItemsCost,
                ShippingCost = existingOrder.ShippingCost,
                TotalCost = existingOrder.TotalCost,
                Address = address,
                ShippingMethod = existingOrder.ShippingMethod,
                StripePaymentIntentId = paymentIntentId,
                Status = OrderStatus.AwaitingApproval
            };

            // update order with new details sent from Stripe
            var response = await _orderRepository.UpdateAsync(order);

            if (response != null) return true;

            return false;
        };

        return false;
    }

    public async Task<bool> UpdateOrderCosts(OrderBasketDto orderBasketDto)
    {
        var existingOrder = await _orderRepository.GetSingleAsync(o => o.Id == orderBasketDto.OrderId);

        if (existingOrder != null)
        {
            decimal itemsCost = orderBasketDto.BasketItems.Sum(x => x.Total);
            decimal totalCost = itemsCost + orderBasketDto.ShippingCost;

            var order = new Order()
            {
                Id = existingOrder.Id,
                OrderCreated = existingOrder.OrderCreated,
                Items = orderBasketDto.BasketItems,
                ItemsCost = (BsonDecimal128)itemsCost,
                ShippingCost = (BsonDecimal128)orderBasketDto.ShippingCost,
                TotalCost = (BsonDecimal128)totalCost,
                ShippingMethod = orderBasketDto.ShippingMethod,
                Status = existingOrder.Status
            };

            var response = await _orderRepository.UpdateAsync(order);

            if (response != null) return true;

            return false;
        };

        return false;
    }

    public async Task<OrderDetailsDto> GetOrderDetailsFromId(string orderId)
    {
        var order = await _orderRepository.GetSingleAsync(o => o.Id == orderId);

        if (order is null) return null!;

        var orderDetails = new OrderDetailsDto()
        {
            Id = order.Id,
            Name = order.Name,
            EmailAddress = order.EmailAddress,
            OrderDate = order.PaymentCompleted!.ToLocalTime(),
            Items = order.Items,
            ShippingCost = order.ShippingCost.ToDecimal(),
            TotalCost = order.TotalCost.ToDecimal(),
            Address = order.Address,
            ShippingMethod = order.ShippingMethod,
            StripePaymentIntentId = order.StripePaymentIntentId,
            Status = order.Status.ToString()
        };

        return orderDetails;
    }

    public async Task<List<OrderDetailsDto>> GetOrderDetails(OrderSpecificationParams? orderParams)
    {
        var orders = new List<Order>();

        if (orderParams is null)
        {
            orders = await _orderRepository.GetAllAsync();
        }
        else
        {
            orders = await _orderRepository.GetFilteredOrdersAsync(orderParams);
        }

        List<OrderDetailsDto> orderDetails = orders.ConvertAll(
            new Converter<Order, OrderDetailsDto>(OrderToDetailsConverter));

        return orderDetails;
    }

    private OrderDetailsDto OrderToDetailsConverter(Order order)
    {
        var orderDetails = new OrderDetailsDto()
        {
            Id = order.Id,
            Name = order.Name,
            EmailAddress = order.EmailAddress,
            Items = order.Items,
            ShippingCost = order.ShippingCost.ToDecimal(),
            TotalCost = order.TotalCost.ToDecimal(),
            Address = order.Address,
            ShippingMethod = order.ShippingMethod,
            StripePaymentIntentId = order.StripePaymentIntentId,
            Status = order.Status.ToString()
        };

        if (order.PaymentCompleted is null)
        {
           orderDetails.OrderDate = order.OrderCreated.ToLocalTime();
        }
        else
        {
            orderDetails.OrderDate = order.PaymentCompleted.ToLocalTime();
        }

        return orderDetails;
    }

    public async Task<bool> ShouldApproveOrder(string orderId)
    {
        var sitePrefsId = _config["SitePreferencesId"];
        if (string.IsNullOrWhiteSpace(sitePrefsId)) return false;

        var prefs = await _preferencesRepository.GetSingleAsync(p => p.Id == sitePrefsId);
        var order = await _orderRepository.GetSingleAsync(o => o.Id == orderId);

        if (prefs is null || order is null) return false;

        prefs.Metadata.TryGetValue("OrdersSentToProdigiAutomatically", out string autoApproval);
        prefs.Metadata.TryGetValue("OrderAutoApproveLimit", out string approvalLimitString);

        var totalCost = order.TotalCost.ToDecimal();
        var approvalLimit = decimal.Parse(approvalLimitString);
        bool approveDecision = false;

        approveDecision = autoApproval switch
        {
            "AutoApproveAll" => true,
            "ManuallyApproveAll" => false,
            _ => false
        };

        // check for auto-approval below a limit
        if (autoApproval == "AutoApproveBelow")
        {
            if (totalCost < approvalLimit) approveDecision = true;
            else approveDecision = false;
        }

        return approveDecision;
    }
}
