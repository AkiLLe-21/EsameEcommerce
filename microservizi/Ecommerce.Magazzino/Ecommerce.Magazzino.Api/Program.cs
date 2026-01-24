using Ecommerce.Magazzino.Business.Abstraction;
using Ecommerce.Magazzino.Business;
using Ecommerce.Magazzino.Repository.Abstraction;
using Ecommerce.Magazzino.Repository;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// DB Context
builder.Services.AddDbContext<MagazzinoDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("MagazzinoDbContext"),
    b => b.MigrationsAssembly("Ecommerce.Magazzino.Api")));

// Dependency Injection
builder.Services.AddScoped<IRepository, Repository>();
builder.Services.AddScoped<IBusiness, Business>();

var app = builder.Build();
using (var scope = app.Services.CreateScope()) {
    var context = scope.ServiceProvider.GetRequiredService<MagazzinoDbContext>();
    context.Database.EnsureCreated();
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment()) {
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

await app.RunAsync();