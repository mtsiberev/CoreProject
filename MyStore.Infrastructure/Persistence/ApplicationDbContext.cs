using MassTransit;
using Microsoft.EntityFrameworkCore;
using MyStore.Application.Common.Interfaces;
using MyStore.Domain.Entities;
namespace MyStore.Infrastructure.Persistence;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options), IApplicationDbContext
{
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("orders");

        base.OnModelCreating(modelBuilder);

        modelBuilder.AddTransactionalOutboxEntities();

        modelBuilder.Entity<Order>().HasKey(o => o.Id);
        modelBuilder.Entity<OrderItem>().HasKey(x => x.Id);

        modelBuilder.Entity<Order>()
            .HasMany(o => o.Items)
            .WithOne()
            .OnDelete(DeleteBehavior.Cascade);

        base.OnModelCreating(modelBuilder);
    }
}