using System.Text.Json;
using Ecommerce.Rifornimento.Business.Abstraction;
using Utility.Kafka.Abstractions.Clients;

namespace Ecommerce.Rifornimento.Api.Worker;

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
        _consumerClient.Subscribe(new List<string> { "sotto-scorta" });
        _logger.LogInformation("FORNITORE: In attesa di ordini...");

        while (!stoppingToken.IsCancellationRequested) {
            try {
                var result = await _consumerClient.ConsumeAsync(stoppingToken);
                if (result != null) {
                    using var doc = JsonDocument.Parse(result.Message.Value);

                    int id = doc.RootElement.GetProperty("ProdottoId").GetInt32();
                    int qta = doc.RootElement.GetProperty("QuantitaRichiesta").GetInt32();

                    _logger.LogInformation($"ORDINE RICEVUTO: ID {id}, Pezzi {qta}. Spedizione in corso...");

                    using (var scope = _serviceProvider.CreateScope()) {
                        var business = scope.ServiceProvider.GetRequiredService<IBusiness>();
                        await business.ProcessaRifornimentoAsync(id, qta, stoppingToken);
                    }

                    _logger.LogInformation($"ORDINE SPEDITO: ID {id}");
                    _consumerClient.Commit(result);
                }
            } catch (Exception ex) {
                _logger.LogError(ex, "Errore fornitore");
                await Task.Delay(5000);
            }
        }
    }
}