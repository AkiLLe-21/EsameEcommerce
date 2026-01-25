using Ecommerce.Ordini.Repository.Model;

namespace Ecommerce.Ordini.Repository.Abstraction;

public interface IRepository {
    Task CreateOrdineAsync(Ordine ordine, CancellationToken cancellationToken = default);
    Task<Ordine?> GetOrdineAsync(int id, CancellationToken cancellationToken = default);
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}