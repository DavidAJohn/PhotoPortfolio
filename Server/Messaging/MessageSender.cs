using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Azure;
using PhotoPortfolio.Shared.Models;
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

    public async Task<bool> SendOrderApprovedMessageAsync(OrderDetailsDto order)
    {
        try
        {
            var serializedOrder = JsonSerializer.Serialize(order);

            var serializedMessage = new ServiceBusMessage(serializedOrder)
            {
                Subject = $"Order Approved: {order.Id}",
                ContentType = "application/json;charset=utf-8",
                ScheduledEnqueueTime = DateTimeOffset.UtcNow.AddMinutes(1),
            };

            await _sender.SendMessageAsync(serializedMessage);
            _logger.LogInformation("Order Approved message sent to Azure Service Bus - Order No: {orderNo}", order.Id);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending message to Azure Service Bus - Order No: {orderNo}", order.Id);
            return false;
        }
    }
}
