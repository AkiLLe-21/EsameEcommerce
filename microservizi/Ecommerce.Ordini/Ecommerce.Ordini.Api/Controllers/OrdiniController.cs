using Microsoft.AspNetCore.Mvc;
using Ecommerce.Ordini.Business.Abstraction;
using Ecommerce.Ordini.Shared;

namespace Ecommerce.Ordini.Api.Controllers;

[ApiController]
[Route("[controller]")]
public class OrdiniController(IBusiness business) : ControllerBase {
    [HttpPost]
    public async Task<ActionResult<OrdineDto>> Create(OrdineDto dto) {
        var result = await business.CreateOrdineAsync(dto);
        if (result == null)
            return BadRequest("Prodotto non disponibile o errore nella creazione.");

        return Ok(result);
    }
}