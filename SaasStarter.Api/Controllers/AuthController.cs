using Microsoft.AspNetCore.Mvc;
using SaasStarter.Application.Services.Auth;

namespace SaasStarter.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService) => _authService = authService;

    [HttpPost("register")]
    [ProducesResponseType(typeof(RegisterResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request, CancellationToken cancellationToken)
    {
        var result = await _authService.RegisterAsync(request, cancellationToken);
        return CreatedAtAction(nameof(Register), new { id = result.UserId }, result);
    }

    [HttpPost("login")]
    [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] LoginRequest request, CancellationToken cancellationToken)
    {
        var result = await _authService.LoginAsync(request, cancellationToken);
        return Ok(result);
    }

    [HttpPost("confirm-email")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ConfirmEmail([FromBody] ConfirmEmailRequest request, CancellationToken cancellationToken)
    {
        await _authService.ConfirmEmailAsync(request, cancellationToken);
        return NoContent();
    }

    [HttpPost("forgot-password")]
    [ProducesResponseType(StatusCodes.Status202Accepted)]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request, CancellationToken cancellationToken)
    {
        await _authService.ForgotPasswordAsync(request, cancellationToken);
        return Accepted();
    }

    [HttpPost("reset-password")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request, CancellationToken cancellationToken)
    {
        await _authService.ResetPasswordAsync(request, cancellationToken);
        return NoContent();
    }

    [HttpPost("google")]
    [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> LoginWithGoogle([FromBody] GoogleLoginRequest request, CancellationToken cancellationToken)
    {
        var result = await _authService.LoginWithGoogleAsync(request, cancellationToken);
        return Ok(result);
    }

    [HttpPost("resend-confirmation")]
    [ProducesResponseType(StatusCodes.Status202Accepted)]
    public async Task<IActionResult> ResendEmailConfirmation([FromBody] ResendEmailConfirmationRequest request, CancellationToken cancellationToken)
    {
        await _authService.ResendEmailConfirmationAsync(request, cancellationToken);
        return Accepted();
    }
}
