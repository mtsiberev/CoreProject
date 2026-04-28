namespace MyStore.Contracts.Events;

public record OrderCreatedEvent(Guid OrderId, string CustomerName, decimal Amount);
