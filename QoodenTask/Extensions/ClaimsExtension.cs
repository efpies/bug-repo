using System.Security.Claims;

namespace QoodenTask.Extensions;

public static class ClaimsExtension
{
    public static int GetIdFromClaims(this ClaimsPrincipal user) => int.Parse(user.Claims.FirstOrDefault(c => c.Type == ClaimsIdentity.DefaultNameClaimType)?.Value);
}