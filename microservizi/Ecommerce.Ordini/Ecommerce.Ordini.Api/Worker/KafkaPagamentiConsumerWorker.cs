using System.Text.Json;
using Ecommerce.Ordini.Business.Abstraction;
using Utility.Kafka.Abstractions.Clients;

namespace Ecommerce.Ordini.Api.Worker;

public class KafkaPagamentiConsumerWorker : BackgroundService {
    private readonly IConsumerClient<string, string> _consumerClient;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<KafkaPagamentiConsumerWorker> _logger;

    public KafkaPagamentiConsumerWorker(
        IConsumerClient<string, string> consumerClient,
        IServiceProvider serviceProvider,
        ILogger<KafkaPagamentiConsumerWorker> logger) {
        _consumerClient = consumerClient;
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken) {
        // Ci iscriviamo agli eventi dei pagamenti
        _consumerClient.Subscribe(new List<string> { "pagamento-riuscito", "pagamento-fallito" });
        _logger.LogInformation("Ordini Worker: In ascolto su esiti pagamenti...");

        while (!stoppingToken.IsCancellationRequested) {
            try {
                var result = await _consumerClient.ConsumeAsync(stoppingToken);

                if (result != null && result.Message != null) {
                    _logger.LogInformation($"Ricevuto esito pagamento: {result.Topic}");

                    using var doc = JsonDocument.Parse(result.Message.Value);

                    if (doc.RootElement.TryGetProperty("OrdineId", out var idProp)) {
                        int ordineId = idProp.GetInt32();
                        string nuovoStato = result.Topic == "pagamento-riuscito" ? "Confermato" : "Annullato";

                        using (var scope = _serviceProvider.CreateScope()) {
                            var business = scope.ServiceProvider.GetRequiredService<IBusiness>();
                            await business.AggiornaStatoOrdineAsync(ordineId, nuovoStato, stoppingToken);
                        }

                        _logger.LogInformation($"Ordine {ordineId} aggiornato a: {nuovoStato}");
                    }

                    _consumerClient.Commit(result);
                }
            } catch (Exception ex) {
                _logger.LogError(ex, "Errore consumo esito pagamento");
                await Task.Delay(5000, stoppingToken);
            }
        }
    }
}