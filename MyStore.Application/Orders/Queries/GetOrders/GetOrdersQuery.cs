using MediatR;
using MyStore.Contracts.Common;

namespace MyStore.Application.Orders.Queries.GetOrders;

public record GetOrdersQuery : IRequest<List<OrderDto>>;