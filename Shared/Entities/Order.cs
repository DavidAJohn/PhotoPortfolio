using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using PhotoPortfolio.Shared.Models;
using PhotoPortfolio.Shared.Models.Prodigi.Orders;
using System.ComponentModel.DataAnnotations;

namespace PhotoPortfolio.Shared.Entities;

[BsonIgnoreExtraElements]
public class Order : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string EmailAddress { get; set; } = string.Empty;

    [Required]
    public BsonDateTime OrderCreated { get; set; }

    public BsonDateTime? PaymentCompleted { get; set; }

    [Required]
    public List<BasketItem> Items { get; set; }

    [Required]
    public BsonDecimal128 ItemsCost { get; set; }

    [Required]
    public BsonDecimal128 ShippingCost { get; set; }

    [Required]
    public BsonDecimal128 TotalCost { get; set; }

    public Address Address { get; set; }

    [Required]
    public string ShippingMethod { get; set; }
    
    public string StripePaymentIntentId { get; set; } = string.Empty;

    public OrderStatus Status { get; set; } = OrderStatus.PaymentIncomplete;

    public OrderResponse? ProdigiDetails { get; set; }
}
