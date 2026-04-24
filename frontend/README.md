# Bud 2.0 Web Application

> web application for bud 2.0 project

## 🚀 Requisitos

```
 Node @22.14.0
 Yarn @1.22.22
```

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

## 🚀 Instalando Web Application

Para instalar o Web Application, siga estas etapas:

```
 yarn install 
```

## ☕ Usando Web Application

Para usar Web Application, siga estas etapas:

```
 yarn dev
```

## ☕ Usando Docker

Para rodar a imagem da aplicação, é necessário adicionar o arquivo .env como no exemplo:

```
 docker run -d -p 3000:3000 --env-file .env bud-web:latest
```


## 📝 Licença

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
