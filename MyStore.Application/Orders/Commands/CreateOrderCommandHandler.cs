using MediatR;
using MyStore.Application.Common.Interfaces;
using MyStore.Application.Orders.Commands;
using MyStore.Domain.Entities;

public class CreateOrderCommandHandler(IOrderRepository repository, IApplicationDbContext context)
    : IRequestHandler<CreateOrderCommand, Guid>
{
    public async Task<Guid> Handle(CreateOrderCommand request, CancellationToken cancellationToken)
    {
        var order = new Order { CustomerName = request.CustomerName };
        order.AddItem(request.ProductName, request.Price, 1);

        await repository.AddAsync(order, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);

        return order.Id;
    }
}