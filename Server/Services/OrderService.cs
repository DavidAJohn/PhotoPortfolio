﻿using MongoDB.Bson;
using PhotoPortfolio.Shared.Entities;
using PhotoPortfolio.Shared.Models;
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
            ShippingMethod = string.IsNullOrWhiteSpace(shippingMethod) ? "" : shippingMethod,
            OrderCreated = BsonDateTime.Create(DateTime.UtcNow)
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
                Address = address,
                ShippingMethod = existingOrder.ShippingMethod
            };

            // update order with new details sent from Stripe
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
            Items = order.Items,
            Address = order.Address,
            ShippingMethod = order.ShippingMethod
        };

        return orderDetails;
    }
}
