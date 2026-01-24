using Ecommerce.Magazzino.Shared;

namespace Ecommerce.Magazzino.ClientHttp.Abstraction;

public interface IMagazzinoClient {
    Task<ProdottoDto?> GetProdottoAsync(int id, CancellationToken cancellationToken = default);
    Task<bool> CheckAvailabilityAsync(int id, int quantita, CancellationToken cancellationToken = default);
}