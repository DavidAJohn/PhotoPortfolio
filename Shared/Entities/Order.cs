using MongoDB.Bson.Serialization.Attributes;
using Prodigi = PhotoPortfolio.Shared.Models.Prodigi.Orders;
using PhotoPortfolio.Shared.Models.Stripe;

namespace PhotoPortfolio.Shared.Entities;

[BsonIgnoreExtraElements]
public class Order : BaseEntity
{
    public string Name { get; set; }
    public string EmailAddress { get; set; }
    public Prodigi.Address Address { get; set; }
    public StripeOrder StripeDetails { get; set; }
}
