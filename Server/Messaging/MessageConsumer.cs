using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Azure;
using PhotoPortfolio.Server.Mapping;
using System.Text.Json;

namespace PhotoPortfolio.Server.Messaging;

public class MessageConsumer : BackgroundService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<MessageConsumer> _logger;
    private readonly IOrderService _orderService;
    private readonly ServiceBusReceiver _receiver;

    public MessageConsumer(IAzureClientFactory<ServiceBusReceiver> serviceBusReceiverFactory, IConfiguration configuration, ILogger<MessageConsumer> logger, IOrderService orderService)
    {
        _configuration = configuration;
        _logger = logger;
        _orderService = orderService;
        _receiver = serviceBusReceiverFactory.CreateClient(_configuration.GetValue<string>("ServiceBus:Queue"));
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
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

                        var typedMessage = JsonSerializer.Deserialize(body, type);

                        if (typedMessage == null)
                        {
                            _logger.LogWarning("Message Consumer -> Error when deserializing message body: {body}", body);
                            return;
                        }

                        if (type == typeof(OrderApproved))
                        {
                            var order = (OrderApproved)typedMessage;
                            var orderCreated = await _orderService.CreateProdigiOrder(order.ToOrderDetailsMessage());

                            if (!orderCreated)
                            {
                                _logger.LogError("Message Consumer -> Error when creating Prodigi order: {orderId}", order.Id);
                                return;
                            }

                            _logger.LogInformation("Message Consumer -> {type.Name} Received: {order.Id}", type.Name, order.Id);
                        }
                        else
                        {
                            _logger.LogWarning("Unexpected message type received: {type.Name}", type.Name);
                        }

                        await _receiver.CompleteMessageAsync(message, stoppingToken);
                        _logger.LogInformation("Message Consumer -> Message completed");
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Message Consumer -> Error when deserializing message body: {message}", ex.Message);
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
