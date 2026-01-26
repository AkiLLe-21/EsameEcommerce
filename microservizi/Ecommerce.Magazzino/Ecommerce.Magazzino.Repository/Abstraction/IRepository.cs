using Ecommerce.Magazzino.Repository.Model;

namespace Ecommerce.Magazzino.Repository.Abstraction;

public interface IRepository {
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    Task CreateProdottoAsync(Prodotto prodotto, CancellationToken cancellationToken = default);
    Task<Prodotto?> GetProdottoAsync(int id, CancellationToken cancellationToken = default);
    Task<bool> CheckDisponibilitaAsync(int id, int quantitaRichiesta, CancellationToken cancellationToken = default);
    Task DecrementaQuantitaAsync(int prodottoId, int quantita, CancellationToken token = default);
    Task<Prodotto?> GetProdottoByIdAsync(int id, CancellationToken token = default);
    Task UpdateProdottoAsync(Prodotto prodotto, CancellationToken token = default);
}