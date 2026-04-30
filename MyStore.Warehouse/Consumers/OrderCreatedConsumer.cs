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
        await using var transaction = await db.Database.BeginTransactionAsync(ct);
        try
        {
            var productId = context.Message.Items.FirstOrDefault()?.ProductId;
            if (productId == null) return;

            var stock = await db.Stocks
                .FromSqlRaw("SELECT * FROM warehouse.\"Stocks\" WHERE \"ProductId\" = {0} FOR UPDATE", productId)
                .FirstOrDefaultAsync(ct);

            if (stock == null || stock.Quantity <= 0)
            {
                logger.LogWarning("Product {Id} is not available", productId);
                return;
            }

            await Task.Delay(2000, ct);

            stock.Quantity -= 1;

            await db.SaveChangesAsync(ct);
            await transaction.CommitAsync(ct);

            logger.LogInformation("Order {OrderId} processed. Stock: {Qty}",
                context.Message.OrderId, stock.Quantity);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Order processing error");
            throw;
        }
    }
}
