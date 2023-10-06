using PhotoPortfolio.Server.Messaging;
using PhotoPortfolio.Shared.Models;

namespace PhotoPortfolio.Server.Mapping;

public static class OrderDetailsToOrderApprovedMapper
{
    public static OrderApproved ToOrderApprovedMessage(this OrderDetailsDto orderDetailsDto)
    {
        return new OrderApproved
        {
            Id = orderDetailsDto.Id,
            Name = orderDetailsDto.Name,
            EmailAddress = orderDetailsDto.EmailAddress,
            OrderDate = orderDetailsDto.OrderDate,
            Items = orderDetailsDto.Items,
            ShippingCost = orderDetailsDto.ShippingCost,
            TotalCost = orderDetailsDto.TotalCost,
            Address = orderDetailsDto.Address,
            ShippingMethod = orderDetailsDto.ShippingMethod,
            StripePaymentIntentId = orderDetailsDto.StripePaymentIntentId
        };
    }
}
