using System.ComponentModel.DataAnnotations.Schema;

namespace Ecommerce.Magazzino.Repository.Model;

public class Prodotto {
    public int Id { get; set; }
    public required string Nome { get; set; }
    public string? Descrizione { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal Prezzo { get; set; }

    public int QuantitaDisponibile { get; set; }
}