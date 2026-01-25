using Microsoft.Extensions.Logging;
using Ecommerce.Ordini.Business.Abstraction;
using Ecommerce.Ordini.Repository.Abstraction;
using Ecommerce.Ordini.Repository.Model;
using Ecommerce.Ordini.Shared;
using Ecommerce.Magazzino.ClientHttp.Abstraction;
using Utility.Kafka.Abstractions.Clients;
using Utility.Kafka.Constants;
using Utility.Kafka.Messages;
using System.Text.Json;

namespace Ecommerce.Ordini.Business;

public class Business(
    IRepository repository,
    IMagazzinoClient magazzinoClient,
    IProducerClient<string, string> producerClient, // Client puro stringa/stringa
    ILogger<Business> logger
    ) : IBusiness {
    public async Task<OrdineDto?> CreateOrdineAsync(OrdineDto dto, CancellationToken cancellationToken = default) {
        logger.LogInformation("Ricevuta richiesta ordine per prodotto {Id}", dto.ProdottoId);

        // 1. Check Disponibilità
        bool disponibile = await magazzinoClient.CheckAvailabilityAsync(dto.ProdottoId, dto.Quantita, cancellationToken);

        if (!disponibile) {
            logger.LogWarning("Prodotto {Id} non disponibile.", dto.ProdottoId);
            return null;
        }

        // 2. Salva Ordine
        var ordine = new Ordine {
            ProdottoId = dto.ProdottoId,
            Quantita = dto.Quantita,
            DataCreazione = DateTime.UtcNow,
            Stato = "Creato"
        };

        await repository.CreateOrdineAsync(ordine, cancellationToken);
        await repository.SaveChangesAsync(cancellationToken);

        // 3. Invia messaggio Kafka
        var messageObj = new OperationMessage<Ordine> {
            Operation = "Create",
            Dto = ordine
        };

        string jsonMessage = JsonSerializer.Serialize(messageObj);

        await producerClient.ProduceAsync(
            "ordine-creato",          // Topic
            ordine.Id.ToString(),     // Key (Nuovo parametro mancante!)
            jsonMessage,              // Value (Messaggio JSON)
            cancellationToken         // Token
        );

        logger.LogInformation("Ordine {Id} creato e notificato su Kafka", ordine.Id);

        dto.Id = ordine.Id;
        dto.Stato = ordine.Stato;
        dto.DataCreazione = ordine.DataCreazione;

        return dto;
    }
}