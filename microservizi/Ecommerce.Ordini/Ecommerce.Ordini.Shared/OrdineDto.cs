namespace Ecommerce.Ordini.Shared;

public class OrdineDto {
    public int Id { get; set; }
    public int ProdottoId { get; set; }
    public int Quantita { get; set; }
    public string? Stato { get; set; } // "Creato", "Confermato", "Annullato"
    public DateTime DataCreazione { get; set; }
}