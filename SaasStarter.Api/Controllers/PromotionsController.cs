using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SaasStarter.Application.Services.Promotions;

namespace SaasStarter.Api.Controllers;

[ApiController]
[Route("api/promotions")]
[Authorize]
[Produces("application/json")]
public class PromotionsController : ControllerBase
{
    private readonly IPromotionService _promotionService;

    public PromotionsController(IPromotionService promotionService) => _promotionService = promotionService;

    /// <summary>Valida um código promocional e retorna o preço com desconto aplicado.</summary>
    [HttpPost("validate")]
    [ProducesResponseType(typeof(ValidatePromoCodeResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> Validate([FromBody] ValidatePromoCodeRequest request, CancellationToken cancellationToken)
        => Ok(await _promotionService.ValidateAsync(request, cancellationToken));

    /// <summary>Cria um novo código promocional. Restrito a administradores.</summary>
    [Authorize(Roles = "Admin")]
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create([FromBody] CreatePromoCodeRequest request, CancellationToken cancellationToken)
    {
        var result = await _promotionService.CreateAsync(request, cancellationToken);
        if (result.IsSuccess) return Created();
        return Ok(result); // DomainResultFilter converte para o status de erro adequado
    }
}
