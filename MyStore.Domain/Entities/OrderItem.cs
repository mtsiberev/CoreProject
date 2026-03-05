namespace MyStore.Domain.Entities
{
    public record OrderItem(string ProductName, decimal Price, int Quantity)
    {
        public Guid Id { get; init; } = Guid.NewGuid();
    }
}
