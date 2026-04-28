using MassTransit;
using Microsoft.EntityFrameworkCore;
using MyStore.Warehouse.Data;

var builder = Host.CreateApplicationBuilder(args);

// 1. Настройка БД (WarehouseDbContext создадим на следующем шаге)
builder.Services.AddDbContext<WarehouseDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("WarehouseDb")));

// 2. Настройка MassTransit
builder.Services.AddMassTransit(x =>
{
    // Добавляем Outbox (обязательно для надежности)
    x.AddEntityFrameworkOutbox<WarehouseDbContext>(o =>
    {
        o.UsePostgres();
        o.UseBusOutbox();
    });

    // Регистрация будущего Consumer
    // x.AddConsumer<OrderCreatedConsumer>(); 

    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host(builder.Configuration["MessageBroker:Host"] ?? "localhost", "/", h => {
            h.Username("guest");
            h.Password("guest");
        });
        cfg.ConfigureEndpoints(context);
    });
});

var host = builder.Build();
host.Run();
