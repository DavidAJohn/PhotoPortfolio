using Microsoft.AspNetCore.Mvc;

namespace PhotoPortfolio.Server.Controllers;

public class CreditsController : BaseApiController
{
    private readonly ICreditService _creditService;

    public CreditsController(ICreditService creditService)
    {
        _creditService = creditService;
    }

    [HttpGet]
    public async Task<IActionResult> GetPhotoCredits()
    {
        var credits = await _creditService.GetPhotoCredits();

        if (credits == null) return NotFound();

        return Ok(credits);
    }
}
