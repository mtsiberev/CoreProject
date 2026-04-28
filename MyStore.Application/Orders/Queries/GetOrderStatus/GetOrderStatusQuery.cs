using MediatR;

namespace MyStore.Application.Orders.Queries.GetOrderStatus;

public record GetOrderStatusQuery(Guid id) : IRequest<OrderStatusResponse>;
