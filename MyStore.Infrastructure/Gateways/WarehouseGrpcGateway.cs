using Grpc.Core;
using Microsoft.Extensions.Logging;
using MyStore.Application.Common.Interfaces;
using MyStore.Warehouse.Grpc;

namespace MyStore.Infrastructure.Clients;

public class WarehouseGrpcGateway(
    WarehouseService.WarehouseServiceClient grpcClient,
    ILogger<WarehouseGrpcGateway> logger)
    : IWarehouseClient
{
    public async Task<Dictionary<Guid, int>> GetProductStocksAsync(IEnumerable<Guid> productIds, CancellationToken ct)
    {
        var distinctIds = productIds.Distinct().ToList();
        if (distinctIds.Count == 0)
        {
            return [];
        }

        var request = new BatchStocksRequest();
        request.ProductIds.AddRange(productIds.Select(id => id.ToString()));

        try
        {
            var response = await grpcClient.GetBatchStocksInfoAsync(request, cancellationToken: ct);

            return response.Stocks
                .Select(x => new
                {
                    Guid = Guid.TryParse(x.ProductId, out var parsedGuid) ? parsedGuid : Guid.Empty,
                    x.Quantity
                })
                .Where(x => x.Guid != Guid.Empty)
                .ToDictionary(x => x.Guid, x => x.Quantity);
        }
        catch (RpcException ex)
        {
            logger.LogError(ex, "gRPC call of WarehouseService throws error. Status: {Status}", ex.Status);
            throw new InvalidOperationException("WarehouseService is unavailable", ex);
        }
    }
}
