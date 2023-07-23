using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QoodenTask.ServiceInterfaces;

namespace QoodenTask.Controllers;

[Authorize]
[Route("admin/users")]
public class AdminUserController: ControllerBase
{
    [Authorize(Roles = "Admin")]
    [HttpGet]
    public async Task<IActionResult> GetUsers([FromServices] IUserService userService)
    {
        return Ok(await userService.GetAll());
    }
    
    [Authorize(Roles = "Admin")]
    [HttpGet("block/{userId}")]
    public async Task<IActionResult> BlockUser([FromServices] IUserService userService, int userId)
    {
        if (await userService.GetById(userId) is not { } user)
        {
            return NotFound();
        }
        
        userService.Block(userId);
        return Ok();
    }
    
    [Authorize(Roles = "Admin")]
    [HttpGet("unblock/{userId}")]
    public async Task<IActionResult> UnblockUser([FromServices] IUserService userService, int userId)
    {
        if (await userService.GetById(userId) is not { } user)
        {
            return NotFound();
        }
        
        userService.Unblock(userId);
        return Ok();
    }
}