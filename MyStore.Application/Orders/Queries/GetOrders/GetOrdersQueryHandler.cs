using Mapster;
using MediatR;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using MyStore.Application.Common.Interfaces;
using MyStore.Application.Orders.Queries.GetOrders;
using System.Text.Json;

public class GetOrdersQueryHandler(IOrderRepository repository, IDistributedCache cache, ILogger<GetOrdersQueryHandler> logger)
    : IRequestHandler<GetOrdersQuery, List<OrderDto>>
{
    private const string CacheKey = "orders_list";

    public async Task<List<OrderDto>> Handle(GetOrdersQuery request, CancellationToken ct)
    {
        var cachedOrders = await cache.GetStringAsync(CacheKey, ct);
        if (!string.IsNullOrEmpty(cachedOrders))
        {
            try
            {
                return JsonSerializer.Deserialize<List<OrderDto>>(cachedOrders) ?? new();
            }
            catch (JsonException)
            {        
                logger.LogWarning("Failed to deserialize cache for {Key}", CacheKey);
            }
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