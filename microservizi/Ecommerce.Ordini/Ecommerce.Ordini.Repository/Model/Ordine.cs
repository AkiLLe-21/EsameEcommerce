namespace Ecommerce.Ordini.Repository.Model;

public class Ordine {
    public int Id { get; set; }
    public int ProdottoId { get; set; }
    public int Quantita { get; set; }
    public string Stato { get; set; } = "Creato";
    public DateTime DataCreazione { get; set; }
}