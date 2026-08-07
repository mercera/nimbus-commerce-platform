# Project Journal

## 2026-08-02 — Sprint 2, Milestone 1: Authentication Infrastructure

### Completed

Implemented the authentication infrastructure layer only — no endpoints, business logic, DTOs, validators, migrations, or seed data.

### Major architectural decisions

- ASP.NET Core Identity registered via `AddIdentityCore<ApplicationUser>()` rather than `AddIdentity`, to avoid pulling in the default cookie authentication scheme on an API that only uses JWT bearer tokens.
- JWT access tokens signed with HS256 (symmetric key). Token validation is configured now; token issuance is deferred to the login/register milestone.
- Refresh tokens stored as hashes (never plaintext), one per device/session, with schema support for rotation (`RevokedAtUtc`, `ReplacedByTokenHash`) even though no issuance/rotation logic exists yet.
- `IIdentityService` (Application) / `IdentityService` (Infrastructure) established as the only boundary between Application and ASP.NET Core Identity; `IdentityOperationResult` used instead of exposing `IdentityResult` across that boundary.
- `ApplicationUser` and `RefreshToken` placed in `Infrastructure/Identity`, not Domain — `IdentityUser` is a framework/persistence type, and Domain must stay framework-free. Discussed and confirmed explicitly before implementation.
- By approved adjustment, `ApplicationUser` includes `FirstName`, `LastName`, and `IsActive` directly (originally recommended as a separate profile entity during the earlier architecture review; the project owner made a deliberate, informed decision to keep it simpler for this project's scope).
- `DeviceName` used instead of `DeviceId` on `RefreshToken`, since what's stored is a descriptive label, not a stable device identifier.

### Files/modules introduced

Application: `Authentication/Interfaces/IIdentityService.cs`, `Common/Models/IdentityOperationResult.cs`.

Infrastructure: `Identity/ApplicationUser.cs`, `Identity/RefreshToken.cs`, `Identity/JwtSettings.cs`, `Identity/IdentityService.cs`, `Persistence/ApplicationDbContext.cs`, `Persistence/Configurations/RefreshTokenConfiguration.cs`, `Persistence/Configurations/ApplicationUserConfiguration.cs`, `DependencyInjection.cs`.

Api: `Extensions/JwtAuthenticationExtensions.cs`; modified `Program.cs`, `appsettings.json`, `appsettings.Development.json`.

Packages added: `Microsoft.AspNetCore.Identity.EntityFrameworkCore`, `Microsoft.EntityFrameworkCore.SqlServer`, `Microsoft.AspNetCore.Authentication.JwtBearer` (all 10.0.10), plus a `FrameworkReference` to `Microsoft.AspNetCore.App` in Infrastructure.

### Lessons learned

- `SignInManager<T>` lives in the ASP.NET Core shared framework, not in the `Microsoft.Extensions.Identity.*` NuGet packages. A plain `Microsoft.NET.Sdk` class library (Infrastructure) needs an explicit `<FrameworkReference Include="Microsoft.AspNetCore.App" />` to use it — this only surfaced as a build error (`CS0246`) after everything else compiled, since `UserManager<T>` alone doesn't require it.
- `Program.cs` was missing `app.UseAuthentication()` entirely before this milestone — authorization middleware was present with nothing to authenticate against. Fixed as part of wiring the JWT pipeline.
- `dotnet add package` resolved all first-party Microsoft packages to `10.0.10`, matching the installed .NET 10 SDK; no manual version pinning was needed.

### Outstanding work

- Register, login, refresh, logout, `/me` endpoints.
- Token issuance service (access token signing, refresh token generation/hashing) — not part of `IIdentityService`.
- Refresh token rotation and reuse-detection logic (schema exists, behavior does not).
- Database migrations and initial schema application.
- Role seed data (Admin, Manager, Employee).
- Rate limiting on future `/login`, `/register`, `/refresh` endpoints.
- Pre-existing `NU1903` advisory on `Microsoft.OpenApi` 2.0.0 (transitive via `Microsoft.AspNetCore.OpenApi`), not addressed this milestone.

### Next milestone

Sprint 2, Milestone 2: Register & Login — exercises `IIdentityService.CreateUserAsync` end-to-end, introduces token issuance, and requires deciding refresh-token client delivery (HttpOnly cookie, per the architecture decision already on record) before any endpoint ships.

## 2026-08-04 — Sprint 2, Milestone 2: Register & Login

### Completed

Implemented Register and Login end-to-end: request validation, credential validation, JWT access token issuance, refresh token generation/hashing/persistence, and the two controller endpoints. Refresh, logout, `/me`, email verification, password reset, MFA, and rate limiting remain explicitly out of scope.

### Major architectural decisions

- `IIdentityService.CheckPasswordAsync`/`GetUserIdAsync` removed and replaced with a single `ValidateCredentialsAsync(email, password)` returning one nullable user id, collapsing "unknown account," "wrong password," and "inactive account" into an identical failure signal — closes the email-enumeration vector the original two-call shape would have exposed to Login.
- `ApplicationUser.IsActive` is now enforced at login (present in the schema since Milestone 1 but unused until now).
- New `ITokenService` (Application) / `TokenService` (Infrastructure) — generates and hashes tokens only, no persistence or identity lookups. Access tokens: HS256, claims `sub`/`email`/`role`(s)/`jti`. Refresh tokens: 256-bit `RandomNumberGenerator` output, hashed with SHA-256 (not a slow password KDF — the value is high-entropy and machine-generated, not a user secret).
- New `IRefreshTokenStore` (Application) / `RefreshTokenStore` (Infrastructure) — the only Infrastructure type that writes `RefreshToken` rows; Application only ever passes primitives (user id, hash, expiry, device name).
- Login returns a generic `401` for every failure cause, with no field-level detail — deliberately asymmetric with Register (which does return field-level `IdentityOperationResult` errors): disclosing "email already in use" at registration is normal UX, disclosing which login credential was wrong is not.
- Refresh token delivered via `HttpOnly`, `Secure`, `SameSite=Strict` cookie, path-scoped to `/api/auth/refresh` (the not-yet-implemented refresh endpoint). `SameSite=Strict` chosen as the default-safest option for a same-site deployment.
- Register validation is a hybrid: `System.ComponentModel.DataAnnotations` on `RegisterRequest` for structural checks (`Required`, `EmailAddress`, `MinLength`, `MaxLength`, `Compare`), auto-enforced by `[ApiController]`, plus a defensive `ConfirmPassword` equality re-check inside `RegisterService` for callers outside the MVC pipeline. No FluentValidation introduced.
- Cookie assembly is an `AuthController` responsibility, not `LoginService`'s — Application never touches `HttpContext`; `LoginService` returns plain values (access token, expiry, raw refresh token, expiry) and the controller shapes the HTTP response.
- Application-layer feature folders (`Authentication/Register`, `Authentication/Login`, `Authentication/Interfaces`) confirmed as the long-term organizing pattern for all future auth features (Refresh, Logout, Password Reset, Email Verification, MFA): one folder per use case, flat under `Authentication/`, no deeper nesting, shared abstractions stay in `Interfaces/`. Discussed and agreed before implementation.
- `internal sealed` for interface implementations extended from an Infrastructure-only convention to also cover Application's own service implementations (`RegisterService`, `LoginService`).

### Files/modules introduced

Application: `Authentication/Register/{RegisterRequest,IRegisterService,RegisterService}.cs`, `Authentication/Login/{LoginRequest,LoginResult,ILoginService,LoginService}.cs`, `Authentication/Interfaces/{ITokenService,IRefreshTokenStore}.cs`, `DependencyInjection.cs`. Modified: `Authentication/Interfaces/IIdentityService.cs`.

Infrastructure: `Identity/TokenService.cs`, `Identity/RefreshTokenStore.cs`. Modified: `Identity/IdentityService.cs`, `DependencyInjection.cs`.

Api: `Controllers/AuthController.cs`. Modified: `Program.cs` (added `AddApplication()` to the composition root).

Packages added: `System.IdentityModel.Tokens.Jwt` 8.22.0 (Infrastructure), `Microsoft.Extensions.DependencyInjection.Abstractions` 10.0.10 (Application).

### Lessons learned

- Application had no DI package reference at all before this milestone — it previously only declared interfaces, no concrete classes. Introducing `AddApplication` as an `IServiceCollection` extension method surfaced a build error (`CS0234`/`CS0246`) until `Microsoft.Extensions.DependencyInjection.Abstractions` was added. Not part of the originally approved file list; called out as a build-time necessity, same category as Milestone 1's `FrameworkReference` discovery.
- Collapsing `IIdentityService`'s credential-check surface to one method (`ValidateCredentialsAsync`) closes enumeration at the call-site level but doesn't fully normalize timing inside the implementation itself. Worth remembering this is a partial mitigation, not a complete one, before treating Login as "enumeration-safe."

### Outstanding work

- Refresh, Logout, `/me` endpoints.
- Refresh token rotation and reuse-detection logic (schema exists, behavior does not).
- Database migrations and initial schema application — **Register and Login cannot be exercised against a real database until this exists.**
- Role seed data (Admin, Manager, Employee).
- Rate limiting on `/login` and `/register` — now live, reachable endpoints rather than a theoretical future concern.
- Email verification, password reset, MFA.
- Full timing-attack normalization for `ValidateCredentialsAsync` (currently a partial mitigation — see "Known limitations" in `Architecture.md`).
- Pre-existing `NU1903` advisory on `Microsoft.OpenApi` 2.0.0, still not addressed.

### Next milestone

Implementation review session: exercise Register/Login together (requires a migration first), decide on any refinements, and scope the Refresh endpoint milestone.

## 2026-08-06 — API Documentation Tooling & Contract Boundary Cleanup

### Completed

Added an interactive OpenAPI UI (Scalar) for local development, wired JWT bearer authentication into the generated OpenAPI document, added response metadata to `AuthController`, and corrected the layer placement of `LoginResponse` after a code-review finding. Not a planned milestone — an out-of-band follow-up prompted by the need to exercise the auth endpoints from a browser during development.

### Major architectural decisions

- Scalar (`Scalar.AspNetCore`) added as the OpenAPI UI, mapped via `app.MapScalarApiReference()`. `Microsoft.AspNetCore.OpenApi` only generates the OpenAPI document; it ships no interactive UI on its own. Both `app.MapOpenApi()` and `app.MapScalarApiReference()` remain inside the existing `IsDevelopment()` guard in `Program.cs` — the document and its UI are development-only.
- New `BearerSecuritySchemeTransformer` (`Api/Extensions/OpenApi/`), an `IOpenApiDocumentTransformer` registered on `AddOpenApi()`, declares the JWT bearer security scheme on the generated document and applies it only to operations whose action carries `[Authorize]` — anonymous endpoints (Register, Login) are left undecorated. Without it, `AddOpenApi()` emitted no security scheme, so an OpenAPI UI's "Authorize" control had nothing to bind to.
- `[ProducesResponseType]` added to both `AuthController` actions (`Register`: 200 / 400 with `IReadOnlyList<string>`; `Login`: 200 with `LoginResponse` / 401), so the generated document carries real response schemas instead of leaving them undocumented.
- **`LoginResponse` moved from `Application/Authentication/Login/` to `Api/Contracts/Authentication/`** (namespace `NimbusCommerce.Api.Contracts.Authentication`), following a code-review finding that it was misplaced. `LoginResponse` is an HTTP response DTO — nothing in Application produces or consumes it — while `LoginResult` (unchanged, still in Application) is the genuine Application/Api boundary result. `AuthController` now translates `LoginResult` → `LoginResponse` explicitly. The move was verified not to change the public contract: the generated OpenAPI schema is still named `LoginResponse` with the same `accessToken`/`expiresAtUtc` shape, since ASP.NET Core derives schema IDs from the type name, not its namespace.
- Documented the Application ↔ Api boundary rule generally (`coding-standards.md`, "HTTP contracts vs use-case types"): a type belongs to Application only if Application code references it. This is also why `RegisterRequest`/`LoginRequest` stay in Application (Application consumes them) while `LoginResponse` does not (nothing in Application does).
- Decided **not** to introduce separate Api-layer request DTOs, Application-layer commands, or FluentValidation at this stage. `RegisterRequest`/`LoginRequest` keep their `System.ComponentModel.DataAnnotations` attributes in Application — accepted because the attributes are BCL metadata, not ASP.NET Core types, and Application references no ASP.NET Core package. `RegisterRequest.ConfirmPassword` / `[Compare]` was noted as the one attribute that is presentation-oriented rather than transport-neutral ("confirm your password" is a form-UX concept); recorded as an accepted tradeoff rather than acted on, to avoid mapping ceremony disproportionate to the project's current size and caller count.
- Resolved the `NU1903` advisory on `Microsoft.OpenApi` 2.0.0 (tracked as outstanding since Milestone 1) as a side effect of this work: `Microsoft.OpenApi` is now pinned to `2.7.5` directly in `NimbusCommerce.Api.csproj`, above the transitive `2.0.0` pulled in by `Microsoft.AspNetCore.OpenApi`, patching `GHSA-v5pm-xwqc-g5wc`.

### Files/modules introduced

Api: `Extensions/OpenApi/BearerSecuritySchemeTransformer.cs`, `Contracts/Authentication/LoginResponse.cs`. Modified: `Program.cs`, `Controllers/AuthController.cs`, `NimbusCommerce.Api.csproj`.

Application: removed `Authentication/Login/LoginResponse.cs`.

Packages added: `Scalar.AspNetCore` 2.16.17. Version changes: `Microsoft.AspNetCore.OpenApi` 10.0.9 → 10.0.10; `Microsoft.OpenApi` pinned to 2.7.5 (new direct reference, overriding the transitive 2.0.0).

### Lessons learned

- .NET 10 ships `Microsoft.OpenApi` v2, which reshaped the security-scheme types from the v1 shape most existing examples show — notably `SecuritySchemes` values are `IOpenApiSecurityScheme`, and security requirements reference schemes via `OpenApiSecuritySchemeReference` rather than an inline `OpenApiSecurityScheme { Reference = ... }`. Writing the transformer from the intended shape and fixing exact member names from compiler errors was faster than researching the v2 API surface up front.
- Verifying "no client-visible change" after moving `LoginResponse` required inspecting the actual generated OpenAPI JSON (schema name, property names, response wiring), not just confirming the build succeeded — a namespace move can silently break generated tooling in ways that only show up in the emitted document.

### Outstanding work

Unchanged from Milestone 2 (see above), with `NU1903` now resolved: refresh, logout, `/me` endpoints; refresh token rotation and reuse-detection logic; database migrations and initial schema application; role seed data; rate limiting on `/login` and `/register`; email verification, password reset, MFA; full timing-attack normalization for `ValidateCredentialsAsync`.

### Next milestone

Unchanged: implementation review session (Register/Login end-to-end against a real database), then scope the Refresh endpoint milestone.

## 2026-08-07 — Sprint 2, Milestone 3: Refresh Token Endpoint (Rotation + Reuse Detection)

### Completed

Implemented `POST /api/auth/refresh`: reads the `refreshToken` cookie set by Login, validates it, rotates it, and issues a new access token. Turned the previously schema-only `RevokedAtUtc`/`ReplacedByTokenHash` columns into live behavior. Logout, `/me`, and rate limiting remain out of scope.

### Major architectural decisions

- **Validation order is load-bearing, not incidental**: not-found → expired-or-revoked distinguished from reuse → active-user check → rotate-then-mint. Reuse detection (a revoked token presented again) is checked *before* expiry, because a revoked-and-expired replay is still theft evidence; an unknown-hash lookup revokes nothing, so a garbage token can't be used to force-revoke a real session by guessing.
- **`RefreshResult.Failure()` takes no detail**, mirroring `LoginResult`. `AuthController` returns one identical generic `401` for unknown/expired/revoked/deactivated-user, so a stolen token can't be probed for its state. No `ReuseDetected` flag exists anywhere — it would inevitably leak into the response.
- **Reuse detection revokes every active token for the user**, not just the affected chain — there's no `TokenFamilyId` column, only the forward-pointing `ReplacedByTokenHash`, and per the parallel-refresh limitation below, chain links can be orphaned, so a chain walk wouldn't be reliable anyway. Matches RFC 9700 / OAuth 2.1 BCP guidance.
- **`RotateAsync` is one `SaveChangesAsync`** (revoke-old + insert-new), relying on EF Core's implicit transaction rather than an explicit one. **`DeviceName` is re-read from the current request's `User-Agent` on every rotation, not carried over** from the row being replaced — `Architecture.md` already defines it as a descriptive label, not a stable identifier, so carrying it forward would just fossilize a stale browser string.
- **`IRefreshTokenStore` widened from insert-only to a small repository** (`FindByHashAsync`, `RotateAsync`, `RevokeAllActiveForUserAsync` added). Still boundary-clean: `FindByHashAsync` returns `StoredRefreshToken(UserId, ExpiresAtUtc, RevokedAtUtc)`, a primitives-only record, never the entity.
- **`RefreshToken.IsActive` (the original computed property) was deleted.** It was unmapped by EF and had zero references anywhere in the solution; `.Where(rt => rt.IsActive)` would compile but throw `InvalidOperationException` at runtime. Token usability is now decided in exactly two places on purpose: `RefreshService`'s explicit state machine, and `RevokeAllActiveForUserAsync`'s hand-expanded, SQL-translatable predicate (`RevokedAtUtc == null && ExpiresAtUtc > nowUtc`).
- **`IIdentityService.GetActiveUserAsync(userId)` returns email + roles + the active-check in one lookup**, rather than a separate email-lookup plus the existing `GetRolesAsync`. Deliberately *not* the two-call lookup-then-check pattern Milestone 2 removed from credential validation — the caller already holds a `userId` proven valid by refresh-token possession, not user input, so there's no enumeration surface; a single lookup also avoids a deactivation landing between two round trips.
- **`/api/auth/refresh` returns the existing `LoginResponse`, not a new `RefreshResponse`.** The wire shape (`accessToken`, `expiresAtUtc`) is byte-identical, and `coding-standards.md`'s "introduce a contract only where the wire shape diverges" rule argues directly against a duplicate type. The generated OpenAPI schema for `/refresh` is consequently named `LoginResponse`, which is intentional and commented in `AuthController`, not an oversight.
- **Reuse detection is logged via `ILogger<RefreshService>`**, added at the point the security event is actually detected (Application layer), not deferred to Infrastructure logging that already existed transitively. This required adding `Microsoft.Extensions.Logging.Abstractions` to `Application.csproj` — Application's first new package dependency since Milestone 2's `DependencyInjection.Abstractions`, same first-party-abstractions category.
- **Cookie assembly extracted into `AuthController.SetRefreshTokenCookie`/`DeleteRefreshTokenCookie` helpers**, reused by both `Login` and `Refresh` so `Path`/`Secure`/`SameSite` are defined once. `DeleteRefreshTokenCookie` explicitly passes the same `CookieOptions.Path` used when setting the cookie — `Response.Cookies.Delete` silently no-ops otherwise, since a browser only honors a clearing `Set-Cookie` when its `Path` matches the original exactly.
- **No new EF migration.** Verified by reading `20260805155825_InitialCreate.cs` before writing any code: all eight `RefreshTokens` columns and both indexes (`IX_RefreshTokens_TokenHash` unique, `IX_RefreshTokens_UserId`) were already present from Milestone 1.
- **Parallel-refresh race deliberately left unmitigated this milestone** (documented as a known limitation, not fixed): two concurrent `/refresh` calls with the same cookie can both rotate successfully, since each generates a different new token hash and the unique index doesn't catch the collision. A later retry with the stale cookie then trips reuse detection and logs the legitimate user out everywhere. The real fix is a frontend single-flight refresh; a server-side fix (`RowVersion` column, or a compare-and-swap `RotateAsync`) was scoped out as disproportionate to this milestone. Recorded in `Architecture.md`, "Known limitations".

### Files/modules introduced

Application: `Authentication/Refresh/{IRefreshService,RefreshResult,RefreshService}.cs`. Modified: `Authentication/Interfaces/ITokenService.cs` (+ `HashRefreshToken`), `Authentication/Interfaces/IRefreshTokenStore.cs` (+ `FindByHashAsync`, `RotateAsync`, `RevokeAllActiveForUserAsync`, `StoredRefreshToken`), `Authentication/Interfaces/IIdentityService.cs` (+ `GetActiveUserAsync`, `ActiveUser`), `DependencyInjection.cs`, `NimbusCommerce.Application.csproj`.

Infrastructure: Modified: `Identity/TokenService.cs` (+ `HashRefreshToken`; `GenerateRefreshToken` now delegates to it), `Identity/RefreshTokenStore.cs` (+ the three new methods), `Identity/IdentityService.cs` (+ `GetActiveUserAsync`), `Identity/RefreshToken.cs` (removed the unmapped `IsActive` property).

Api: Modified: `Controllers/AuthController.cs` (`Refresh` action, `SetRefreshTokenCookie`/`DeleteRefreshTokenCookie` helpers, `IRefreshService` injected).

Packages added: `Microsoft.Extensions.Logging.Abstractions` 10.0.10 (Application).

### Lessons learned

- Reading the migration file *before* proposing any schema change avoided an unnecessary `dotnet ef migrations add` — the columns this feature needed had quietly existed since Milestone 1, just unused.
- `RefreshToken.IsActive` was a solved problem waiting to become a bug: it compiled cleanly everywhere because nothing had ever tried to query on it. Grepping for zero references before deleting it, rather than assuming a computed property is harmless, is worth doing on any entity before adding the first query against it.

### Outstanding work

- Logout, `/me` endpoints.
- Rate limiting on `/login`, `/register`, `/refresh`.
- Parallel-refresh race (frontend single-flight, or a server-side `RowVersion`/compare-and-swap) — see `Architecture.md`, "Known limitations".
- Per-device (chain-scoped) reuse-detection blast radius — would need a `TokenFamilyId` column; deferred.
- Email verification, password reset, MFA.
- Full timing-attack normalization for `ValidateCredentialsAsync` (unchanged from Milestone 2).
- No test project exists yet; rotation and reuse detection are exactly the kind of ordering-sensitive logic that argues for standing one up next.

### Next milestone

Candidates, not yet prioritized: a test project (unit tests for `RefreshService`'s state machine would have caught ordering regressions cheaply), Logout, or rate limiting on the three auth endpoints now live.
