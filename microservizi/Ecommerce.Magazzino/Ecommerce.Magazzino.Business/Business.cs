using Ecommerce.Magazzino.Business.Abstraction;
using Ecommerce.Magazzino.Repository.Abstraction;
using Ecommerce.Magazzino.Repository.Model;
using Ecommerce.Magazzino.Shared;
using Microsoft.Extensions.Logging;

namespace Ecommerce.Magazzino.Business;

public class Business(IRepository repository, ILogger<Business> logger) : IBusiness {
    public async Task<ProdottoDto> CreateProdottoAsync(ProdottoDto dto, CancellationToken cancellationToken = default) {
        logger.LogInformation("Creazione prodotto {Nome}", dto.Nome);

        var prodotto = new Prodotto {
            Nome = dto.Nome,
            Descrizione = dto.Descrizione,
            Prezzo = dto.Prezzo,
            QuantitaDisponibile = dto.QuantitaDisponibile
        };

        await repository.CreateProdottoAsync(prodotto, cancellationToken);
        await repository.SaveChangesAsync(cancellationToken);

        dto.Id = prodotto.Id; // Assegno l'ID generato
        return dto;
    }

    public async Task<ProdottoDto?> GetProdottoAsync(int id, CancellationToken cancellationToken = default) {
        var p = await repository.GetProdottoAsync(id, cancellationToken);
        if (p == null) return null;

        return new ProdottoDto {
            Id = p.Id,
            Nome = p.Nome,
            Descrizione = p.Descrizione,
            Prezzo = p.Prezzo,
            QuantitaDisponibile = p.QuantitaDisponibile
        };
    }

    public async Task<bool> CheckAvailabilityAsync(int id, int quantita, CancellationToken cancellationToken = default) {
        return await repository.CheckDisponibilitaAsync(id, quantita, cancellationToken);
    }

    public async Task AggiornaMagazzinoDaOrdineAsync(int prodottoId, int quantita, CancellationToken token = default) {
        // Qui richiamiamo il repository
        await repository.DecrementaQuantitaAsync(prodottoId, quantita, token);
        await repository.SaveChangesAsync(token); // Salviamo le modifiche su DB
    }
}