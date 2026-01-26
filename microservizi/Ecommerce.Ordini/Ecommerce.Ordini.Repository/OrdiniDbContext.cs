using Microsoft.EntityFrameworkCore;
using Ecommerce.Ordini.Repository.Model;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace Ecommerce.Ordini.Repository;

public class OrdiniDbContext(DbContextOptions<OrdiniDbContext> options) : DbContext(options) {
    public DbSet<Ordine> Ordini { get; set; }
    public DbSet<OutboxMessage> OutboxMessages { get; set; }
    protected override void OnModelCreating(ModelBuilder modelBuilder) {
        modelBuilder.Entity<Ordine>().HasKey(x => x.Id);
        modelBuilder.Entity<Ordine>().Property(x => x.Id).ValueGeneratedOnAdd();
    }
}