using Azure.Messaging.ServiceBus;
using MediatR;
using Microsoft.Extensions.Azure;
using System.Text.Json;

namespace PhotoPortfolio.Server.Messaging;

public class MessageConsumer : BackgroundService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<MessageConsumer> _logger;
    private readonly ServiceBusReceiver _receiver;
    private readonly IServiceScopeFactory _serviceScopeFactory;

    public MessageConsumer(IAzureClientFactory<ServiceBusReceiver> serviceBusReceiverFactory, IConfiguration configuration, ILogger<MessageConsumer> logger, IServiceScopeFactory serviceScopeFactory)
    {
        _configuration = configuration;
        _logger = logger;
        _receiver = serviceBusReceiverFactory.CreateClient(_configuration.GetValue<string>("AzureServiceBus:Queue"));
        _serviceScopeFactory = serviceScopeFactory;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            using (IServiceScope scope = _serviceScopeFactory.CreateScope())
            {
                var receivedMessages = _receiver.ReceiveMessagesAsync(stoppingToken);

                await foreach (ServiceBusReceivedMessage message in receivedMessages)
                {
                    string? body;

                    if (message.ContentType.ToLower().StartsWith("application/json"))
                    {
                        try
                        {
                            var messageType = message.ApplicationProperties.FirstOrDefault(x => x.Key == "MessageType").Value ?? "";
                            var type = Type.GetType($"PhotoPortfolio.Server.Messaging.{messageType}");

                            if (type is null)
                            {
                                _logger.LogWarning("Message Consumer -> Unknown message type received");
                                continue;
                            }

                            body = message.Body.ToString();

                            var typedMessage = (IServiceBusMessage)JsonSerializer.Deserialize(body, type)!;

                            if (typedMessage == null)
                            {
                                _logger.LogWarning("Message Consumer -> Error when deserializing message body: {body}", body);
                                return;
                            }

                            _logger.LogInformation("Message Consumer -> {typeName} message received", type.Name);

                            var mediatr = scope.ServiceProvider.GetRequiredService<IMediator>();
                            await mediatr.Send(typedMessage, stoppingToken);
                            
                            await _receiver.CompleteMessageAsync(message, stoppingToken);
                            _logger.LogInformation("Message Consumer -> Message completed");
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Message Consumer -> Error processing message body: {message}", ex.Message);
                        }
                    }
                    else
                    {
                        body = message.Body.ToString();
                        _logger.LogWarning("Message Consumer -> Received unexpected non-JSON content: {body}", body);

                        await _receiver.CompleteMessageAsync(message, stoppingToken);
                        _logger.LogInformation("Message Consumer -> Message completed");
                    }
                }
            }
        }
    }
}
