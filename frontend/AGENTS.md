# Agent Note (non-normative)

Contract-Version: 2026-04-14

This file supplements `/AGENTS.md` with frontend-specific rules.
`CLAUDE.md` and `GEMINI.md` in this directory should point to this file and must not diverge.

## Frontend Scope

- Runtime app: `frontend/src/app`
- Package root: `frontend/package.json`
- Build/runtime container: `frontend/Dockerfile`

---

## Frontend Contract

### MUST

- Keep all user-facing text in `pt-BR`.
- Preserve the current frontend stack: Next.js 15 with App Router and TypeScript.
- Keep instructions and code aligned with the actual frontend implementation; do not reintroduce Blazor-era paths or documentation.
- Use `NEXT_PUBLIC_API_URL` as the public API base URL convention unless the repository adopts another explicit pattern.
- Respect tenant behavior expected by the backend:
  - Include `X-Tenant-Id` when the user selected a specific organization.
  - Omit `X-Tenant-Id` for all-organizations global-admin flows.
- Validate changes with the relevant frontend commands available in `package.json`.
- Update frontend docs when routes, setup, runtime assumptions, or developer workflows change.
- When adding a new feature, follow the **Presentation Structure** defined below without deviation.
- Validate API responses with a Zod schema placed in `src/schemas/` before using the data anywhere.

### SHOULD

- Prefer existing App Router patterns over adding parallel routing conventions.
- Keep the frontend contract lean and faithful to the current codebase.
- Avoid creating standalone design-system documents unless they are backed by code that actually exists in `frontend/`.

---

## Architecture: Data Flow Overview

```
Browser
  └── src/app/             (Next.js App Router pages)
        └── src/presentation/<feature>/    (feature UI modules)
              ├── hooks/useXxx.ts          (TanStack Query hooks → /api/… routes)
              └── components/              (dumb components)

src/app/api/<resource>/route.ts    (Next.js Route Handlers — server-side proxy)
  └── BUD_API_URL (backend ASP.NET Core)
```

**Rule:** The browser NEVER calls the ASP.NET backend directly. All backend calls go through Next.js Route Handlers in `src/app/api/`. This is where auth tokens are added and data is mapped.

---

## Provider Stack (`src/providers/AppProviders.tsx`)

Providers wrap the entire app in this exact nesting order:

```
QueryProvider                  ← TanStack React Query client
  LoggedUserProvider           ← logged user (REAL: /api/auth/me)
    OrganizationProvider       ← organization list + active org selection (REAL: /api/organizations)
      ConfigDataProvider       ← config data: tags, cycles, roles, company values (TEMP: localStorage)
        ActivityDataProvider   ← user activity log (TEMP: localStorage)
          PeopleDataProvider   ← teams + users (TEMP: localStorage)
            MissionsDataProvider ← missions + check-ins (TEMP: localStorage)
              SettingsDataProvider ← AI settings (TEMP: localStorage)
                IntegrationsDataProvider ← integrations (TEMP: localStorage)
```

**TEMP** = backed by `src/lib/tempStorage/` (localStorage, pre-migration placeholders — see section below).
**REAL** = backed by real API calls via Next.js Route Handlers.

---

## Contexts: Canonical Reference

### Real API-backed Contexts (use freely)

| Context | Hook | What it provides | Source |
|---|---|---|---|
| `LoggedUserContext` | `useLoggedUser()` | `loggedUser`, `isLoading` | `GET /api/auth/me` |
| `OrganizationContext` | `useOrganization()` | `organizations`, `activeOrganization`, `activeOrgId`, `setActiveOrg` | `GET /api/organizations` |

`OrganizationContext` persists the active org selection in a cookie (`selectedOrgId`, 30-day TTL). Always read `activeOrgId` from `useOrganization()` — never from `useConfigData()` — for API calls.

### Temporary Placeholder Contexts (marked for migration)

These contexts are backed by `src/lib/tempStorage/` (localStorage). They serve as a bridge from the legacy `bud-2-saas` app while real API endpoints are built. All have `// TODO: migrate` comments.

| Context | Hook | Manages | tempStorage file |
|---|---|---|---|
| `ConfigDataContext` | `useConfigData()` | Tags, cycles, roles, company values, org profiles | `config-store.ts` |
| `ActivityDataContext` | `useActivityData()` | User activity log | `activity-store.ts` |
| `PeopleDataContext` | `usePeopleData()` | Teams, users (in-memory mirror) | `people-store.ts` |
| `MissionsDataContext` | `useMissionsData()` | Missions, check-ins | `missions-store.ts` |
| `SettingsDataContext` | `useSettingsData()` | AI/notification settings | `settings-store.ts` |
| `IntegrationsDataContext` | `useIntegrationsData()` | Integration connections | `integrations-store.ts` |

**Rule for placeholder contexts:** If a real API endpoint exists for a resource (see below), use a TanStack Query hook instead of the placeholder context. The placeholder context remains available as a fallback only when no API endpoint exists yet.

### Other Contexts (not in AppProviders — consumed locally)

| Context | Hook | What it provides |
|---|---|---|
| `WorkspaceContext` | `useWorkspace()` | Active workspace name (cookie-persisted) |
| `SidebarContext` | `useSidebar()` | Sidebar open/collapsed state |
| `SavedViewsContext` | `useSavedViews()` | Saved view definitions for missions |
| `AssistantContext` | `useAssistant()` | AI assistant state |
| `QueryContext` | — | TanStack `QueryClient` provider (do not access directly; use `useQueryClient()`) |

---

## Real API Endpoints (Route Handlers — use TanStack Query hooks)

All Route Handlers live in `src/app/api/`. They forward requests to the backend (`BUD_API_URL`) with a bearer token from `getBudToken()`.

| Frontend Route | Method(s) | Backend endpoint | Tenant-scoped |
|---|---|---|---|
| `/api/auth/me` | GET | `/api/auth/me` | No |
| `/api/organizations` | GET | `/api/organizations` | No |
| `/api/organizations/[id]` | GET, PATCH | `/api/organizations/:id` | No |
| `/api/organizations/cycles` | GET | `/api/organizations/cycles` | No |
| `/api/teams` | GET, POST | `/api/teams` | Yes |
| `/api/teams/[id]` | GET, PATCH, DELETE | `/api/teams/:id` | Yes |
| `/api/teams/bulk-archive` | POST | `/api/teams/bulk-archive` | Yes |
| `/api/teams/bulk-delete` | POST | `/api/teams/bulk-delete` | Yes |
| `/api/employees` | GET, POST | `/api/employees` | Yes |
| `/api/employees/[id]` | PATCH, DELETE | `/api/employees/:id` | Yes |
| `/api/cycles` | GET, POST | `/api/cycles` | Yes |
| `/api/cycles/[id]` | PATCH, DELETE | `/api/cycles/:id` | Yes |
| `/api/tags` | GET, POST | `/api/tags` | Yes |
| `/api/o/invite` | POST | public (no auth) | No |

`src/app/api/o/` = public routes. Do not add auth-required logic there.

---

## Hook Patterns

### Query hooks (`use<Resource>.ts`)

```ts
// src/presentation/<feature>/hooks/use<Resource>.ts
"use client";

import { useQuery } from "@tanstack/react-query";
import { ResourceListResponseSchema, type ResourceResponse } from "@/schemas/resource";

export const RESOURCE_QUERY_KEY = "resource"; // exported constant — used in mutations

async function fetchResources(orgId: string): Promise<ResourceResponse[]> {
  const res = await fetch("/api/resource", {
    headers: { "X-Tenant-Id": orgId },
  });
  if (!res.ok) throw new Error(`HTTP ${res.status}`);
  return ResourceListResponseSchema.parse(await res.json()); // always parse with Zod
}

export function useResources(orgId: string | null) {
  return useQuery<ResourceResponse[]>({
    queryKey: [RESOURCE_QUERY_KEY, orgId],
    queryFn: () => fetchResources(orgId!),
    enabled: !!orgId,  // never fetch without orgId
  });
}
```

### Mutation hooks (`use<Resource>Mutations.ts`)

```ts
"use client";

import { useMutation, useQueryClient } from "@tanstack/react-query";
import { RESOURCE_QUERY_KEY } from "./useResources";
import type { ResourceResponse } from "@/schemas/resource";

export function useCreateResource(orgId: string | null) {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: async (payload: CreateResourcePayload) => {
      const res = await fetch("/api/resource", {
        method: "POST",
        headers: {
          "Content-Type": "application/json",
          ...(orgId ? { "X-Tenant-Id": orgId } : {}),
        },
        body: JSON.stringify(payload),
      });
      if (!res.ok) throw new Error(`HTTP ${res.status}`);
      return res.json() as Promise<ResourceResponse>;
    },
    onSuccess: (created) => {
      // optimistic cache update — no refetch needed
      queryClient.setQueryData<ResourceResponse[]>(
        [RESOURCE_QUERY_KEY, orgId],
        (prev) => (prev ? [...prev, created] : [created]),
      );
    },
  });
}
```

**Rules:**
- Export the query key constant from the query hook file.
- Always pass `enabled: !!orgId` to prevent fetching with null orgId.
- Update the cache via `setQueryData` in `onSuccess` — avoid redundant refetches.
- For non-tenant endpoints, omit the `X-Tenant-Id` header.

---

## Zod Validation (`src/schemas/`)

All API response types must be defined in `src/schemas/` using Zod. **Never trust raw `res.json()` without parsing.**

### File structure

```
src/schemas/
  employee.ts       ← EmployeeResponseSchema, EmployeeListResponseSchema
  organization.ts   ← OrganizationResponseSchema, OrganizationListResponseSchema
  tag.ts            ← TagResponseSchema, TagListResponseSchema
  cycle.ts          ← CycleResponseSchema, CycleListResponseSchema
  <resource>.ts     ← (add new schemas here, one file per resource)
```

### Schema convention

```ts
// src/schemas/<resource>.ts
import { z } from "zod";

export const ResourceResponseSchema = z.object({
  id: z.string(),
  organizationId: z.string(),
  name: z.string(),
  // ...all fields returned by the backend
  createdAt: z.string(),
  updatedAt: z.string(),
});

export const ResourceListResponseSchema = z.array(ResourceResponseSchema);
// OR if backend wraps in { items: [...] }:
export const ResourceListResponseSchema = z.object({
  items: z.array(ResourceResponseSchema),
});

export type ResourceResponse = z.infer<typeof ResourceResponseSchema>;
```

**Rules:**
- Use `.nullable().optional()` for backend nullable fields.
- Use `.transform()` only when shape conversion is needed (e.g., `status` → lowercase).
- Export the inferred `type` so hooks can use it without re-declaring.
- Parse in the fetch function: `Schema.parse(await res.json())`.

---

## Presentation Structure (`src/presentation/`)

Each feature lives in its own folder under `src/presentation/<feature>/`.

```
src/presentation/<feature>/
  index.tsx              ← root module component; what the page imports
  consts.ts              ← feature-scoped constants (options arrays, labels, etc.)
  types.ts               ← local view types (NOT raw API types; derived from schema types)
  utils.ts               ← pure utility functions
  components/
    <FeatureFormModal>.tsx
    <FeatureTableRow>.tsx
    <FeatureFilterBar>.tsx
    Delete<Feature>Modal.tsx
    ...
  hooks/
    use<Resource>.ts        ← query hook
    use<Resource>Mutations.ts ← mutation hooks
  __tests__/              ← colocated tests for this feature
```

**Rules:**
- `index.tsx` is the entry point; it owns local state (modals, selection) and composes hooks + components.
- `components/` contains dumb/presentational components. They receive data and callbacks via props.
- `hooks/` contains only TanStack Query hooks (queries and mutations). No business logic in components.
- `consts.ts` for static option arrays. No magic strings inline in JSX.
- `types.ts` for local view models (e.g., `TagView` derived from `TagResponse` with added display fields).

### Real-world example: Tags module

```
presentation/configuration/tags/
  index.tsx              ← TagsModule: owns state, composes useTags + useConfigData
  types.ts               ← TagView interface
  components/
    TagFormModal.tsx
    DeleteTagModal.tsx
  hooks/
    useTags.ts           ← useQuery → /api/tags
```

`TagsModule` reads real data from `useTags(activeOrgId)` and writes via `useConfigData()` (temporary, will be replaced by mutation hooks when the tag mutation API is wired up).

---

## Route Handler Pattern (`src/app/api/<resource>/route.ts`)

```ts
import { getBudToken } from "@/lib/bud-token";
import { NextRequest, NextResponse } from "next/server";

export async function GET(request: NextRequest) {
  const apiUrl = process.env.BUD_API_URL;
  const token = await getBudToken();
  const tenantId = request.headers.get("X-Tenant-Id"); // forward if present

  const response = await fetch(`${apiUrl}/api/<resource>`, {
    headers: {
      Authorization: `Bearer ${token}`,
      ...(tenantId ? { "X-Tenant-Id": tenantId } : {}),
    },
  });

  if (!response.ok) {
    return NextResponse.json({ error: "..." }, { status: response.status });
  }

  const data = await response.json();
  return NextResponse.json(data);
}
```

**Rules:**
- Always use `getBudToken()` (server-side only) — never expose tokens to the browser.
- Map backend response shapes to frontend types inside the Route Handler when they differ.
- When mapping logic is shared by more than one Route Handler, extract it to `src/lib/api/<resource>-mapper.ts`.
- Forward `X-Tenant-Id` from the incoming request when the resource is tenant-scoped.
- Do not add business logic to Route Handlers — mapping only.
- Use `{ error: "..." }` (in pt-BR) for frontend-generated errors. For backend error pass-through (mutations), forward the response body as-is and use `{ detail: "Erro desconhecido" }` as the JSON-parse fallback.

---

## tempStorage — What It Is and When NOT to Use It

`src/lib/tempStorage/` contains localStorage-backed stores ported from the legacy `bud-2-saas` app. They are **placeholders** while real API endpoints are being built.

| File | Serves | Migration status |
|---|---|---|
| `config-store.ts` | `ConfigDataContext` | Partial: tags/cycles have real APIs now |
| `missions-store.ts` | `MissionsDataContext` | Not started |
| `people-store.ts` | `PeopleDataContext` | Not started |
| `activity-store.ts` | `ActivityDataContext` | Not started |
| `settings-store.ts` | `SettingsDataContext` | Not started |
| `integrations-store.ts` | `IntegrationsDataContext` | Not started |

**Do not** add new business logic to `tempStorage/` files. Do not create new `tempStorage/` files for new features. New features must use real API + TanStack Query hooks from day one.

**Migration pattern:** When a real API endpoint is ready for a resource currently in tempStorage, create a query hook in `presentation/<feature>/hooks/`, add a Zod schema, and update the consuming module to read from TanStack Query instead of the context.

---

## Validation

Run from `frontend/` when relevant:

```bash
npm run type-check
npm run build
```

If more validation is needed in the future, add it to `package.json` first and then document it here.

---

## Documentation References

- `README.md`
- `../README.md`
- `../DEPLOY.md`
- `../AGENTS.md` (root contract)
