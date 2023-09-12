using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QoodenTask.Extensions;
using QoodenTask.Models;
using QoodenTask.ServiceInterfaces;

namespace QoodenTask.Controllers;

[Authorize]
[Route("auth")]
public class AuthController : ControllerBase
{
    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromServices] IUserService userService, [FromBody] LoginDto loginDto)
    {
        if (await userService.GetById(loginDto.UserId) is not { } user)
        {
            return NotFound();
        }

        if (!user.Password.Equals(loginDto.Password))
        {
            return Unauthorized();
        }
        
        SetClaims(user);
        
        return Ok(user);
    }

    [Authorize(Roles = "Admin, User")]
    [HttpGet("logout")]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(
            CookieAuthenticationDefaults.AuthenticationScheme);

        return Redirect("/");
    }
    
    [AllowAnonymous]
    [HttpPost("sign-up")]
    public async Task<IActionResult> SignUp([FromServices] IUserService userService,
        [FromBody] UserDto userDto)
    {
        var newUser = await userService.Create(userDto);
        
        SetClaims(newUser);
        
        return Ok(newUser);
    }
    
    [Authorize(Roles = "Admin, User")]
    [HttpPatch("change-password")]
    public async Task<IActionResult> ChangePassword([FromServices] IUserService userService,
        [FromQuery] string newPass, [FromQuery] string currentPass)
    {
        var userId = User.GetIdFromClaims();

        if (await userService.GetById(userId) is not { } user)
        {
            return NotFound();
        }

        if (currentPass != user.Password)
        {
            return Unauthorized();
        }

        userService.ChangePassword(user,newPass);
        return Ok();
    } 
    
    private async void SetClaims(User user)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.Name, user.Id.ToString()),
            new(ClaimTypes.Name, user.UserName),
            new(ClaimTypes.Role, user.Role)
        };
            
        var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity));
    }
}