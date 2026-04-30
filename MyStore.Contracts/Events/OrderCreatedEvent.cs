using MyStore.Contracts.Common;

namespace MyStore.Contracts.Events;

public record OrderCreatedEvent(
    Guid OrderId,
    string CustomerName,
    decimal TotalAmount,
    List<OrderItemDto> Items
);

