using MongoDB.Bson.Serialization.Attributes;
using Prodigi = PhotoPortfolio.Shared.Models.Prodigi.Orders;
using PhotoPortfolio.Shared.Models.Stripe;
using MongoDB.Bson;

namespace PhotoPortfolio.Shared.Entities;

[BsonIgnoreExtraElements]
public class Order : BaseEntity
{
    public string Name { get; set; }
    public string EmailAddress { get; set; }
    public BsonDateTime OrderCreated { get; set; }
    public BsonDateTime? PaymentCompleted { get; set; }
    public List<BasketItem> Items { get; set; }
    public Prodigi.Address Address { get; set; }
    [BsonIgnore]
    public StripeOrder StripeDetails { get; set; }
    public string ShippingMethod { get; set; }
}
