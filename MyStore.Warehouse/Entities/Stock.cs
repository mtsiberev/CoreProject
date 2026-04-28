namespace MyStore.Warehouse.Entities;

public class Stock
{
    public Guid Id { get; init; } = Guid.NewGuid();

    public required Guid ProductId { get; set; }

    public int Quantity { get; set; }
}
