# Agent Note (non-normative)

Contract-Version: 2026-04-14

This is the base agent contract for the Bud monorepo.
`CLAUDE.md` and `GEMINI.md` in the same directory should point to this file and must not diverge.

## Monorepo Structure

The repository is organized as a monorepo with isolated backend and frontend directories:

- **`backend/`**: ASP.NET Core 10 API, MCP server, and all backend tests
  - Solution file: `backend/Bud.sln`
  - Dockerfiles: `backend/Dockerfile` and `backend/Dockerfile.Production`
- **`frontend/`**: Next.js 15 frontend
  - Package file: `frontend/package.json`
  - Dockerfile: `frontend/Dockerfile`
- **`docs/`**, **`scripts/`**: shared documentation and deployment scripts
- **`compose.yml`**: local development orchestration

## Scope and Precedence

- This file is the normative base instruction set for the whole monorepo.
- `backend/AGENTS.md` and `frontend/AGENTS.md` supplement this file with area-specific rules.
- Agents should read the closest `AGENTS.md` in the current working area first, then apply this root contract as the shared baseline.
- `README.md` is human-oriented documentation.
- `DEPLOY.md` is deployment-oriented documentation.
- ADRs under `docs/adr/` are the architecture decision history.

## Quick Reference

1. **pt-BR**: all user-facing messages must be in Brazilian Portuguese.
2. **Closest contract wins on detail**: local `AGENTS.md` files complement the root contract.
3. **No drift**: prefer extending existing patterns over introducing parallel conventions.
4. **Docs move with code**: update affected documentation in the same change.
5. **Validation required**: run the relevant checks for the area you changed.

## Shared Rules (MUST)

- Keep all user-facing text in `pt-BR`.
- Preserve the backend/frontend boundary of the monorepo; do not document or implement cross-layer behavior in the wrong area.
- Prefer minimal, coherent changes that follow the repository's established structure.
- Update tests together with production changes when behavior changes.
- Keep documentation aligned with the current repository layout and runtime behavior.
- Do not leave broken links, stale paths, or instructions that reference removed components.

## Documentation Update Rule (MUST)

When a feature, workflow, or architecture behavior is added, changed, or removed, review and update when applicable:

- `AGENTS.md`
- `backend/AGENTS.md`
- `frontend/AGENTS.md`
- `README.md`
- `DEPLOY.md`
- ADRs under `docs/adr/`

Minimum expected behavior:

- Update affected docs in the same change.
- If no documentation update is required, explicitly justify this in the final task summary.

## Agent Definition of Done (MUST)

Before finishing, verify:

- `Scope`: the change respects monorepo boundaries.
- `Language`: user-facing text remains in `pt-BR`.
- `Validation`: relevant checks for the touched area were run, or the reason they were not run is stated.
- `Docs`: repository-level and area-level documentation reflect the current behavior.
- `No drift`: no conflicting parallel pattern was introduced.
