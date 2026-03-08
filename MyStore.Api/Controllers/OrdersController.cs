using MediatR;
using Microsoft.AspNetCore.Mvc;
using MyStore.Application.Orders.Commands;
using MyStore.Application.Orders.Queries.GetOrders;
using MyStore.Domain.Entities;

namespace MyStore.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OrdersController(IMediator mediator) : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<Guid>> Create(CreateOrderCommand command)
    {
        var result = await mediator.Send(command);

        return Ok(result);
    }

    [HttpGet]
    public async Task<ActionResult<List<Order>>> Get()
    {
        var result = await mediator.Send(new GetOrdersQuery());

        return Ok(result);
    }

}
