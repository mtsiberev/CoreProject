using MassTransit;
using Microsoft.EntityFrameworkCore;
using MyStore.Contracts.Events;
using MyStore.Warehouse.Consumers;
using MyStore.Warehouse.Data;
using MyStore.Warehouse.Services;
using Npgsql;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.Seq("http://seq:5341")
    .CreateLogger();

builder.Host.UseSerilog();

builder.Services.AddOpenTelemetry()
    .ConfigureResource(resource => resource.AddService("MyStore.Warehouse"))
    .WithTracing(tracing => tracing
        .AddAspNetCoreInstrumentation()
        .AddSource("MassTransit")
        .AddNpgsql()
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

    var kafkaBootstrap = builder.Configuration["Kafka:BootstrapServers"] ?? "kafka:9092";

    x.AddRider(rider =>
    {
        rider.AddConsumer<OrderCreatedConsumer>();
        rider.AddConsumer<StockReservedConsumer>();
        rider.AddProducer<string, StockReserved>("stock-reserved-topic");

        rider.UsingKafka((context, k) =>
        {
            if(builder.Configuration.IsKafka())
            {
                k.Host(kafkaBootstrap);

                k.TopicEndpoint<OrderCreated>("order-created-topic", "warehouse-order-group", e =>
                {
                    e.ConfigureConsumer<OrderCreatedConsumer>(context);
                    e.CreateIfMissing(p =>
                    {
                        p.NumPartitions = 1;
                        p.ReplicationFactor = 1;
                    });
                });

                k.TopicEndpoint<StockReserved>("stock-reserved-topic", "warehouse-robot-group", e =>
                {
                    e.ConfigureConsumer<StockReservedConsumer>(context);
                    e.CreateIfMissing(p =>
                    {
                        p.NumPartitions = 1;
                        p.ReplicationFactor = 1;
                    });
                });
            }
            else
            {
                k.Host("localhost:9092");
            }
        });
    });

    if (builder.Configuration.IsKafka())
    {
        x.UsingInMemory((context, cfg) =>
        {
            cfg.ConfigureEndpoints(context);
        });
    }
    else
    {
        x.AddConsumer<OrderCreatedConsumer>();
        x.AddConsumer<StockReservedConsumer>();

        x.UsingRabbitMq((context, cfg) =>
        {
            cfg.Host(builder.Configuration["MessageBroker:Host"] ?? "localhost", "/", h => {
                h.Username("guest");
                h.Password("guest");
            });

            cfg.ConfigureEndpoints(context);
        });
    }
});

var app = builder.Build();

app.MapGrpcService<WarehouseGrpcService>();

app.Run();
