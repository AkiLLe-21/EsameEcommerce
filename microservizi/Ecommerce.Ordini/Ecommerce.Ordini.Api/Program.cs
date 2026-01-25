using Microsoft.EntityFrameworkCore;
using Ecommerce.Ordini.Business;
using Ecommerce.Ordini.Business.Abstraction;
using Ecommerce.Ordini.Repository;
using Ecommerce.Ordini.Repository.Abstraction;
using Utility.Kafka.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// 1. DB Context
builder.Services.AddDbContext<OrdiniDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("OrdiniDbContext"),
    b => b.MigrationsAssembly("Ecommerce.Ordini.Api")));

// 2. Kafka (Producer)
builder.Services.AddKafka(builder.Configuration);

// 3. Client HTTP Magazzino
// Questo extension method viene dal pacchetto NuGet "Ecommerce.Magazzino.ClientHttp"
builder.Services.AddMagazzinoClient(builder.Configuration);

// 4. DI
builder.Services.AddScoped<IRepository, Repository>();
builder.Services.AddScoped<IBusiness, Business>();

var app = builder.Build();

// AUTO-MIGRAZIONE CON RETRY (Copiato dal Magazzino)
using (var scope = app.Services.CreateScope()) {
    var services = scope.ServiceProvider;
    var logger = services.GetRequiredService<ILogger<Program>>();
    var context = services.GetRequiredService<OrdiniDbContext>();

    for (int i = 0; i < 10; i++) {
        try {
            logger.LogInformation($"Tentativo connessione DB ({i + 1}/10)...");
            context.Database.EnsureCreated();
            logger.LogInformation("DB Ordini pronto!");
            break;
        } catch (Exception ex) {
            logger.LogWarning($"Tentativo fallito: {ex.Message}");
            if (i == 9) throw;
            System.Threading.Thread.Sleep(3000);
        }
    }
}

if (app.Environment.IsDevelopment()) {
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

await app.RunAsync();