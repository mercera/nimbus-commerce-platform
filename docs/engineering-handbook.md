# Engineering Handbook

## Development workflow

Work proceeds in small, explicitly scoped milestones. For each milestone:

1. Architecture decisions are settled and reviewed before any code is written.
2. Before implementation, the exact set of files to be created/modified is listed with a reason for each, and implementation waits for explicit approval.
3. Implementation follows the approved plan. Any necessary deviation (a build-time necessity, a bug in the existing setup, etc.) is called out explicitly rather than made silently.
4. The solution is built and any errors are resolved before the milestone is considered done.
5. A self-review is performed against Clean Architecture, SOLID, security, naming, duplication, maintainability, and production-readiness before moving on.
6. Documentation is updated to reflect the current state — not planned or future state — and a journal entry is added.

## Notes for future developers (and future Claude Code sessions) on authentication

- **Register and Login are implemented; refresh, logout, and `/me` are not.** `AuthController` exposes `POST /api/auth/register` and `POST /api/auth/login` only. `ITokenService`/`IRefreshTokenStore` exist and are exercised by Login, but nothing yet reads a refresh token back in — there is no rotation, reuse-detection, or revocation logic anywhere.
- **`IIdentityService.ValidateCredentialsAsync` is the only credential-check entry point.** `CheckPasswordAsync`/`GetUserIdAsync` (Milestone 1's original two-call shape) were removed and replaced with this single method specifically to stop Login from being able to distinguish "no such account" from "wrong password" through its own control flow. A residual timing difference still exists one layer down, inside `IdentityService` itself (it short-circuits before hashing on an unknown/inactive account) — see `Architecture.md`'s "Known limitations". Don't reintroduce a two-call lookup-then-check pattern for credential validation without re-reading that section.
- **`ApplicationUser.IsActive` is now enforced.** `ValidateCredentialsAsync` rejects inactive accounts. There is currently no code path that ever sets `IsActive` to `false` (no deactivation feature exists yet) — the field is enforced but not yet actionable from any endpoint.
- **The refresh token client-storage decision is implemented.** Login sets the refresh token as an `HttpOnly`, `Secure`, `SameSite=Strict` cookie, path-scoped to `/api/auth/refresh`, not returned in the JSON body. `SameSite=Strict` was chosen as the safest default for a same-site deployment — revisit before ever hosting the frontend on a different site than the API.
- **`RefreshToken`'s rotation fields (`RevokedAtUtc`, `ReplacedByTokenHash`) are still schema only.** Login only inserts new rows. No code currently sets or reads these two fields — don't assume rotation is "already handled" because the columns exist.
- **`internal sealed` for interface implementations now applies inside Application too, not just Infrastructure.** `RegisterService`/`LoginService` implement their own same-layer interfaces (`IRegisterService`/`ILoginService`) and are `internal sealed`, registered by a new `Application/DependencyInjection.AddApplication`. See `coding-standards.md`.
- **The JWT signing key in `appsettings.Development.json` is a placeholder.** It must never be copied into a non-development environment.
- **`ApplicationUser` carries `FirstName`, `LastName`, `IsActive` directly** rather than in a separate profile entity — a deliberate, scoped decision for this project's size. See `Architecture.md` for the reasoning and the caveat about referencing users from Domain.
- **No migrations exist.** The EF Core model is defined in code only; it has not been applied to any database. This means Register and Login, despite being implemented, cannot be exercised end-to-end against a real database yet — a migration is required before manual/integration testing is possible.
