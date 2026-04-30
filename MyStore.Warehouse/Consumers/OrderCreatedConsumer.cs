using MassTransit;
using Microsoft.EntityFrameworkCore;
using MyStore.Contracts.Events;
using MyStore.Warehouse.Data;

namespace MyStore.Warehouse.Consumers;

public class OrderCreatedConsumer(WarehouseDbContext db, ILogger<OrderCreatedConsumer> logger)
    : IConsumer<OrderCreatedEvent>
{

    public async Task Consume(ConsumeContext<OrderCreatedEvent> context)
    {
        var ct = context.CancellationToken;

        var productIds = context.Message.Items.Select(i => i.ProductId).OrderBy(id => id).ToList();

        await using var transaction = await db.Database.BeginTransactionAsync(ct);
        try
        {
            var stocks = await db.Stocks
                .FromSqlRaw("SELECT * FROM warehouse.\"Stocks\" WHERE \"ProductId\" = ANY({0}) FOR UPDATE", productIds)
                .ToListAsync(ct);

            foreach (var item in context.Message.Items)
            {
                var stock = stocks.FirstOrDefault(s => s.ProductId == item.ProductId);

                if (stock == null || stock.Quantity < item.Quantity)
                {
                    logger.LogWarning("Product {Id} is not available", item.ProductId);
                    await transaction.RollbackAsync(ct);
                    return;
                }

                await Task.Delay(2000, ct);
                stock.Quantity -= item.Quantity;
            }

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
