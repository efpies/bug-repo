using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QoodenTask.Common;
using QoodenTask.Models;
using QoodenTask.Models.Deposit;
using QoodenTask.ServiceInterfaces;

namespace QoodenTask.Controllers;

[Authorize]
[Route("wallet")]
public class WalletController : ControllerBase
{
    [Authorize(Roles = Constants.User)]
    [HttpGet("balance")]
    public async Task<IActionResult> GetBalance([FromServices] IBalanceService balanceService)
    {
        var userId = int.Parse(User.Claims.FirstOrDefault(c => c.Type == ClaimsIdentity.DefaultNameClaimType)?.Value);

        var balances = await balanceService.GetBalance(userId);
        return Ok(balances);
    }
    
    [Authorize(Roles = Constants.Admin)]
    [HttpGet("balance/{userId}")]
    public async Task<IActionResult> GetBalance([FromServices] IBalanceService balanceService, [FromServices] IUserService userService,int userId)
    {
        if (!await userService.CheckIfExistById(userId))
            return NotFound(userId);
        
        var balances = await balanceService.GetBalance(userId);
        return Ok(balances);
    }
    
    [Authorize(Roles = Constants.User)]
    [HttpPost("deposit/{currencyId}")]
    public async Task<IActionResult> Deposit([FromServices] IDepositeService depositeService,
        [FromBody] BaseDepositModel depositModel, string currencyId)
    {
        Transaction? tx = null;
        
        var userId = int.Parse(User.Claims.FirstOrDefault(c => c.Type == ClaimsIdentity.DefaultNameClaimType)?.Value);
        
        if (depositModel is DepositFiatModel depositFiatModel)
        {
            tx = await depositeService.DepositFiat(userId,
                depositFiatModel, currencyId);
        } else if (depositModel is DepositCryptoModel depositCryptoModel)
        {
            tx = await depositeService.DepositCrypto(userId,
                depositCryptoModel, currencyId);
        }
        if (tx is not { })
            return NotFound(currencyId);
        return Ok(tx);
    }

    [Authorize(Roles = Constants.User)]
    [HttpGet("tx")]
    public async Task<IActionResult> GetTxs([FromServices] ITransactionService transactionService)
    {
        var userId = int.Parse(User.Claims.FirstOrDefault(c => c.Type == ClaimsIdentity.DefaultNameClaimType)?.Value);
        var txs = await transactionService.GetTxsByUser(userId);
        return Ok(txs);
    }
}