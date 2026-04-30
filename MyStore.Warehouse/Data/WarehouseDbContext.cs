using MassTransit;
using Microsoft.EntityFrameworkCore;
using MyStore.Warehouse.Entities;

namespace MyStore.Warehouse.Data;

public class WarehouseDbContext(DbContextOptions<WarehouseDbContext> options) : DbContext(options)
{
    private static readonly Guid LaptopStockEntryId = Guid.Parse("1881ac71-f3f2-4ce5-b3fb-3f697a8773b3");
    private static readonly Guid LaptopProductId = Guid.Parse("7f39564c-8367-4a6a-81f1-80775a96860a");

    private static readonly Guid KeyboardStockEntryId = Guid.Parse("20a7bc51-2c67-4e70-a9e1-286285bf62b6");
    private static readonly Guid KeyboardId = Guid.Parse("f13ea1ad-1bd0-4e0f-8a76-11368fe7178d");

    private static readonly Guid MouseStockEntryId = Guid.Parse("f20a12a9-bffd-4d87-8876-badfdca9a12a");
    private static readonly Guid MouseId = Guid.Parse("9e2ad445-3188-46e2-b2c4-a7d70c569017");

    public DbSet<Stock> Stocks => Set<Stock>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("warehouse");

        base.OnModelCreating(modelBuilder);

        modelBuilder.AddInboxStateEntity();
        modelBuilder.AddOutboxMessageEntity();
        modelBuilder.AddOutboxStateEntity();

        modelBuilder.Entity<Stock>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.ProductId).IsRequired();
            entity.Property(e => e.Quantity).IsRequired();
        });

        modelBuilder.Entity<Stock>().HasData(
            new Stock
            {
                Id = LaptopStockEntryId,
                ProductId = LaptopProductId,
                Quantity = 10
            }
        );

        modelBuilder.Entity<Stock>().HasData(
            new Stock
            {
                Id = KeyboardStockEntryId,
                ProductId = KeyboardId,
                Quantity = 20
            }
        );

        modelBuilder.Entity<Stock>().HasData(
            new Stock
            {
                Id = MouseStockEntryId,
                ProductId = MouseId,
                Quantity = 30
            }
        );
    }
}
