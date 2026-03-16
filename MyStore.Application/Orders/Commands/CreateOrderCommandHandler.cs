using MassTransit;
using MediatR;
using Microsoft.Extensions.Caching.Distributed;
using MyStore.Application.Common.Events;
using MyStore.Application.Common.Interfaces;
using MyStore.Application.Orders.Commands;
using MyStore.Domain.Entities;

public class CreateOrderCommandHandler(
    IOrderRepository repository, 
    IApplicationDbContext context, 
    IPublishEndpoint publishEndpoint,
    IDistributedCache cache)
    : IRequestHandler<CreateOrderCommand, Guid>
{

    private const string CacheKey = "orders_list";
    public async Task<Guid> Handle(CreateOrderCommand request, CancellationToken cancellationToken)
    {
        var order = new Order { CustomerName = request.CustomerName };
        order.AddItem(request.ProductName, request.Price, 1);

        await repository.AddAsync(order, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);

        await cache.RemoveAsync(CacheKey, cancellationToken);

        await publishEndpoint.Publish(new OrderCreatedEvent(order.Id, order.CustomerName, order.TotalAmount), cancellationToken);

        return order.Id;
    }
}