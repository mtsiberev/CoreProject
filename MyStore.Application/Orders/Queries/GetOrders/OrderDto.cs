namespace MyStore.Application.Orders.Queries.GetOrders;

public record OrderDto(
    Guid Id,
    string CustomerName,
    decimal TotalAmount,
    DateTime CreatedAt,
    string Status,
    List<OrderItemDto> Items);

public record OrderItemDto(Guid ProductId, decimal Price, int Quantity);