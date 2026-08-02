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

- **No authentication endpoints exist yet.** Register, login, refresh, logout, and `/me` are not implemented. `IIdentityService` and the underlying Identity/JWT configuration exist so those endpoints have something to build on, but nothing currently issues a token.
- **Token issuance is deliberately not built yet.** `IIdentityService` covers user/credential/role operations only. Signing an access token and generating a refresh token is authentication business logic and belongs with the login/register milestone, likely as a small dedicated service (e.g. `ITokenService`) rather than folded into `IIdentityService`.
- **The refresh token client-storage decision is made but not implemented.** Refresh tokens are intended to be delivered via an HttpOnly, Secure, SameSite cookie, not returned in a JSON body for client-side storage. This has direct implications (CSRF protection needed on cookie-bearing endpoints) for whoever implements the login/refresh endpoints.
- **`RefreshToken`'s rotation fields (`RevokedAtUtc`, `ReplacedByTokenHash`) are schema only.** No code currently sets or reads them. Don't assume rotation is "already handled" because the columns exist.
- **The JWT signing key in `appsettings.Development.json` is a placeholder.** It must never be copied into a non-development environment.
- **`ApplicationUser` carries `FirstName`, `LastName`, `IsActive` directly** rather than in a separate profile entity — a deliberate, scoped decision for this project's size. See `Architecture.md` for the reasoning and the caveat about referencing users from Domain.
- **No migrations exist.** The EF Core model is defined in code only; it has not been applied to any database.
