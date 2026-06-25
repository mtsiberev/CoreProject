using Grpc.Core;
using Microsoft.EntityFrameworkCore;
using MyStore.Warehouse.Grpc;
using MyStore.Warehouse.Data;

namespace MyStore.Warehouse.Services;

public class WarehouseGrpcService : WarehouseService.WarehouseServiceBase
{
    private readonly WarehouseDbContext _dbContext;

    public WarehouseGrpcService(WarehouseDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public override async Task<BatchStocksResponse> GetBatchStocksInfo(
        BatchStocksRequest request,
        ServerCallContext context)
    {
        var requestedGuids = request.ProductIds
            .Select(id => Guid.TryParse(id, out var guid) ? guid : Guid.Empty)
            .Where(guid => guid != Guid.Empty)
            .ToList();

        var dbStocks = await _dbContext.Stocks
            .AsNoTracking()
            .Where(s => requestedGuids.Contains(s.ProductId))
            .GroupBy(s => s.ProductId)
            .Select(g => new StockItemInfo
            {
                ProductId = g.Key.ToString(),
                Quantity = g.Sum(s => s.Quantity)
            })
            .ToListAsync(context.CancellationToken);

        var response = new BatchStocksResponse();
        response.Stocks.AddRange(dbStocks);

        return response;
    }
}
