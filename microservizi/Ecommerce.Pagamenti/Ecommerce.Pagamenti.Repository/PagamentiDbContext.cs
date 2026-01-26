using System.Collections.Generic;
using Ecommerce.Pagamenti.Repository.Model;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.Pagamenti.Repository;

public class PagamentiDbContext(DbContextOptions<PagamentiDbContext> options) : DbContext(options) {
    public DbSet<Pagamento> Pagamenti { get; set; }
    public DbSet<OutboxMessage> OutboxMessages { get; set; }
}