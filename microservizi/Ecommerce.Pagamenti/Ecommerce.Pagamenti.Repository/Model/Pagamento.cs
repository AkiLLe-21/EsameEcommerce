namespace Ecommerce.Pagamenti.Repository.Model;

public class Pagamento {
    public int Id { get; set; }
    public int OrdineId { get; set; }
    public decimal Importo { get; set; }
    public string Stato { get; set; } = "In Corso"; // In Corso, Riuscito, Fallito
    public DateTime DataTransazione { get; set; } = DateTime.UtcNow;
}