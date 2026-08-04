# Development Setup

## Prerequisites

- .NET 10 SDK
- SQL Server instance reachable for local development (LocalDB on Windows is sufficient; the checked-in development connection string targets `(localdb)\mssqllocaldb`)

No database migrations exist yet, so no database needs to be created or reachable to build the solution. It will be required starting with the milestone that introduces migrations.

## Building

```
dotnet build backend/src/NimbusCommerce/NimbusCommerce.slnx
```

## NuGet packages introduced for authentication

These are restored automatically on build; listed here for reference only.

`NimbusCommerce.Infrastructure`:
- `Microsoft.AspNetCore.Identity.EntityFrameworkCore`
- `Microsoft.EntityFrameworkCore.SqlServer`
- `System.IdentityModel.Tokens.Jwt` (Milestone 2) — used by `TokenService` to sign access tokens (`JwtSecurityTokenHandler`).
- `FrameworkReference: Microsoft.AspNetCore.App` — required because `SignInManager<T>` lives in the ASP.NET Core shared framework, which a class library project does not reference by default.

`NimbusCommerce.Application`:
- `Microsoft.Extensions.DependencyInjection.Abstractions` (Milestone 2) — needed for `DependencyInjection.AddApplication`, an `IServiceCollection` extension method; Application had no DI package reference before this milestone since it previously only declared interfaces.

`NimbusCommerce.Api`:
- `Microsoft.AspNetCore.Authentication.JwtBearer`

## Required configuration

Two configuration sections are required for the app to start: `ConnectionStrings:DefaultConnection` and `Jwt` (`Key`, `Issuer`, `Audience`, `AccessTokenExpirationMinutes`, `RefreshTokenExpirationDays`).

`appsettings.json` defines the shape with empty placeholders — real values must come from `dotnet user-secrets`, environment variables, or Azure Key Vault in any environment that isn't local development, and must never be committed.

`appsettings.Development.json` already contains working local defaults:
- A LocalDB connection string.
- A JWT signing key clearly labeled as a development-only placeholder. **This key must never be reused outside local development.**

## Known pre-existing item

`NimbusCommerce.Api` restores with an `NU1903` advisory (`Microsoft.OpenApi` 2.0.0, high severity), a transitive dependency of `Microsoft.AspNetCore.OpenApi`. It predates the authentication work and has not been addressed; tracked in `project-journal.md`.
