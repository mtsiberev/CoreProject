using Mapster;
using MediatR;
using MyStore.Application.Common.Interfaces;

namespace MyStore.Application.Orders.Queries.GetOrders;

public record GetOrdersQuery : IRequest<List<OrderDto>>;

public class GetOrdersQueryHandler(IOrderRepository repository)
    : IRequestHandler<GetOrdersQuery, List<OrderDto>>
{
    public async Task<List<OrderDto>> Handle(GetOrdersQuery request, CancellationToken cancellationToken)
    {
        var orders = await repository.GetAllAsync(cancellationToken);

        return orders.Adapt<List<OrderDto>>();
    }
}