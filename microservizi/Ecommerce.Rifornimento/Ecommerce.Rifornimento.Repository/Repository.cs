using Ecommerce.Rifornimento.Repository.Abstraction;
using Ecommerce.Rifornimento.Repository.Model;

namespace Ecommerce.Rifornimento.Repository;

public class Repository(RifornimentoDbContext context) : IRepository {
    public async Task AddRichiestaAsync(RichiestaRifornimento richiesta, CancellationToken token = default) {
        await context.Richieste.AddAsync(richiesta, token);
    }
    public async Task AddOutboxAsync(OutboxMessage message, CancellationToken token = default) {
        await context.OutboxMessages.AddAsync(message, token);
    }
    public async Task SaveChangesAsync(CancellationToken token = default) {
        await context.SaveChangesAsync(token);
    }
}