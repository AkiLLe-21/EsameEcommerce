using System.Net.Http.Json;
using Ecommerce.Magazzino.Shared;
using Ecommerce.Magazzino.ClientHttp.Abstraction;

namespace Ecommerce.Magazzino.ClientHttp;

public class MagazzinoClient(HttpClient httpClient) : IMagazzinoClient {
    public async Task<ProdottoDto?> GetProdottoAsync(int id, CancellationToken cancellationToken = default) {
        var response = await httpClient.GetAsync($"/Prodotti/Get?id={id}", cancellationToken);
        if (!response.IsSuccessStatusCode) return null;
        return await response.Content.ReadFromJsonAsync<ProdottoDto>(cancellationToken: cancellationToken);
    }

    public async Task<bool> CheckAvailabilityAsync(int id, int quantita, CancellationToken cancellationToken = default) {
        var response = await httpClient.GetAsync($"/Prodotti/CheckAvailability?id={id}&quantita={quantita}", cancellationToken);
        if (!response.IsSuccessStatusCode) return false;
        return await response.Content.ReadFromJsonAsync<bool>(cancellationToken: cancellationToken);
    }
}