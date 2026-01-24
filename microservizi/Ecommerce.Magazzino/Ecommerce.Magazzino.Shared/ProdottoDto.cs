namespace Ecommerce.Magazzino.Shared;

public class ProdottoDto {
    public int Id { get; set; }
    public required string Nome { get; set; }
    public string? Descrizione { get; set; }
    public decimal Prezzo { get; set; }
    public int QuantitaDisponibile { get; set; }
}