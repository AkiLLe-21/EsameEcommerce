using System.Collections.Generic;
using Ecommerce.Rifornimento.Repository.Model;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.Rifornimento.Repository;

public class RifornimentoDbContext : DbContext {
    public RifornimentoDbContext(DbContextOptions<RifornimentoDbContext> options) : base(options) { }
    public DbSet<RichiestaRifornimento> Richieste { get; set; }
    public DbSet<OutboxMessage> OutboxMessages { get; set; }
}