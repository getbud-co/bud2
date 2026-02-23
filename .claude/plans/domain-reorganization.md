# Reorganizar Domain/ — Estrutura de Pastas, VOs e Enums

## Context

`Domain/Model/` mistura 31 arquivos: entidades, value objects, enums, interfaces marcadoras e exceção. Apenas `EmailAddress` está corretamente em `Domain/ValueObjects/`. Interfaces/exceção de domínio deveriam estar em `Domain/Abstractions/`. 4 VOs sem `Create()` (inconsistente). `MissionEnums.cs` tem 5 enums juntos.

## Estratégia: Global Usings para minimizar impacto

Adicionar global usings evita editar ~193 arquivos. Depois remover 65 usings explícitos redundantes.

## Passo 1 — Global usings (3 arquivos)

Adicionar a cada GlobalUsings.cs:
- `src/Bud.Server/GlobalUsings.cs`
- `tests/Bud.Server.Tests/GlobalUsings.cs`
- `tests/Bud.Server.IntegrationTests/GlobalUsings.cs`

```
global using Bud.Server.Domain.Abstractions;
global using Bud.Server.Domain.ValueObjects;
```

## Passo 2 — Mover 3 abstrações para Domain/Abstractions/ (git mv + namespace)

| Arquivo | `Domain.Model` → `Domain.Abstractions` |
|---------|----------------------------------------|
| `IAggregateRoot.cs` | marker interface |
| `ITenantEntity.cs` | tenant contract |
| `DomainInvariantException.cs` | exceção base |

## Passo 3 — Mover 9 VOs para Domain/ValueObjects/ (git mv + namespace)

| Arquivo | `Domain.Model` → `Domain.ValueObjects` |
|---------|----------------------------------------|
| `EntityName.cs` | |
| `PersonName.cs` | |
| `ConfidenceLevel.cs` | |
| `MetricRange.cs` | |
| `MissionScope.cs` | |
| `NotificationTitle.cs` | |
| `NotificationMessage.cs` | |
| `EngagementScore.cs` | |
| `PerformanceIndicator.cs` | |

`EmailAddress.cs` já está em `Domain/ValueObjects/` — sem mudança.

## Passo 4 — Remover usings explícitos redundantes

- 58 arquivos: remover `using Bud.Server.Domain.Abstractions;`
- 7 arquivos: remover `using Bud.Server.Domain.ValueObjects;`

## Passo 5 — Splittar MissionEnums.cs → 5 arquivos (namespace `Domain.Model` mantido)

| Novo arquivo | Enum |
|-------------|------|
| `MissionStatus.cs` | `MissionStatus` |
| `MissionScopeType.cs` | `MissionScopeType` |
| `MetricType.cs` | `MetricType` |
| `QuantitativeMetricType.cs` | `QuantitativeMetricType` |
| `MetricUnit.cs` | `MetricUnit` (manter `#pragma warning`) |

Deletar `MissionEnums.cs`.

## Passo 6 — Adicionar Create() a 4 VOs (padrão EntityName.Create)

| VO | Mensagem de erro |
|----|-----------------|
| `PersonName` | `"O nome informado é inválido."` |
| `NotificationTitle` | `"O título da notificação é inválido."` |
| `NotificationMessage` | `"A mensagem da notificação é inválida."` |
| `EmailAddress` | `"O e-mail informado é inválido."` |

## Passo 7 — Testes

- Adicionar testes `Create()` válido/inválido para os 4 VOs em `ValueObjectTests.cs`
- Verificar architecture tests passam

## Verificação

1. `dotnet build` — 0 erros, 0 warnings
2. `dotnet test` — todos passam
