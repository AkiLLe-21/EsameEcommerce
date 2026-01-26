using System;

namespace Ecommerce.Ordini.Repository.Model;

public class OutboxMessage {
    public int Id { get; set; }
    public string Topic { get; set; } = string.Empty;
    public string Payload { get; set; } = string.Empty;
    public DateTime DataCreazione { get; set; } = DateTime.UtcNow;
    public DateTime? DataProcessato { get; set; }
}