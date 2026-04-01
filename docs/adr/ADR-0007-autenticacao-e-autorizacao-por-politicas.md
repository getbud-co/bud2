# ADR-0007: Autenticação e Autorização por Políticas

## Status
Accepted

## Contexto
A segurança do domínio depende de autenticação confiável e autorização consistente.

## Decisão
- Autenticação via JWT.
- Políticas de borda limitadas a autenticação/tenant (`TenantSelected`, `GlobalAdmin`).
- Autorização contextual orientada por políticas e handlers compartilhados (`ResourceRead`, `ResourceWrite`).
- Regras de permissão contextual implementadas como regras tipadas por recurso/contexto na Infrastructure e orquestradas pelos use cases via gateway.

## Consequências
- Redução de condicionais de permissão espalhadas no código.
- Menor duplicação estrutural de policies, handlers e gateways por feature.
- Facilidade de evolução de regras de acesso por recurso/contexto sem expandir o catálogo de policies.

## Alternativas consideradas
- Autorização ad-hoc por `if` em cada controller/repositório.
- Misturar autenticação e autorização em serviços de domínio.
