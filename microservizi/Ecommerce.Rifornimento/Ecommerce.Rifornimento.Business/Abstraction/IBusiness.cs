namespace Ecommerce.Rifornimento.Business.Abstraction;

public interface IBusiness {
    Task ProcessaRifornimentoAsync(int prodottoId, int quantita, CancellationToken token = default);
}