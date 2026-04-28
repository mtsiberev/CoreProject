using Microsoft.EntityFrameworkCore;

namespace MyStore.Warehouse.Data
{
    public class WarehouseDbContext(DbContextOptions<WarehouseDbContext> options) : DbContext(options)
    {

    }
}
