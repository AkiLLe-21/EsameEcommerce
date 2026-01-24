using Ecommerce.Magazzino.Repository.Model;
using System.Collections.Generic;
using System.Reflection.Emit;
using Microsoft.EntityFrameworkCore;
using Ecommerce.Magazzino.Repository.Model;

namespace Ecommerce.Magazzino.Repository;

public class MagazzinoDbContext(DbContextOptions<MagazzinoDbContext> options) : DbContext(options) {
    public DbSet<Prodotto> Prodotti { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder) {
        modelBuilder.Entity<Prodotto>().HasKey(x => x.Id);
        modelBuilder.Entity<Prodotto>().Property(x => x.Id).ValueGeneratedOnAdd();
    }
}