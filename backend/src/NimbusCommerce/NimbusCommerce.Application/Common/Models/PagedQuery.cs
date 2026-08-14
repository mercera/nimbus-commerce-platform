using System.ComponentModel.DataAnnotations;

namespace NimbusCommerce.Application.Common.Models;

/// <summary>
/// Shared base for paged list requests. Invalid page sizes are rejected via [Range], not silently
/// clamped to MaxPageSize — a caller asking for 500 gets a 400, not a surprising 100.
/// </summary>
public abstract class PagedQuery
{
    public const int DefaultPageSize = 20;
    public const int MaxPageSize = 100;

    [Range(1, int.MaxValue, ErrorMessage = "Page must be 1 or greater.")]
    public int Page { get; set; } = 1;

    [Range(1, MaxPageSize, ErrorMessage = "PageSize must be between 1 and 100.")]
    public int PageSize { get; set; } = DefaultPageSize;
}
