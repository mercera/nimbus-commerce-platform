# Coding Standards

Conventions here describe *how* code is written. Architectural decisions (what depends on what, why a technology was chosen) belong in `Architecture.md`.

## Naming

- `DateTime` properties are suffixed `AtUtc` (e.g. `CreatedAtUtc`, `ExpiresAtUtc`, `RevokedAtUtc`) and always store UTC. Never store or compare unqualified local `DateTime` values.
- Configuration/options classes expose their configuration section name as a `public const string SectionName` on the class itself (e.g. `JwtSettings.SectionName`), instead of repeating the section string at each call site.

## Dependency injection

- Each project that needs to register its own services exposes a static `DependencyInjection` class with an `Add<ProjectName>(IServiceCollection, ...)` extension method (e.g. `Infrastructure.DependencyInjection.AddInfrastructure`). `Program.cs` stays a thin composition root that calls these extension methods rather than registering services inline.
- Host-specific middleware/pipeline configuration (e.g. JWT bearer authentication setup) lives in its own extension method under `Api/Extensions/`, kept separate from project-level DI registration since it configures the ASP.NET Core pipeline rather than a service graph.

## Implementations of Application abstractions

- Infrastructure classes that exist solely to implement an Application-layer interface (e.g. `IdentityService`) are declared `internal sealed`. The interface is the only public contract consumers should see; the implementation is a Infrastructure-internal detail.

## Persistence configuration

- Entity configuration uses `IEntityTypeConfiguration<T>` classes under `Persistence/Configurations/`, applied via `ApplyConfigurationsFromAssembly`. Data annotations are not used for entity mapping, to keep persistence concerns out of entity classes.

## Cross-boundary results

- Types that cross the Application/Infrastructure boundary to report an outcome (e.g. `IdentityOperationResult`) are immutable records with static factory methods (`Success()`, `Failure(errors)`), not exceptions, for expected/handleable failure paths.
