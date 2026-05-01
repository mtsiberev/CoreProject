using MyStore.Contracts.Common;

namespace MyStore.Contracts.Events;

public record StockReserved(
    Guid OrderId,
    List<OrderItemDto> Items
);