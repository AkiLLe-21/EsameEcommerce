using System.Text.Json;
using Ecommerce.Pagamenti.Business.Abstraction;
using Utility.Kafka.Abstractions.Clients;

namespace Ecommerce.Pagamenti.Api.Worker;

public class KafkaConsumerWorker : BackgroundService
{
    private readonly IConsumerClient<string, string> _consumerClient;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<KafkaConsumerWorker> _logger;

   public KafkaConsumerWorker(
        IConsumerClient<string, string> consumerClient, 
        IServiceProvider serviceProvider,
        ILogger<KafkaConsumerWorker> logger)
    {
        _consumerClient = consumerClient;
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // 1. Iscrizione al topic "ordine-creato"
        _consumerClient.Subscribe(new List<string> { "ordine-creato" });
        _logger.LogInformation("Pagamenti Worker: In ascolto su 'ordine-creato'...");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var result = await _consumerClient.ConsumeAsync(stoppingToken);

                if (result != null && result.Message != null)
                {
                    _logger.LogInformation($"Ricevuto ordine: {result.Message.Value}");

                    // 2. Deserializziamo il messaggio
                    using var doc = JsonDocument.Parse(result.Message.Value);
                    var dtoElement = doc.RootElement.GetProperty("Dto");

                    // Estraiamo i dati che ci servono
                    int ordineId = dtoElement.GetProperty("Id").GetInt32();
                    int quantita = dtoElement.GetProperty("Quantita").GetInt32();
                    int prodottoId = dtoElement.GetProperty("ProdottoId").GetInt32();
                    decimal importo = quantita * 10.0m;

                    // 3. Eseguiamo la logica Business (in uno scope nuovo)
                    using (var scope = _serviceProvider.CreateScope())
                    {
                        var business = scope.ServiceProvider.GetRequiredService<IBusiness>();
                        await business.ProcessaPagamentoOrdineAsync(ordineId, prodottoId, quantita, importo, stoppingToken);
                    }

                    // 4. Confermiamo la lettura a Kafka
                    _consumerClient.Commit(result);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore durante il consumo del messaggio");
                await Task.Delay(5000, stoppingToken); // Backoff in caso di errore
            }
        }
    }
}