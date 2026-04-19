using SaasStarter.Application.Common;

namespace SaasStarter.Application.Services.Auth;

public interface IAuthService
{
    Task<Result<RegisterResponse>> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default);
    Task<Result<LoginResponse>> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default);
    Task<Result> ConfirmEmailAsync(ConfirmEmailRequest request, CancellationToken cancellationToken = default);
    Task<Result> ForgotPasswordAsync(ForgotPasswordRequest request, CancellationToken cancellationToken = default);
    Task<Result> ResetPasswordAsync(ResetPasswordRequest request, CancellationToken cancellationToken = default);
    Task<Result<LoginResponse>> LoginWithGoogleAsync(GoogleLoginRequest request, CancellationToken cancellationToken = default);
    Task<Result> ResendEmailConfirmationAsync(ResendEmailConfirmationRequest request, CancellationToken cancellationToken = default);
}
