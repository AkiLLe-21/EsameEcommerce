using Ecommerce.Pagamenti.Api.Worker;
using Ecommerce.Rifornimento.Api.Worker;
using Ecommerce.Rifornimento.Business.Abstraction;
using Ecommerce.Rifornimento.Business;
using Ecommerce.Rifornimento.Repository.Abstraction;
using Ecommerce.Rifornimento.Repository;
using Microsoft.EntityFrameworkCore;
using Utility.Kafka.Abstractions.Clients;
using Utility.Kafka.Clients;

var builder = WebApplication.CreateBuilder(args);

// 1. DB
builder.Services.AddDbContext<RifornimentoDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("RifornimentoDbContext")));

// 2. KAFKA (Serve sia Producer che Consumer)
builder.Services.Configure<KafkaProducerClientOptions>(builder.Configuration.GetSection("Kafka:ProducerClient"));
builder.Services.AddSingleton<IProducerClient<string, string>, ProducerClient>();

builder.Services.Configure<KafkaConsumerClientOptions>(builder.Configuration.GetSection("Kafka:ConsumerClient"));
builder.Services.AddSingleton<IConsumerClient<string, string>, ConsumerClient>();

// 3. DI
builder.Services.AddScoped<IRepository, Repository>();
builder.Services.AddScoped<IBusiness, Business>();

// 4. Workers
builder.Services.AddHostedService<KafkaConsumerWorker>();
builder.Services.AddHostedService<OutboxPublisherWorker>();

var app = builder.Build();

// 5. Auto-Create DB
using (var scope = app.Services.CreateScope()) {
    var db = scope.ServiceProvider.GetRequiredService<RifornimentoDbContext>();
    // ... logica retry ensure created ...
    db.Database.EnsureCreated();
}

app.Run();