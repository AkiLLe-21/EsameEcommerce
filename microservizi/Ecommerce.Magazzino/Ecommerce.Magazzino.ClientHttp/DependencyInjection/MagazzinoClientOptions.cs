namespace Microsoft.Extensions.DependencyInjection;

public class MagazzinoClientOptions {
    public const string SectionName = "MagazzinoClientHttp";
    public string BaseAddress { get; set; } = "";
}