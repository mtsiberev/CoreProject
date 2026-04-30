namespace MyStore.Domain.Entities
{
    public record OrderItem(Guid ProductId, string ProductName, decimal Price, int Quantity)
    {
        public Guid Id { get; init; } = Guid.NewGuid();
    }

}
