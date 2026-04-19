namespace SaasStarter.Application.Common.Interfaces;

public record GoogleUserInfo(string Email, string Name);

public interface IGoogleTokenValidator
{
    Task<GoogleUserInfo> ValidateAsync(string idToken, CancellationToken cancellationToken = default);
}
