# Renomear entidades MissionTemplate* → Template*

## Context

O refactoring de `MissionObjective`→`Objective` e `MissionMetric`→`Metric` já foi feito. Porém `MissionTemplate`, `MissionTemplateObjective`, `MissionTemplateMetric` e seus Drafts ficaram para trás — rotas, DbSets, contracts e repositórios já usam "Template", mas as entidades de domínio ainda têm o prefixo "Mission".

## Renames

| Antigo | Novo | Arquivo |
|--------|------|---------|
| `MissionTemplate` | `Template` | `MissionTemplate.cs` → `Template.cs` |
| `MissionTemplateObjective` | `TemplateObjective` | `MissionTemplateObjective.cs` → `TemplateObjective.cs` |
| `MissionTemplateMetric` | `TemplateMetric` | `MissionTemplateMetric.cs` → `TemplateMetric.cs` |
| `MissionTemplateObjectiveDraft` | `TemplateObjectiveDraft` | `MissionTemplateObjectiveDraft.cs` → `TemplateObjectiveDraft.cs` |
| `MissionTemplateMetricDraft` | `TemplateMetricDraft` | `MissionTemplateMetricDraft.cs` → `TemplateMetricDraft.cs` |

## Passo 1 — git mv dos 5 arquivos

## Passo 2 — Renomear classes dentro dos arquivos movidos

## Passo 3 — Find & replace em produção (~20 arquivos, excl. migrations)

Ordem (mais específico primeiro):
1. `MissionTemplateObjectiveDraft` → `TemplateObjectiveDraft`
2. `MissionTemplateMetricDraft` → `TemplateMetricDraft`
3. `MissionTemplateObjective` → `TemplateObjective`
4. `MissionTemplateMetric` → `TemplateMetric`
5. `MissionTemplate` → `Template` (somente como tipo)

Arquivos:
- `Domain/Repositories/ITemplateRepository.cs`
- `Infrastructure/Persistence/ApplicationDbContext.cs`
- `Infrastructure/Persistence/DbSeeder.cs`
- `Infrastructure/Persistence/Configurations/Template*.cs` (3 configs)
- `Infrastructure/Repositories/TemplateRepository.cs`
- `Infrastructure/Querying/TemplateSearchSpecification.cs`
- `Application/UseCases/Templates/*.cs` (4 use cases)
- `Controllers/TemplatesController.cs`
- `Client/Services/ApiClient.cs`

## Passo 4 — Find & replace nos testes (~10 arquivos)

- `tests/.../MissionTemplates/MissionTemplateUseCasesTests.cs`
- `tests/.../Architecture/AggregateRootArchitectureTests.cs`
- `tests/.../Architecture/ArchitectureTests.cs`
- `tests/.../Domain/Models/AggregateInvariantsTests.cs`
- `tests/.../Infrastructure/Persistence/DbSeederTests.cs`
- `tests/.../Specifications/TemplateSearchSpecificationTests.cs`
- `tests/.../Endpoints/MissionTemplatesEndpointsTests.cs`
- `tests/.../TypeAliases.cs`
- `tests/.../Validators/MissionTemplateValidatorTests.cs`

## Passo 5 — Atualizar CLAUDE.md e AGENTS.md

## NÃO TOCAR

- `Infrastructure/Persistence/Migrations/` — auto-gerado

## Verificação

1. `dotnet build` — 0 erros, 0 warnings
2. `dotnet test` — todos passam
