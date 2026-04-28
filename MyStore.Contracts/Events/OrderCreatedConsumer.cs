using MassTransit;
using Microsoft.Extensions.Logging;


namespace MyStore.Contracts.Events;

public class OrderCreatedConsumer(ILogger<OrderCreatedConsumer> logger) : IConsumer<OrderCreatedEvent>
{
    public async Task Consume(ConsumeContext<OrderCreatedEvent> context)
    {
        var message = context.Message;

        logger.LogInformation("[RabbitMQ] Event received: Order {OrderId} for {Customer} ({Amount}) accepted for processing.",
            message.OrderId, message.CustomerName, message.Amount);

        await Task.CompletedTask;
    }
}