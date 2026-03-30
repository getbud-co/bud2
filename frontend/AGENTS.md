# Agent Note (non-normative)

Contract-Version: 2026-03-30

This file supplements `/AGENTS.md` with frontend-specific rules.
`CLAUDE.md` and `GEMINI.md` in this directory should point to this file and must not diverge.

## Frontend Scope

- Runtime app: `frontend/src/app`
- Package root: `frontend/package.json`
- Build/runtime container: `frontend/Dockerfile`

## Frontend Contract

### MUST

- Keep all user-facing text in `pt-BR`.
- Preserve the current frontend stack: Next.js 15 with App Router and TypeScript.
- Keep instructions and code aligned with the actual frontend implementation; do not reintroduce Blazor-era paths or documentation.
- Use `NEXT_PUBLIC_API_URL` as the public API base URL convention unless the repository adopts another explicit pattern.
- Respect tenant behavior expected by the backend:
  - include `X-Tenant-Id` when the user selected a specific organization;
  - omit `X-Tenant-Id` for all-organizations global-admin flows.
- Validate changes with the relevant frontend commands available in `package.json`.
- Update frontend docs when routes, setup, runtime assumptions, or developer workflows change.

### SHOULD

- Prefer existing App Router patterns over adding parallel routing conventions.
- Keep the frontend contract lean and faithful to the current codebase.
- Avoid creating standalone design-system documents unless they are backed by code that actually exists in `frontend/`.

## Validation

Run from `frontend/` when relevant:

```bash
npm run type-check
npm run build
```

If more validation is needed in the future, add it to `package.json` first and then document it here.

## Documentation References

- `README.md`
- `../README.md`
- `../DEPLOY.md`
