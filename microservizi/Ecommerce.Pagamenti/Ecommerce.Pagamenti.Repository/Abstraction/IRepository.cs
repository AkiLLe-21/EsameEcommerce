using Ecommerce.Pagamenti.Repository.Model;

namespace Ecommerce.Pagamenti.Repository.Abstraction;

public interface IRepository {
    Task AddPagamentoAsync(Pagamento pagamento, CancellationToken token = default);
    Task AddOutboxAsync(OutboxMessage messaggio, CancellationToken token = default);
    Task SaveChangesAsync(CancellationToken token = default);
}