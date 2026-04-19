namespace SaasStarter.Application.Services.Auth;

public record RegisterRequest(string Name, string Email, string Password);
public record RegisterResponse(Guid UserId, string Name, string Email, string? Plan, bool RequiresEmailConfirmation);

public record LoginRequest(string Email, string Password);
public record LoginResponse(string Token, string Name, string Email, string? Plan);

public record ConfirmEmailRequest(string UserId, string Token);
public record ForgotPasswordRequest(string Email);
public record ResetPasswordRequest(string Email, string Token, string NewPassword);
public record GoogleLoginRequest(string IdToken);
public record ResendEmailConfirmationRequest(string Email);
