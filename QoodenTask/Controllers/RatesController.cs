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
    [HttpGet("rates")]
    public async Task<IActionResult> GetRates([FromServices] IRateService rateService)
    {
        var currentRates = await rateService.GetCurrentRates();

        if (currentRates is null)
            return NotFound(currentRates);
        
        return Ok(currentRates);
    }
}