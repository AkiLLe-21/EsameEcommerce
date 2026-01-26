using System.Text.Json;
using System.Text.Json.Nodes;
using Ecommerce.Magazzino.Business.Abstraction;
using Utility.Kafka.Abstractions.Clients;
using Confluent.Kafka; // <--- FONDAMENTALE per ConsumeResult

namespace Ecommerce.Magazzino.Api.Worker;

public class KafkaConsumerWorker : BackgroundService {
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<KafkaConsumerWorker> _logger;
    private readonly IConsumerClient<string, string> _consumerClient;

    public KafkaConsumerWorker(
        IServiceProvider serviceProvider,
        ILogger<KafkaConsumerWorker> logger,
        IConsumerClient<string, string> consumerClient) {
        _serviceProvider = serviceProvider;
        _logger = logger;
        _consumerClient = consumerClient;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken) {
        // 1. Iscrizione al topic
        _consumerClient.Subscribe(new List<string> { "ordine-creato" });
        _logger.LogInformation("Magazzino Worker: In ascolto su 'ordine-creato'...");

        try {
            while (!stoppingToken.IsCancellationRequested) {
                var result = await _consumerClient.ConsumeAsync(stoppingToken);

                if (result != null && result.Message != null) {
                    _logger.LogInformation($"Ricevuto messaggio Kafka: {result.Message.Value}");

                    // 3. Elaborazione
                    await ProcessaMessaggioAsync(result.Message.Value, stoppingToken);

                    // 4. Commit
                    _consumerClient.Commit(result);
                }
            }
        } catch (OperationCanceledException) {
            // Stop richiesto, tutto ok
        } catch (Exception ex) {
            _logger.LogError(ex, "Errore critico nel consumer Kafka");
        }
    }

    private async Task ProcessaMessaggioAsync(string jsonMessage, CancellationToken token) {
        using (var scope = _serviceProvider.CreateScope()) {
            // Usiamo IBusiness per scalare la merce
            var business = scope.ServiceProvider.GetRequiredService<IBusiness>();

            try {
                // Parsing del JSON
                var rootNode = JsonNode.Parse(jsonMessage);
                var dtoNode = rootNode?["Dto"];

                if (dtoNode != null) {
                    int pid = dtoNode["ProdottoId"]?.GetValue<int>() ?? 0;
                    int qta = dtoNode["Quantita"]?.GetValue<int>() ?? 0;

                    if (pid > 0 && qta > 0) {
                        _logger.LogInformation($"Scalo {qta} pezzi dal prodotto {pid}...");

                        await business.AggiornaMagazzinoDaOrdineAsync(pid, qta, token);

                        _logger.LogInformation("Magazzino aggiornato con successo!");
                    }
                }
            } catch (Exception ex) {
                _logger.LogError(ex, "Errore elaborazione ordine: {Message}", ex.Message);
            }
        }
    }
}