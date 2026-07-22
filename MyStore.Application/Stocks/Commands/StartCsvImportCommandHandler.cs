using MassTransit;
using MediatR;
using MyStore.Application.Common.Interfaces;
using MyStore.Contracts.Events;

namespace MyStore.Application.Stocks.Commands;

public class StartCsvImportCommandHandler(
    ITopicProducer<string, StartCsvImport> kafkaProducer,
    IApplicationDbContext context)
    : IRequestHandler<StartCsvImportCommand>
{
    public async Task Handle(StartCsvImportCommand request, CancellationToken ct)
    {
        await kafkaProducer.Produce(Guid.NewGuid().ToString(), new StartCsvImport(), ct);

        await context.SaveChangesAsync(ct);
    }
}
