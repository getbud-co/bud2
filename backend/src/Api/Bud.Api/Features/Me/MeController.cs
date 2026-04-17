using System.Security.Claims;
using Bud.Api.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Bud.Api.Features.Me;

[ApiController]
[Authorize]
[Route("api/me")]
[Produces("application/json")]
public sealed class MeController(
    ListMyOrganizations listMyOrganizations) : ApiControllerBase
{
    /// <summary>
    /// Lista organizações disponíveis para o usuário autenticado.
    /// </summary>
    [HttpGet("organizations")]
    [ProducesResponseType(typeof(List<MyOrganizationResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<List<MyOrganizationResponse>>> GetOrganizations(CancellationToken cancellationToken)
    {
        var email = User.FindFirstValue(ClaimTypes.Email) ?? User.FindFirstValue("email");
        if (string.IsNullOrWhiteSpace(email))
        {
            return BadRequest(new ProblemDetails { Detail = "Não foi possível identificar o e-mail do usuário autenticado." });
        }

        var result = await listMyOrganizations.ExecuteAsync(email, cancellationToken);
        return FromResultOk(result);
    }

}
