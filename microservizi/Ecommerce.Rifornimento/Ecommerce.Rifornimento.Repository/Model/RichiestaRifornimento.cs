namespace Ecommerce.Rifornimento.Repository.Model;

public class RichiestaRifornimento {
    public int Id { get; set; }
    public int ProdottoId { get; set; }
    public int Quantita { get; set; }
    public DateTime DataRichiesta { get; set; }
    public string Stato { get; set; } = string.Empty; // "In Corso", "Completato"
}