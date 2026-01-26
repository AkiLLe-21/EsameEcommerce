using Microsoft.EntityFrameworkCore;
using Ecommerce.Pagamenti.Business;
using Ecommerce.Pagamenti.Business.Abstraction;
using Ecommerce.Pagamenti.Repository;
using Ecommerce.Pagamenti.Repository.Abstraction;
using Utility.Kafka.Abstractions.Clients;
using Utility.Kafka.Clients;
using Ecommerce.Pagamenti.Api.Worker;

var builder = WebApplication.CreateBuilder(args);

// 1. DB Context
builder.Services.AddDbContext<PagamentiDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("PagamentiDbContext")));

// 2. KAFKA CONFIGURATION (Manuale, stile Ordini)

// A) Configurazione Producer (Per inviare l'esito)
// Legge la sezione "Kafka:ProducerClient" dal docker-compose
builder.Services.Configure<KafkaProducerClientOptions>(builder.Configuration.GetSection("Kafka:ProducerClient"));
builder.Services.AddSingleton<IProducerClient<string, string>, ProducerClient>();

// B) Configurazione Consumer (Per ascoltare gli ordini)
// Legge la sezione "Kafka:ConsumerClient" dal docker-compose
builder.Services.Configure<KafkaConsumerClientOptions>(builder.Configuration.GetSection("Kafka:ConsumerClient"));
builder.Services.AddSingleton<IConsumerClient<string, string>, ConsumerClient>();

// 3. DI (Business & Repository)
builder.Services.AddScoped<IRepository, Repository>();
builder.Services.AddScoped<IBusiness, Business>();

// 4. Background Workers
builder.Services.AddHostedService<KafkaConsumerWorker>();   // Ascolta
builder.Services.AddHostedService<OutboxPublisherWorker>(); // Spedisce

var app = builder.Build();

// 5. Auto-Migrazione DB (Come in Ordini/Magazzino)
using (var scope = app.Services.CreateScope()) {
    var services = scope.ServiceProvider;
    var logger = services.GetRequiredService<ILogger<Program>>();
    var context = services.GetRequiredService<PagamentiDbContext>();

    for (int i = 0; i < 10; i++) {
        try {
            logger.LogInformation($"Tentativo connessione DB Pagamenti ({i + 1}/10)...");
            context.Database.EnsureCreated();
            logger.LogInformation("DB Pagamenti pronto!");
            break;
        } catch (Exception ex) {
            logger.LogWarning($"Tentativo fallito: {ex.Message}");
            if (i == 9) throw;
            Thread.Sleep(3000);
        }
    }
}

app.MapGet("/", () => "Servizio Pagamenti Attivo ??");

app.Run();