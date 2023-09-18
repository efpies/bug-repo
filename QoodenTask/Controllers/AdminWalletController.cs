using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QoodenTask.Common;
using QoodenTask.Enums;
using QoodenTask.ServiceInterfaces;

namespace QoodenTask.Controllers;

[Authorize(Roles = Roles.Admin)]
[Route("admin/wallets")]
public class AdminWalletController: ControllerBase
{
    [HttpGet("tx")]
    public async Task<IActionResult> GetTxs([FromServices] ITransactionService transactionService)
    {
        return Ok(await transactionService.GetAllTxs());
    }
    
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