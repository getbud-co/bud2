using Bud.Api.Authorization;
using Bud.Application.Features.Cycles;
using Bud.Application.Features.Cycles.UseCases;
using Bud.Shared.Contracts.Features.Cycles;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Bud.Api.Features.Cycles;

[ApiController]
[Authorize(Policy = AuthorizationPolicies.TenantSelected)]
[Route("api/cycles")]
[Produces("application/json")]
public sealed class CyclesController(
    CreateCycle createCycle,
    PatchCycle patchCycle,
    DeleteCycle deleteCycle,
    GetCycleById getCycleById,
    ListCycles listCycles,
    IValidator<CreateCycleRequest> createValidator,
    IValidator<PatchCycleRequest> updateValidator) : ApiControllerBase
{
    /// <summary>
    /// Cria um novo ciclo.
    /// </summary>
    /// <response code="201">Ciclo criado com sucesso.</response>
    /// <response code="400">Payload inválido ou erro de validação.</response>
    [HttpPost]
    [Consumes("application/json")]
    [ProducesResponseType(typeof(CycleResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<CycleResponse>> Create(CreateCycleRequest request, CancellationToken cancellationToken)
    {
        var validationResult = await createValidator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return ValidationProblemFrom(validationResult);
        }

        var command = new CreateCycleCommand(
            request.Name,
            request.Cadence,
            request.StartDate,
            request.EndDate,
            request.Status);

        var result = await createCycle.ExecuteAsync(command, cancellationToken);
        return FromResult<Cycle, CycleResponse>(result, cycle =>
            CreatedAtAction(nameof(GetById), new { id = cycle.Id }, cycle.ToResponse()));
    }

    /// <summary>
    /// Atualiza um ciclo existente.
    /// </summary>
    /// <response code="200">Ciclo atualizado com sucesso.</response>
    /// <response code="400">Payload inválido ou erro de validação.</response>
    /// <response code="404">Ciclo não encontrado.</response>
    [HttpPatch("{id:guid}")]
    [Consumes("application/json")]
    [ProducesResponseType(typeof(CycleResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<CycleResponse>> Update(Guid id, PatchCycleRequest request, CancellationToken cancellationToken)
    {
        var validationResult = await updateValidator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return ValidationProblemFrom(validationResult);
        }

        var command = new PatchCycleCommand(
            request.Name,
            request.Cadence,
            request.StartDate,
            request.EndDate,
            request.Status);

        var result = await patchCycle.ExecuteAsync(id, command, cancellationToken);
        return FromResultOk(result, cycle => cycle.ToResponse());
    }

    /// <summary>
    /// Remove um ciclo pelo identificador.
    /// </summary>
    /// <response code="204">Ciclo removido com sucesso.</response>
    /// <response code="404">Ciclo não encontrado.</response>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var result = await deleteCycle.ExecuteAsync(id, cancellationToken);
        return FromResult(result, NoContent);
    }

    /// <summary>
    /// Busca um ciclo pelo identificador.
    /// </summary>
    /// <response code="200">Ciclo encontrado.</response>
    /// <response code="404">Ciclo não encontrado.</response>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(CycleResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CycleResponse>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await getCycleById.ExecuteAsync(id, cancellationToken);
        return FromResultOk(result, cycle => cycle.ToResponse());
    }

    /// <summary>
    /// Lista todos os ciclos da organização ativa.
    /// </summary>
    /// <response code="200">Lista retornada com sucesso.</response>
    [HttpGet]
    [ProducesResponseType(typeof(List<CycleResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<CycleResponse>>> GetAll(CancellationToken cancellationToken)
    {
        var result = await listCycles.ExecuteAsync(cancellationToken);
        return FromResultOk(result, cycles => cycles.Select(c => c.ToResponse()).ToList());
    }
}
