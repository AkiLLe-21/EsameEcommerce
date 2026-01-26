using Microsoft.AspNetCore.Mvc;
using Ecommerce.Ordini.Business.Abstraction;
using Ecommerce.Ordini.Shared;
using Ecommerce.Magazzino.ClientHttp.Abstraction;

namespace Ecommerce.Ordini.Api.Controllers;

[ApiController]
[Route("[controller]")]
public class OrdiniController : ControllerBase {
    private readonly IBusiness _business;
    // 1. INIETTIAMO IL CLIENT HTTP
    private readonly IMagazzinoClient _magazzinoClient;

    public OrdiniController(IBusiness business, IMagazzinoClient magazzinoClient) {
        _business = business;
        _magazzinoClient = magazzinoClient;
    }

    [HttpPost]
    public async Task<IActionResult> CreateOrdine([FromBody] OrdineDto dto) {
        // --- STEP 1: VALIDAZIONE SINCRONA (HTTP) ---

        var disponibile = await _magazzinoClient.CheckAvailabilityAsync(dto.ProdottoId, dto.Quantita);

        if (!disponibile) {
            return BadRequest($"Ci dispiace, il prodotto {dto.ProdottoId} non ha sufficiente disponibilità.");
        }

        // --- STEP 2: PROCEDURA ASINCRONA (KAFKA) ---
        var ordineCreato = await _business.CreateOrdineAsync(dto);
        return Ok(ordineCreato);
    }
}