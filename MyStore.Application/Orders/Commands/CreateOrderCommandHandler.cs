using MassTransit;
using MediatR;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using MyStore.Application.Common.Interfaces;
using MyStore.Contracts.Common;
using MyStore.Contracts.Events;
using MyStore.Domain.Entities;
using MyStore.Domain.Enums;

namespace MyStore.Application.Orders.Commands;

public class CreateOrderCommandHandler(
    IOrderRepository repository,
    IApplicationDbContext context,
    IPublishEndpoint publishEndpoint,
    IDistributedCache cache,
    IWarehouseClient warehouseClient,
    ITopicProducer<string, OrderCreated> kafkaProducer,
    IConfiguration configuration)
    : IRequestHandler<CreateOrderCommand, Guid>
{
    private const string CacheKey = "orders_list";

    public async Task<Guid> Handle(CreateOrderCommand request, CancellationToken ct)
    {
        var productIds = request.Items.Select(x => x.ProductId);

        var stockDict = await warehouseClient.GetProductStocksAsync(productIds, ct);

        foreach (var item in request.Items)
        {
            var availableQuantity = stockDict.TryGetValue(item.ProductId, out var qty) ? qty : 0;
            if (availableQuantity < item.Quantity)
            {
                throw new InvalidOperationException(
                    $"Not enough '{item.ProductName}' on Warehouse. Available: {availableQuantity}, needed: {item.Quantity}.");
            }
        }

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

        var orderCreatedEvent = new OrderCreated(
            order.Id,
            order.CustomerName,
            order.TotalAmount,
            eventItems);

        if (configuration.IsKafka())
        {
            await kafkaProducer.Produce(order.Id.ToString(), orderCreatedEvent, ct);
        }
        else
        {
            await publishEndpoint.Publish(orderCreatedEvent, ct);
        }

        await context.SaveChangesAsync(ct);

        await cache.RemoveAsync(CacheKey, ct);

        return order.Id;
    }
}
