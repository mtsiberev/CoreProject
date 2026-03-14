namespace MyStore.Application.Common.Events;

public record OrderCreatedEvent(Guid OrderId, string CustomerName, decimal Amount);
