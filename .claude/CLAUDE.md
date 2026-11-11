# Passwordless.dev Server

Backend for Bitwarden Passwordless.dev, a toolkit that lets developers add FIDO2 WebAuthn passkey sign-in to their apps. This repo holds the public API that runs FIDO2 ceremonies and stores credentials, the Admin Console where customers create applications and manage API keys, their databases, and a self-hosting Docker image. It is security-sensitive: it holds registered passkey credentials and customers' API secrets.

## Authoritative docs

Read these instead of relying on a summary here:

- `README.md`: product overview and how customers integrate.
- `CONTRIBUTING.md`: running the apps locally, databases, EF migrations, tests.
- `src/Api/readme.md`, `src/AdminConsole/readme.md`: per-app setup and configuration keys.
- `self-host/README.md`: building and running the self-hosted image.
- `SECURITY.md`: vulnerability disclosure (HackerOne).
- `.github/pull_request_template.md`: PR structure, including the testing and security checklist.
- External: [docs.passwordless.dev](https://docs.passwordless.dev).

The docs say .NET 7, but `global.json` pins SDK 9.0.101 and `Directory.Build.props` targets `net9.0`. Trust the build files.

## Layout

- `PasswordlessServer.sln`: the solution.
- `src/Api`: public API (FIDO2 register/sign-in, tokens, key validation). Depends on `src/Service`.
- `src/Service`: API domain logic (`Fido2Service.cs`, `TokenService.cs`, `ApplicationService.cs`), FIDO metadata (`MDS/`), EF storage (`Storage/Ef/`) and API migrations (`Migrations/{Mssql,Sqlite}`).
- `src/AdminConsole`: customer GUI with its own database (`Db/`, `Migrations/{Mssql,Sqlite}`) and Tailwind CSS (`package.json`).
- `src/Common`: code shared by both apps, including `Utils/ApiKeyUtils.cs`.
- `tests/`: `Api.Tests`, `Service.Tests`, `Common.Tests`, `AdminConsole.Tests` (unit) and `Api.IntegrationTests`.
- `self-host/`: Dockerfile, nginx, supervisord and `settings.env` for the self-hosted image.
- `.github/workflows/`: CI, Docker, migrations and deploy workflows.

## Commands

Run from the repo root unless noted.

```bash
dotnet restore
dotnet build --configuration Release
dotnet test                              # all tests; see prerequisites below
dotnet format --verify-no-changes        # CI gate; run `dotnet format` to fix

# Local development (CONTRIBUTING.md): run both apps
dotnet watch --project src/Api           # http://localhost:7001, https://localhost:7002
dotnet watch --project src/AdminConsole  # http://localhost:8001, https://localhost:8002

# AdminConsole CSS (from src/AdminConsole; the .csproj also runs npm ci + npm run build on build)
npm run build
npm run tw:watch

# Self-hosted image (self-host/README.md)
docker build . -f self-host/Dockerfile -t bitwarden/passwordless
```

Test prerequisites (CONTRIBUTING.md): a container runtime such as Docker and Chrome. `Api.IntegrationTests` uses Testcontainers (MSSQL) and Selenium.

## Rules that are easy to break

- **Formatting gates CI.** `.github/workflows/_test.yml` runs `dotnet format --verify-no-changes` and fails on any diff. Format before committing.
- **Every schema change needs both providers.** Both apps have separate SQLite and MSSQL DbContexts and migration folders. Generate migrations for both, following the steps in `CONTRIBUTING.md` (API contexts `DbGlobalSqliteContext`/`DbGlobalMsSqlContext`, AdminConsole contexts `SqliteConsoleDbContext`/`MssqlConsoleDbContext`; `src/AdminConsole/migrate.sh` does both for the AdminConsole). `.github/workflows/migrations.yml` generates idempotent MSSQL scripts from them for deployment.
- **Don't hand-edit the version.** `<Version>` in `Directory.Build.props` is bumped by an external release workflow; its comment says not to edit it by hand.
- **All changes need tests** that prove the intended behavior (`README.md`). Keep PRs small; large ones are less likely to be merged.
- **WebAuthn needs a secure context.** It works on `localhost` or over HTTPS only (`CONTRIBUTING.md`).

## Security-sensitive areas

What the code and docs show, so you know where to look before changing anything here:

- **FIDO2 ceremonies**: `src/Service/Fido2Service.cs`, using the `Fido2` / `Fido2.AspNet` packages. Authenticator metadata comes from the FIDO Metadata Service in `src/Service/MDS`, with `src/Api/mds.jwt` shipped alongside the API.
- **API keys and secrets**: `src/Common/Utils/ApiKeyUtils.cs` generates keys (`<app>:public:...`, `<app>:secret:...`). Secrets are persisted only as a salted PBKDF2-HMACSHA512 hash (`SharedManagementService.cs` stores `hashedApiKey`) and checked with `ApiKeyUtils.Validate`.
- **API authentication**: `src/Api/Authorization`. Endpoint groups opt into a key type with `RequirePublicKey`, `RequireSecretKey` or `RequireManagementKey` (`EndpointConventionBuilderExtensions.cs`). The management key is compared with `CryptographicOperations.FixedTimeEquals`.
- **Tokens**: `src/Service/TokenService.cs` signs register/verify tokens with keys derived from the `SALT_TOKEN` setting. `TokenService` throws if it is missing.
- **Tenant isolation**: `src/Service/Storage/Ef/DbTenantContext.cs` applies a per-tenant query filter to tenant-owned entities.
- **Configuration secrets**: the readmes and `self-host/settings.env` show the config keys (`SALT_TOKEN`, `Passwordless:ApiSecret`, mail provider keys, connection strings) with placeholder values. Never put real values in the repo, in tests, or in examples.

## When to stop and ask

Stop and confirm with the user before:

- Changing how keys, secrets or tokens are generated, hashed, compared or validated (the files above).
- Changing FIDO2 verification behavior, attestation or MDS handling.
- Adding an API endpoint, or changing which key type or scope an endpoint requires.
- Changing tenant filters or adding a tenant-owned entity.
- Adding or changing a migration, or altering an existing one.
- Editing `.github/workflows/` or Docker files. CODEOWNERS shares workflows with `@bitwarden/dept-bre` and Docker files with `@bitwarden/team-appsec` and `@bitwarden/dept-bre`.

## Contributing

- Branch from and open PRs against `main`.
- Commit subjects in recent history start with a Jira key in brackets, for example `[PM-36014] Enhanced Invite Validation`; Renovate uses `[deps]: ...`.
- The PR template asks for a `PAS-XXX | short description` title, a ticket link, and what you did to test the change and to avoid introducing security vulnerabilities.
- Security reports go through HackerOne (`SECURITY.md`), not public issues.
