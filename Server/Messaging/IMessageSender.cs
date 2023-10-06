namespace PhotoPortfolio.Server.Messaging;

public interface IMessageSender
{
    Task<bool> SendMessage<T>(T message);
}
