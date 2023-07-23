using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QoodenTask.Data;
using QoodenTask.Models;
using QoodenTask.ServiceInterfaces;
using QoodenTask.Services;

namespace QoodenTask.Controllers;

public class RatesController : ControllerBase
{
    [AllowAnonymous]
    [HttpPost("rates")]
    public async Task<IActionResult> GetRates([FromServices] IBalanceService balanceService)
    {
        var currentRates = await balanceService.GetCurrentRates();

        if (currentRates.Rates is null)
            return NotFound(currentRates);
        
        return Ok(currentRates);
    }
}