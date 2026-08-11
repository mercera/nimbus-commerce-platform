namespace NimbusCommerce.Api.Contracts.Authentication;

/// <summary>
/// The authenticated caller's current account state, read from the database rather than decoded
/// from their access token — so it reflects changes made after that token was issued.
/// </summary>
public sealed record MeResponse(
    string Id,
    string Email,
    string FirstName,
    string LastName,
    IReadOnlyList<string> Roles);
