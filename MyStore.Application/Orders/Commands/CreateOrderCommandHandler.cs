using MassTransit;
using MediatR;
using Microsoft.Extensions.Caching.Distributed;
using MyStore.Application.Common.Interfaces;
using MyStore.Application.Orders.Commands;
using MyStore.Contracts.Common;
using MyStore.Contracts.Events;
using MyStore.Domain.Entities;
using MyStore.Domain.Enums;


public class CreateOrderCommandHandler(
    IOrderRepository repository, 
    IApplicationDbContext context, 
    IPublishEndpoint publishEndpoint,
    IDistributedCache cache)
    : IRequestHandler<CreateOrderCommand, Guid>
{

    private const string CacheKey = "orders_list";

    public async Task<Guid> Handle(CreateOrderCommand request, CancellationToken ct)
    {
        var order = new Order
        {
            CustomerName = request.CustomerName,
            Status = OrderStatus.Processing
        };

        foreach (var item in request.Items)
        {
            order.AddItem(item.ProductId, item.ProductName, item.Price, item.Quantity);
        }

        await repository.AddAsync(order, ct);

        var eventItems = order.Items.Select(x => new OrderItemDto(
            x.ProductId,
            x.ProductName,
            x.Price,
            x.Quantity)).ToList();

        await publishEndpoint.Publish(new OrderCreated(
            order.Id,
            order.CustomerName,
            order.TotalAmount,
            eventItems), ct);

        await context.SaveChangesAsync(ct);

        await cache.RemoveAsync(CacheKey, ct);

        return order.Id;
    }
}