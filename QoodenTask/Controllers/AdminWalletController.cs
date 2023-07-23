using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QoodenTask.Enums;
using QoodenTask.ServiceInterfaces;

namespace QoodenTask.Controllers;

[Authorize]
[Route("admin/wallets")]
public class AdminWalletController: ControllerBase
{
    [Authorize(Roles = "Admin")]
    [HttpGet("tx")]
    public async Task<IActionResult> GetTxs([FromServices] IBalanceService balanceService)
    {
        return Ok(await balanceService.GetTxs());
    }
    
    [Authorize(Roles = "Admin")]
    [HttpGet("/approve/{txId}")]
    public async Task<IActionResult> ApproveTx([FromServices] IBalanceService balanceService, int txId)
    {
        if (await balanceService.GetTxById(txId) is not { } user)
        {
            return NotFound();
        }

        await balanceService.ChangeStatusTx(txId, TransactionStatusEnum.IsApproved);
        return Ok();
    }
    
    [Authorize(Roles = "Admin")]
    [HttpGet("decline/{txId}")]
    public async Task<IActionResult> DeclineTx([FromServices] IBalanceService balanceService, int txId)
    {
        if (await balanceService.GetTxById(txId) is not { } user)
        {
            return NotFound();
        }

        await balanceService.ChangeStatusTx(txId, TransactionStatusEnum.IsDeclined);
        return Ok();
    }
}