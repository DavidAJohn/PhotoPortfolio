using PhotoPortfolio.Server.Messaging;
using PhotoPortfolio.Shared.Models;

namespace PhotoPortfolio.Server.Mapping;

public static class OrderApprovedToOrderDetailsMapper
{
    public static OrderDetailsDto ToOrderDetails(this OrderApproved orderApproved)
    {
        return new OrderDetailsDto
        {
            Id = orderApproved.Id,
            Name = orderApproved.Name,
            EmailAddress = orderApproved.EmailAddress,
            OrderDate = orderApproved.OrderDate,
            Items = orderApproved.Items,
            ShippingCost = orderApproved.ShippingCost,
            TotalCost = orderApproved.TotalCost,
            Address = orderApproved.Address,
            ShippingMethod = orderApproved.ShippingMethod,
            StripePaymentIntentId = orderApproved.StripePaymentIntentId,
            Status = OrderStatus.Approved.ToString()
        };
    }
}
