namespace MyStore.Application.Common.Interfaces;

using Microsoft.EntityFrameworkCore;
using MyStore.Domain.Entities;

public interface IApplicationDbContext
{
    DbSet<Order> Orders { get; }
    DbSet<OrderItem> OrderItems { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
