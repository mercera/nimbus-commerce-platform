# Development Setup

## Prerequisites

- .NET 10 SDK
- SQL Server instance reachable for local development (LocalDB on Windows is sufficient; the checked-in development connection string targets `(localdb)\mssqllocaldb`)
- Node.js 20+ and npm, for the frontend (`frontend/`)

A reachable database is not required to build the solution, but is required to run it — `InitialCreate` (Sprint 2, Milestone 2) must be applied before Register, Login, or Refresh can be exercised end-to-end.

## Building

```
dotnet build backend/src/NimbusCommerce/NimbusCommerce.slnx
```

## Running the frontend

The frontend (`frontend/`) is a separate Vite/React/TypeScript project with its own `package.json` — it is not part of the `.slnx` and is not built by `dotnet build`.

```
cd frontend
npm install
npm run dev
```

This starts the Vite dev server on `http://localhost:5173`. **The backend API must also be running** (`https://localhost:7096`, see below) — `vite.config.ts` proxies every `/api/*` request from the dev server to it, so the browser only ever talks to `http://localhost:5173`. Run both at once:

```
# terminal 1 — backend
dotnet run --project backend/src/NimbusCommerce/NimbusCommerce.Api/NimbusCommerce.Api.csproj --launch-profile https

# terminal 2 — frontend
cd frontend && npm run dev
```

`npm run build` (`tsc -b && vite build`) type-checks and produces a production bundle in `frontend/dist/`.

### Dev-cert trust

The backend's HTTPS launch profile uses the ASP.NET Core development certificate. If browser requests through the proxy fail with a certificate error, trust it once:

```
dotnet dev-certs https --trust
```

`vite.config.ts` also sets `secure: false` on the proxy target, which accepts the dev certificate even if it isn't independently trusted by Node's HTTP client — but the browser still needs to trust it for the initial `dotnet dev-certs` handshake in some setups.

### Why a proxy, not CORS

`Program.cs` has no CORS policy, and none was added to support the frontend. `vite.config.ts` proxies `/api` to `https://localhost:7096` instead, so the browser sees only same-origin requests to `localhost:5173` — no CORS, no preflight, and the refresh-token cookie's `SameSite=Strict`/`Path=/api/auth` scoping (see `Architecture.md`) work unmodified. See `Architecture.md` → "Frontend" for the full reasoning and the known gap this leaves for any deployment that doesn't proxy the two origins together.

## Applying migrations

```
dotnet ef database update \
  --project backend/src/NimbusCommerce/NimbusCommerce.Infrastructure \
  --startup-project backend/src/NimbusCommerce/NimbusCommerce.Api
```

The Refresh milestone did not require a new EF migration: `RevokedAtUtc`, `ReplacedByTokenHash`, and the relevant `RefreshTokens` indexes already existed in the `InitialCreate` migration. Applying `InitialCreate` is sufficient for the current authentication functionality.

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

The `NU1903` advisory (`Microsoft.OpenApi` 2.0.0, transitive via `Microsoft.AspNetCore.OpenApi`) has been resolved: `Microsoft.OpenApi` is pinned to `2.7.5` directly in `NimbusCommerce.Api.csproj`. See `project-journal.md` for details.
