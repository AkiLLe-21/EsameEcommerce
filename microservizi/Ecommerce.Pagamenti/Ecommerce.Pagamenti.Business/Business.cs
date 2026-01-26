using System.Text.Json;
using Ecommerce.Pagamenti.Business.Abstraction;
using Ecommerce.Pagamenti.Repository.Abstraction;
using Ecommerce.Pagamenti.Repository.Model;

namespace Ecommerce.Pagamenti.Business;

public class Business(IRepository repository) : IBusiness {
    // CORREZIONE QUI: Ho aggiunto int prodottoId e int quantita tra gli argomenti
    public async Task ProcessaPagamentoOrdineAsync(int ordineId, int prodottoId, int quantita, decimal importo, CancellationToken token = default) {
        // 1. Logica 50/50 (Testa o Croce)
        var random = new Random();
        // Genera 0 o 1. Se 0 -> True (Pagato), Se 1 -> False (Fallito)
        bool esitoPositivo = random.Next(0, 2) == 0;

        // Creiamo la transazione locale nel DB Pagamenti
        var pagamento = new Pagamento {
            OrdineId = ordineId,
            Importo = importo,
            Stato = "In Corso",
            DataTransazione = DateTime.UtcNow
        };

        await repository.AddPagamentoAsync(pagamento, token);

        // Simuliamo attesa banca (2 secondi)
        await Task.Delay(2000, token);

        // Aggiorniamo lo stato in base al "lancio della moneta"
        pagamento.Stato = esitoPositivo ? "Riuscito" : "Fallito";

        // 2. Prepariamo l'evento di risposta per la SAGA
        // Ora possiamo usare prodottoId e quantita perché sono passati come argomenti
        var evento = new {
            OrdineId = ordineId,
            Esito = esitoPositivo,
            Data = DateTime.UtcNow,
            ProdottoId = prodottoId, // <--- Ora questa variabile esiste!
            Quantita = quantita      // <--- Ora questa variabile esiste!
        };

        var outbox = new OutboxMessage {
            // Decidiamo il topic in base all'esito
            Topic = esitoPositivo ? "pagamento-riuscito" : "pagamento-fallito",
            Payload = JsonSerializer.Serialize(evento),
            DataCreazione = DateTime.UtcNow
        };

        await repository.AddOutboxAsync(outbox, token);

        // 3. Commit atomico (Salva Pagamento + Outbox insieme)
        await repository.SaveChangesAsync(token);
    }
}