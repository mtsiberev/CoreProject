namespace MyStore.Domain.Entities;

using MyStore.Domain.Enums;

public class Order
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public required string CustomerName { get; set; }
    public decimal TotalAmount { get; private set; }
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;
    public List<OrderItem> Items { get; private set; } = new();
    public OrderStatus Status { get; set; } = OrderStatus.Processing;


    public void AddItem(Guid productId, string productName, decimal price, int quantity)
    {
        Items.Add(new OrderItem(productId, productName, price, quantity));
        TotalAmount += price * quantity;
    }
}