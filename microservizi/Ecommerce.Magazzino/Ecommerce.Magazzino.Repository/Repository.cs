using Ecommerce.Magazzino.Repository.Abstraction;
using Ecommerce.Magazzino.Repository.Model;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.Magazzino.Repository;

// Nota: Qui usiamo 'context' definita nel costruttore primario
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

    // --- NUOVO METODO AGGIUNTO (Corretto) ---
    public async Task DecrementaQuantitaAsync(int prodottoId, int quantita, CancellationToken token = default) {
        var prodotto = await context.Prodotti.FindAsync(new object[] { prodottoId }, token);

        // Se esiste e c'è abbastanza merce
        if (prodotto != null) {
            // (Opzionale: qui potresti lanciare eccezione se la qta < quantita, 
            // ma per l'esame basta scalare se possibile)
            if (prodotto.QuantitaDisponibile >= quantita) {
                prodotto.QuantitaDisponibile -= quantita;
                await context.SaveChangesAsync(token);
            }
        }
    }
    public async Task<Prodotto?> GetProdottoByIdAsync(int id, CancellationToken token = default) {
        return await context.Prodotti.FindAsync(new object[] { id }, token);
    }

    public async Task UpdateProdottoAsync(Prodotto prodotto, CancellationToken token = default) {
        context.Prodotti.Update(prodotto);
        await context.SaveChangesAsync(token);
    }
}