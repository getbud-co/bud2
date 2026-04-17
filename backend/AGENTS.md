# Agent Note (non-normative)

Contract-Version: 2026-03-30

This file supplements `/AGENTS.md` with backend-specific rules.
`CLAUDE.md` and `GEMINI.md` in this directory should point to this file and must not diverge.

## Backend Scope

- Main solution: `backend/Bud.sln`
- Main runtime projects:
  - `backend/src/Api/Bud.Api`
  - `backend/src/Mcp/Bud.Mcp`
- Main test suites:
  - `backend/tests/Api/*`
  - `backend/tests/Mcp/*`

## Quick Reference (Top 5 Rules)

1. **pt-BR**: all user-facing messages must be in Brazilian Portuguese.
2. **Boundaries**: Controller -> Use Case -> Repository (via interfaces).
3. **TDD**: write/update tests before production code (`Red -> Green -> Refactor`).
4. **Tenant isolation**: tenant-scoped entities require `OrganizationId` + query filter + interceptor.
5. **No warnings**: `TreatWarningsAsErrors=true` (zero warning build/test).

## Rule Priority

1. Security, tenant isolation, and authorization.
2. User-facing language rules (`pt-BR`).
3. Architectural boundaries and design patterns.
4. Testing and validation requirements.
5. Style and organizational conventions.

## Agent Operating Contract (Normative)

### MUST

- Enforce tenant isolation, authentication, and authorization before business changes.
- Keep all user-facing text in `pt-BR` (errors, validation, API problem responses, UI text).
- Preserve architectural boundaries:
  - Controllers -> Use Cases.
  - Use Cases -> Repository interfaces (`Application/Features/<Feature>/`), feature ports (`Application/Features/<Feature>/`) and cross-cutting ports (`Application/Ports`).
  - Explicit namespaces MUST mirror `RootNamespace + folder path` (for example `Bud.Application.Features.Missions.UseCases`, `Bud.Domain.Missions.Events`, `Bud.Shared.Kernel.Enums`).
  - Domain MUST NOT reference `Bud.Infrastructure` (or sub-namespaces).
  - Domain MUST NOT depend on `Infrastructure/`.
  - Application MUST depend only on abstractions (repository interfaces in `Application/Features/<Feature>/`, feature-scoped ports in `Application/Features/<Feature>/`, and cross-cutting ports in `Application/Ports`).
  - Repositories MUST NOT return HTTP DTOs from `Bud.Shared.Contracts`.
  - Repositories MUST return domain entities/read models; mapping to `Bud.Shared.Contracts` happens in Application.
- Respect established patterns (specification, policy-based auth, composition root modules).
- Apply TDD workflow (`Red -> Green -> Refactor`) for features, fixes, and behavior changes.
- Update tests together with production code changes.
- Keep OpenAPI semantic documentation aligned with implementation.
- Update or create ADR when architectural behavior changes.
- Keep the solution warning-free (`TreatWarningsAsErrors=true`).
- For `Bud.Api` logging, use source-generated logging (`[LoggerMessage]`) local to each component (`partial` class); do not introduce centralized ad-hoc log catalogs.
- For observability, register via `AddBudObservability()` in `BudObservabilityCompositionExtensions` (structured logging + OpenTelemetry). OTel config is **config-as-environment**: all export/resource/service-name settings come from standard OTel env vars, never hardcoded in code or appsettings.
- Reserve EventId ranges per domain: 3100–3199 RequestTelemetryMiddleware; 4010–4019 Organization; 4040–4049 Employee/Auth; 4050–4059 Notification; 4090–4099 Session; 5000–5009 McpRequestLoggingMiddleware.
- For `Bud.Mcp`, keep tool schemas explicit (`required`, field types/formats) e limitadas às capacidades atuais do host.
- For `Bud.Mcp` HTTP transport, use `MCP-Session-Id` as the session header.
- For `Bud.Mcp` protocol compatibility, keep `prompts/list` implemented (empty list when no prompts are published).

### SHOULD

- Prefer composition/extensions over ad-hoc wiring in `Program.cs`.
- Prefer reusable specifications/policies over duplicated conditionals.
- Prefer changing existing patterns consistently instead of introducing parallel alternatives.

## Language Requirements (MUST)

All user-facing text MUST be in Brazilian Portuguese (`pt-BR`).

This includes:

- Error messages in controllers/repositories.
- FluentValidation messages.
- API `ProblemDetails` / `ValidationProblemDetails`.
- Frontend-visible text returned by backend APIs.
- Log messages that may be displayed to users.

Comments in code should remain in English for maintainability.

## Architecture Essentials

### System Overview

Bud backend is an ASP.NET Core 10 solution with PostgreSQL persistence and an MCP server:

- `src/Api/Bud.Api`: HTTP API and hosting entrypoint.
- `src/Api/Bud.Application`: use cases, mappings, read models, and ports.
- `src/Api/Bud.Domain`: pure domain, value objects, events, and repository-related abstractions.
- `src/Api/Bud.Infrastructure`: EF Core, repositories, concrete services, and migrations.
- `src/Mcp/Bud.Mcp`: MCP server over HTTP treated as a conversational client of `Bud.Api`.
- `src/Shared/Bud.Shared.Kernel`: stable shared types.
- `src/Shared/Bud.Shared.Contracts`: shared edge contracts.
- `tests/Api/*`: backend API unit tests.
- `tests/Mcp/*`: MCP tests.

The current base keeps a neutral notification inbox capability. It is infrastructure-oriented and must remain decoupled from product-specific event names or workflows.

### Layering and Dependencies (MUST)

- Controllers orchestrate validators + use cases.
- Use cases depend on abstractions only (repository interfaces in `Application/Features/<Feature>/`, feature-scoped ports in `Application/Features/<Feature>/`, and cross-cutting ports in `Application/Ports`).
- Repository interfaces live in `Bud.Application/Features/<Feature>/`; implementations in `Bud.Infrastructure/Features/<Feature>/`.
- Domain stays infrastructure-agnostic.
- Mapping to shared HTTP contracts happens in Application.

### Multi-Tenancy (MUST)

- Row-level isolation by `OrganizationId`.
- Tenant entities implement `ITenantEntity` with `Guid OrganizationId`.
- Tenant entities must have query filters and tenant-aware persistence via `TenantSaveChangesInterceptor`.
- `TenantRequiredMiddleware` enforces tenant/auth rules for API endpoints, preserving configured exclusions.
- Frontend integration rules:
  - Include `X-Tenant-Id` when a specific organization is selected.
  - Omit `X-Tenant-Id` for global-admin all-organizations context.
  - Tenant-scoped pages must react to organization changes.

### Authorization Pattern (MUST)

Authorization is **declarative** — expressed as `[Authorize(Policy = ...)]` attributes on controllers. Use cases are pure business logic and contain no authorization code.

**Available policies:**

| Policy | Where applied | Meaning |
|---|---|---|
| `TenantSelected` | Controller class level | Authenticated user with a tenant selected (JWT `X-Tenant-Id`) |
| `GlobalAdmin` | Specific endpoints (Organizations) | Global administrator only |
| `LeaderRequired` | Employee write methods | Líder do tenant atual ou administrador global |


**Tenant data isolation** is enforced by EF Core query filters (`OrganizationId == TenantId`) — no per-use-case ownership checks are needed for standard CRUD.

**Adding a new authorization rule:**
- If it is role-based (GlobalAdmin, future tenant role) → create a new `IAuthorizationRequirement` + `AuthorizationHandler<T>` in `Bud.Api/Authorization/`, register in `BudSecurityCompositionExtensions`, annotate the controller method.
- Never add `IApplicationAuthorizationGateway` or resource/context records — that pattern has been removed.

### Controller Pattern (MUST)

Controllers must:

1. Validate payloads first.
2. Call use case only after validation success.
3. Map `Result.ErrorType` consistently:
   - `NotFound` -> 404
   - `Validation` -> 400
   - `Conflict` -> 409
   - `Forbidden` -> 403
4. Return `ProblemDetails` / `ValidationProblemDetails` in `pt-BR`.

### Validation and OpenAPI (MUST)

- Use FluentValidation co-located with controllers in `src/Api/Bud.Api/Features/<Feature>/`, registered in DI.
- Validators must not access `ApplicationDbContext` directly.
- Keep OpenAPI available at `/swagger` and `/openapi/v1.json` in Development.
- Keep `ProducesResponseType`, `Consumes`, and `Produces` aligned with behavior.
- Document operation summary/description, success/error status codes, and key DTO fields.

### Data Access Essentials

- EF Core with Npgsql.
- DbContext: `ApplicationDbContext`.
- Every entity in `ApplicationDbContext` must have `IEntityTypeConfiguration<T>` in `Infrastructure/Persistence/Configurations/`.
- Tenant query filters are configured at EF entity configuration level.

### Database Setup

- **Development:** schema created via `EnsureCreated()`. On model changes, recreate DB (for example `docker compose down -v && docker compose up --build` from the repository root).
- **Production:** EF Core migrations are used (`dotnet-ef migrations bundle`) with migrations in `Infrastructure/Persistence/Migrations/`.

To add a migration from the `backend/` directory:

```bash
dotnet ef migrations add <Name> --project src/Api/Bud.Infrastructure --startup-project src/Api/Bud.Api
```

## Testing Guidelines (Normative)

### TDD Workflow (MUST)

- Follow `Red -> Green -> Refactor`.
- New feature: write tests first.
- Bug fix: write failing test first.
- Behavior change: update tests first, then code.

### Coverage Expectations (MUST)

- Validators: unit tests.
- Middleware/auth/tenant rules: unit tests.
- MCP protocol/session behavior: unit tests.

### Test Strategy Essentials

- Unit tests: xUnit + Moq + FluentAssertions.
- Include tenant scenarios: global admin, tenant selecionado e autenticação ausente quando relevante.

## Operational References

Operational command details are maintained in:

- `../README.md`
- `../DEPLOY.md`
- `README.md`

## Key Files (Essential)

- `src/Api/Bud.Api/Controllers/ApiControllerBase.cs` (base class compartilhada)
- `src/Api/Bud.Api/Features/Missions/MissionsController.cs`
- `src/Api/Bud.Api/Features/Organizations/OrganizationsController.cs`
- `src/Api/Bud.Application/Common/Result.cs`
- `src/Api/Bud.Application/BudApplicationCompositionExtensions.cs`
- `src/Api/Bud.Api/DependencyInjection/BudObservabilityCompositionExtensions.cs`
- `src/Api/Bud.Api/DependencyInjection/BudSecurityCompositionExtensions.cs`
- `src/Api/Bud.Infrastructure/Persistence/ApplicationDbContext.cs`
- `src/Api/Bud.Application/Ports/ITenantProvider.cs`
- `src/Api/Bud.Api/MultiTenancy/TenantRequiredMiddleware.cs`
- `tests/Api/Bud.ArchitectureTests/Architecture/ArchitectureTests.cs`
- `../docs/adr/README.md`
