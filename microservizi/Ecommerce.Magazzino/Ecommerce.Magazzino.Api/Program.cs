using Ecommerce.Magazzino.Business.Abstraction;
using Ecommerce.Magazzino.Business;
using Ecommerce.Magazzino.Repository.Abstraction;
using Ecommerce.Magazzino.Repository;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// DB Context
builder.Services.AddDbContext<MagazzinoDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("MagazzinoDbContext"),
    b => b.MigrationsAssembly("Ecommerce.Magazzino.Api")));

// Dependency Injection
builder.Services.AddScoped<IRepository, Repository>();
builder.Services.AddScoped<IBusiness, Business>();

var app = builder.Build();

// ... (codice precedente: var app = builder.Build();)

// --- INIZIO BLOCCO AUTO-MIGRAZIONE CON RETRY ---
using (var scope = app.Services.CreateScope()) {
    var services = scope.ServiceProvider;
    var logger = services.GetRequiredService<ILogger<Program>>();
    var context = services.GetRequiredService<MagazzinoDbContext>();

    // Tentiamo la connessione per 10 volte aspettando 3 secondi ogni volta
    for (int i = 0; i < 10; i++) {
        try {
            logger.LogInformation($"Tentativo di connessione al DB ({i + 1}/10)...");

            // Verifica se può connettersi
            if (context.Database.CanConnect()) {
                logger.LogInformation("DB Connesso. Creazione schema se necessario...");
                context.Database.EnsureCreated();
                logger.LogInformation("Database pronto!");
                break; // Usciamo dal ciclo se tutto va bene
            }
        } catch (Exception ex) {
            logger.LogWarning($"Tentativo fallito: {ex.Message}");
        }

        // Se siamo all'ultimo tentativo e fallisce ancora, logghiamo errore grave
        if (i == 9) {
            logger.LogError("Impossibile connettersi al Database dopo vari tentativi.");
        } else {
            // Aspetta 3 secondi prima di riprovare
            System.Threading.Thread.Sleep(3000);
        }
    }
}
// --- FINE BLOCCO ---

// ... (resto del codice)

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment()) {
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

await app.RunAsync();