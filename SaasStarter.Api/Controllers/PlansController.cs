using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SaasStarter.Api.Extensions;
using SaasStarter.Application.Services.Payments;
using SaasStarter.Application.Services.Plans;

namespace SaasStarter.Api.Controllers;

public record ActivatePlanRequest(Guid PlanId);

[ApiController]
[Route("api/plans")]
[Authorize]
[Produces("application/json")]
public class PlansController : ControllerBase
{
    private readonly IPlanService _planService;
    private readonly IPaymentService _paymentService;

    public PlansController(IPlanService planService, IPaymentService paymentService)
    {
        _planService = planService;
        _paymentService = paymentService;
    }

    /// <summary>Catálogo público de planos (sem autenticação).</summary>
    [AllowAnonymous]
    [HttpGet("catalog")]
    [ProducesResponseType(typeof(IEnumerable<PublicPlanCatalogResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCatalog(CancellationToken cancellationToken)
        => Ok(await _planService.GetPublicCatalogAsync(cancellationToken));

    /// <summary>Lista planos disponíveis destacando o plano atual do usuário.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<AvailablePlanResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
        => Ok(await _planService.GetAvailablePlansAsync(User.GetUserId(), cancellationToken));

    /// <summary>Retorna o plano ativo do usuário autenticado.</summary>
    [HttpGet("current")]
    [ProducesResponseType(typeof(CurrentPlanResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> GetCurrent(CancellationToken cancellationToken)
    {
        var result = await _planService.GetCurrentPlanAsync(User.GetUserId(), cancellationToken);
        return result is null ? NoContent() : Ok(result);
    }

    /// <summary>
    /// Inicia o checkout Stripe para o plano. O acesso só é liberado após webhook de confirmação.
    /// </summary>
    [HttpPost("activate")]
    [ProducesResponseType(typeof(CreateCheckoutSessionResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Activate([FromBody] ActivatePlanRequest request, CancellationToken cancellationToken)
        => Ok(await _paymentService.CreateCheckoutSessionAsync(
            User.GetUserId(), User.GetUserEmail(), request.PlanId, cancellationToken: cancellationToken));
}
