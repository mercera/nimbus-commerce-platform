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

## 2026-08-09 — Sprint 2, Milestone 4: Logout Endpoint

### Completed

Implemented `POST /api/auth/logout`: revokes only the single session identified by the current `refreshToken` cookie, always deletes the cookie, and always returns `204 No Content`. Preceded by an architecture review session (no code changes) that surfaced two findings acted on below. Logout-all-devices, `/me`, rate limiting, and a test project remain out of scope.

### Major architectural decisions

- **Refresh-token cookie path widened from `/api/auth/refresh` to `/api/auth`.** The review found this was not optional: per RFC 6265 §5.1.4, `/api/auth/refresh` does not prefix-match `/api/auth/logout` (they diverge at `r` vs `l`), so under the original scope Logout would never receive the cookie at all — it would sit permanently in the "missing cookie" branch, returning `204` and revoking nothing, with every idempotency case passing for the wrong reason and no error anywhere. This reverses the specific path chosen in Milestone 2 (`project-journal.md`, 2026-08-04); the reversal is deliberate and driven by a requirement Milestone 2 didn't yet have (a second endpoint reading the same cookie), not a correction of that decision.
- **Old cookies at the legacy path are not migrated.** Widening the constant does not move a cookie a browser already stored at `/api/auth/refresh`; such a client would present two `refreshToken` cookies to `/refresh`, with the more path-specific (stale) one likely read first per RFC 6265 §5.4. Accepted without a compatibility shim because there is no deployed environment and no real sessions to break — verification instead starts by clearing local cookies once. Recorded in `Architecture.md` as a decision with a stated fallback (a second `Set-Cookie` clearing the legacy path in `DeleteRefreshTokenCookie`) if this is ever revisited against live sessions.
- **New `IRefreshTokenStore.RevokeActiveByHashAsync(tokenHash)`, not a bare `RevokeByHashAsync`.** Reuses the existing store abstraction rather than introducing a second persistence mechanism, per the review. Scoped to currently-active rows only — same hand-expanded `RevokedAtUtc == null && ExpiresAtUtc > nowUtc` predicate as `RevokeAllActiveForUserAsync` — because `RevokedAtUtc` is not a status flag: it also records *when* a token was rotated away or bulk-revoked by reuse detection, and logout unconditionally overwriting it would destroy that forensic ordering for a token that's already unusable either way. Implemented as a single `ExecuteUpdateAsync`, returning whether a row was affected.
- **New `ILogoutService`/`LogoutService`** in `Application/Authentication/Logout/`, following the feature-folder pattern the Milestone 2 entry already named Logout under. Deliberately has no `LogoutResult` and no `LogoutRequest` — a single `Task LogoutAsync(string refreshToken)` — because logout has exactly one outcome, and a result record with a permanently-`true` `Succeeded` would exist only to be branched on, which `coding-standards.md`'s cross-boundary-result guidance doesn't call for.
- **No `[Authorize]` on `/logout`.** The refresh-token cookie identifies the session being terminated; requiring a valid access token would block exactly the case that matters most — a user whose access token already expired trying to log out. A comment on `AuthController.Logout` records the future trap: if `[Authorize]` is ever added, the token's owning user must be checked against the authenticated `sub` claim, or one authenticated user could revoke another's session by presenting that user's cookie.
- **Always `204`, unconditionally, regardless of what was found.** This is a security property, not convenience: distinguishing "found and revoked" from "not found" via status code would turn an unauthenticated `/logout` into a refresh-token validity oracle, reopening exactly the probe `RefreshResult.Failure()`'s detail-free shape (Milestone 3) was built to close.
- **Logout does not call `RevokeAllActiveForUserAsync`.** Presenting an unknown or already-revoked token to `/logout` is not treated as reuse/theft evidence the way it is in `RefreshService` — cascading here would silently turn ordinary single-session logout into logout-all-devices whenever a benign race occurred (duplicate click, background refresh timer overlapping a logout). Logout-all-devices stays a distinct, unbuilt feature.
- **A latent concurrency issue was found, not fixed.** `RefreshTokenStore.RotateAsync` is not a compare-and-swap: it loads the current token by `TokenHash` alone and unconditionally overwrites `RevokedAtUtc`/`ReplacedByTokenHash` before inserting the replacement. A `/refresh` that has already read a token as active can be overtaken by a concurrent `/logout`, which atomically revokes it; `RotateAsync` then loads the (now-revoked) row anyway and commits a new active child token, silently undoing the logout and leaving the row indistinguishable from an ordinary rotation. Judged out of scope for this milestone: the obvious cheap fix (adding a `RevokedAtUtc == null` predicate to `RotateAsync`'s read) only narrows the window under READ COMMITTED, and would introduce a real bug on its own — `RotateAsync` currently returns `void` and silently no-ops when its row isn't found, so a newly-conditional read would need `RotateAsync` to report failure and `RefreshService` to add a new branch, or a rotation that silently didn't happen would still get a minted access token, exactly what Milestone 3's persist-before-issue ordering guarantees against. Filed as an extension of the parallel-refresh limitation already on record from Milestone 3 (same root cause, second trigger), not a new one. Residual risk judged low: the orphaned child token is minted after the client's cookie is already deleted, so no client ever holds it.

### Files/modules introduced

Application: `Authentication/Logout/{ILogoutService,LogoutService}.cs`. Modified: `Authentication/Interfaces/IRefreshTokenStore.cs` (+ `RevokeActiveByHashAsync`), `DependencyInjection.cs`.

Infrastructure: Modified: `Identity/RefreshTokenStore.cs` (+ `RevokeActiveByHashAsync`).

Api: Modified: `Controllers/AuthController.cs` (`Logout` action, cookie path constant widened, `ILogoutService` injected).

No packages added. No migration — `20260805155825_InitialCreate.cs` already has every `RefreshTokens` column and index this milestone needed; only a cookie `Path` string changed, which is not schema.

### Lessons learned

- The cookie-path bug would not have been caught by any of the idempotency tests this milestone specifically calls for — a cookie that never arrives produces the same `204` as a cookie that arrives and is correctly handled. It only surfaced by reasoning through RFC 6265 path-matching before writing code, not by testing the endpoint's stated behavior. Worth remembering for any future endpoint that reads a path-scoped cookie set by a different route.
- Tracing the `/logout`-vs-`/refresh` race required reading `RotateAsync`'s actual implementation line by line rather than trusting its docstring ("atomically revokes ... and inserts the replacement") — the docstring is accurate about the insert+revoke pair committing together, but says nothing about what the *read* that feeds them is guarded against, which is where the gap actually is.

### Outstanding work

- `/me` endpoint.
- Rate limiting on `/login`, `/register`, `/refresh`, `/logout` — now four unthrottled endpoints.
- Parallel-refresh / logout-refresh race (`RotateAsync` non-atomicity) — unchanged from Milestone 3's assessment, now with a second trigger; needs a `RowVersion`/compare-and-swap redesign to close properly.
- Successor-token survival: a rotated-away token presented to `/logout` no-ops while its replacement stays active for up to 7 days; not chased because `ReplacedByTokenHash` chains can already be orphaned by the race above.
- Cookie path migration shim for the `/api/auth/refresh` → `/api/auth` change — not needed today (no live sessions), fallback documented in `Architecture.md` if ever needed.
- Revoked/expired `RefreshTokens` rows accumulate with no cleanup job — pre-existing, not introduced here.
- No test project exists yet; `LogoutService` (four cases, no time dependence) is the cheapest possible first target.
- Email verification, password reset, MFA (unchanged).
- Full timing-attack normalization for `ValidateCredentialsAsync` (unchanged from Milestone 2).

### Next milestone

Candidates, not yet prioritized: a test project (starting with `LogoutService` and `RefreshService`), rate limiting across all four live auth endpoints, or `/me`.

## 2026-08-11 — Sprint 2, Milestone 5: Authenticated User (`GET /api/auth/me`)

### Completed

Implemented `GET /api/auth/me`: the first endpoint in the solution to carry `[Authorize]`. It resolves the caller's `sub` claim to a live database lookup and returns id, email, first name, last name, and roles, rejecting users who have been deleted or deactivated since their token was issued. Deliberately the smallest milestone so far — it builds no new security machinery, it puts the JWT pipeline that has existed since Milestone 1 to work for the first time. Role-based authorization, an `ICurrentUser` abstraction, and token revocation all remain out of scope.

Verified end-to-end against a running API and a real database, not just by building: no header → `401 WWW-Authenticate: Bearer`; malformed token → `401 invalid_token`; token signed with a different key → `401 "The signature key was not found"`; correctly-signed but expired token → `401 "The token expired at …"` (exact, since `ClockSkew` is zero); valid token → `200` with `roles: []`. **The acceptance test — changing `FirstName` directly in the database and re-calling `/me` with the unchanged token — returned the new value**, which is the only check that distinguishes this implementation from projecting the JWT's own claims. Deactivating the user then produced `401` from `/me`, `/refresh`, and `/login` alike. Full `Login → /me → Refresh → /me → Logout → Refresh` regression behaved as in Milestone 4 (final refresh `401`), and the generated OpenAPI document marks `/api/auth/me` as the only secured operation.

### Major architectural decisions

- **`/me` is database-backed, not claim-projected.** Returning the JWT's own `email`/`role` claims would make the endpoint a server-side token decoder — something the client can already do locally — and would report the account as it stood up to 15 minutes earlier. The database read is the entire point: `/me` reports *current* account state. This is what the milestone's acceptance test checks (edit `FirstName` in the database, re-call `/me` with the unchanged token, see the new value).
- **`sub` does not arrive as `sub` — a trap found before writing the endpoint, not after.** `TokenService` writes `JwtRegisteredClaimNames.Sub`, but `JwtBearerOptions.MapInboundClaims` defaults to `true` and `JwtAuthenticationExtensions` never overrode it, so ASP.NET's inbound map rewrites the claim to `ClaimTypes.NameIdentifier` before the action sees it. `User.FindFirstValue(JwtRegisteredClaimNames.Sub)` returns null, which would have 401'd every single request with a valid token. Verified empirically with a throwaway probe that mints a token exactly the way `TokenService` does and validates it exactly the way `AddJwtBearer` does, rather than trusting the reasoning: the probe also confirmed `email` is remapped to `ClaimTypes.Email`, and that roles round-trip to `ClaimTypes.Role` so `IsInRole`/`[Authorize(Roles=…)]` remain correct. Decided to leave the JWT configuration untouched and read `ClaimTypes.NameIdentifier`, since flipping `MapInboundClaims` would mean simultaneously setting `NameClaimType`/`RoleClaimType` and rewriting `TokenService`'s role claim — touching working token minting for a cosmetic gain in a milestone whose purpose was to *use* existing infrastructure. Recorded in `engineering-handbook.md`.
- **New `IIdentityService.GetUserProfileAsync` + `UserProfile` record, rather than reusing or widening `ActiveUser`.** `ActiveUser(UserId, Email, Roles)` carries exactly what the JWT already carries, so returning it would have made the database round trip invisible to clients and reduced the design decision above to a liveness check. Widening it with `FirstName`/`LastName` was rejected for the opposite reason: its docstring scopes it to "the claims needed to issue [an access token]", and Milestone 3 specifically defended its single-lookup shape, so adding profile fields would make every token issuance fetch and carry data it never uses. The sibling record costs the same — one `FindByIdAsync`, one `GetRolesAsync`, the same `IsActive` guard.
- **`GetCurrentUserAsync` returns `UserProfile?`, not a `Success`/`Failure` record.** A deliberate departure from `coding-standards.md`'s cross-boundary-result guidance, which targets outcomes worth branching on with detail — `LoginResult` needs a wrapper because it carries four fields and must not leak *why* it failed. `/me` has one failure mode and one caller, and `GetActiveUserAsync` already returns `ActiveUser?` for the structurally identical "look up an already-authenticated subject" job.
- **`ICurrentUserService`/`CurrentUserService` introduced despite being a single delegation.** The service holds no logic — it forwards to `IIdentityService`. Kept anyway because every other `AuthController` action resolves through a use-case service, and letting this one inject `IIdentityService` directly would make Api the first consumer of an identity *primitive* rather than a use case, inviting controllers to compose identity operations ad hoc. `LogoutService` set the precedent that a thin use case is still a use case.
- **`ICurrentUser` (ambient caller identity) deliberately **not** introduced.** It would have exactly one consumer today, and `/me`'s controller can read the claim in one line and pass the id inward — precisely how `RefreshService` already receives a `userId` it did not look up. `IHttpContextAccessor` is still registered nowhere. Revisit at the second authenticated use case; `CreateOrder` populating `Order.CustomerId` is the expected trigger. The naming collision is a real hazard and is recorded in the handbook: `ICurrentUserService` is a use case taking a `userId`, *not* an accessor answering "who is calling?".
- **Role-based authorization deferred entirely, because it is currently unverifiable.** `AddRoles<IdentityRole>()` is registered and the role tables exist, but nothing anywhere calls `AddToRoleAsync` and there is no seeding — every user's role list is empty. `[Authorize(Roles = "Admin")]` would reject *everyone*, and proving it works would first require building role seeding, which is its own milestone. Roles still flow through `/me`'s response as `[]`, so the contract is ready when seeding lands.
- **`[Authorize]` on the action, not the controller.** Four of five actions are intentionally anonymous, and Logout's anonymity is a documented security decision from Milestone 4, so controller-level `[Authorize]` would need four `[AllowAnonymous]` attributes to express the same thing. The tipping point is recorded in a comment on the action and in the handbook: once protected endpoints outnumber anonymous ones, flip it, because that default is fail-safe.
- **Deactivated user → `401`, not `403`.** `403` means "authenticated but not permitted for *this resource*"; a deactivated account is not a per-resource permission problem — the credential should no longer be honoured anywhere. It is also the consistent answer, since `RefreshService` already fails a deactivated user via `GetActiveUserAsync`, so the client's automatic refresh retry fails too and lands them at login rather than looping. Both the "no such user" and "deactivated" paths return one generic message; unlike Login there is no enumeration concern, because the caller already holds a validly-signed token for that subject.
- **No OpenAPI work was needed.** `BearerSecuritySchemeTransformer` (Milestone 2) already applies the bearer requirement to operations carrying `IAuthorizeData` without `IAllowAnonymous`, so `/me` gained its Scalar lock icon for free and the four anonymous endpoints stayed undecorated. Found by reading the transformer before planning any change to it.

### Files/modules introduced

Application: `Authentication/CurrentUser/{ICurrentUserService,CurrentUserService}.cs`. Modified: `Authentication/Interfaces/IIdentityService.cs` (+ `GetUserProfileAsync`, + `UserProfile` record), `DependencyInjection.cs`.

Infrastructure: Modified: `Identity/IdentityService.cs` (+ `GetUserProfileAsync`).

Api: `Contracts/Authentication/MeResponse.cs`. Modified: `Controllers/AuthController.cs` (`Me` action, `ICurrentUserService` injected).

No packages added. No migration — `FirstName`, `LastName`, and `IsActive` are already columns on `AspNetUsers` in `20260805155825_InitialCreate.cs`; this milestone changed no schema. `JwtAuthenticationExtensions.cs`, `Program.cs`, `TokenService.cs`, and `BearerSecuritySchemeTransformer.cs` were all deliberately left untouched.

### Lessons learned

- **The claim-mapping trap would have cost real debugging time and looked like a configuration bug.** Every symptom — valid token, correct signature, `[Authorize]` passing, then a 401 from our own code — points at token validation rather than at a silent claim rename happening after validation succeeds. Writing a 40-line probe that mints and validates a token outside the app confirmed the exact claim types in under a minute, and was far cheaper than reasoning about it or discovering it against a running API with a database attached. Worth repeating for any future assumption about what the framework hands the action.
- **Reading `BearerSecuritySchemeTransformer` before planning saved a fabricated work item.** "Make `/me` show as secured in Scalar" is the kind of task that gets assumed into a plan; the transformer written three milestones ago already handled it generically off endpoint metadata. The existing code answered the question faster than designing around it would have.
- **The most valuable verification step is the one that distinguishes this implementation from the obvious wrong one.** Steps like "valid token → 200" pass identically whether `/me` reads the database or decodes the JWT. Only editing `FirstName` in the database and re-calling with the *unchanged* token proves the design decision was actually implemented.

### Outstanding work

- Role seeding — now the blocker for any role-protected endpoint, and a prerequisite for testing `[Authorize(Roles = "...")]` at all.
- Rate limiting on `/login`, `/register`, `/refresh`, `/logout` (unchanged; `/me` is `[Authorize]`-gated and not exposed on the same terms).
- Deactivation has no endpoint that triggers it — `IsActive` is enforced by Login, Refresh, and now `/me`, but only settable directly in the database. An admin deactivation feature would need role seeding first.
- Per-endpoint `IsActive` checking does not scale: every future `[Authorize]` endpoint must repeat it or serve deactivated users for up to 15 minutes. Centralising it (claims transformation or an authorization requirement) becomes worthwhile as authenticated endpoints multiply — at the cost of a database read per request.
- Parallel-refresh / logout-refresh race (`RotateAsync` non-atomicity) — unchanged from Milestones 3 and 4.
- Successor-token survival; revoked/expired `RefreshTokens` rows accumulate with no cleanup job (both unchanged).
- Cookie path migration shim — still not needed (no live sessions).
- No test project exists yet. `CurrentUserService` is too thin to be a useful first target; `LogoutService` and `RefreshService` remain the right starting points.
- Email verification, password reset, MFA (unchanged).
- Full timing-attack normalization for `ValidateCredentialsAsync` (unchanged from Milestone 2).

### Next milestone

Candidates, not yet prioritized: a test project (starting with `LogoutService` and `RefreshService`), role seeding (which unblocks role-based authorization and admin features), or rate limiting across the four anonymous auth endpoints.
