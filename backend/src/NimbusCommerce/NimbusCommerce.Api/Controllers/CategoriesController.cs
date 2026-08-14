using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NimbusCommerce.Api.Extensions;
using NimbusCommerce.Application.Catalog.Categories.CreateCategory;
using NimbusCommerce.Application.Catalog.Categories.DeleteCategory;
using NimbusCommerce.Application.Catalog.Categories.GetCategory;
using NimbusCommerce.Application.Catalog.Categories.ListCategories;
using NimbusCommerce.Application.Catalog.Categories.SetCategoryStatus;
using NimbusCommerce.Application.Catalog.Categories.UpdateCategory;
using NimbusCommerce.Application.Catalog.Interfaces;
using NimbusCommerce.Application.Common.Models;

namespace NimbusCommerce.Api.Controllers;

// Every catalogue endpoint requires authentication and there are no anonymous exceptions here
// (unlike AuthController), so [Authorize] sits on the controller — the fail-safe default — per
// the inversion AuthController's own comment calls for once protected actions outnumber
// anonymous ones.
[Authorize]
[ApiController]
[Route("api/categories")]
public sealed class CategoriesController : ControllerBase
{
    private readonly IListCategoriesService _listCategoriesService;
    private readonly IGetCategoryService _getCategoryService;
    private readonly ICreateCategoryService _createCategoryService;
    private readonly IUpdateCategoryService _updateCategoryService;
    private readonly ISetCategoryStatusService _setCategoryStatusService;
    private readonly IDeleteCategoryService _deleteCategoryService;

    public CategoriesController(
        IListCategoriesService listCategoriesService,
        IGetCategoryService getCategoryService,
        ICreateCategoryService createCategoryService,
        IUpdateCategoryService updateCategoryService,
        ISetCategoryStatusService setCategoryStatusService,
        IDeleteCategoryService deleteCategoryService)
    {
        _listCategoriesService = listCategoriesService;
        _getCategoryService = getCategoryService;
        _createCategoryService = createCategoryService;
        _updateCategoryService = updateCategoryService;
        _setCategoryStatusService = setCategoryStatusService;
        _deleteCategoryService = deleteCategoryService;
    }

    [HttpGet]
    [ProducesResponseType<PagedResult<CategoryListItem>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> List([FromQuery] ListCategoriesRequest request, CancellationToken cancellationToken)
    {
        var result = await _listCategoriesService.ListAsync(request, cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType<CategoryDetail>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _getCategoryService.GetAsync(id, cancellationToken);
        return result.ToActionResult(this);
    }

    [HttpPost]
    [ProducesResponseType<CategoryDetail>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create(CreateCategoryRequest request, CancellationToken cancellationToken)
    {
        var result = await _createCategoryService.CreateAsync(request, cancellationToken);
        if (!result.Succeeded)
        {
            return this.ToProblemResult(result.ErrorCode, result.Message, result.Failures);
        }

        return CreatedAtAction(nameof(GetById), new { id = result.Value!.Id }, result.Value);
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType<CategoryDetail>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Update(Guid id, UpdateCategoryRequest request, CancellationToken cancellationToken)
    {
        var result = await _updateCategoryService.UpdateAsync(id, request, cancellationToken);
        return result.ToActionResult(this);
    }

    [HttpPost("{id:guid}/activate")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Activate(Guid id, CancellationToken cancellationToken)
    {
        var result = await _setCategoryStatusService.SetStatusAsync(id, isActive: true, cancellationToken);
        return result.ToActionResult(this);
    }

    [HttpPost("{id:guid}/deactivate")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Deactivate(Guid id, CancellationToken cancellationToken)
    {
        var result = await _setCategoryStatusService.SetStatusAsync(id, isActive: false, cancellationToken);
        return result.ToActionResult(this);
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var result = await _deleteCategoryService.DeleteAsync(id, cancellationToken);
        return result.ToActionResult(this);
    }
}
