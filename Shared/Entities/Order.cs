using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using PhotoPortfolio.Shared.Models.Prodigi.Orders;

namespace PhotoPortfolio.Shared.Entities;

[BsonIgnoreExtraElements]
public class Order : BaseEntity
{
    public string Name { get; set; }
    public string EmailAddress { get; set; }
    public BsonDateTime OrderCreated { get; set; }
    public BsonDateTime? PaymentCompleted { get; set; }
    public List<BasketItem> Items { get; set; }
    public Address Address { get; set; }
    public string ShippingMethod { get; set; }
}
