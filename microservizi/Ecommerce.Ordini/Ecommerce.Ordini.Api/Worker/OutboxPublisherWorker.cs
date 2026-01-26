using Ecommerce.Ordini.Repository;
using Utility.Kafka.Abstractions.Clients;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.Ordini.Api.Worker;

public class OutboxPublisherWorker : BackgroundService {
    private readonly IServiceProvider _serviceProvider;
    private readonly IProducerClient<string, string> _producerClient;
    private readonly ILogger<OutboxPublisherWorker> _logger;

    public OutboxPublisherWorker(
        IServiceProvider serviceProvider,
        IProducerClient<string, string> producerClient,
        ILogger<OutboxPublisherWorker> logger) {
        _serviceProvider = serviceProvider;
        _producerClient = producerClient;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken) {
        _logger.LogInformation("Outbox Worker (Publisher) avviato...");

        while (!stoppingToken.IsCancellationRequested) {
            try {
                // Creiamo uno Scope perché DbContext è Scoped, mentre il Worker è Singleton
                using (var scope = _serviceProvider.CreateScope()) {
                    var dbContext = scope.ServiceProvider.GetRequiredService<OrdiniDbContext>();

                    // 1. CERCA MESSAGGI DA SPEDIRE (DataProcessato == NULL)
                    // Prendiamone 10 alla volta
                    var messaggi = await dbContext.OutboxMessages
                        .Where(m => m.DataProcessato == null)
                        .OrderBy(m => m.DataCreazione)
                        .Take(10)
                        .ToListAsync(stoppingToken);

                    if (messaggi.Any()) {
                        foreach (var msg in messaggi) {
                            _logger.LogInformation($"Spedisco Outbox ID {msg.Id} (Topic: {msg.Topic})...");

                            // 2. INVIA A KAFKA
                            // Nota: La tua libreria vuole key e value. Usiamo un GUID come key.
                            await _producerClient.ProduceAsync(msg.Topic, Guid.NewGuid().ToString(), msg.Payload);

                            // 3. MARCA COME SPEDITO
                            msg.DataProcessato = DateTime.UtcNow;
                        }

                        // 4. SALVA LO STATO NEL DB (Questi messaggi non verranno più ripescati)
                        await dbContext.SaveChangesAsync(stoppingToken);

                        _logger.LogInformation($"Spediti {messaggi.Count} messaggi.");
                    }
                }
            } catch (Exception ex) {
                _logger.LogError(ex, "Errore worker outbox");
            }

            // Aspetta 2 secondi prima di ricontrollare il DB
            await Task.Delay(2000, stoppingToken);
        }
    }
}