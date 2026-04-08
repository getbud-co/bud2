using Bud.Api.Authorization;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Bud.Api.Features.Employees;

[ApiController]
[Authorize(Policy = AuthorizationPolicies.TenantSelected)]
[Route("api/employees")]
[Produces("application/json")]
public sealed class EmployeesController(
    CreateEmployee createEmployee,
    PatchEmployee patchEmployee,
    DeleteEmployee deleteEmployee,
    GetEmployeeById getEmployeeById,
    GetEmployeeLookup listEmployeeOptions,
    ListLeaderEmployees listLeaderEmployees,
    ListEmployees listEmployees,
    GetEmployeeHierarchy getEmployeeHierarchy,
    ListEmployeeTeams listEmployeeTeams,
    PatchEmployeeTeams patchEmployeeTeams,
    ListAvailableTeamsForEmployee listAvailableTeamsForEmployee,
    IValidator<CreateEmployeeRequest> createValidator,
    IValidator<PatchEmployeeRequest> updateValidator) : ApiControllerBase
{
    /// <summary>
    /// Cria um funcionário.
    /// </summary>
    /// <response code="201">Funcionário criado com sucesso.</response>
    /// <response code="400">Payload inválido.</response>
    /// <response code="404">Time não encontrado.</response>
    /// <response code="403">Sem permissão para criar funcionário.</response>
    [HttpPost]
    [Consumes("application/json")]
    [ProducesResponseType(typeof(EmployeeResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<EmployeeResponse>> Create(CreateEmployeeRequest request, CancellationToken cancellationToken)
    {
        var validationResult = await createValidator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return ValidationProblemFrom(validationResult);
        }

        var command = new CreateEmployeeCommand(
            request.FullName,
            request.Email,
            request.Role,
            request.TeamId,
            request.LeaderId);

        var result = await createEmployee.ExecuteAsync(User, command, cancellationToken);
        return FromResult<OrganizationEmployeeMember, EmployeeResponse>(result, member =>
            CreatedAtAction(nameof(GetById), new { id = member.EmployeeId }, member.ToEmployeeResponse()));
    }

    /// <summary>
    /// Atualiza um funcionário.
    /// </summary>
    /// <response code="200">Funcionário atualizado com sucesso.</response>
    /// <response code="400">Payload inválido.</response>
    /// <response code="404">Funcionário não encontrado.</response>
    /// <response code="403">Sem permissão para atualizar funcionário.</response>
    [HttpPatch("{id:guid}")]
    [Consumes("application/json")]
    [ProducesResponseType(typeof(EmployeeResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<EmployeeResponse>> Update(Guid id, PatchEmployeeRequest request, CancellationToken cancellationToken)
    {
        var validationResult = await updateValidator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return ValidationProblemFrom(validationResult);
        }

        var command = new PatchEmployeeCommand(
            request.FullName,
            request.Email,
            request.Role,
            request.LeaderId);

        var result = await patchEmployee.ExecuteAsync(User, id, command, cancellationToken);
        return FromResultOk(result, member => member.ToEmployeeResponse());
    }

    /// <summary>
    /// Exclui um funcionário.
    /// </summary>
    /// <response code="204">Funcionário removido com sucesso.</response>
    /// <response code="404">Funcionário não encontrado.</response>
    /// <response code="409">Conflito de integridade ao remover funcionário.</response>
    /// <response code="403">Sem permissão para excluir funcionário.</response>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var result = await deleteEmployee.ExecuteAsync(User, id, cancellationToken);
        return FromResult(result, NoContent);
    }

    /// <summary>
    /// Busca funcionário por identificador.
    /// </summary>
    /// <response code="200">Funcionário encontrado.</response>
    /// <response code="404">Funcionário não encontrado.</response>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(EmployeeResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<EmployeeResponse>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await getEmployeeById.ExecuteAsync(User, id, cancellationToken);
        return FromResultOk(result, member => member.ToEmployeeResponse());
    }

    /// <summary>
    /// Lista funcionários com paginação e filtros.
    /// </summary>
    /// <response code="200">Lista paginada retornada com sucesso.</response>
    /// <response code="400">Parâmetros inválidos.</response>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<EmployeeResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PagedResult<EmployeeResponse>>> GetAll(
        [FromQuery] Guid? teamId,
        [FromQuery] string? search = null,
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

        var result = await listEmployees.ExecuteAsync(teamId, searchValidation.Value, page, pageSize, cancellationToken);
        return FromResultOk(result, paged => paged.MapPaged(m => m.ToEmployeeResponse()));
    }

    /// <summary>
    /// Lista opções simplificadas de funcionários.
    /// </summary>
    [HttpGet("lookup")]
    [ProducesResponseType(typeof(List<EmployeeLookupResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<List<EmployeeLookupResponse>>> GetLookup(
        [FromQuery] string? search,
        CancellationToken cancellationToken = default)
    {
        var searchValidation = ValidateAndNormalizeSearch(search);
        if (searchValidation.Failure is not null)
        {
            return searchValidation.Failure;
        }

        var result = await listEmployeeOptions.ExecuteAsync(searchValidation.Value, cancellationToken);
        return FromResultOk(result);
    }

    /// <summary>
    /// Lista funcionários líderes.
    /// </summary>
    [HttpGet("leaders")]
    [ProducesResponseType(typeof(List<EmployeeLeaderResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<EmployeeLeaderResponse>>> GetLeaders(
        [FromQuery] Guid? organizationId = null,
        CancellationToken cancellationToken = default)
    {
        var result = await listLeaderEmployees.ExecuteAsync(organizationId, cancellationToken);
        return FromResultOk(result);
    }

    /// <summary>
    /// Lista subordinados (liderados) em hierarquia recursiva.
    /// </summary>
    /// <response code="200">Hierarquia retornada com sucesso.</response>
    /// <response code="404">Funcionário não encontrado.</response>
    [HttpGet("{id:guid}/subordinates")]
    [ProducesResponseType(typeof(List<EmployeeSubordinateResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<List<EmployeeSubordinateResponse>>> GetSubordinates(Guid id, CancellationToken cancellationToken)
    {
        var result = await getEmployeeHierarchy.ExecuteAsync(User, id, cancellationToken);
        return FromResultOk(result);
    }

    /// <summary>
    /// Lista times associados ao funcionário.
    /// </summary>
    /// <response code="200">Lista retornada com sucesso.</response>
    /// <response code="404">Funcionário não encontrado.</response>
    [HttpGet("{id:guid}/teams")]
    [ProducesResponseType(typeof(List<EmployeeTeamResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<List<EmployeeTeamResponse>>> GetTeams(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var associatedTeams = await listEmployeeTeams.ExecuteAsync(User, id, cancellationToken);
        return FromResultOk(associatedTeams);
    }

    /// <summary>
    /// Lista times disponíveis para vínculo com o funcionário.
    /// </summary>
    [HttpGet("{id:guid}/teams/eligible-for-assignment")]
    [ProducesResponseType(typeof(List<EmployeeTeamEligibleResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<List<EmployeeTeamEligibleResponse>>> GetEligibleTeamsForAssignment(
        Guid id,
        [FromQuery] string? search = null,
        CancellationToken cancellationToken = default)
    {
        var searchValidation = ValidateAndNormalizeSearch(search);
        if (searchValidation.Failure is not null)
        {
            return searchValidation.Failure;
        }

        var availableTeams = await listAvailableTeamsForEmployee.ExecuteAsync(User, id, searchValidation.Value, cancellationToken);
        return FromResultOk(availableTeams);
    }

    /// <summary>
    /// Atualiza times associados ao funcionário.
    /// </summary>
    /// <response code="204">Vínculos atualizados com sucesso.</response>
    /// <response code="400">Payload inválido.</response>
    /// <response code="404">Funcionário não encontrado.</response>
    /// <response code="403">Sem permissão para atualizar vínculos.</response>
    [HttpPatch("{id:guid}/teams")]
    [Consumes("application/json")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> UpdateTeams(Guid id, PatchEmployeeTeamsRequest request, CancellationToken cancellationToken)
    {
        var command = new PatchEmployeeTeamsCommand(request.TeamIds);
        var result = await patchEmployeeTeams.ExecuteAsync(User, id, command, cancellationToken);
        return FromResult(result, NoContent);
    }
}
