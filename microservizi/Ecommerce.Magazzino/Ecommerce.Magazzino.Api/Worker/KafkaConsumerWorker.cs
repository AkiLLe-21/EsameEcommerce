using System.Text.Json;
using Ecommerce.Magazzino.Business.Abstraction;
using Utility.Kafka.Abstractions.Clients;

namespace Ecommerce.Magazzino.Api.Worker;

public class KafkaConsumerWorker : BackgroundService {
    private readonly IConsumerClient<string, string> _consumerClient;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<KafkaConsumerWorker> _logger;

    public KafkaConsumerWorker(
        IConsumerClient<string, string> consumerClient,
        IServiceProvider serviceProvider,
        ILogger<KafkaConsumerWorker> logger) {
        _consumerClient = consumerClient;
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken) {
        _consumerClient.Subscribe(new List<string> { "ordine-creato", "pagamento-fallito", "rifornimento-arrivato" });
        _logger.LogInformation("Magazzino Worker: In ascolto su 'ordine-creato' e 'pagamento-fallito'...");

        while (!stoppingToken.IsCancellationRequested) {
            try {
                var result = await _consumerClient.ConsumeAsync(stoppingToken);

                if (result != null && result.Message != null) {
                    _logger.LogInformation($"Ricevuto messaggio su topic: {result.Topic}");

                    using var doc = JsonDocument.Parse(result.Message.Value);

                    using (var scope = _serviceProvider.CreateScope()) {
                        var business = scope.ServiceProvider.GetRequiredService<IBusiness>();

                        if (result.Topic == "ordine-creato") {
                            // --- CASO 1: SCALO MERCE (Avanti) ---
                            if (doc.RootElement.TryGetProperty("Dto", out var dto)) {
                                int id = dto.GetProperty("ProdottoId").GetInt32();
                                int qta = dto.GetProperty("Quantita").GetInt32();

                                await business.DecrementaQuantitaAsync(id, qta, stoppingToken);
                                _logger.LogInformation($"Merce scalata: ID {id}, Qta {qta}");
                            }

                        } else if (result.Topic == "pagamento-fallito") {
                            // --- CASO 2: SAGA ROLLBACK (Indietro) ---
                            int id = doc.RootElement.GetProperty("ProdottoId").GetInt32();
                            int qta = doc.RootElement.GetProperty("Quantita").GetInt32();

                            await business.CompensaOrdinaFallitoAsync(id, qta, stoppingToken);
                            _logger.LogWarning($"SAGA COMPENSAZIONE: Restituita merce ID {id}, Qta {qta}");

                        } else if (result.Topic == "rifornimento-arrivato") {
                            // --- CASO 3: RIFORNISCO IL MAGAZZINO ---
                            int id = doc.RootElement.GetProperty("ProdottoId").GetInt32();
                            int qta = doc.RootElement.GetProperty("Quantita").GetInt32();

                            await business.IncrementaQuantitaAsync(id, qta, stoppingToken);
                            _logger.LogInformation($"MAGAZZINO: Rifornimento completato! ID {id}, +{qta} pezzi.");
                        }
                    }

                    // Conferma lettura a Kafka
                    _consumerClient.Commit(result);
                }
            } catch (Exception ex) {
                _logger.LogError(ex, "Errore worker magazzino");
                // Piccolo ritardo per non bombardare il log se Kafka è giù
                await Task.Delay(5000, stoppingToken);
            }
        }
    }
}