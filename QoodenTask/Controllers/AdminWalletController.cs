using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QoodenTask.Common;
using QoodenTask.Enums;
using QoodenTask.ServiceInterfaces;

namespace QoodenTask.Controllers;

[Authorize]
[Route("admin/wallets")]
public class AdminWalletController: ControllerBase
{
    [Authorize(Roles = Roles.Admin)]
    [HttpGet("tx")]
    public async Task<IActionResult> GetTxs([FromServices] ITransactionService transactionService)
    {
        return Ok(await transactionService.GetAllTxs());
    }
    
    [Authorize(Roles = Roles.Admin)]
    [HttpPatch("approve/{txId}")]
    public async Task<IActionResult> ApproveTx([FromServices] ITransactionService transactionService, int txId)
    {
        if (await transactionService.GetTxById(txId) is not { } tx)
        {
            return NotFound();
        }

        await transactionService.ApproveTx(tx);
        return Ok();
    }
    
    [Authorize(Roles = Roles.Admin)]
    [HttpPatch("decline/{txId}")]
    public async Task<IActionResult> DeclineTx([FromServices] ITransactionService transactionService, int txId)
    {
        if (await transactionService.GetTxById(txId) is not { } tx)
        {
            return NotFound();
        }

        await transactionService.DeclineTx(tx);
        return Ok();
    }
}