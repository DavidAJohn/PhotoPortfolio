﻿using MongoDB.Bson.Serialization.Attributes;
using PhotoPortfolio.Shared.Entities;
using PhotoPortfolio.Shared.Models.Prodigi.Orders;

namespace PhotoPortfolio.Shared.Models;

[BsonIgnoreExtraElements]
public class OrderDetailsDto : BaseEntity
{
    public string Name { get; set; }
    public string EmailAddress { get; set; }
    public DateTime OrderDate { get; set; }
    public List<BasketItem> Items { get; set; }
    public decimal ShippingCost { get; set; }
    public decimal TotalCost { get; set; }
    public Address Address { get; set; }
    public string ShippingMethod { get; set; }
    public string StripePaymentIntentId { get; set; }
    public string Status { get; set; }
    public OrderResponse? ProdigiDetails { get; set; }
}
