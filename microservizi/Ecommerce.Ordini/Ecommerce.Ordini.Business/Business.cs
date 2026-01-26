using Ecommerce.Ordini.Business.Abstraction;
using Ecommerce.Ordini.Repository.Abstraction;
using Ecommerce.Ordini.Repository.Model;
using Ecommerce.Ordini.Shared;
using System.Text.Json;

namespace Ecommerce.Ordini.Business;

public class Business(IRepository repository) : IBusiness {
    public async Task<OrdineDto?> CreateOrdineAsync(OrdineDto dto, CancellationToken token = default) {
        // 1. Creiamo l'Entità dal DTO
        var ordine = new Ordine {
            ProdottoId = dto.ProdottoId,
            Quantita = dto.Quantita,
            Stato = "Creato",
            DataCreazione = DateTime.UtcNow
        };

        // Aggiungiamo al contesto (ma non salviamo ancora)
        await repository.CreateOrdineAsync(ordine, token);

        // 2. Outbox Pattern
        var evento = new {
            Operation = "Create",
            Dto = ordine // Serializziamo l'ordine completo
        };

        var outboxMsg = new OutboxMessage {
            Topic = "ordine-creato",
            Payload = JsonSerializer.Serialize(evento),
            DataCreazione = DateTime.UtcNow,
            DataProcessato = null
        };

        await repository.AggiungiOutboxAsync(outboxMsg, token);

        // 3. Salvataggio Atomico (Transazione implicita)
        await repository.SaveChangesAsync(token);

        // 4. MAPPING DI RITORNO (Fix dell'errore)
        return new OrdineDto {
            Id = ordine.Id,
            ProdottoId = ordine.ProdottoId,
            Quantita = ordine.Quantita,
            Stato = ordine.Stato,
            DataCreazione = ordine.DataCreazione
        };
    }
}