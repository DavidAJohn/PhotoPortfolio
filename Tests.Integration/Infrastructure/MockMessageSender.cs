using PhotoPortfolio.Server.Messaging;

namespace PhotoPortfolio.Tests.Integration.Infrastructure;

public class MockMessageSender : IMessageSender
{
    public MockMessageSender()
    {
    }

    public async Task<bool> SendMessage<T>(T message)
    {
        return await Task.FromResult(true);
    }
}
