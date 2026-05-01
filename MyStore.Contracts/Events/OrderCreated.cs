using MyStore.Contracts.Common;

namespace MyStore.Contracts.Events;

public record OrderCreated(
    Guid OrderId,
    string CustomerName,
    decimal TotalAmount,
    List<OrderItemDto> Items
);

