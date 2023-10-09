using MediatR;
using PhotoPortfolio.Shared.Entities;
using PhotoPortfolio.Shared.Models.Prodigi.Orders;

namespace PhotoPortfolio.Server.Messaging;

public class OrderApproved : BaseEntity, IRequest
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
}
