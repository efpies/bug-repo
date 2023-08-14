using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QoodenTask.Common;
using QoodenTask.ServiceInterfaces;

namespace QoodenTask.Controllers;

[Authorize]
[Route("admin/users")]
public class AdminUserController: ControllerBase
{
    [Authorize(Roles = Constants.Admin)]
    [HttpGet]
    public async Task<IActionResult> GetUsers([FromServices] IUserService userService)
    {
        return Ok(await userService.GetAll());
    }
    
    [Authorize(Roles = Constants.Admin)]
    [HttpPatch("block/{userId}")]
    public async Task<IActionResult> BlockUser([FromServices] IUserService userService, int userId)
    {
        if (await userService.GetById(userId) is not { } user)
        {
            return NotFound();
        }
        
        await userService.Block(userId);
        return Ok();
    }
    
    [Authorize(Roles = Constants.Admin)]
    [HttpPatch("unblock/{userId}")]
    public async Task<IActionResult> UnblockUser([FromServices] IUserService userService, int userId)
    {
        if (await userService.GetById(userId, true) is not { } user)
        {
            return NotFound();
        }
        
        await userService.Unblock(userId);
        return Ok();
    }
}