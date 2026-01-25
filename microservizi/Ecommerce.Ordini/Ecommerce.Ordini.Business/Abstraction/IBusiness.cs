using Ecommerce.Ordini.Shared;

namespace Ecommerce.Ordini.Business.Abstraction;

public interface IBusiness {
    Task<OrdineDto?> CreateOrdineAsync(OrdineDto dto, CancellationToken cancellationToken = default);
}