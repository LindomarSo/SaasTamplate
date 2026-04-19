using System.Security.Claims;

namespace SaasStarter.Api.Extensions;

public static class ClaimsPrincipalExtensions
{
    public static Guid GetUserId(this ClaimsPrincipal user)
    {
        var value = user.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? throw new UnauthorizedAccessException("Claim NameIdentifier ausente.");
        return Guid.Parse(value);
    }

    public static string GetUserEmail(this ClaimsPrincipal user)
        => user.FindFirstValue(ClaimTypes.Email)
            ?? throw new UnauthorizedAccessException("Claim Email ausente.");

    public static string? GetPlan(this ClaimsPrincipal user)
        => user.FindFirstValue("plan");
}
