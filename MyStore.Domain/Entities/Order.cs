namespace MyStore.Domain.Entities;

public class Order
{
    public Guid Id { get; init; } = Guid.NewGuid();

    public required string CustomerName { get; set; }

    public decimal TotalAmount { get; private set; }

    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;

    public List<OrderItem> Items { get; private set; } = new();

    public void AddItem(string productName, decimal price, int quantity)
    {
        if (string.IsNullOrWhiteSpace(productName))
            throw new ArgumentException("productName should not be empty");

        if (price <= 0)
            throw new ArgumentException("price should be greater than 0");

        if (quantity <= 0)
            throw new ArgumentException("quantity should be greater than 0");

        Items.Add(new OrderItem(productName, price, quantity));
        TotalAmount += price * quantity;
    }
}