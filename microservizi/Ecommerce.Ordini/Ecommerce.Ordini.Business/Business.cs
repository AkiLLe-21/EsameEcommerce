using Microsoft.Extensions.Logging;
using Ecommerce.Ordini.Business.Abstraction;
using Ecommerce.Ordini.Repository.Abstraction;
using Ecommerce.Ordini.Repository.Model;
using Ecommerce.Ordini.Shared;
using Ecommerce.Magazzino.ClientHttp.Abstraction; // Dal pacchetto NuGet
using Utility.Kafka.Abstractions.Clients; // Dal pacchetto NuGet
using Utility.Kafka.Constants; // Dal pacchetto NuGet

namespace Ecommerce.Ordini.Business;

public class Business(
    IRepository repository,
    IMagazzinoClient magazzinoClient, // Client HTTP
    IProducerClient producerClient,   // Kafka Producer
    ILogger<Business> logger
    ) : IBusiness {
    public async Task<OrdineDto?> CreateOrdineAsync(OrdineDto dto, CancellationToken cancellationToken = default) {
        logger.LogInformation("Ricevuta richiesta ordine per prodotto {Id}", dto.ProdottoId);

        // 1. COMUNICAZIONE SINCRONA (HTTP): Chiedo al magazzino se c'è disponibilità
        bool disponibile = await magazzinoClient.CheckAvailabilityAsync(dto.ProdottoId, dto.Quantita, cancellationToken);

        if (!disponibile) {
            logger.LogWarning("Prodotto {Id} non disponibile nel magazzino.", dto.ProdottoId);
            return null; // O lanciare eccezione
        }

        // 2. Salvo l'ordine in stato "Creato"
        var ordine = new Ordine {
            ProdottoId = dto.ProdottoId,
            Quantita = dto.Quantita,
            DataCreazione = DateTime.UtcNow,
            Stato = "Creato"
        };

        await repository.CreateOrdineAsync(ordine, cancellationToken);
        await repository.SaveChangesAsync(cancellationToken);

        // 3. COMUNICAZIONE ASINCRONA (KAFKA): Notifico che l'ordine è stato creato
        // Questo messaggio verrà ascoltato da "Pagamenti" (o dal Magazzino per scalare la merce)
        await producerClient.ProduceAsync("ordine-creato", new Utility.Kafka.Messages.OperationMessage {
            Operation = "Create",
            Model = System.Text.Json.JsonSerializer.Serialize(ordine)
        }, cancellationToken);

        logger.LogInformation("Ordine {Id} creato e notificato su Kafka", ordine.Id);

        dto.Id = ordine.Id;
        dto.Stato = ordine.Stato;
        dto.DataCreazione = ordine.DataCreazione;

        return dto;
    }
}