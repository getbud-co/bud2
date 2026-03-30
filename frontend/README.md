# Bud Web - Frontend

Frontend Next.js da aplicação Bud.

## Tech Stack

- **Framework**: Next.js 15 (App Router)
- **Language**: TypeScript
- **Auth**: dependências preparadas para NextAuth.js
- **Testing**: Vitest + Testing Library (planejado)

## Estrutura

```
frontend/
├── src/
│   └── app/               # App Router (layout e páginas)
│       ├── layout.tsx
│       └── page.tsx
├── public/                # Static assets
├── AGENTS.md              # Contrato local para agentes
├── package.json
├── tsconfig.json
├── next.config.ts
└── Dockerfile             # Build + runtime Node.js
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

O frontend usa `NEXT_PUBLIC_API_URL` para consumir a API do backend.

Futuramente, pode ser gerado automaticamente a partir do OpenAPI com:

```bash
npx openapi-typescript http://localhost:8082/openapi/v1.json -o src/types/api.ts
npx openapi-fetch -i src/types/api.ts
```

## Deploy

Ver `../DEPLOY.md` para instruções de deploy no GCP Cloud Run.
