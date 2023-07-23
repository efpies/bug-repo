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
        var balances = await balanceService.GetBalance(Convert.ToInt32(ClaimsIdentity.DefaultNameClaimType));
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
        if (depositModel is DepositFiatModel depositFiatModel)
        {
            tx = await balanceService.DepositFiat(Convert.ToInt32(ClaimsIdentity.DefaultNameClaimType),
                depositFiatModel, currencyId);
        } else if (depositModel is DepositCryptoModel depositCryptoModel)
        {
            tx = await balanceService.DepositCrypto(Convert.ToInt32(ClaimsIdentity.DefaultNameClaimType),
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
        var txs = await balanceService.GetTxsByUser(Convert.ToInt32(ClaimsIdentity.DefaultNameClaimType));
        return Ok(txs);
    }
}