using MassTransit;
using Microsoft.EntityFrameworkCore;
using MyStore.Warehouse.Entities;

namespace MyStore.Warehouse.Data;

public class WarehouseDbContext(DbContextOptions<WarehouseDbContext> options) : DbContext(options)
{
    private static readonly Guid StockEntryId = Guid.Parse("00000000-0000-0000-0000-000000000001");
    private static readonly Guid LaptopProductId = Guid.Parse("7f39564c-8367-4a6a-81f1-80775a96860a");

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
                Id = StockEntryId,
                ProductId = LaptopProductId,
                Quantity = 10
            }
        );
    }
}
