using Ecommerce.Magazzino.ClientHttp.Abstraction;
using Ecommerce.Magazzino.ClientHttp;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.Extensions.DependencyInjection;

public static class MagazzinoClientExtensions {
    public static IServiceCollection AddMagazzinoClient(this IServiceCollection services, IConfiguration configuration) {
        var section = configuration.GetSection(MagazzinoClientOptions.SectionName);
        var options = section.Get<MagazzinoClientOptions>() ?? new MagazzinoClientOptions();

        services.AddHttpClient<IMagazzinoClient, MagazzinoClient>(client => {
            client.BaseAddress = new Uri(options.BaseAddress);
        }).ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler {
            ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => true
        });

        return services;
    }
}