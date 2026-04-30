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
        var product = context.Message.Items.FirstOrDefault();
        if (product == null)
        {
            logger.LogWarning("Товар не указан");
            return;
        }
        var productId = product.ProductId; 

        var stock = await db.Stocks
            .FirstOrDefaultAsync(x => x.ProductId == productId);

        if (stock == null || stock.Quantity <= 0)
        {
            logger.LogWarning("Товара {Id} нет в наличии", productId);
            return;
        }

        // make the bug
        await Task.Delay(2000);

        stock.Quantity -= 1;
        await db.SaveChangesAsync();

        logger.LogInformation("Заказ {OrderId} обработан. Остаток: {Qty}",
            context.Message.OrderId, stock.Quantity);
    }
}
