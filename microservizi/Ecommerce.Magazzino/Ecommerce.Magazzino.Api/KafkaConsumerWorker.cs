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
        // 1. Iscrizione
        _consumerClient.Subscribe(new List<string> { "ordine-creato" });
        _logger.LogInformation("Magazzino Worker: In ascolto su 'ordine-creato'...");

        // 2. Loop Infinito
        while (!stoppingToken.IsCancellationRequested) {
            try {
                // Proviamo a consumare.
                var result = await _consumerClient.ConsumeAsync(stoppingToken);

                if (result != null && result.Message != null) {
                    _logger.LogInformation($"Ricevuto messaggio Kafka: {result.Message.Value}");

                    await ProcessaMessaggioAsync(result.Message.Value, stoppingToken);

                    _consumerClient.Commit(result);
                }
            } catch (OperationCanceledException) {
                // Chiusura
                break;
            } catch (Exception ex) {
                // 3. GESTIONE ERRORE (Retry Logic)
                _logger.LogError($"Errore connessione Kafka (Riprovo tra 5s): {ex.Message}");

                // Pausa di 5 secondi prima di riprovare
                await Task.Delay(5000, stoppingToken);
            }
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