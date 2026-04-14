using Bud.Api.Authorization;
using Bud.Application.Features.Tags;
using Bud.Application.Features.Tags.UseCases;
using Bud.Shared.Contracts.Features.Tags;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Bud.Api.Features.Tags;

[ApiController]
[Authorize(Policy = AuthorizationPolicies.TenantSelected)]
[Route("api/tags")]
[Produces("application/json")]
public sealed class TagsController(
    CreateTag createTag,
    PatchTag patchTag,
    DeleteTag deleteTag,
    GetTagById getTagById,
    ListTags listTags,
    IValidator<CreateTagRequest> createValidator,
    IValidator<PatchTagRequest> updateValidator) : ApiControllerBase
{
    /// <summary>
    /// Cria uma nova tag. Requer perfil HRManager ou superior.
    /// </summary>
    /// <response code="201">Tag criada com sucesso.</response>
    /// <response code="400">Payload inválido ou erro de validação.</response>
    /// <response code="403">Sem permissão para criar tags.</response>
    /// <response code="409">Já existe uma tag com este nome na organização.</response>
    [HttpPost]
    [Consumes("application/json")]
    [ProducesResponseType(typeof(TagResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<TagResponse>> Create(CreateTagRequest request, CancellationToken cancellationToken)
    {
        var validationResult = await createValidator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return ValidationProblemFrom(validationResult);
        }

        var command = new CreateTagCommand(request.Name, request.Color);
        var result = await createTag.ExecuteAsync(command, cancellationToken);
        return FromResult<Tag, TagResponse>(result, tag =>
            CreatedAtAction(nameof(GetById), new { id = tag.Id }, tag.ToResponse()));
    }

    /// <summary>
    /// Atualiza uma tag existente. Requer perfil HRManager ou superior.
    /// </summary>
    /// <response code="200">Tag atualizada com sucesso.</response>
    /// <response code="400">Payload inválido ou erro de validação.</response>
    /// <response code="403">Sem permissão para atualizar tags.</response>
    /// <response code="404">Tag não encontrada.</response>
    /// <response code="409">Já existe uma tag com este nome na organização.</response>
    [HttpPatch("{id:guid}")]
    [Consumes("application/json")]
    [ProducesResponseType(typeof(TagResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<TagResponse>> Update(Guid id, PatchTagRequest request, CancellationToken cancellationToken)
    {
        var validationResult = await updateValidator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return ValidationProblemFrom(validationResult);
        }

        var command = new PatchTagCommand(request.Name, request.Color);
        var result = await patchTag.ExecuteAsync(id, command, cancellationToken);
        return FromResultOk(result, tag => tag.ToResponse());
    }

    /// <summary>
    /// Remove uma tag pelo identificador. Requer perfil HRManager ou superior.
    /// </summary>
    /// <response code="204">Tag removida com sucesso.</response>
    /// <response code="403">Sem permissão para remover tags.</response>
    /// <response code="404">Tag não encontrada.</response>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var result = await deleteTag.ExecuteAsync(id, cancellationToken);
        return FromResult(result, NoContent);
    }

    /// <summary>
    /// Busca uma tag pelo identificador.
    /// </summary>
    /// <response code="200">Tag encontrada.</response>
    /// <response code="404">Tag não encontrada.</response>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(TagResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TagResponse>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await getTagById.ExecuteAsync(id, cancellationToken);
        return FromResultOk(result, t => t.ToResponse());
    }

    /// <summary>
    /// Lista todas as tags da organização ativa.
    /// </summary>
    /// <response code="200">Lista retornada com sucesso.</response>
    [HttpGet]
    [ProducesResponseType(typeof(List<TagResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<TagResponse>>> GetAll(CancellationToken cancellationToken)
    {
        var result = await listTags.ExecuteAsync(cancellationToken);
        return FromResultOk(result, tags => tags.Select(t => t.ToResponse()).ToList());
    }
}
