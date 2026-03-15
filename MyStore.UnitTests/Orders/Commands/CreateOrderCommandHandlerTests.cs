using FluentAssertions;
using MassTransit;
using MyStore.Application.Common.Events;
using MyStore.Application.Common.Interfaces;
using MyStore.Application.Orders.Commands;
using MyStore.Domain.Entities;
using NSubstitute;

namespace MyStore.UnitTests.Orders.Commands;

public class CreateOrderCommandHandlerTests
{
    private readonly IOrderRepository _repository = Substitute.For<IOrderRepository>();
    private readonly IApplicationDbContext _context = Substitute.For<IApplicationDbContext>();
    private readonly IPublishEndpoint _publishEndpoint = Substitute.For<IPublishEndpoint>();
    private readonly CreateOrderCommandHandler _handler;

    public CreateOrderCommandHandlerTests()
    {
        _handler = new CreateOrderCommandHandler(_repository, _context, _publishEndpoint);
    }

    [Fact]
    public async Task Handle_Should_CreateOrder_And_PublishEvent()
    {
        var command = new CreateOrderCommand("Ivan", "Laptop", 1500);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().NotBeEmpty();

        await _repository.Received(1).AddAsync(Arg.Any<Order>(), Arg.Any<CancellationToken>());

        await _context.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());

        await _publishEndpoint.Received(1).Publish(Arg.Any<OrderCreatedEvent>(), Arg.Any<CancellationToken>());
    }
}
