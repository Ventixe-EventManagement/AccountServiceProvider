using Azure.Messaging.ServiceBus;
using Business.Interfaces;
using Business.Models;
using Microsoft.Extensions.Configuration;
using System.Text.Json;

namespace Business.Services;

public class EmailQueuePublisher : IEmailQueuePublisher
{
    private readonly IConfiguration _config;

    public EmailQueuePublisher(IConfiguration config)
    {
        _config = config;
    }

    public async Task PublishVerificationEmailAsync(VerifyEmailMessage message)
    {
        var connectionString = _config["ServiceBus:ConnectionString"];
        var queueName = _config["ServiceBus:QueueName"];

        await using var client = new ServiceBusClient(connectionString);
        var sender = client.CreateSender(queueName);

        var body = JsonSerializer.Serialize(message);
        var serviceBusMessage = new ServiceBusMessage(body);

        await sender.SendMessageAsync(serviceBusMessage);
    }
}
