using Mapster;
using MediatR;
using Microsoft.Extensions.Caching.Distributed;
using MyStore.Application.Common.Interfaces;
using MyStore.Application.Orders.Queries.GetOrders;
using System.Text.Json;

public class GetOrdersQueryHandler(IOrderRepository repository, IDistributedCache cache)
    : IRequestHandler<GetOrdersQuery, List<OrderDto>>
{
    private const string CacheKey = "orders_list";

    public async Task<List<OrderDto>> Handle(GetOrdersQuery request, CancellationToken ct)
    {
        var cachedOrders = await cache.GetStringAsync(CacheKey, ct);
        if (!string.IsNullOrEmpty(cachedOrders))
        {
            return JsonSerializer.Deserialize<List<OrderDto>>(cachedOrders)!;
        }

        var orders = await repository.GetAllAsync(ct);
        var dtos = orders.Adapt<List<OrderDto>>();

        var cacheOptions = new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(2)
        };

        await cache.SetStringAsync(CacheKey, JsonSerializer.Serialize(dtos), cacheOptions, ct);

        return dtos;
    }
}