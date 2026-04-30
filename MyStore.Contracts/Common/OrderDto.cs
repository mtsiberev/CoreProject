namespace MyStore.Contracts.Common;

public record OrderDto(
    Guid Id,
    string CustomerName,
    decimal TotalAmount,
    DateTime CreatedAt,
    string Status,
    List<OrderItemDto> Items);

public record OrderItemDto(Guid ProductId, string ProductName, decimal Price, int Quantity);