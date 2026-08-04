# Architecture

## Overview

Nimbus Commerce Platform is a Clean Architecture, Modular Monolith built on .NET 10 / ASP.NET Core. Source dependencies point inward only: outer layers (Api, Infrastructure) may depend on inner layers (Application, Domain), but never the reverse.

```
NimbusCommerce.Api            (ASP.NET Core host, controllers, composition root)
    -> NimbusCommerce.Infrastructure   (EF Core, ASP.NET Core Identity, external concerns)
        -> NimbusCommerce.Application  (use cases, abstractions)
            -> NimbusCommerce.Domain   (business rules, framework-free)
```

`Application` depends only on abstractions it defines itself. `Infrastructure` implements those abstractions and is the only project referencing ASP.NET Core Identity or Entity Framework Core directly. `Api` wires everything together via dependency injection and contains no business logic.

## Authentication Infrastructure

Authentication is built on ASP.NET Core Identity for credential/user management and JWT bearer tokens for API authentication. Register and Login are implemented (Sprint 2, Milestone 2); Refresh, Logout, and `/me` are not (see "Current Implementation Status" below).

### Identity provider: `AddIdentityCore`, not `AddIdentity`

Identity is registered with `AddIdentityCore<ApplicationUser>()` rather than `AddIdentity<...>()`. `AddIdentity` also wires up cookie authentication as the default scheme, which is unnecessary and misleading for an API that authenticates exclusively via JWT bearer tokens. `AddIdentityCore` provides user/role management, password hashing, and lockout without the cookie scheme, and is composed with `.AddRoles<IdentityRole>()`, `.AddEntityFrameworkStores<ApplicationDbContext>()`, `.AddSignInManager()`, and `.AddDefaultTokenProviders()`.

### Token strategy: JWT (HS256)

Access tokens are validated as JWTs signed with a symmetric (HS256) key, configured via `Jwt` settings (`Key`, `Issuer`, `Audience`, `AccessTokenExpirationMinutes`, `RefreshTokenExpirationDays`) bound from configuration into `JwtSettings`. HS256 was chosen for simplicity while the system is a single deployable monolith; if authentication is ever split into its own service, moving to an asymmetric algorithm (RS256) would let other services validate tokens via a public key without sharing the signing secret.

Token *validation* is configured in `Api/Extensions/JwtAuthenticationExtensions.cs` (`AddJwtAuthentication`), wired into the ASP.NET Core authentication pipeline in `Program.cs`. Token *issuance* is implemented by `ITokenService` (`Application/Authentication/Interfaces/ITokenService.cs`) / `TokenService` (`Infrastructure/Identity/TokenService.cs`). `TokenService` signs access tokens with claims `sub`, `email`, one `role` claim per role, and `jti`, using `JwtSecurityTokenHandler`, and generates refresh tokens as 256-bit values from `RandomNumberGenerator`. It performs no persistence or identity lookups — see `IIdentityService` for credential operations and `IRefreshTokenStore` for persistence.

### Refresh tokens

Refresh tokens are represented by a dedicated `RefreshToken` entity, stored in SQL Server as a **hash**, never in plaintext — the same principle applied to password storage. Hashing uses SHA-256, not a slow password KDF (bcrypt/Argon2): refresh tokens are high-entropy, machine-generated values rather than low-entropy user secrets, so a slow KDF would add latency without a corresponding security benefit. The entity carries the fields needed to support refresh token rotation and one-active-token-per-device/session:

- `TokenHash` — hash of the token value; the raw token is never persisted.
- `DeviceName` — a human-readable label for the session, taken from the `User-Agent` request header and truncated to 256 characters. Not a stable device identifier.
- `ExpiresAtUtc`, `RevokedAtUtc`, `ReplacedByTokenHash` — together make rotation and revocation representable. **`RevokedAtUtc`/`ReplacedByTokenHash` remain schema-only**: Login only ever inserts a new `RefreshToken` row via `IRefreshTokenStore` (`Application/Authentication/Interfaces/IRefreshTokenStore.cs`) / `RefreshTokenStore` (`Infrastructure/Identity/RefreshTokenStore.cs`); nothing currently sets or reads these two fields. Rotation and reuse detection are deferred to the Refresh endpoint milestone.

Refresh tokens are delivered to clients via a cookie set by `AuthController`: `HttpOnly`, `Secure`, `SameSite=Strict`, path-scoped to `/api/auth/refresh` (the not-yet-implemented refresh endpoint) so the browser never attaches it elsewhere. `SameSite=Strict` was chosen as the default-safest option for a same-site frontend/backend deployment; revisit if a cross-site frontend deployment is ever required, since `Strict` withholds the cookie on top-level navigations arriving from another site.

### Application/Infrastructure boundary

Application code never references ASP.NET Core Identity types directly. These constructs enforce it:

- **`IIdentityService`** (`Application/Authentication/Interfaces`) — the only door Application has into identity operations: create a user, validate credentials, read roles. Infrastructure's `IdentityService` implements it by wrapping `UserManager<ApplicationUser>` and `SignInManager<ApplicationUser>`. `ValidateCredentialsAsync(email, password)` is the credential-check entry point used by Login: it looks up the user by email, rejects inactive accounts, and — only if the account exists and is active — checks the password via `SignInManager.CheckPasswordSignInAsync` (not `UserManager.CheckPasswordAsync`), so account lockout tracking is active without extra code. It returns a single nullable user id rather than separate "account found" / "password correct" signals, so callers cannot distinguish "no such account" from "wrong password" through the shape of the response. (A residual timing difference between those two cases still exists at the Identity layer itself — see "Known limitations" below.)
- **`ITokenService`** and **`IRefreshTokenStore`** (`Application/Authentication/Interfaces`) — described under "Token strategy" and "Refresh tokens" above.
- **`IdentityOperationResult`** (`Application/Common/Models`) — a success/errors record that `IdentityService` returns instead of `Microsoft.AspNetCore.Identity.IdentityResult`, which is an Infrastructure-shaped type Application must not depend on. Register reuses this same type as its own use-case result rather than introducing a parallel one.

All three Infrastructure implementations (`IdentityService`, `TokenService`, `RefreshTokenStore`) are `internal sealed`, per `coding-standards.md`.

### Entity placement: `ApplicationUser` and `RefreshToken` live in Infrastructure

Both entities live under `Infrastructure/Identity`, not Domain. `ApplicationUser : IdentityUser` inherits a framework/persistence type (`IdentityUser`) whose shape (`PasswordHash`, `SecurityStamp`, `ConcurrencyStamp`, ...) is dictated by ASP.NET Core Identity's storage model, not by business rules — placing it in Domain would force the innermost layer to depend on a specific auth framework. `RefreshToken` has no business invariant reasoned about outside the auth mechanism and is meaningless without `ApplicationUser`, so it is kept alongside it.

By deliberate scope decision for this project, `ApplicationUser` also carries `FirstName`, `LastName`, and `IsActive` directly, rather than splitting profile data into a separate Domain-level entity. This keeps the model simple for the project's current size; if a future module needs to reference "the user" from Domain, it should do so via a plain `UserId` value, not a direct reference to `ApplicationUser`.

Domain currently has no authentication-related types and no dependency on ASP.NET Core Identity.

### Register and Login use cases

Implemented in `Application/Authentication/Register` and `Application/Authentication/Login`, one folder per use case (request type, result type where needed, service interface, service implementation) — the concrete instantiation of the "feature-based organization inside the Application layer" principle from `CLAUDE.md`. `RegisterService` and `LoginService` are `internal sealed`, registered against their public interfaces by `Application`'s own `DependencyInjection.AddApplication`, mirroring the "public interface, internal implementation" pattern already used in Infrastructure.

- **Register** (`POST /api/auth/register`) validates `Email`, `Password`, `ConfirmPassword`, `FirstName`, `LastName` via `System.ComponentModel.DataAnnotations` on `RegisterRequest` (`Required`, `EmailAddress`, `MinLength`, `MaxLength`, `Compare`), enforced automatically by `[ApiController]`'s model validation, plus a defensive `ConfirmPassword` equality re-check inside `RegisterService` for any caller that bypasses the MVC pipeline. On success it returns only a success response — no auto-login, no tokens issued. Email verification is not implemented, so a newly registered user can log in immediately; this is a deliberate scope decision (email verification is a deferred milestone), not an oversight.
- **Login** (`POST /api/auth/login`) validates credentials via `IIdentityService.ValidateCredentialsAsync`, issues an access token and refresh token via `ITokenService`, persists the refresh token hash via `IRefreshTokenStore`, and returns `{ accessToken, expiresAtUtc }` in the JSON body. The refresh token cookie itself is set by `AuthController`, not `LoginService` — `LoginService` never touches `HttpContext`, only returning plain values up to the controller. Every failure cause (unknown email, wrong password, inactive account, lockout) produces an identical generic `401` (`"Invalid email or password."`); unlike Register, no field-level error detail is returned.

### Known limitations

- **Residual login timing side-channel.** `ValidateCredentialsAsync` returns immediately for an unknown or inactive account, before any password hash comparison runs; only the "account exists and is active" path pays that cost. This closes the enumeration vector that existed when Login required two separate `IIdentityService` calls, but a timing difference between "no such account" and "wrong password" still exists at the Identity layer. Full timing normalization (e.g. a dummy hash comparison for unknown accounts) was discussed and intentionally deferred as out of scope for this milestone.
- **No rate limiting** on `/register` or `/login` — both are reachable and unthrottled. Tracked as deferred since Milestone 1; worth prioritizing now that real endpoints exist to attack.

## Current Implementation Status

Implemented: `ApplicationUser`, `RefreshToken`, `ApplicationDbContext`, Identity and JWT-validation configuration, `IIdentityService` / `IdentityService`, `ITokenService` / `TokenService`, `IRefreshTokenStore` / `RefreshTokenStore`, Register and Login endpoints (`AuthController`), dependency injection wiring for both Application and Infrastructure.

Not yet implemented: refresh/logout/`me` endpoints, refresh token rotation and reuse detection (schema exists, behavior does not), database migrations, seed data, rate limiting, email verification, password reset, MFA. These are tracked in `project-journal.md`.
