using MediatR;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace MyStore.Application.Common.Behaviors;

public class LoggingBehavior<TRequest, TResponse>(ILogger<TRequest> logger)
    : IPipelineBehavior<TRequest, TResponse> where TRequest : IRequest<TResponse>
{
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;
        var timer = Stopwatch.StartNew();

        logger.LogInformation("Command started: {Name} {@Request}", requestName, request);

        try
        {
            var response = await next();
            timer.Stop();

            logger.LogInformation("Command {Name} completed in {Elapsed}ms", requestName, timer.ElapsedMilliseconds);

            return response;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error executing command {Name}", requestName);
            throw;
        }
    }
}
