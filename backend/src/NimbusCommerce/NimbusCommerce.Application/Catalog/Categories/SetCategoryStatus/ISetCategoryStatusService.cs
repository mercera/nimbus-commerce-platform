using NimbusCommerce.Application.Common.Models;

namespace NimbusCommerce.Application.Catalog.Categories.SetCategoryStatus;

/// <summary>
/// Backs both POST /activate and POST /deactivate — the two endpoints share the entire
/// lookup/ownership/rule path and differ only in the flag and which guard fires.
/// </summary>
public interface ISetCategoryStatusService
{
    Task<OperationResult> SetStatusAsync(Guid categoryId, bool isActive, CancellationToken cancellationToken);
}
