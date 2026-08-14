# Coding Standards

Conventions here describe *how* code is written. Architectural decisions (what depends on what, why a technology was chosen) belong in `Architecture.md`.

## Naming

- `DateTime` properties are suffixed `AtUtc` (e.g. `CreatedAtUtc`, `ExpiresAtUtc`, `RevokedAtUtc`) and always store UTC. Never store or compare unqualified local `DateTime` values.
- Configuration/options classes expose their configuration section name as a `public const string SectionName` on the class itself (e.g. `JwtSettings.SectionName`), instead of repeating the section string at each call site.

## Dependency injection

- Each project that needs to register its own services exposes a static `DependencyInjection` class with an `Add<ProjectName>(IServiceCollection, ...)` extension method (e.g. `Infrastructure.DependencyInjection.AddInfrastructure`). `Program.cs` stays a thin composition root that calls these extension methods rather than registering services inline.
- Host-specific middleware/pipeline configuration (e.g. JWT bearer authentication setup) lives in its own extension method under `Api/Extensions/`, kept separate from project-level DI registration since it configures the ASP.NET Core pipeline rather than a service graph.

## Implementations of interfaces

- Classes that exist solely to implement an interface are declared `internal sealed`, whether the implementation sits in Infrastructure (e.g. `IdentityService`, `TokenService`, `RefreshTokenStore` implementing Application-layer interfaces) or directly in Application (e.g. `RegisterService`, `LoginService` implementing their own same-layer interfaces `IRegisterService`, `ILoginService`). The interface is the only public contract consumers should see; the implementation is an internal detail of whichever assembly owns it.

## Persistence configuration

- Entity configuration uses `IEntityTypeConfiguration<T>` classes under `Persistence/Configurations/`, applied via `ApplyConfigurationsFromAssembly`. Data annotations are not used for entity mapping, to keep persistence concerns out of entity classes.

## Cross-boundary results

- Types that cross a layer boundary to report an outcome — Application/Infrastructure (e.g. `IdentityOperationResult`) or Application/Api (e.g. `LoginResult`) — are immutable records with static factory methods (`Success(...)`, `Failure(...)`), not exceptions, for expected/handleable failure paths. Where the outcome type would otherwise leak account-existence information (e.g. `LoginResult.Failure()`), the failure factory takes no detail, deliberately.
- **`OperationResult` / `OperationResult<T>` (`Application/Common/Models/`) is the shared result type for new use cases, starting with the Product Catalogue.** It carries an `ErrorCode` (`NotFound`, `Conflict`, `Validation`, `RuleViolation`) instead of a bespoke `Succeeded`/error shape per use case — justified once a feature has enough use cases and enough repeated failure shapes that one bespoke record per use case becomes duplication rather than clarity. `IdentityOperationResult`, `LoginResult`, and `RefreshResult` are deliberately **not** refactored onto it — this is a forward-only evolution of the original per-use-case pattern, not a replacement for it. Application never references HTTP status codes; `Api/Extensions/OperationResultExtensions.cs` is the only place an `ErrorCode` is mapped to a status code and a `ProblemDetails`/`ValidationProblemDetails` body.

## Catalogue ownership filtering

- Every catalogue aggregate root (`Category`, `Product`, `AttributeDefinition`) carries `OwnerUserId`. Child entities that belong to one of those roots (`CategoryAttributeDefinition`, `ProductAttributeValue`, `ProductImage`) do **not** carry their own `OwnerUserId` column. This is safe only because of a rule that must hold everywhere a catalogue store is written: **a child entity may only be loaded through a query that has already filtered its parent by `OwnerUserId`.** A store method that queries a child table directly, without joining through an owner-filtered parent, reintroduces the cross-tenant leak the missing column was relying on the parent to prevent. `CategoryStore`'s two product-count methods (`CountActiveProductsAsync`, `CountProductsAsync`) filter `Products` by `OwnerUserId` directly rather than relying on this rule, precisely because they don't go through a loaded `Category` — when in doubt, filter explicitly rather than lean on the rule.
- Ownership is enforced by an explicit `.Where(x => x.OwnerUserId == ownerUserId)` in each store query, not an EF global query filter — see `Architecture.md`, "Product Catalogue" → "Catalogue ownership", for why.
- A lookup that fails the ownership check returns `404` (`OperationResult.NotFound`), never `403` — a caller must not be able to distinguish "doesn't exist" from "exists but isn't yours."

## HTTP contracts vs use-case types

HTTP response contracts belong in the Api project, not Application. Request DTOs are an accepted exception to that rule — see below.

- A type belongs to Application only if Application code references it. `LoginRequest` (parameter of `ILoginService.LoginAsync`) and `LoginResult` (its return type) qualify. `LoginResponse` does not — nothing in Application produces or consumes it, and it exists to shape the HTTP body by omitting the refresh token, which is a presentation decision. Types like it live in `Api/Contracts/<Feature>/`.
- Introduce an Api-layer contract only where the wire shape actually diverges from the use-case result. Register returns `IdentityOperationResult` directly and needs no contract type; Login does, because the response deliberately drops fields the result carries.
- **Accepted exception — request DTOs:** `RegisterRequest`/`LoginRequest` keep their `System.ComponentModel.DataAnnotations` attributes in Application. These are BCL metadata, not ASP.NET Core types — Application references no ASP.NET Core package — and they declare what the use case requires independently of transport. `[ApiController]` enforces them at the MVC boundary; services additionally re-check invariants they cannot delegate (see `RegisterService`'s `ConfirmPassword` check). No separate Api-layer request contracts or Application-layer commands are introduced for this; the request type is shared as-is.
- **Accepted tradeoff:** `RegisterRequest.ConfirmPassword` and its `[Compare(nameof(Password))]` attribute are the one part of this exception that is presentation-oriented rather than transport-neutral — "confirm your password" is a form-UX concept, not something every caller of `RegisterService` would need. This was discussed and intentionally accepted alongside the decision above: separating it into an Api-layer field was judged disproportionate ceremony for the project's current size and caller count.
