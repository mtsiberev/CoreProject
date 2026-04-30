using MediatR;
using MyStore.Contracts.Common;

namespace MyStore.Application.Orders.Commands;

public record CreateOrderCommand(string CustomerName, List<OrderItemDto> Items) : IRequest<Guid>;

