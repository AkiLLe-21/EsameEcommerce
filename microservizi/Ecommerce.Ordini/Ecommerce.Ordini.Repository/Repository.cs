using Ecommerce.Ordini.Repository.Abstraction;
using Ecommerce.Ordini.Repository.Model;

namespace Ecommerce.Ordini.Repository;

public class Repository(OrdiniDbContext context) : IRepository {
    public async Task CreateOrdineAsync(Ordine ordine, CancellationToken cancellationToken = default) {
        await context.Ordini.AddAsync(ordine, cancellationToken);
    }
    public async Task<Ordine?> GetOrdineAsync(int id, CancellationToken cancellationToken = default) {
        return await context.Ordini.FindAsync([id], cancellationToken);
    }
    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) {
        return await context.SaveChangesAsync(cancellationToken);
    }
    public async Task AggiungiOutboxAsync(OutboxMessage messaggio, CancellationToken token = default) {
        await context.OutboxMessages.AddAsync(messaggio, token);
    }
}