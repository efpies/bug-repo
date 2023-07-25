using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QoodenTask.Models;
using QoodenTask.Models.Deposit;
using QoodenTask.ServiceInterfaces;

namespace QoodenTask.Controllers;

[Authorize]
[Route("wallet")]
public class WalletController : ControllerBase
{
    [Authorize(Roles = "User")]
    [HttpGet("balance")]
    public async Task<IActionResult> GetBalance([FromServices] IBalanceService balanceService)
    {
        var userId = Convert.ToInt32(User.Claims.FirstOrDefault(c => c.Type == ClaimsIdentity.DefaultNameClaimType)?.Value);

        var balances = await balanceService.GetBalance(userId);
        return Ok(balances);
    }
    
    [Authorize(Roles = "Admin")]
    [HttpGet("balance/{userId}")]
    public async Task<IActionResult> GetBalance([FromServices] IBalanceService balanceService, [FromServices] IUserService userService,int userId)
    {
        if (await userService.GetById(userId) is not { })
            return NotFound(userId);
        
        var balances = await balanceService.GetBalance(userId);
        return Ok(balances);
    }
    
    [Authorize(Roles = "User")]
    [HttpPost("deposit/{currencyId}")]
    public async Task<IActionResult> DepositFiat([FromServices] IBalanceService balanceService,
        [FromBody] BaseDepositModel depositModel, string currencyId)
    {
        Transaction tx = null;
        
        var userId = Convert.ToInt32(User.Claims.FirstOrDefault(c => c.Type == ClaimsIdentity.DefaultNameClaimType)?.Value);
        
        if (depositModel is DepositFiatModel depositFiatModel)
        {
            tx = await balanceService.DepositFiat(userId,
                depositFiatModel, currencyId);
        } else if (depositModel is DepositCryptoModel depositCryptoModel)
        {
            tx = await balanceService.DepositCrypto(userId,
                depositCryptoModel, currencyId);
        }
        if (tx is not { })
            return NotFound(currencyId);
        return Ok(tx);
    }

    [Authorize(Roles = "User")]
    [HttpPost("tx")]
    public async Task<IActionResult> GetTxs([FromServices] IBalanceService balanceService)
    {
        var userId = Convert.ToInt32(User.Claims.FirstOrDefault(c => c.Type == ClaimsIdentity.DefaultNameClaimType)?.Value);
        var txs = await balanceService.GetTxsByUser(userId);
        return Ok(txs);
    }
}