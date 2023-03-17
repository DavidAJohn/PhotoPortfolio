using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using PhotoPortfolio.Shared.Models.Prodigi.Orders;
using System.ComponentModel.DataAnnotations;

namespace PhotoPortfolio.Shared.Entities;

[BsonIgnoreExtraElements]
public class Order : BaseEntity
{
    public string Name { get; set; }
    public string EmailAddress { get; set; }

    [Required]
    public BsonDateTime OrderCreated { get; set; }

    public BsonDateTime? PaymentCompleted { get; set; }

    [Required]
    public List<BasketItem> Items { get; set; }

    [Required] 
    public BsonDecimal128 TotalCost { get; set; }

    public Address Address { get; set; }

    [Required] 
    public string ShippingMethod { get; set; }

    public OrderStatus Status { get; set; } = OrderStatus.NotReady;
}

public enum OrderStatus
{
    NotReady,
    Ready,
    Approved,
    InProgress,
    Completed,
    Cancelled
}
