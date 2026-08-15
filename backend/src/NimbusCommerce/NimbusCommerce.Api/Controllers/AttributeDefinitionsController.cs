using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NimbusCommerce.Api.Extensions;
using NimbusCommerce.Application.Catalog.AttributeDefinitions.CreateAttributeDefinition;
using NimbusCommerce.Application.Catalog.AttributeDefinitions.DeleteAttributeDefinition;
using NimbusCommerce.Application.Catalog.AttributeDefinitions.GetAttributeDefinition;
using NimbusCommerce.Application.Catalog.AttributeDefinitions.ListAttributeDefinitionOptions;
using NimbusCommerce.Application.Catalog.AttributeDefinitions.ListAttributeDefinitions;
using NimbusCommerce.Application.Catalog.AttributeDefinitions.SetAttributeDefinitionStatus;
using NimbusCommerce.Application.Catalog.AttributeDefinitions.UpdateAttributeDefinition;
using NimbusCommerce.Application.Catalog.Interfaces;
using NimbusCommerce.Application.Common.Models;

namespace NimbusCommerce.Api.Controllers;

// Every catalogue endpoint requires authentication with no anonymous exceptions, so [Authorize]
// sits on the controller — the same fail-safe default CategoriesController already uses.
[Authorize]
[ApiController]
[Route("api/attribute-definitions")]
[ProducesResponseType(StatusCodes.Status401Unauthorized)]
public sealed class AttributeDefinitionsController : ControllerBase
{
    private readonly IListAttributeDefinitionsService _listAttributeDefinitionsService;
    private readonly IListAttributeDefinitionOptionsService _listAttributeDefinitionOptionsService;
    private readonly IGetAttributeDefinitionService _getAttributeDefinitionService;
    private readonly ICreateAttributeDefinitionService _createAttributeDefinitionService;
    private readonly IUpdateAttributeDefinitionService _updateAttributeDefinitionService;
    private readonly ISetAttributeDefinitionStatusService _setAttributeDefinitionStatusService;
    private readonly IDeleteAttributeDefinitionService _deleteAttributeDefinitionService;

    public AttributeDefinitionsController(
        IListAttributeDefinitionsService listAttributeDefinitionsService,
        IListAttributeDefinitionOptionsService listAttributeDefinitionOptionsService,
        IGetAttributeDefinitionService getAttributeDefinitionService,
        ICreateAttributeDefinitionService createAttributeDefinitionService,
        IUpdateAttributeDefinitionService updateAttributeDefinitionService,
        ISetAttributeDefinitionStatusService setAttributeDefinitionStatusService,
        IDeleteAttributeDefinitionService deleteAttributeDefinitionService)
    {
        _listAttributeDefinitionsService = listAttributeDefinitionsService;
        _listAttributeDefinitionOptionsService = listAttributeDefinitionOptionsService;
        _getAttributeDefinitionService = getAttributeDefinitionService;
        _createAttributeDefinitionService = createAttributeDefinitionService;
        _updateAttributeDefinitionService = updateAttributeDefinitionService;
        _setAttributeDefinitionStatusService = setAttributeDefinitionStatusService;
        _deleteAttributeDefinitionService = deleteAttributeDefinitionService;
    }

    [HttpGet]
    [ProducesResponseType<PagedResult<AttributeDefinitionListItem>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> List([FromQuery] ListAttributeDefinitionsRequest request, CancellationToken cancellationToken)
    {
        var result = await _listAttributeDefinitionsService.ListAsync(request, cancellationToken);
        return Ok(result);
    }

    // Active-only, unpaginated — for the "add attribute to category" picker. Listed before
    // {id:guid} so routing doesn't try to parse "options" as a Guid.
    [HttpGet("options")]
    [ProducesResponseType<IReadOnlyList<AttributeDefinitionOption>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> ListOptions(CancellationToken cancellationToken)
    {
        var result = await _listAttributeDefinitionOptionsService.ListAsync(cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType<AttributeDefinitionDetail>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _getAttributeDefinitionService.GetAsync(id, cancellationToken);
        return result.ToActionResult(this);
    }

    [HttpPost]
    [ProducesResponseType<AttributeDefinitionDetail>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create(CreateAttributeDefinitionRequest request, CancellationToken cancellationToken)
    {
        var result = await _createAttributeDefinitionService.CreateAsync(request, cancellationToken);
        if (!result.Succeeded)
        {
            return this.ToProblemResult(result.Code, result.Message, result.Failures);
        }

        return CreatedAtAction(nameof(GetById), new { id = result.Value!.Id }, result.Value);
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType<AttributeDefinitionDetail>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Update(Guid id, UpdateAttributeDefinitionRequest request, CancellationToken cancellationToken)
    {
        var result = await _updateAttributeDefinitionService.UpdateAsync(id, request, cancellationToken);
        return result.ToActionResult(this);
    }

    [HttpPost("{id:guid}/activate")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Activate(Guid id, CancellationToken cancellationToken)
    {
        var result = await _setAttributeDefinitionStatusService.SetStatusAsync(id, isActive: true, cancellationToken);
        return result.ToActionResult(this);
    }

    [HttpPost("{id:guid}/deactivate")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Deactivate(Guid id, CancellationToken cancellationToken)
    {
        var result = await _setAttributeDefinitionStatusService.SetStatusAsync(id, isActive: false, cancellationToken);
        return result.ToActionResult(this);
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var result = await _deleteAttributeDefinitionService.DeleteAsync(id, cancellationToken);
        return result.ToActionResult(this);
    }
}
