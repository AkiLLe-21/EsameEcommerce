using System.Text.Json;
using Ecommerce.Rifornimento.Business.Abstraction;
using Ecommerce.Rifornimento.Repository.Abstraction;
using Ecommerce.Rifornimento.Repository.Model;

namespace Ecommerce.Rifornimento.Business;

public class Business(IRepository repository) : IBusiness {
    public async Task ProcessaRifornimentoAsync(int prodottoId, int quantita, CancellationToken token = default) {
        // 1. Salviamo la richiesta "In Corso"
        var richiesta = new RichiestaRifornimento {
            ProdottoId = prodottoId,
            Quantita = quantita,
            DataRichiesta = DateTime.UtcNow,
            Stato = "In Corso"
        };
        await repository.AddRichiestaAsync(richiesta, token);
        await repository.SaveChangesAsync(token);

        // 2. SIMULAZIONE DEL FORNITORE
        // Aspettiamo 10 secondi
        await Task.Delay(10000, token);

        // 3. Merce arrivata: Aggiorniamo stato DB
        richiesta.Stato = "Completato";

        // 4. Prepariamo l'evento per il Magazzino
        var eventoRisposta = new {
            ProdottoId = prodottoId,
            Quantita = quantita,
            DataConsegna = DateTime.UtcNow
        };

        var outbox = new OutboxMessage {
            Topic = "rifornimento-arrivato",
            Payload = JsonSerializer.Serialize(eventoRisposta),
            DataCreazione = DateTime.UtcNow
        };

        await repository.AddOutboxAsync(outbox, token);
        await repository.SaveChangesAsync(token);
    }
}