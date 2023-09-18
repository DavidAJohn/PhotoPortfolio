using PhotoPortfolio.Shared.Models;

namespace PhotoPortfolio.Server.Messaging;

public interface IMessageSender
{
    Task<bool> SendOrderApprovedMessageAsync(OrderDetailsDto order);
}
