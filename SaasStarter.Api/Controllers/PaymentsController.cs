using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SaasStarter.Api.Extensions;
using SaasStarter.Application.Services.Payments;
using System.Text;

namespace SaasStarter.Api.Controllers;

[ApiController]
[Route("api/payments")]
[Produces("application/json")]
public class PaymentsController : ControllerBase
{
    private readonly IPaymentService _paymentService;
    private readonly ILogger<PaymentsController> _logger;

    public PaymentsController(IPaymentService paymentService, ILogger<PaymentsController> logger)
    {
        _paymentService = paymentService;
        _logger = logger;
    }

    /// <summary>
    /// Cria sessão de checkout no Stripe. Redirecione o usuário para a URL retornada.
    /// O acesso só é liberado após confirmação via webhook — NUNCA pela success_url.
    /// </summary>
    [Authorize]
    [HttpPost("create-checkout-session")]
    [ProducesResponseType(typeof(CreateCheckoutSessionResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> CreateCheckoutSession(
        [FromBody] CreateCheckoutSessionRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _paymentService.CreateCheckoutSessionAsync(
            User.GetUserId(), User.GetUserEmail(), request.PlanId, request.PromoCode, cancellationToken);

        return Ok(result);
    }

    /// <summary>
    /// Webhook do Stripe. Chamado exclusivamente pelo Stripe.
    /// A assinatura é validada via Stripe-Signature header.
    /// Retorna 200 mesmo para eventos ignorados (Stripe requer 2xx para não retentar).
    /// </summary>
    [AllowAnonymous]
    [HttpPost("webhook")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Webhook(CancellationToken cancellationToken)
    {
        // NÃO use [FromBody] — o Stripe valida o payload exato recebido.
        string payload;
        using (var reader = new StreamReader(Request.Body, Encoding.UTF8, leaveOpen: true))
            payload = await reader.ReadToEndAsync(cancellationToken);

        var stripeSignature = Request.Headers["Stripe-Signature"].FirstOrDefault();
        if (string.IsNullOrEmpty(stripeSignature))
        {
            _logger.LogWarning("Webhook sem Stripe-Signature. IP: {IP}", HttpContext.Connection.RemoteIpAddress);
            return BadRequest("Cabeçalho Stripe-Signature ausente.");
        }

        try
        {
            await _paymentService.HandleWebhookAsync(payload, stripeSignature, cancellationToken);
            return Ok();
        }
        catch (UnauthorizedAccessException)
        {
            return Unauthorized();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao processar webhook do Stripe.");
            return StatusCode(StatusCodes.Status500InternalServerError);
        }
    }
}
