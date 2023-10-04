using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Azure;
using PhotoPortfolio.Shared.Models;
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
                        body = message.Body.ToString();

                        var order = JsonSerializer.Deserialize<OrderDetailsDto>(body);

                        if (order == null)
                        {
                            _logger.LogInformation("Message Consumer -> Error when deserializing message body: {body}", body);
                            return;
                        }

                        _logger.LogInformation("Message Consumer -> Received: {orderId}", order.Id);

                        var orderCreated = await _orderService.CreateProdigiOrder(order);
                        if (!orderCreated)
                        {
                            _logger.LogError("Message Consumer -> Error when creating Prodigi order: {orderId}", order.Id);
                            return;
                        }

                        _logger.LogInformation("Message Consumer -> Prodigi order created: {orderId}", order.Id);

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
