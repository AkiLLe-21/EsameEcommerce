namespace Ecommerce.Pagamenti.Business.Abstraction;

public interface IBusiness {
    Task ProcessaPagamentoOrdineAsync(int ordineId, decimal importo, CancellationToken token = default);
}