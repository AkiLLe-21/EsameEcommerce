using System.Text.Json;
using Ecommerce.Magazzino.Repository.Abstraction;
using Ecommerce.Magazzino.Repository.Model;
using Microsoft.EntityFrameworkCore;

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

    public async Task DecrementaQuantitaAsync(int prodottoId, int quantita, CancellationToken token = default) {
        var prodotto = await context.Prodotti.FindAsync(new object[] { prodottoId }, token);

        if (prodotto != null) {
            // 1. Decremento 
            if (prodotto.QuantitaDisponibile >= quantita) {
                prodotto.QuantitaDisponibile -= quantita;

                // --- 2. Controllo Sotto Scorta ---
                if (prodotto.QuantitaDisponibile < prodotto.SogliaMinima) {
                    var evento = new {
                        ProdottoId = prodotto.Id,
                        QuantitaRichiesta = prodotto.QuantitaRiordino, // Usiamo la qta configurata
                        Data = DateTime.UtcNow
                    };

                    var outbox = new OutboxMessage {
                        Topic = "sotto-scorta",
                        Payload = JsonSerializer.Serialize(evento),
                        DataCreazione = DateTime.UtcNow
                    };

                    await context.OutboxMessages.AddAsync(outbox, token);
                }

                // 3. Salvataggio Atomico
                await context.SaveChangesAsync(token);
            }
        }
    }

    public async Task IncrementaQuantitaAsync(int prodottoId, int quantita, CancellationToken token = default) {
        Console.WriteLine($"[DEBUG DB] 🟢 TENTATIVO INCREMENTO. Cerco Prodotto ID: {prodottoId}. Quantità da aggiungere: {quantita}");

        // Cerchiamo il prodotto
        var prodotto = await context.Prodotti.FindAsync(new object[] { prodottoId }, token);

        if (prodotto != null) {
            Console.WriteLine($"[DEBUG DB] ✅ Prodotto TROVATO: {prodotto.Nome} (ID: {prodotto.Id}). Quantità attuale: {prodotto.QuantitaDisponibile}");

            // Modifica
            prodotto.QuantitaDisponibile += quantita;

            Console.WriteLine($"[DEBUG DB] 🔄 Nuova quantità calcolata: {prodotto.QuantitaDisponibile}. Chiamo SaveChanges...");

            // Forza l'update esplicito (per sicurezza)
            context.Prodotti.Update(prodotto);

            // Salvataggio
            int righeModificate = await context.SaveChangesAsync(token);

            Console.WriteLine($"[DEBUG DB] 💾 SaveChanges completato. Righe toccate nel DB: {righeModificate}");

            if (righeModificate == 0) {
                Console.WriteLine($"[DEBUG DB] ⚠️ ATTENZIONE: SaveChanges ha restituito 0! I dati non sono cambiati. Forse EF Core non ha rilevato modifiche?");
            }
        } else {
            // ECCO IL COLPEVOLE SE LO VEDI
            Console.WriteLine($"[DEBUG DB] ❌ ERRORE CRITICO: Il prodotto con ID {prodottoId} NON ESISTE nel database! Il messaggio Kafka verrà ignorato.");
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