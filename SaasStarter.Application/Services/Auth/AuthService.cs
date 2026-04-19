using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SaasStarter.Application.Common.Interfaces;
using SaasStarter.Application.Services.Auth.Validators;
using SaasStarter.Domain.Identity;
using System.Text;

namespace SaasStarter.Application.Services.Auth;

public class AuthService : IAuthService
{
    private readonly ITokenService _tokenService;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IEmailSenderService _emailSenderService;
    private readonly IGoogleTokenValidator _googleTokenValidator;
    private readonly IConfiguration _configuration;
    private readonly IUserSubscriptionRepository _subscriptionRepository;
    private readonly ILogger<AuthService> _logger;

    public AuthService(
        ITokenService tokenService,
        UserManager<ApplicationUser> userManager,
        IEmailSenderService emailSenderService,
        IGoogleTokenValidator googleTokenValidator,
        IConfiguration configuration,
        IUserSubscriptionRepository subscriptionRepository,
        ILogger<AuthService> logger)
    {
        _tokenService = tokenService;
        _userManager = userManager;
        _emailSenderService = emailSenderService;
        _googleTokenValidator = googleTokenValidator;
        _configuration = configuration;
        _subscriptionRepository = subscriptionRepository;
        _logger = logger;
    }

    public async Task<RegisterResponse> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default)
    {
        await new RegisterRequestValidator().ValidateAndThrowAsync(request, cancellationToken);

        var normalizedEmail = request.Email.ToLowerInvariant();
        var existingUser = await _userManager.FindByEmailAsync(normalizedEmail);
        if (existingUser is not null)
            throw new InvalidOperationException("E-mail já cadastrado.");

        var user = new ApplicationUser
        {
            UserName = normalizedEmail,
            Email = normalizedEmail,
            FullName = request.Name,
            EmailConfirmed = false
        };

        var result = await _userManager.CreateAsync(user, request.Password);
        if (!result.Succeeded)
            throw new InvalidOperationException(string.Join("; ", result.Errors.Select(x => x.Description)));

        var confirmationToken = await _userManager.GenerateEmailConfirmationTokenAsync(user);
        var encodedToken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(confirmationToken));
        var confirmationLink = BuildFrontendLink("confirmar-email", normalizedEmail, user.Id.ToString(), encodedToken);

        try
        {
            await _emailSenderService.SendEmailConfirmationAsync(normalizedEmail, confirmationLink, user.FullName, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Falha ao enviar e-mail de confirmação para {Email}.", normalizedEmail);
        }

        return new RegisterResponse(user.Id, user.FullName, user.Email!, null, true);
    }

    public async Task<LoginResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
    {
        await new LoginRequestValidator().ValidateAndThrowAsync(request, cancellationToken);

        var normalizedEmail = request.Email.ToLowerInvariant();
        var user = await _userManager.FindByEmailAsync(normalizedEmail);
        if (user is null)
            throw new UnauthorizedAccessException("E-mail ou senha inválidos.");

        if (!user.EmailConfirmed)
            throw new UnauthorizedAccessException("Confirme seu e-mail antes de acessar.");

        var passwordValid = await _userManager.CheckPasswordAsync(user, request.Password);
        if (!passwordValid)
            throw new UnauthorizedAccessException("E-mail ou senha inválidos.");

        var subscription = await _subscriptionRepository.GetActiveByUserIdAsync(user.Id, cancellationToken);
        var token = _tokenService.GenerateToken(user, subscription?.PlanDefinition.Name);

        return new LoginResponse(token, user.FullName, user.Email!, subscription?.PlanDefinition.Name);
    }

    public async Task ConfirmEmailAsync(ConfirmEmailRequest request, CancellationToken cancellationToken = default)
    {
        await new ConfirmEmailRequestValidator().ValidateAndThrowAsync(request, cancellationToken);

        if (!Guid.TryParse(request.UserId, out var userId))
            throw new InvalidOperationException("UserId inválido.");

        var user = await _userManager.FindByIdAsync(userId.ToString())
            ?? throw new InvalidOperationException("Usuário não encontrado.");

        var decodedToken = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(request.Token));
        var result = await _userManager.ConfirmEmailAsync(user, decodedToken);

        if (!result.Succeeded)
            throw new InvalidOperationException("Não foi possível confirmar o e-mail.");
    }

    public async Task ForgotPasswordAsync(ForgotPasswordRequest request, CancellationToken cancellationToken = default)
    {
        await new ForgotPasswordRequestValidator().ValidateAndThrowAsync(request, cancellationToken);

        var normalizedEmail = request.Email.ToLowerInvariant();
        var user = await _userManager.FindByEmailAsync(normalizedEmail);

        if (user is null || !user.EmailConfirmed)
            return;

        var token = await _userManager.GeneratePasswordResetTokenAsync(user);
        var encodedToken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));
        var resetLink = BuildFrontendLink("nova-senha", normalizedEmail, user.Id.ToString(), encodedToken);
        await _emailSenderService.SendPasswordResetAsync(normalizedEmail, resetLink, cancellationToken);
    }

    public async Task ResetPasswordAsync(ResetPasswordRequest request, CancellationToken cancellationToken = default)
    {
        await new ResetPasswordRequestValidator().ValidateAndThrowAsync(request, cancellationToken);

        var normalizedEmail = request.Email.ToLowerInvariant();
        var user = await _userManager.FindByEmailAsync(normalizedEmail)
            ?? throw new InvalidOperationException("Usuário não encontrado.");

        var decodedToken = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(request.Token));
        var result = await _userManager.ResetPasswordAsync(user, decodedToken, request.NewPassword);

        if (!result.Succeeded)
            throw new InvalidOperationException(string.Join("; ", result.Errors.Select(x => x.Description)));
    }

    public async Task<LoginResponse> LoginWithGoogleAsync(GoogleLoginRequest request, CancellationToken cancellationToken = default)
    {
        await new GoogleLoginRequestValidator().ValidateAndThrowAsync(request, cancellationToken);

        var googleUser = await _googleTokenValidator.ValidateAsync(request.IdToken, cancellationToken);
        var normalizedEmail = googleUser.Email.ToLowerInvariant();

        var user = await _userManager.FindByEmailAsync(normalizedEmail);
        if (user is null)
        {
            user = new ApplicationUser
            {
                UserName = normalizedEmail,
                Email = normalizedEmail,
                FullName = googleUser.Name,
                EmailConfirmed = true
            };

            var result = await _userManager.CreateAsync(user);
            if (!result.Succeeded)
                throw new InvalidOperationException(string.Join("; ", result.Errors.Select(x => x.Description)));
        }

        var subscription = await _subscriptionRepository.GetActiveByUserIdAsync(user.Id, cancellationToken);
        var token = _tokenService.GenerateToken(user, subscription?.PlanDefinition.Name);

        return new LoginResponse(token, user.FullName, user.Email!, subscription?.PlanDefinition.Name);
    }

    public async Task ResendEmailConfirmationAsync(ResendEmailConfirmationRequest request, CancellationToken cancellationToken = default)
    {
        await new ResendEmailConfirmationRequestValidator().ValidateAndThrowAsync(request, cancellationToken);

        var normalizedEmail = request.Email.ToLowerInvariant();
        var user = await _userManager.FindByEmailAsync(normalizedEmail);

        if (user is null || user.EmailConfirmed)
            return;

        var confirmationToken = await _userManager.GenerateEmailConfirmationTokenAsync(user);
        var encodedToken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(confirmationToken));
        var confirmationLink = BuildFrontendLink("confirmar-email", normalizedEmail, user.Id.ToString(), encodedToken);
        await _emailSenderService.SendEmailConfirmationAsync(normalizedEmail, confirmationLink, user.FullName, cancellationToken);
    }

    private string BuildFrontendLink(string route, string email, string userId, string token)
    {
        var baseUrl = _configuration["Frontend:BaseUrl"] ?? "http://localhost:5173";
        return $"{baseUrl.TrimEnd('/')}/{route}?email={Uri.EscapeDataString(email)}&userId={Uri.EscapeDataString(userId)}&token={Uri.EscapeDataString(token)}";
    }
}
