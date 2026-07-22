using MediatR;

namespace MyStore.Application.Stocks.Commands
{
    public record StartCsvImportCommand : IRequest;

}
