using MassTransit;
using Microsoft.EntityFrameworkCore;
using MyStore.Contracts.Events;
using MyStore.Warehouse.Data;

namespace MyStore.Warehouse.Consumers;

public class OrderCreatedConsumer(WarehouseDbContext db, ILogger<OrderCreatedConsumer> logger)
    : IConsumer<OrderCreated>
{

    public async Task Consume(ConsumeContext<OrderCreated> context)
    {
        var ct = context.CancellationToken;
        var orderId = context.Message.OrderId;

        var groupedItems = context.Message.Items
            .GroupBy(i => i.ProductId)
            .Select(g => new { ProductId = g.Key, TotalQuantity = g.Sum(x => x.Quantity) })
            .ToList();

        var productIds = groupedItems.Select(i => i.ProductId).OrderBy(id => id).ToList();

        await using var transaction = await db.Database.BeginTransactionAsync(ct);
        try
        {
            var stocks = await db.Stocks
                .FromSqlRaw("SELECT * FROM warehouse.\"Stocks\" WHERE \"ProductId\" = ANY({0}) FOR UPDATE", productIds)
                .ToListAsync(ct);

            foreach (var item in groupedItems)
            {
                var stock = stocks.FirstOrDefault(s => s.ProductId == item.ProductId);

                if (stock == null || stock.Quantity < item.TotalQuantity)
                {
                    logger.LogWarning("Product {Id} is not available for Order {Id}", item.ProductId, orderId);

                    await context.Publish(new StockReservationFailed(orderId, "Product is not available"), ct);

                    await db.SaveChangesAsync(ct);
                    await transaction.CommitAsync(ct);

                    return;
                }
            }

            foreach (var item in groupedItems)
            {
                var stock = stocks.First(s => s.ProductId == item.ProductId);
                stock.Quantity -= item.TotalQuantity;
            }

            await context.Publish(new StockReserved(context.Message.OrderId, context.Message.Items), ct);

            await db.SaveChangesAsync(ct);
            await transaction.CommitAsync(ct);

            logger.LogInformation("Order {OrderId} processed", context.Message.OrderId);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Order {OrderId} processing error", context.Message.OrderId);
            throw;
        }
    }
}
