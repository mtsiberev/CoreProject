using MediatR;
using Microsoft.EntityFrameworkCore;
using MyStore.Application.Common.Interfaces;
using MyStore.Domain.Entities;

namespace MyStore.Application.Orders.Queries.GetOrders;

public record GetOrdersQuery : IRequest<List<Order>>;

public class GetOrdersQueryHandler(IApplicationDbContext context)
    : IRequestHandler<GetOrdersQuery, List<Order>>
{
    public async Task<List<Order>> Handle(GetOrdersQuery request, CancellationToken cancellationToken)
    {
        return await context.Orders
            .Include(o => o.Items)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }
}
