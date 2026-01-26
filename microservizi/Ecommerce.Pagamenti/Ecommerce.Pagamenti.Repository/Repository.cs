using Ecommerce.Pagamenti.Repository.Abstraction;
using Ecommerce.Pagamenti.Repository.Model;

namespace Ecommerce.Pagamenti.Repository;

public class Repository(PagamentiDbContext context) : IRepository {
    public async Task AddPagamentoAsync(Pagamento pagamento, CancellationToken token = default) {
        await context.Pagamenti.AddAsync(pagamento, token);
    }

    public async Task AddOutboxAsync(OutboxMessage messaggio, CancellationToken token = default) {
        await context.OutboxMessages.AddAsync(messaggio, token);
    }

    public async Task SaveChangesAsync(CancellationToken token = default) {
        await context.SaveChangesAsync(token);
    }
}