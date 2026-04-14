using Bud.Api.Authorization;
using Bud.Application.Common;
using Bud.Application.Features.Missions;
using Bud.Application.Features.Indicators;
using Bud.Application.Features.Tags.UseCases;
using Bud.Application.Features.Tasks;
using Bud.Application.Ports;
using Bud.Shared.Contracts;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Bud.Api.Features.Missions;

[ApiController]
[Authorize(Policy = AuthorizationPolicies.TenantSelected)]
[Route("api/missions")]
[Produces("application/json")]
public sealed class MissionsController(
    CreateMission createMission,
    PatchMission patchMission,
    DeleteMission deleteMission,
    GetMissionById getMissionById,
    ListMissions listMissions,
    ListMissionProgress listMissionProgress,
    ListMissionIndicators listMissionIndicators,
    ListMissionChildren listMissionChildren,
    ListTasks listTasks,
    AssignTagToMission assignTagToMission,
    RemoveTagFromMission removeTagFromMission,
    ITenantProvider tenantProvider,
    IValidator<CreateMissionRequest> createValidator,
    IValidator<PatchMissionRequest> updateValidator) : ApiControllerBase
{
    /// <summary>
    /// Cria uma nova meta.
    /// </summary>
    /// <remarks>
    /// Exemplo de payload:
    /// { "name": "Aumentar NPS", "startDate": "2026-01-01", "endDate": "2026-03-31", "status": "Planned" }
    /// </remarks>
    /// <response code="201">Meta criada com sucesso.</response>
    /// <response code="400">Payload inválido ou erro de validação.</response>
    /// <response code="403">Sem permissão para criar meta no contexto atual.</response>
    /// <response code="404">Meta pai ou colaborador responsável não encontrado.</response>
    [HttpPost]
    [Consumes("application/json")]
    [ProducesResponseType(typeof(MissionResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<MissionResponse>> Create(CreateMissionRequest request, CancellationToken cancellationToken)
    {
        var validationResult = await createValidator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return ValidationProblemFrom(validationResult);
        }

        var command = new CreateMissionCommand(
            request.Name,
            request.Description,
            request.Dimension,
            request.StartDate,
            request.EndDate,
            request.Status,
            request.ParentId,
            request.EmployeeId);

        var result = await createMission.ExecuteAsync(command, cancellationToken);
        return FromResult<Mission, MissionResponse>(result, mission =>
            CreatedAtAction(nameof(GetById), new { id = mission.Id }, mission.ToResponse()));
    }

    /// <summary>
    /// Atualiza uma meta existente.
    /// </summary>
    /// <remarks>
    /// Exemplo de payload:
    /// { "name": "Aumentar NPS em 10 pontos", "status": "InProgress", "endDate": "2026-04-30" }
    /// </remarks>
    /// <response code="200">Meta atualizada com sucesso.</response>
    /// <response code="400">Payload inválido ou erro de validação.</response>
    /// <response code="404">Meta não encontrada.</response>
    [HttpPatch("{id:guid}")]
    [Consumes("application/json")]
    [ProducesResponseType(typeof(MissionResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<MissionResponse>> Update(Guid id, PatchMissionRequest request, CancellationToken cancellationToken)
    {
        var validationResult = await updateValidator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return ValidationProblemFrom(validationResult);
        }

        var command = new PatchMissionCommand(
            request.Name,
            request.Description,
            request.Dimension,
            request.StartDate,
            request.EndDate,
            request.Status,
            request.EmployeeId);

        var result = await patchMission.ExecuteAsync(id, command, cancellationToken);
        return FromResultOk(result, mission => mission.ToResponse());
    }

    /// <summary>
    /// Remove uma meta pelo identificador.
    /// </summary>
    /// <response code="204">Meta removida com sucesso.</response>
    /// <response code="404">Meta não encontrada.</response>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var result = await deleteMission.ExecuteAsync(id, cancellationToken);
        return FromResult(result, NoContent);
    }

    /// <summary>
    /// Busca uma meta pelo identificador.
    /// </summary>
    /// <response code="200">Meta encontrada.</response>
    /// <response code="404">Meta não encontrada.</response>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(MissionResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<MissionResponse>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await getMissionById.ExecuteAsync(id, cancellationToken);
        return FromResultOk(result, mission => mission.ToResponse());
    }

    /// <summary>
    /// Lista metas com filtros e paginação.
    /// </summary>
    /// <response code="200">Lista paginada retornada com sucesso.</response>
    /// <response code="400">Parâmetros de filtro/paginação inválidos.</response>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<MissionResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PagedResult<MissionResponse>>> GetAll(
        [FromQuery] MissionFilter? filter,
        [FromQuery] string? search,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var searchValidation = ValidateAndNormalizeSearch(search);
        if (searchValidation.Failure is not null)
        {
            return searchValidation.Failure;
        }

        var paginationValidation = ValidatePagination(page, pageSize);
        if (paginationValidation is not null)
        {
            return paginationValidation;
        }

        var result = await listMissions.ExecuteAsync(filter, tenantProvider.EmployeeId, searchValidation.Value, page, pageSize, cancellationToken);
        return FromResultOk(result, paged => paged.MapPaged(g => g.ToResponse()));
    }

    /// <summary>
    /// Retorna progresso agregado de metas.
    /// </summary>
    /// <response code="200">Progresso calculado com sucesso.</response>
    /// <response code="400">Parâmetro ids inválido.</response>
    [HttpGet("progress")]
    [ProducesResponseType(typeof(List<MissionProgressResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<List<MissionProgressResponse>>> GetProgress(
        [FromQuery] string ids,
        CancellationToken cancellationToken)
    {
        var parseResult = ParseGuidCsv(ids, "ids");
        if (parseResult.Failure is not null)
        {
            return parseResult.Failure;
        }

        var result = await listMissionProgress.ExecuteAsync(parseResult.Values!, cancellationToken);
        return FromResultOk(result);
    }

    /// <summary>
    /// Lista indicadores associados a uma meta.
    /// </summary>
    /// <response code="200">Indicadores retornados com sucesso.</response>
    /// <response code="400">Parâmetros de paginação inválidos.</response>
    /// <response code="404">Meta não encontrada.</response>
    [HttpGet("{id:guid}/indicators")]
    [ProducesResponseType(typeof(PagedResult<IndicatorResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PagedResult<IndicatorResponse>>> GetIndicators(
        Guid id,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var paginationValidation = ValidatePagination(page, pageSize);
        if (paginationValidation is not null)
        {
            return paginationValidation;
        }

        var result = await listMissionIndicators.ExecuteAsync(id, page, pageSize, cancellationToken);
        return FromResultOk(result, paged => paged.MapPaged(i => i.ToResponse()));
    }

    /// <summary>
    /// Lista submetas de uma meta com paginação.
    /// </summary>
    /// <response code="200">Submetas retornadas com sucesso.</response>
    /// <response code="400">Parâmetros de paginação inválidos.</response>
    /// <response code="404">Meta não encontrada.</response>
    [HttpGet("{id:guid}/children")]
    [ProducesResponseType(typeof(PagedResult<MissionResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PagedResult<MissionResponse>>> GetChildren(
        Guid id,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var paginationValidation = ValidatePagination(page, pageSize);
        if (paginationValidation is not null)
        {
            return paginationValidation;
        }

        var result = await listMissionChildren.ExecuteAsync(id, page, pageSize, cancellationToken);
        return FromResultOk(result, paged => paged.MapPaged(g => g.ToResponse()));
    }

    /// <summary>
    /// Lista tarefas de uma meta.
    /// </summary>
    /// <response code="200">Tarefas retornadas com sucesso.</response>
    /// <response code="400">Parâmetros de paginação inválidos.</response>
    /// <response code="404">Meta não encontrada.</response>
    [HttpGet("{id:guid}/tasks")]
    [ProducesResponseType(typeof(PagedResult<TaskResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PagedResult<TaskResponse>>> GetTasks(
        Guid id,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50,
        CancellationToken cancellationToken = default)
    {
        var paginationValidation = ValidatePagination(page, pageSize);
        if (paginationValidation is not null)
        {
            return paginationValidation;
        }

        var result = await listTasks.ExecuteAsync(id, page, pageSize, cancellationToken);
        return FromResultOk(result, paged => paged.MapPaged(t => t.ToResponse()));
    }

    /// <summary>
    /// Atribui uma tag a uma meta.
    /// Colaboradores só podem atribuir tags em metas pelas quais são responsáveis.
    /// </summary>
    /// <response code="204">Tag atribuída com sucesso.</response>
    /// <response code="403">Sem permissão para atribuir a tag.</response>
    /// <response code="404">Meta ou tag não encontrada.</response>
    [HttpPost("{id:guid}/tags/{tagId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AssignTag(Guid id, Guid tagId, CancellationToken cancellationToken)
    {
        var result = await assignTagToMission.ExecuteAsync(id, tagId, cancellationToken);
        return FromResult(result, NoContent);
    }

    /// <summary>
    /// Remove uma tag de uma meta.
    /// Colaboradores só podem remover tags de metas pelas quais são responsáveis.
    /// </summary>
    /// <response code="204">Tag removida com sucesso.</response>
    /// <response code="403">Sem permissão para remover a tag.</response>
    /// <response code="404">Meta não encontrada.</response>
    [HttpDelete("{id:guid}/tags/{tagId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RemoveTag(Guid id, Guid tagId, CancellationToken cancellationToken)
    {
        var result = await removeTagFromMission.ExecuteAsync(id, tagId, cancellationToken);
        return FromResult(result, NoContent);
    }
}
