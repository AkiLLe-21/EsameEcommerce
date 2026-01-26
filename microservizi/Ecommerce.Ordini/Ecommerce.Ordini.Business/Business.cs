using Ecommerce.Ordini.Business.Abstraction;
using Ecommerce.Ordini.Repository.Abstraction;
using Ecommerce.Ordini.Repository.Model;
using Ecommerce.Ordini.Shared;
using System.Text.Json;

namespace Ecommerce.Ordini.Business;

public class Business(IRepository repository) : IBusiness {
    public async Task<OrdineDto?> CreateOrdineAsync(OrdineDto dto, CancellationToken cancellationToken = default) {
        // Mappiamo il DTO nell'Entità
        var ordine = new Ordine {
            ProdottoId = dto.ProdottoId,
            Quantita = dto.Quantita,
            Stato = "Creato",
            DataCreazione = DateTime.UtcNow
            // Nota: Id è 0 qui
        };

        await repository.CreateOrdineAsync(ordine, cancellationToken);

        var evento = new {
            Operation = "Create",
            Dto = new {
                Id = ordine.Id,
                ProdottoId = ordine.ProdottoId,
                Quantita = ordine.Quantita,
                Stato = ordine.Stato,
                DataCreazione = ordine.DataCreazione
            }
        };

        var outbox = new OutboxMessage {
            Topic = "ordine-creato",
            Payload = JsonSerializer.Serialize(evento),
            DataCreazione = DateTime.UtcNow
        };

        //Salviamo l'Outbox
        await repository.AggiungiOutboxAsync(outbox, cancellationToken);


        return dto;
    }

    public async Task AggiornaStatoOrdineAsync(int id, string stato, CancellationToken token = default) {
        await repository.UpdateStatoOrdineAsync(id, stato, token);
    }
}