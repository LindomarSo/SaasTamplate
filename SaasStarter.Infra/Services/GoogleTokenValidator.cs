using Google.Apis.Auth;
using Microsoft.Extensions.Configuration;
using SaasStarter.Application.Common.Interfaces;

namespace SaasStarter.Infra.Services;

public class GoogleTokenValidator : IGoogleTokenValidator
{
    private readonly string _clientId;

    public GoogleTokenValidator(IConfiguration configuration)
    {
        _clientId = configuration["Authentication:Google:ClientId"]
            ?? throw new InvalidOperationException("Authentication:Google:ClientId não configurado.");
    }

    public async Task<GoogleUserInfo> ValidateAsync(string idToken, CancellationToken cancellationToken = default)
    {
        var settings = new GoogleJsonWebSignature.ValidationSettings
        {
            Audience = [_clientId]
        };

        try
        {
            var payload = await GoogleJsonWebSignature.ValidateAsync(idToken, settings);
            return new GoogleUserInfo(payload.Email, payload.Name ?? payload.Email);
        }
        catch (InvalidJwtException ex)
        {
            throw new UnauthorizedAccessException("Token do Google inválido.", ex);
        }
    }
}
