using MediatR;
using MyStore.Application.Common.Interfaces;
using MyStore.Domain.Entities;

namespace MyStore.Application.Orders.Commands;

public class CreateOrderCommandHandler(IApplicationDbContext context)
    : IRequestHandler<CreateOrderCommand, Guid>
{
    public async Task<Guid> Handle(CreateOrderCommand request, CancellationToken cancellationToken)
    {
        var order = new Order { CustomerName = request.CustomerName };
        order.AddItem(request.ProductName, request.Price, 1);
      
        context.Orders.Add(order);
        await context.SaveChangesAsync(cancellationToken);

        return order.Id;
    }
}
