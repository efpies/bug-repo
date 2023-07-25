using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QoodenTask.Models;
using QoodenTask.ServiceInterfaces;

namespace QoodenTask.Controllers;

[Authorize]
[Route("auth")]
public class AuthController : ControllerBase
{
    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromServices] IUserService userService, [FromQuery] int userId, [FromQuery] string password)
    {
        if (await userService.GetById(userId) is not { } user)
        {
            return NotFound();
        }

        if (!user.Password.Equals(password))
        {
            return Unauthorized();
        }
        
        SetClaims(user);
        
        return Ok(user);
    }

    [Authorize(Roles = "Admin, User")]
    [HttpPost("logou")]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(
            CookieAuthenticationDefaults.AuthenticationScheme);
        
        return Ok();
    }
    
    [AllowAnonymous]
    [HttpPost("sign-up")]
    public async Task<IActionResult> SignUp([FromServices] IUserService userService,
        [FromBody] UserVM userVm)
    {
        var newUser = await userService.Create(userVm);
        
        SetClaims(newUser);
        
        return Ok(newUser);
    }
    
    [Authorize(Roles = "Admin, User")]
    [HttpPatch("change-password")]
    public async Task<IActionResult> ChangePassword([FromServices] IUserService userService,
        [FromQuery] string newPass)
    {
        var userId = Convert.ToInt32(User.Claims.FirstOrDefault(c => c.Type == ClaimsIdentity.DefaultNameClaimType)?.Value);
        
        if (await userService.GetById(userId) is not { } user)
        {
            return NotFound();
        }

        user.Password = newPass;
        
        userService.Update(user);
        return Ok();
    } 
    
    private async void SetClaims(User user)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.Name, user.Id.ToString()),
            new(ClaimTypes.Role, user.Role)
        };
            
        var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity));
    }
}