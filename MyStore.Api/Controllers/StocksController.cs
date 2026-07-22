using MediatR;
using Microsoft.AspNetCore.Mvc;
using MyStore.Application.Stocks.Commands;

namespace MyStore.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class StocksController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> TriggerImport(CancellationToken ct)
    {
        await mediator.Send(new StartCsvImportCommand(), ct);

        return Accepted(new { Message = "Send command to start CSV file import." });
    }
}
