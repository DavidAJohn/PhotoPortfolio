using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using PhotoPortfolio.Server.Messaging;
using System;
using System.Text.Json;
using System.Threading.Tasks;

namespace PhotoPortfolio.Tests.Unit.Server.Messaging;

public class MessageSenderTests
{
    private readonly ILogger<MessageSender> _logger;
    private readonly IConfiguration _configuration;
    private readonly IAzureClientFactory<ServiceBusSender> _serviceBusSenderFactory = Substitute.For<IAzureClientFactory<ServiceBusSender>>();

    public MessageSenderTests()
    {
        _logger = Substitute.For<ILogger<MessageSender>>();
        _configuration = Substitute.For<IConfiguration>();
        _serviceBusSenderFactory.CreateClient("test").Returns(Substitute.For<ServiceBusSender>());
    }

    [Fact]
    public async Task SendMessage_ShouldReturnsTrue_WhenInputIsValid()
    {
        // Arrange
        var messageSender = new MessageSender(_serviceBusSenderFactory, _configuration, _logger);

        var message = new
        {
            MessageData = "TestData"
        };

        var serializedMessage = JsonSerializer.Serialize(message);
        var serviceBusMessage = new ServiceBusMessage(serializedMessage)
        {
            Subject = "Test Message",
            ContentType = "application/json;charset=utf-8",
            ScheduledEnqueueTime = DateTimeOffset.UtcNow.AddMinutes(1),
            ApplicationProperties =
                {
                    { "MessageType",  "Test"}
                }
        };

        // Act
        var result = await messageSender.SendMessage(serviceBusMessage);

        // Assert
        result.Should().BeTrue();
    }

}
