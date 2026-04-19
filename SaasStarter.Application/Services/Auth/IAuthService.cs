namespace SaasStarter.Application.Services.Auth;

public interface IAuthService
{
    Task<RegisterResponse> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default);
    Task<LoginResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default);
    Task ConfirmEmailAsync(ConfirmEmailRequest request, CancellationToken cancellationToken = default);
    Task ForgotPasswordAsync(ForgotPasswordRequest request, CancellationToken cancellationToken = default);
    Task ResetPasswordAsync(ResetPasswordRequest request, CancellationToken cancellationToken = default);
    Task<LoginResponse> LoginWithGoogleAsync(GoogleLoginRequest request, CancellationToken cancellationToken = default);
    Task ResendEmailConfirmationAsync(ResendEmailConfirmationRequest request, CancellationToken cancellationToken = default);
}
