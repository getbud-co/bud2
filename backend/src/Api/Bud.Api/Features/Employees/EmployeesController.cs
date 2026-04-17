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
    UpdateEmployee updateEmployee,
    DeleteEmployee deleteEmployee,
    GetEmployeeById getEmployeeById,
    ListEmployees listEmployees,
    IValidator<CreateEmployeeRequest> createValidator,
    IValidator<PatchEmployeeRequest> updateValidator) : ApiControllerBase
{
    [Authorize(Policy = AuthorizationPolicies.LeaderRequired)]
    [HttpPost]
    [Consumes("application/json")]
    [ProducesResponseType(typeof(EmployeeResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<EmployeeResponse>> Create(CreateEmployeeRequest request, CancellationToken cancellationToken)
    {
        var validationResult = await createValidator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return ValidationProblemFrom(validationResult);
        }

        var command = new CreateEmployeeCommand(request.FullName, request.Email, request.Role);
        var result = await createEmployee.ExecuteAsync(command, cancellationToken);

        return FromResult<Employee, EmployeeResponse>(result, employee =>
            CreatedAtAction(nameof(GetById), new { id = employee.Id }, employee.ToEmployeeResponse()));
    }

    [Authorize(Policy = AuthorizationPolicies.LeaderRequired)]
    [HttpPatch("{id:guid}")]
    [Consumes("application/json")]
    [ProducesResponseType(typeof(EmployeeResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<EmployeeResponse>> Update(Guid id, PatchEmployeeRequest request, CancellationToken cancellationToken)
    {
        var validationResult = await updateValidator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return ValidationProblemFrom(validationResult);
        }

        var command = new UpdateEmployeeCommand(request.FullName, request.Email, request.Role);
        var result = await updateEmployee.ExecuteAsync(id, command, cancellationToken);
        return FromResultOk(result, employee => employee.ToEmployeeResponse());
    }

    [Authorize(Policy = AuthorizationPolicies.LeaderRequired)]
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var result = await deleteEmployee.ExecuteAsync(id, cancellationToken);
        return FromResult(result, NoContent);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(EmployeeResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<EmployeeResponse>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await getEmployeeById.ExecuteAsync(id, cancellationToken);
        return FromResultOk(result, employee => employee.ToEmployeeResponse());
    }

    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<EmployeeResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PagedResult<EmployeeResponse>>> GetAll(
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

        var result = await listEmployees.ExecuteAsync(searchValidation.Value, page, pageSize, cancellationToken);
        return FromResultOk(result, paged => paged.MapPaged(c => c.ToEmployeeResponse()));
    }

}
