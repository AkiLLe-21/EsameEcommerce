using Ecommerce.Magazzino.Repository;
using Microsoft.EntityFrameworkCore;
using Utility.Kafka.Abstractions.Clients;

namespace Ecommerce.Magazzino.Api.Worker;

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
                using (var scope = _serviceProvider.CreateScope()) {
                    var dbContext = scope.ServiceProvider.GetRequiredService<MagazzinoDbContext>();

                    // 1. Prende i messaggi non processati
                    var messaggi = await dbContext.OutboxMessages
                        .Where(m => m.DataProcessato == null)
                        .OrderBy(m => m.DataCreazione)
                        .Take(10)
                        .ToListAsync(stoppingToken);

                    if (messaggi.Any()) {
                        foreach (var msg in messaggi) {
                            _logger.LogInformation($"Invio evento: {msg.Topic}...");

                            // 2. Invia a Kafka
                            await _producerClient.ProduceAsync(msg.Topic, Guid.NewGuid().ToString(), msg.Payload);

                            // 3. Marca come spedito
                            msg.DataProcessato = DateTime.UtcNow;
                        }

                        await dbContext.SaveChangesAsync(stoppingToken);
                    }
                }
            } catch (Exception ex) {
                _logger.LogError(ex, "Errore invio outbox");
            }

            await Task.Delay(2000, stoppingToken);
        }
    }
}