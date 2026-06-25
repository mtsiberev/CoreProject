using MassTransit;
using Microsoft.EntityFrameworkCore;
using MyStore.Warehouse.Consumers;
using MyStore.Warehouse.Data;
using MyStore.Warehouse.Services;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenTelemetry()
    .ConfigureResource(resource => resource.AddService("MyStore.Warehouse"))
    .WithTracing(tracing => tracing
        .AddAspNetCoreInstrumentation()
        .AddSource("MassTransit")
        .AddOtlpExporter())
    .WithMetrics(metrics => metrics
        .AddAspNetCoreInstrumentation()
        .AddOtlpExporter());

builder.Services.AddGrpc();

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

var app = builder.Build();

app.MapGrpcService<WarehouseGrpcService>();

app.Run();
