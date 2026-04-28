namespace MyStore.Domain.Entities;

using MyStore.Domain.Enums;

public class Order
{
    public Guid Id { get; init; } = Guid.NewGuid();

    public required string CustomerName { get; set; }

    public decimal TotalAmount { get; private set; }

    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;

    public List<OrderItem> Items { get; private set; } = new();

    public required OrderStatus Status { get; set; }

    public void AddItem(string productName, decimal price, int quantity)
    {
        Items.Add(new OrderItem(productName, price, quantity));
        TotalAmount += price * quantity;
    }
}