using MassTransit;
using Microsoft.EntityFrameworkCore;
using MyStore.Warehouse.Consumers;
using MyStore.Warehouse.Data;

var builder = Host.CreateApplicationBuilder(args);

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
        //cfg.Message<OrderCreatedEvent>(m => m.SetEntityName("order-created-event"));

        cfg.ConfigureEndpoints(context);
    });
});

var host = builder.Build();
host.Run();
