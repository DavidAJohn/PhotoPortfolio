using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Azure;
using System.Text.Json;

namespace PhotoPortfolio.Server.Messaging;

public class MessageSender : IMessageSender
{
    private readonly ServiceBusSender _sender;
    private readonly IConfiguration _configuration;
    private readonly ILogger<MessageSender> _logger;

    public MessageSender(IAzureClientFactory<ServiceBusSender> serviceBusSenderFactory, IConfiguration configuration, ILogger<MessageSender> logger)
    {
        _configuration = configuration;
        _logger = logger;
        _sender = serviceBusSenderFactory.CreateClient(_configuration.GetValue<string>("AzureServiceBus:Queue"));
    }

    public async Task<bool> SendMessage<T>(T message)
    {
        try
        {
            var serializedMessage = JsonSerializer.Serialize(message);

            var serviceBusMessage = new ServiceBusMessage(serializedMessage)
            {
                Subject = $"{typeof(T).Name} Message",
                ContentType = "application/json;charset=utf-8",
                ScheduledEnqueueTime = DateTimeOffset.UtcNow.AddMinutes(1),
                ApplicationProperties =
                {
                    { "MessageType", typeof(T).Name }
                }
            };

            await _sender.SendMessageAsync(serviceBusMessage);
            _logger.LogInformation("{MessageType} message sent to Azure Service Bus", typeof(T).Name);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending {MessageType} message to Azure Service Bus", typeof(T).Name);
            return false;
        }
    }
}
