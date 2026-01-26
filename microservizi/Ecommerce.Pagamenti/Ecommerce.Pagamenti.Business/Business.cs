using System.Text.Json;
using Ecommerce.Pagamenti.Business.Abstraction;
using Ecommerce.Pagamenti.Repository.Abstraction;
using Ecommerce.Pagamenti.Repository.Model;

namespace Ecommerce.Pagamenti.Business;

public class Business(IRepository repository) : IBusiness {
    public async Task ProcessaPagamentoOrdineAsync(int ordineId, decimal importo, CancellationToken token = default) {
        // 1. Creiamo la transazione locale
        var pagamento = new Pagamento {
            OrdineId = ordineId,
            Importo = importo,
            Stato = "In Corso",
            DataTransazione = DateTime.UtcNow
        };

        await repository.AddPagamentoAsync(pagamento, token);

        // 2. Simuliamo attesa banca (es. 2 secondi)
        await Task.Delay(2000, token);

        // 3. Logica Semplificata: Per ora diciamo che va SEMPRE A BUON FINE.
        // (Dopo aggiungeremo il Random per far fallire e testare il rollback)
        bool esitoPositivo = true;

        pagamento.Stato = esitoPositivo ? "Riuscito" : "Fallito";

        // 4. Prepariamo l'evento di risposta per la SAGA
        var evento = new {
            OrdineId = ordineId,
            Esito = esitoPositivo,
            Data = DateTime.UtcNow
        };

        var outbox = new OutboxMessage {
            Topic = esitoPositivo ? "pagamento-riuscito" : "pagamento-fallito",
            Payload = JsonSerializer.Serialize(evento),
            DataCreazione = DateTime.UtcNow
        };

        await repository.AddOutboxAsync(outbox, token);

        // 5. Commit atomico
        await repository.SaveChangesAsync(token);
    }
}