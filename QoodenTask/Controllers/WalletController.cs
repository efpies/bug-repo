using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QoodenTask.Common;
using QoodenTask.Extensions;
using QoodenTask.Models;
using QoodenTask.Models.Deposit;
using QoodenTask.ServiceInterfaces;

namespace QoodenTask.Controllers;

[Authorize]
[Route("wallet")]
public class WalletController : ControllerBase
{
    [Authorize(Roles = Roles.User)]
    [HttpGet("balance")]
    public async Task<IActionResult> GetBalance([FromServices] IBalanceService balanceService)
    {
        var userId = User.GetIdFromClaims();

        var balances = await balanceService.GetBalance(userId);
        return Ok(balances);
    }
    
    [Authorize(Roles = Roles.Admin)]
    [HttpGet("balance/{userId}")]
    public async Task<IActionResult> GetBalance([FromServices] IBalanceService balanceService, [FromServices] IUserService userService,int userId)
    {
        if (!await userService.CheckIfExistById(userId))
            return NotFound(userId);
        
        var balances = await balanceService.GetBalance(userId);
        return Ok(balances);
    }

    [Authorize(Roles = Roles.User)]
    [HttpPost("deposit/{currencyId}")]
    public async Task<IActionResult> Deposit([FromServices] IDepositService depositService,
        [FromBody] BaseDepositModel depositModel, string? currencyId)
    {
        Transaction? tx = null;

        var userId = User.GetIdFromClaims();

        try
        {
            if (depositModel is DepositFiatModel depositFiatModel)
            {
                tx = await depositService.DepositFiat(userId,
                    depositFiatModel, currencyId);
            }
            else if (depositModel is DepositCryptoModel depositCryptoModel)
            {
                tx = await depositService.DepositCrypto(userId,
                    depositCryptoModel, currencyId);
            }
        }
        catch (Exception ex) when (ex.Message.Equals("Incorrect currency type"))
        {
            return BadRequest();
        }
        if (tx is not { })
            return NotFound(currencyId);
        return Ok(tx);
    }
    
    [Authorize(Roles = Roles.User)]
    [HttpPatch("cancel/{txId}")]
    public async Task<IActionResult> ApproveTx([FromServices] ITransactionService transactionService, int txId)
    {
        if (await transactionService.GetTxById(txId) is not { } tx)
        {
            return NotFound();
        }

        await transactionService.CancelTx(tx);
        return Ok();
    }

    [Authorize(Roles = Roles.User)]
    [HttpGet("tx")]
    public async Task<IActionResult> GetTxs([FromServices] ITransactionService transactionService)
    {
        var userId = User.GetIdFromClaims();
        var txs = await transactionService.GetTxsByUser(userId);
        return Ok(txs);
    }
}