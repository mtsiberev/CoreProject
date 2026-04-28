using Mapster;
using MediatR;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using MyStore.Application.Common.Interfaces;

namespace MyStore.Application.Orders.Queries.GetOrderStatus;

public class GetOrderStatusQueryHandler(IOrderRepository repository, IDistributedCache cache, ILogger<GetOrderStatusQueryHandler> logger)
    : IRequestHandler<GetOrderStatusQuery, OrderStatusResponse>
{
    public async Task<OrderStatusResponse> Handle(GetOrderStatusQuery request, CancellationToken ct)
    {
        var order = await repository.GetByIdAsync(request.id, ct);
        if (order == null) throw new KeyNotFoundException($"Order with ID {request.id} was not found.");

        var dto = order.Adapt<OrderStatusResponse>();

        return dto;
    }
}
