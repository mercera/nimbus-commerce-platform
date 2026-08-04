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
