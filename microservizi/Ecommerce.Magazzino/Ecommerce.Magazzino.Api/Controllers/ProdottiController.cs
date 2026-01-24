using Ecommerce.Magazzino.Business.Abstraction;
using Ecommerce.Magazzino.Shared;
using Microsoft.AspNetCore.Mvc;

namespace Ecommerce.Magazzino.Api.Controllers;

[ApiController]
[Route("[controller]/[action]")]
public class ProdottiController(IBusiness business, ILogger<ProdottiController> logger) : ControllerBase {
    [HttpPost(Name = "Create")]
    public async Task<ActionResult<ProdottoDto>> Create(ProdottoDto dto) {
        var result = await business.CreateProdottoAsync(dto);
        return Ok(result);
    }

    [HttpGet(Name = "Get")]
    public async Task<ActionResult<ProdottoDto?>> Get(int id) {
        var result = await business.GetProdottoAsync(id);
        if (result == null) return NotFound();
        return Ok(result);
    }

    [HttpGet(Name = "CheckAvailability")]
    public async Task<ActionResult<bool>> CheckAvailability(int id, int quantita) {
        return Ok(await business.CheckAvailabilityAsync(id, quantita));
    }
}