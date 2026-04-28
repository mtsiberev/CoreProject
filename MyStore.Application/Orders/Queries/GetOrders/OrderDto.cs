namespace MyStore.Application.Orders.Queries.GetOrders;

public record OrderDto(
    Guid Id,
    string CustomerName,
    decimal TotalAmount,
    DateTime CreatedAt,
    string Status,
    List<OrderItemDto> Items);

public record OrderItemDto(string ProductName, decimal Price, int Quantity);