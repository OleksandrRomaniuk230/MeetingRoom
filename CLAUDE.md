# MeetingRoom

ASP.NET Core **.NET 10** Web API for meeting-room booking. JWT bearer authentication with
role-based authorization (`admin` / `user`).

Single project: `backend/MeetingRoom.Api.csproj` (root namespace `MeetingRoom.Api`).
There is no solution file — always pass the `.csproj` path explicitly, or `cd backend` first.

## Prerequisites

- .NET SDK **10.0.400** (pinned by nothing — there is no `global.json`; add one if drift becomes a problem)
- Verify with `dotnet --list-sdks`

## Commands

Run these from the repository root. All paths are relative to it.

```bash
# Restore
dotnet restore backend/MeetingRoom.Api.csproj

# Build (add --no-restore once packages are restored)
dotnet build backend/MeetingRoom.Api.csproj

# Quiet build - only warnings and errors
dotnet build backend/MeetingRoom.Api.csproj --no-restore -v q --nologo

# Run (Development, http://localhost:5080)
dotnet run --project backend/MeetingRoom.Api.csproj --launch-profile http

# Run over HTTPS (https://localhost:7080 + http://localhost:5080)
dotnet run --project backend/MeetingRoom.Api.csproj --launch-profile https

# Formatting - verify in CI, fix locally
dotnet format backend/MeetingRoom.Api.csproj --verify-no-changes
dotnet format backend/MeetingRoom.Api.csproj
```

Both launch profiles set `ASPNETCORE_ENVIRONMENT=Development` and do not open a browser.
The OpenAPI document is served in Development only, at `/openapi/v1.json`.

### Tests

**There is no test project yet.** `dotnet test` from the root fails with `MSB1003`, and there
is no solution file for it to discover. To add one:

```bash
dotnet new xunit -o tests/MeetingRoom.Api.Tests
dotnet add tests/MeetingRoom.Api.Tests reference backend/MeetingRoom.Api.csproj
dotnet new sln -n MeetingRoom
dotnet sln add backend/MeetingRoom.Api.csproj tests/MeetingRoom.Api.Tests
dotnet test
```

Prefer `Microsoft.AspNetCore.Mvc.Testing` (`WebApplicationFactory`) integration tests over
unit-testing controllers directly — the authorization behaviour that matters most
(fallback policy, `[AllowAnonymous]`, role checks) lives in middleware and attributes, and
is invisible to a test that news up a controller.

### Troubleshooting

- **`MSB3027`/`MSB3021`: "file is locked by MeetingRoom.Api"** — a previous run is still
  alive; killing `dotnet run` does not always kill the child process.
  `Get-Process MeetingRoom.Api | Stop-Process -Force`, then rebuild.
- **Startup throws `InvalidOperationException` about `Jwt:Key`** — the signing key is
  missing or shorter than 32 bytes. See Configuration below.

## Configuration

**First-run setup.** `appsettings.Development.json` is **gitignored** — it holds the signing
key and the seed admin password, which must not enter git history. A fresh clone will not
start until you provide them:

```bash
cp backend/appsettings.Development.template.json backend/appsettings.Development.json
# then fill in Jwt:Key (32+ bytes) and, optionally, the AdminSeed block
```

| Section | Key | Where it lives |
|---|---|---|
| `Jwt` | `Issuer`, `Audience`, `ExpiryMinutes` | `appsettings.json` (committed) |
| `Jwt` | `Key` | `appsettings.Development.json` (gitignored) — **user secrets or env var elsewhere** |
| `AdminSeed` | `Username`, `Email`, `Password` | `appsettings.Development.json` (gitignored) |

`Jwt:Key` must be **at least 32 bytes** — HS256 requires a 256-bit key, and `Program.cs`
fails fast at startup rather than issuing weakly-signed tokens. Outside Development, supply
secrets via user secrets or environment variables (`Jwt__Key`, `AdminSeed__Password`):

```bash
dotnet user-secrets set "Jwt:Key" "<32+ byte value>" --project backend/MeetingRoom.Api.csproj
```

`AdminSeed` is optional and no-ops when unset. It exists because registration always
creates a regular user, so without a seeded admin no account could ever hold `Admin`.
**Never commit a real admin password.**

## Layout

Folder structure mirrors namespaces (`backend/Services/` → `MeetingRoom.Api.Services`):

```
backend/
  Program.cs          # composition root: config validation, DI, middleware pipeline
  Authorization/      # policy names + typed [AuthorizeAdmin] attribute
  Configuration/      # strongly-typed options classes
  Controllers/        # thin HTTP surface, no business logic
  Models/             # entities, enums, request/response DTOs, claim constants
  Services/           # interfaces + implementations (repository, token issuance, seeding)
```

Controllers stay thin: validate, delegate to a service, map the result to a status code.
Business rules belong in `Services/`.

## Code style

Enforced by the compiler where possible, by convention otherwise. `dotnet format` handles
whitespace; the rest is on review.

- **`Nullable` and `ImplicitUsings` are enabled.** Do not disable either, and do not
  silence a nullable warning with `!` where a real check belongs.
- **File-scoped namespaces** — `namespace MeetingRoom.Api.Models;`, no braces.
- **`sealed` by default** on classes. Unseal only when something actually derives from it.
- **4-space indent, Allman braces**, braces even on single-statement `if`.
- **`required` members** instead of constructor boilerplate on entities and DTOs; `init`
  accessors for values fixed at construction.
- **`record` for DTOs and value-like types**, `class` for entities with mutable state.
- **No magic strings.** Claim names, role values and policy names live in `ClaimNames`,
  `RoleNames` and `AuthPolicies`. Reference the constant, never retype the literal.
- **Async**: `Async` suffix, `Task`-returning, `CancellationToken cancellationToken = default`
  as the last parameter — and actually pass it down.
- **Constructor injection** into `private readonly` fields; no service locator, no
  `IServiceProvider` in controllers.
- **Interfaces for anything with a swappable implementation** (`IUserRepository`,
  `ITokenService`), so infrastructure can be replaced without touching callers.
- **Modern C# where it reads better**: collection expressions (`[SecurityAlgorithms.HmacSha256]`),
  switch expressions, target-typed `new`, `Lock` over `object` for mutexes.
- **Options classes** expose a `public const string SectionName` and are validated at
  startup, not first use.
- **Comments explain *why*, never *what*.** Load-bearing decisions get a comment; a comment
  restating the code gets deleted. Public types and members carry XML docs; `<remarks>` is
  the right place for a security rationale.

## Security conventions

These are load-bearing. Read before touching `Program.cs` or adding an endpoint.

- **Authorization is deny-by-default.** A fallback policy requires an authenticated user, so
  an endpoint with *no* attribute is still protected. Never weaken this to make something
  work — add `[AllowAnonymous]` to that one endpoint and say why in a comment.
- **Middleware order is fixed**: `UseAuthentication()` → `UseAuthorization()` →
  `MapControllers()`. Reversing the first two silently 401s every protected endpoint,
  because `HttpContext.User` is not populated when policies are evaluated.
- **Admin-only features use `[AuthorizeAdmin]`** (wraps `AuthPolicies.AdminOnly`), not a
  hand-written `[Authorize(Roles = "admin")]` string. Apply it to room management,
  user administration, and any other privileged surface. Reads may be open to all
  authenticated users; mutations should be admin-only.
- **Roles are never accepted from client input.** Registration hardcodes
  `UserRole.RegularUser`. Any endpoint that changes a role must itself be admin-only.
- **Passwords**: hash with the injected `IPasswordHasher<User>`
  (`Microsoft.Extensions.Identity.Core`, already in the shared framework — no package
  reference needed). Never store, log, or return a plaintext password.
- **Authentication failures return one uniform 401** for both unknown username and wrong
  password, and login does equal work in both cases. Do not add a "no such user" message —
  it turns the endpoint into a username oracle.
- **Errors use `ProblemDetails`**; annotate actions with `[ProducesResponseType]` for every
  status code they return.
- Prefer adding a new policy in `AuthPolicies` over an inline requirement, so authorization
  rules stay enumerable in one file.

## Known gaps

Deliberate, not oversights — worth knowing before building on top:

- `InMemoryUserRepository` is **process-local and non-persistent**. Accounts vanish on
  restart and are not shared across instances. Replace with an EF Core implementation of
  `IUserRepository` before running more than one node.
- **No refresh tokens, account lockout, or rate limiting.** Login is a bare credential check.
- **No test project** (see above).
- `Microsoft.OpenApi` 2.0.0 arrives transitively via `Microsoft.AspNetCore.OpenApi` 10.0.9
  and trips **`NU1903`** (known high-severity advisory `GHSA-v5pm-xwqc-g5wc`). This is the
  one expected build warning; a clean build shows exactly this and nothing else.
