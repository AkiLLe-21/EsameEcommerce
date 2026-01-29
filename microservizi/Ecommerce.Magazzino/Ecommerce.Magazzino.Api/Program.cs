using Ecommerce.Magazzino.Business.Abstraction;
using Ecommerce.Magazzino.Business;
using Ecommerce.Magazzino.Repository.Abstraction;
using Ecommerce.Magazzino.Repository;
using Microsoft.EntityFrameworkCore;
using Utility.Kafka.Abstractions.Clients;
using Utility.Kafka.Clients;
using Ecommerce.Magazzino.Api.Worker;
using Ecommerce.Pagamenti.Api.Worker;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// DB Context
builder.Services.AddDbContext<MagazzinoDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("MagazzinoDbContext"),
    b => b.MigrationsAssembly("Ecommerce.Magazzino.Api")));

builder.Services.Configure<KafkaConsumerClientOptions>(builder.Configuration.GetSection("Kafka:ConsumerClient"));
builder.Services.AddSingleton<IConsumerClient<string, string>, ConsumerClient>();

builder.Services.Configure<KafkaProducerClientOptions>(builder.Configuration.GetSection("Kafka:ProducerClient"));
builder.Services.AddSingleton<IProducerClient<string, string>, ProducerClient>();

builder.Services.AddHostedService<KafkaConsumerWorker>();
builder.Services.AddHostedService<OutboxPublisherWorker>();


// Dependency Injection
builder.Services.AddScoped<IRepository, Repository>();
builder.Services.AddScoped<IBusiness, Business>();

var app = builder.Build();
using (var scope = app.Services.CreateScope()) {
    var context = scope.ServiceProvider.GetRequiredService<MagazzinoDbContext>();
    context.Database.EnsureCreated();
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment()) {
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

await app.RunAsync();