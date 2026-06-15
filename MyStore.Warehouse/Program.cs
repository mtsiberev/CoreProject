using MassTransit;
using Microsoft.EntityFrameworkCore;
using MyStore.Warehouse.Consumers;
using MyStore.Warehouse.Data;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddOpenTelemetry()
    .ConfigureResource(resource => resource.AddService("MyStore.Warehouse"))
    .WithTracing(tracing => tracing
        .AddSource("MassTransit")
        .AddOtlpExporter())
    .WithMetrics(metrics => metrics
        .AddRuntimeInstrumentation()
        .AddOtlpExporter());

builder.Services.AddDbContext<WarehouseDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("WarehouseDb")));

builder.Services.AddMassTransit(x =>
{
    x.AddEntityFrameworkOutbox<WarehouseDbContext>(o =>
    {
        o.UsePostgres();
        o.UseBusOutbox();
    });

    x.AddConsumer<OrderCreatedConsumer>();
    x.AddConsumer<RobotLoaderConsumer>();

    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host(builder.Configuration["MessageBroker:Host"] ?? "localhost", "/", h => {
            h.Username("guest");
            h.Password("guest");
        });

        cfg.UseRawJsonSerializer(isDefault: true);
        cfg.ConfigureEndpoints(context);
    });
});

var host = builder.Build();

host.Run();