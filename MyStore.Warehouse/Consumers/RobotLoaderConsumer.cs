using MassTransit;
using MyStore.Contracts.Events;

public class RobotLoaderConsumer : IConsumer<StockReserved>
{
    private readonly ILogger<RobotLoaderConsumer> _logger;
    private static readonly SemaphoreSlim _robotsPool = new SemaphoreSlim(3, 3);
    private static readonly AsyncLocal<DateTime> _jobStartTime = new AsyncLocal<DateTime>();

    public RobotLoaderConsumer(ILogger<RobotLoaderConsumer> logger)
    {
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<StockReserved> context)
    {
        var ct = context.CancellationToken;
        _logger.LogInformation("Order {OrderId} is received. Waiting for robot", context.Message.OrderId);

        await _robotsPool.WaitAsync(ct);

        try
        {
            _jobStartTime.Value = DateTime.UtcNow;
            _logger.LogInformation("Robot got Order {OrderId}. Loading...", context.Message.OrderId);

            foreach (var item in context.Message.Items)
            {
                _logger.LogDebug("Robot got Product {ProductId}", item.ProductId);
                await Task.Delay(new Random().Next(500, 3000), ct);         
            }
            await FinishJobAsync(context.Message.OrderId);

            _logger.LogInformation("Robot finished Order {OrderId} loading", context.Message.OrderId);
        }
        finally
        {
            _robotsPool.Release();
        }
    }

    private async Task FinishJobAsync(Guid orderId)
    {
        var duration = DateTime.UtcNow - _jobStartTime.Value;
        if (duration.TotalSeconds > 2)
        {
            _logger.LogWarning("Order {OrderId} takes so long time:{sec} sec",
                orderId, duration.TotalSeconds);
        }
        else
        {
            _logger.LogInformation("Order {OrderId} completed in time: {sec} sec",
                orderId, duration.TotalSeconds);
        }
    }
}
