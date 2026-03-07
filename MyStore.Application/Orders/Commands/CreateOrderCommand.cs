using MediatR;

namespace MyStore.Application.Orders.Commands;

public record CreateOrderCommand(string CustomerName, string ProductName, decimal Price) : IRequest<Guid>;
