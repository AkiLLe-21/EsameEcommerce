using Ecommerce.Magazzino.Repository.Abstraction;
using Ecommerce.Magazzino.Repository.Model;
using Ecommerce.Magazzino.Repository;
using Microsoft.EntityFrameworkCore;
using Ecommerce.Magazzino.Repository.Abstraction;
using Ecommerce.Magazzino.Repository.Model;

namespace Ecommerce.Magazzino.Repository;

public class Repository(MagazzinoDbContext context) : IRepository {
    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) {
        return await context.SaveChangesAsync(cancellationToken);
    }

    public async Task CreateProdottoAsync(Prodotto prodotto, CancellationToken cancellationToken = default) {
        await context.Prodotti.AddAsync(prodotto, cancellationToken);
    }

    public async Task<Prodotto?> GetProdottoAsync(int id, CancellationToken cancellationToken = default) {
        return await context.Prodotti.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task<bool> CheckDisponibilitaAsync(int id, int quantitaRichiesta, CancellationToken cancellationToken = default) {
        var prodotto = await context.Prodotti.FindAsync([id], cancellationToken);
        if (prodotto == null) return false;
        return prodotto.QuantitaDisponibile >= quantitaRichiesta;
    }
}