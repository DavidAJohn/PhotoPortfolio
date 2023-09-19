using PhotoPortfolio.Server.Messaging;
using PhotoPortfolio.Shared.Models;

namespace PhotoPortfolio.Tests.Integration.Infrastructure;

public class MockMessageSender : IMessageSender
{
    public MockMessageSender()
    {
    }

    public async Task<bool> SendOrderApprovedMessageAsync(OrderDetailsDto order)
    {
        return true;
    }
}
