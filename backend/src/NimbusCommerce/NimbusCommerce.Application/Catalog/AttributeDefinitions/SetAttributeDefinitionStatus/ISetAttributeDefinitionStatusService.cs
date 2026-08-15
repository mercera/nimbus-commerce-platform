using NimbusCommerce.Application.Common.Models;

namespace NimbusCommerce.Application.Catalog.AttributeDefinitions.SetAttributeDefinitionStatus;

/// <summary>
/// Backs both POST /activate and POST /deactivate — the two endpoints share the entire
/// lookup/ownership path and differ only in the flag. Unlike SetCategoryStatusService,
/// deactivate has no query-dependent guard — see AttributeDefinition.Deactivate's doc comment.
/// </summary>
public interface ISetAttributeDefinitionStatusService
{
    Task<OperationResult> SetStatusAsync(Guid attributeDefinitionId, bool isActive, CancellationToken cancellationToken);
}
