using Ecommerce.Magazzino.Shared;

namespace Ecommerce.Magazzino.Business.Abstraction;

public interface IBusiness {
    Task<ProdottoDto> CreateProdottoAsync(ProdottoDto prodottoDto, CancellationToken cancellationToken = default);
    Task<ProdottoDto?> GetProdottoAsync(int id, CancellationToken cancellationToken = default);
    Task<bool> CheckAvailabilityAsync(int id, int quantita, CancellationToken cancellationToken = default);
    Task AggiornaMagazzinoDaOrdineAsync(int prodottoId, int quantita, CancellationToken token = default);
}