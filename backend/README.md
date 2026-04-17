# Bud Backend

Backend ASP.NET Core 10 reiniciado sobre uma base enxuta, preservando infraestrutura compartilhada e removendo as features legadas do Bud.

## Estado atual

- `Bud.Api`: API HTTP com autenticação JWT, multitenancy por `OrganizationId`, middleware, OpenAPI e health checks.
- `Bud.Application`: casos de uso mínimos para sessão, organizações, colaboradores, notificações e contexto do usuário.
- `Bud.Domain`: domínio reduzido a `Organization`, `Employee`, `Notification` e primitivos compartilhados.
- `Bud.Infrastructure`: EF Core, repositórios mínimos, `ApplicationDbContext`, seed, inbox de notificações e migrations.
- `Bud.Mcp`: host MCP limpo, com sessão, autenticação, seleção de tenant, observabilidade e protocolo JSON-RPC.

## O que ficou no backend base

- autenticação JWT por e-mail
- isolamento por tenant com `X-Tenant-Id`
- organizações e listagem de organizações disponíveis para o usuário
- CRUD de colaboradores (`Employee`) com listagem paginada padrão
- inbox neutro de notificações por colaborador
- observabilidade com structured logging + OpenTelemetry
- health checks (`/health/live` e `/health/ready`)
- Docker dev/prod, CI e migrations
- servidor MCP HTTP sem tools de domínio

## O que saiu

- metas
- indicadores e check-ins
- tarefas
- templates
- times
- dashboard legado
- catálogo MCP gerado a partir do domínio anterior

## Notificações neutras

- inbox por colaborador com listagem paginada
- marcação individual e em lote como lida
- publicação genérica via `PublishNotification`
- categorias livres (`system.info`, `workflow.review`, etc.)
- sem acoplamento a metas, check-ins ou qualquer feature legada

## Estrutura

```text
backend/
├── src/
│   ├── Api/
│   │   ├── Bud.Api
│   │   ├── Bud.Application
│   │   ├── Bud.Domain
│   │   └── Bud.Infrastructure
│   ├── Mcp/
│   │   └── Bud.Mcp
│   └── Shared/
│       ├── Bud.Shared.Contracts
│       └── Bud.Shared.Kernel
├── tests/
│   ├── Api/Bud.Api.UnitTests
│   └── Mcp/Bud.Mcp.Tests
├── Bud.sln
├── Dockerfile
└── Dockerfile.Production
```

## Comandos principais

```bash
dotnet restore Bud.sln
dotnet build Bud.sln
dotnet test Bud.sln
```

## Migrations

```bash
dotnet ef migrations add <Nome> --project src/Api/Bud.Infrastructure --startup-project src/Api/Bud.Api
dotnet ef database update --project src/Api/Bud.Infrastructure --startup-project src/Api/Bud.Api
```

## Execução local

Na raiz do monorepo:

```bash
docker compose up api mcp db
```

## Observação

Este backend está intencionalmente reduzido a uma base operacional para reconstrução do produto a partir do frontend novo.
