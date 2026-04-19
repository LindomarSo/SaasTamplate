using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using SaasStarter.Application.Common.Interfaces;
using SaasStarter.Domain.Identity;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace SaasStarter.Infra.Services;

public class TokenService : ITokenService
{
    private readonly IConfiguration _configuration;

    public TokenService(IConfiguration configuration) => _configuration = configuration;

    public string GenerateToken(ApplicationUser user, string? planName = null)
    {
        var secret = _configuration["Jwt:Secret"]
            ?? throw new InvalidOperationException("Jwt:Secret não configurado.");
        var issuer = _configuration["Jwt:Issuer"] ?? "SaasStarter";
        var audience = _configuration["Jwt:Audience"] ?? "SaasStarter";
        var expirationHours = int.Parse(_configuration["Jwt:ExpirationHours"] ?? "24");

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Email, user.Email!),
            new Claim(ClaimTypes.Name, user.FullName),
            new Claim("plan", planName ?? string.Empty),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: DateTime.UtcNow.AddHours(expirationHours),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
