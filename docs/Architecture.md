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

Authentication is built on ASP.NET Core Identity for credential/user management and JWT bearer tokens for API authentication. As of this milestone, only the infrastructure exists — no endpoints issue or consume tokens yet (see "Current Implementation Status" below).

### Identity provider: `AddIdentityCore`, not `AddIdentity`

Identity is registered with `AddIdentityCore<ApplicationUser>()` rather than `AddIdentity<...>()`. `AddIdentity` also wires up cookie authentication as the default scheme, which is unnecessary and misleading for an API that authenticates exclusively via JWT bearer tokens. `AddIdentityCore` provides user/role management, password hashing, and lockout without the cookie scheme, and is composed with `.AddRoles<IdentityRole>()`, `.AddEntityFrameworkStores<ApplicationDbContext>()`, `.AddSignInManager()`, and `.AddDefaultTokenProviders()`.

### Token strategy: JWT (HS256)

Access tokens are validated as JWTs signed with a symmetric (HS256) key, configured via `Jwt` settings (`Key`, `Issuer`, `Audience`, `AccessTokenExpirationMinutes`, `RefreshTokenExpirationDays`) bound from configuration into `JwtSettings`. HS256 was chosen for simplicity while the system is a single deployable monolith; if authentication is ever split into its own service, moving to an asymmetric algorithm (RS256) would let other services validate tokens via a public key without sharing the signing secret.

Token *validation* is configured in `Api/Extensions/JwtAuthenticationExtensions.cs` (`AddJwtAuthentication`), wired into the ASP.NET Core authentication pipeline in `Program.cs`. Token *issuance* (signing an access token, generating a refresh token) is not yet implemented — it belongs to the login/register milestone, since it is tied to authentication business logic rather than infrastructure.

### Refresh tokens

Refresh tokens are represented by a dedicated `RefreshToken` entity, stored in SQL Server as a **hash**, never in plaintext — the same principle applied to password storage. The entity carries the fields needed to support refresh token rotation and one-active-token-per-device/session, even though the issuance/rotation logic that populates them is not yet implemented:

- `TokenHash` — hash of the token value; the raw token is never persisted.
- `DeviceName` — a human-readable label for the session (e.g. browser/OS string), not a stable device identifier.
- `ExpiresAtUtc`, `RevokedAtUtc`, `ReplacedByTokenHash` — together make rotation and revocation representable: a rotated token is marked revoked and links to the token that replaced it.

Refresh tokens are planned to be delivered to clients via an HttpOnly, Secure, SameSite cookie once the login endpoint exists; this is a decision already made but not yet actionable since no endpoint issues cookies.

### Application/Infrastructure boundary

Application code never references ASP.NET Core Identity types directly. Two constructs enforce this:

- **`IIdentityService`** (`Application/Authentication/Interfaces`) — the only door Application has into identity operations (create user, check password, look up user id, read roles). Infrastructure's `IdentityService` implements it by wrapping `UserManager<ApplicationUser>` and `SignInManager<ApplicationUser>`. Password checks go through `SignInManager.CheckPasswordSignInAsync` (not `UserManager.CheckPasswordAsync`) so account lockout tracking is active without extra code.
- **`IdentityOperationResult`** (`Application/Common/Models`) — a success/errors record that `IdentityService` returns instead of `Microsoft.AspNetCore.Identity.IdentityResult`, which is an Infrastructure-shaped type Application must not depend on.

### Entity placement: `ApplicationUser` and `RefreshToken` live in Infrastructure

Both entities live under `Infrastructure/Identity`, not Domain. `ApplicationUser : IdentityUser` inherits a framework/persistence type (`IdentityUser`) whose shape (`PasswordHash`, `SecurityStamp`, `ConcurrencyStamp`, ...) is dictated by ASP.NET Core Identity's storage model, not by business rules — placing it in Domain would force the innermost layer to depend on a specific auth framework. `RefreshToken` has no business invariant reasoned about outside the auth mechanism and is meaningless without `ApplicationUser`, so it is kept alongside it.

By deliberate scope decision for this project, `ApplicationUser` also carries `FirstName`, `LastName`, and `IsActive` directly, rather than splitting profile data into a separate Domain-level entity. This keeps the model simple for the project's current size; if a future module needs to reference "the user" from Domain, it should do so via a plain `UserId` value, not a direct reference to `ApplicationUser`.

Domain currently has no authentication-related types and no dependency on ASP.NET Core Identity.

## Current Implementation Status

Implemented: `ApplicationUser`, `RefreshToken`, `ApplicationDbContext`, Identity and JWT-validation configuration, `IIdentityService` / `IdentityService`, dependency injection wiring.

Not yet implemented: register/login/refresh/logout/`me` endpoints, token issuance (access token signing, refresh token generation), database migrations, seed data, rate limiting, refresh token reuse detection. These are tracked in `project-journal.md`.
