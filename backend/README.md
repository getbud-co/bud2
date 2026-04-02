# Bud API - Backend

Backend ASP.NET Core da aplicação Bud.

## Tech Stack

- **Framework**: ASP.NET Core 10
- **Database**: PostgreSQL 16 + EF Core 10
- **Architecture**: DDD + Clean Architecture
- **Testing**: xUnit + Moq + Testcontainers
- **Observability**: Structured Logging + OpenTelemetry

## Estrutura

```
backend/
├── src/
│   ├── Api/           # API HTTP (Controllers, Features)
│   ├── Mcp/           # MCP Server (HTTP)
│   └── Shared/        # Contratos e tipos compartilhados
├── tests/
│   ├── Api/           # Testes Backend (Unit, Integration, Architecture)
│   └── Mcp/           # Testes MCP
├── Bud.sln
├── Dockerfile         # Dev targets (dev-api, dev-mcp)
└── Dockerfile.Production  # Prod targets (prod-api, prod-mcp, prod-migrate)
```

## Setup Local

### Pré-requisitos

- .NET 10 SDK
- Docker (para PostgreSQL local)
- Docker Compose

### Iniciar

```bash
cd backend
dotnet restore
dotnet build
dotnet test
```

Ou com Docker Compose (na raiz do repo):

```bash
docker compose up api db
```

### Variáveis de Ambiente

Criar `.env` ou copiar de `.env.example`:

```env
ASPNETCORE_ENVIRONMENT=Development
ConnectionStrings__DefaultConnection=Host=localhost;Port=5432;Database=bud;Username=postgres;Password=postgres
```

## Desenvolvimento

### Estrutura DDD

- **Domain**: Entidades, Value Objects, Eventos, Interfaces de Repositório
- **Application**: Use Cases, Mappers, Ports, Read Models
- **Infrastructure**: EF Core, Repositórios Concretos, Serviços, Domain Event Dispatcher
- **Api**: Controllers + Validators co-localizados em `Features/<Feature>/`, Middleware, Authorization

### Testes

```bash
# Unit tests
dotnet test tests/Api/Bud.Api.UnitTests/

# Integration tests (requer Testcontainers PostgreSQL)
dotnet test tests/Api/Bud.Api.IntegrationTests/

# Architecture tests
dotnet test tests/Api/Bud.ArchitectureTests/

# Tudo
dotnet test
```

### Migrations

```bash
# Add migration
dotnet ef migrations add <Name> --project src/Api/Bud.Infrastructure --startup-project src/Api/Bud.Api

# Remove last migration
dotnet ef migrations remove --project src/Api/Bud.Infrastructure --startup-project src/Api/Bud.Api

# Update database
dotnet ef database update --project src/Api/Bud.Infrastructure --startup-project src/Api/Bud.Api
```

### API Documentation

OpenAPI disponível em `/swagger` e `/openapi/v1.json` em Development.

## Deploy

Ver `../DEPLOY.md` para instruções de deploy no GCP Cloud Run.
