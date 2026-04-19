using Microsoft.AspNetCore.Identity;

namespace SaasStarter.Domain.Identity;

public class ApplicationUser : IdentityUser<Guid>
{
    public string FullName { get; set; } = string.Empty;
}
