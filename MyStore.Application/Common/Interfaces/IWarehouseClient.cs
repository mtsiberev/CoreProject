namespace MyStore.Application.Common.Interfaces;

public interface IWarehouseClient
{
    Task<Dictionary<Guid, int>> GetProductStocksAsync(IEnumerable<Guid> productIds, CancellationToken ct);
}