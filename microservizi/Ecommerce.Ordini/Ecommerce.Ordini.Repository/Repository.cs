using Ecommerce.Ordini.Repository.Abstraction;
using Ecommerce.Ordini.Repository.Model;

namespace Ecommerce.Ordini.Repository;

public class Repository(OrdiniDbContext context) : IRepository {
    public async Task CreateOrdineAsync(Ordine ordine, CancellationToken cancellationToken = default) {
        await context.Ordini.AddAsync(ordine, cancellationToken);
        await context.SaveChangesAsync(cancellationToken); 
    }
    public async Task<Ordine?> GetOrdineAsync(int id, CancellationToken cancellationToken = default) {
        return await context.Ordini.FindAsync([id], cancellationToken);
    }
    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) {
        return await context.SaveChangesAsync(cancellationToken);
    }
    public async Task AggiungiOutboxAsync(OutboxMessage messaggio, CancellationToken token = default) {
        await context.OutboxMessages.AddAsync(messaggio, token);
        await context.SaveChangesAsync(token);
    }
    public async Task UpdateStatoOrdineAsync(int id, string nuovoStato, CancellationToken token = default) {
        var ordine = await context.Ordini.FindAsync(new object[] { id }, token);
        if (ordine != null) {
            ordine.Stato = nuovoStato;
            await context.SaveChangesAsync(token);
        }
    }
}