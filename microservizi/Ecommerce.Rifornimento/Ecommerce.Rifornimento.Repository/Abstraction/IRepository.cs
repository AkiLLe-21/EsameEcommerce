using Ecommerce.Rifornimento.Repository.Model;

namespace Ecommerce.Rifornimento.Repository.Abstraction;

public interface IRepository {
    Task AddRichiestaAsync(RichiestaRifornimento richiesta, CancellationToken token = default);
    Task AddOutboxAsync(OutboxMessage message, CancellationToken token = default);
    Task SaveChangesAsync(CancellationToken token = default);
}