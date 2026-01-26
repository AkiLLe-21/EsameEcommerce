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
    public async Task DecrementaQuantitaAsync(int prodottoId, int quantitaDaScalare, CancellationToken token = default) {
        // Usiamo 'context' invece di '_dbContext'
        var prodotto = await context.Prodotti.FindAsync(new object[] { prodottoId }, token);

        if (prodotto != null) {
            // Usiamo 'QuantitaDisponibile' come fai negli altri metodi
            prodotto.QuantitaDisponibile -= quantitaDaScalare;

            // Sicurezza: non scendere sotto zero
            if (prodotto.QuantitaDisponibile < 0)
                prodotto.QuantitaDisponibile = 0;

            // Non serve chiamare SaveChanges qui perché lo farà il Business
        }
    }
}