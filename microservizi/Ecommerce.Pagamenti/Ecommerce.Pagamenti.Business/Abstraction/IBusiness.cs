namespace Ecommerce.Pagamenti.Business.Abstraction;

public interface IBusiness {
    Task ProcessaPagamentoOrdineAsync(int ordineId, int prodottoId, int quantita, decimal importo, CancellationToken token = default);
}