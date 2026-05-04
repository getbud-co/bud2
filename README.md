# Bud

Monorepo com frontend Next.js e backend ASP.NET Core 10.

## Estado atual

- `frontend/`: base nova do produto, mantida como ponto de partida para a reconstrução da interface.
- `backend/`: backend reiniciado sobre uma fundação enxuta, preservando autenticação, multitenancy, observabilidade, API base e MCP limpo.

## Estrutura

```text
bud/
├── backend/
├── frontend/
├── docs/
├── scripts/
├── compose.yml
├── AGENTS.md
├── DEPLOY.md
└── README.md
```

## Backend preservado

- API ASP.NET Core 10
- PostgreSQL + EF Core 10
- JWT por e-mail
- multitenancy por `OrganizationId`
- organizações e contexto do usuário
- colaboradores (`Employee`) e autenticação associada
- notificações neutras por colaborador
- OpenTelemetry + structured logging
- health checks
- servidor MCP HTTP sem tools de domínio

## Backend removido

- metas
- indicadores
- tarefas
- templates
- times
- dashboard legado
- catálogo MCP derivado do domínio antigo

## Desenvolvimento

```bash
docker compose up
```

Ou, na pasta `backend/`:

```bash
dotnet restore Bud.sln
dotnet build Bud.sln
dotnet test Bud.sln
```

Veja também:

- `backend/README.md`
- `frontend/README.md`
- `backend/AGENTS.md`
- `DEPLOY.md`
