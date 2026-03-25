# Bud Web - Frontend

Frontend Next.js da aplicação Bud.

## Tech Stack

- **Framework**: Next.js 15 (App Router)
- **Language**: TypeScript
- **Styling**: Tailwind CSS (puro, sem componentes pré-built)
- **Auth**: NextAuth.js + backend sessions
- **Testing**: Vitest + Testing Library (planejado)

## Estrutura

```
frontend/
├── src/
│   ├── app/               # App Router (routes, layouts, pages)
│   │   ├── (auth)/        # Route group: login, register
│   │   ├── (dashboard)/   # Route group: app autenticada
│   │   ├── layout.tsx
│   │   └── page.tsx
│   ├── components/        # React components
│   ├── lib/
│   │   ├── api.ts         # Client HTTP para API
│   │   └── auth.ts        # NextAuth config
│   ├── hooks/             # Custom React hooks
│   ├── stores/            # Estado global (Zustand)
│   └── types/             # TypeScript types
├── public/                # Static assets
├── package.json
├── tsconfig.json
├── next.config.ts
├── tailwind.config.ts
└── Dockerfile             # Build + nginx
```

## Setup Local

### Pré-requisitos

- Node.js 20+ com npm

### Iniciar

```bash
cd frontend
npm install
npm run dev
```

Abre http://localhost:3000

### Variáveis de Ambiente

Criar `.env.local`:

```env
NEXT_PUBLIC_API_URL=http://localhost:8082
```

## Desenvolvimento

### Build para Produção

```bash
npm run build
npm run start
```

### Type Checking

```bash
npm run type-check
```

### Testes (planejado)

```bash
npm test
```

## API Client

O cliente HTTP para a API está em `src/lib/api.ts`.

Futuramente, pode ser gerado automaticamente a partir do OpenAPI com:

```bash
npx openapi-typescript http://localhost:8082/openapi/v1.json -o src/types/api.ts
npx openapi-fetch -i src/types/api.ts
```

## Deploy

Ver `../DEPLOY.md` para instruções de deploy no GCP Cloud Run.

## Migração de Blazor WASM

Este projeto substitui `Bud.BlazorWasm`. A migração de funcionalidades será feita gradualmente conforme necessário.
