using MassTransit;
using MediatR;
using MyStore.Application.Common.Events;
using MyStore.Application.Common.Interfaces;
using MyStore.Application.Orders.Commands;
using MyStore.Domain.Entities;

public class CreateOrderCommandHandler(IOrderRepository repository, IApplicationDbContext context, IPublishEndpoint publishEndpoint)
    : IRequestHandler<CreateOrderCommand, Guid>
{
    public async Task<Guid> Handle(CreateOrderCommand request, CancellationToken cancellationToken)
    {
        var order = new Order { CustomerName = request.CustomerName };
        order.AddItem(request.ProductName, request.Price, 1);

        await repository.AddAsync(order, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);

        await publishEndpoint.Publish(new OrderCreatedEvent(order.Id, order.CustomerName, order.TotalAmount), cancellationToken);

        return order.Id;
    }
}