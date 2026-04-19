using SaasStarter.Domain.Identity;

namespace SaasStarter.Application.Common.Interfaces;

public interface ITokenService
{
    string GenerateToken(ApplicationUser user, string? planName = null);
}
