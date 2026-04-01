# Plano Remanescente de Correção dos Achados de Entidades

## Objetivo

Este documento descreve somente o que ainda falta fazer após a rodada de correções já aplicada no backend.

Ele foi escrito para permitir execução por um agente sem contexto prévio, portanto inclui:

- o problema remanescente;
- o comportamento esperado;
- os arquivos mais prováveis de ajuste;
- os testes que precisam existir ou ser atualizados;
- os pontos de decisão que não devem ser implementados sem definição explícita.

## O que já foi resolvido e não deve entrar nesta rodada

Os itens abaixo já foram corrigidos e não fazem parte do escopo remanescente:

- conflito explícito por duplicidade em `organization`;
- normalização real para UTC em create e patch de `checkin`;
- persistência real no fluxo de `PatchCheckin`;
- bloqueio de exclusão de `team` quando houver metas associadas a membros do time;
- revisão do relatório técnico em `specs/relatorio-avaliacao-estatica-entidades.md`.

Antes de iniciar qualquer implementação, assumir que esses pontos já estão resolvidos e evitar retrabalho ou reversão.

## Estado atual

A semântica oficial adotada no backend passou a ser:

- `EmployeeTeams` é a única fonte de verdade para pertencimento a time;
- `Employee.TeamId` não é mais usado em regras de negócio, leitura de membros, filtro de time, dashboard nem bloqueios funcionais;
- o backend foi ajustado para não depender de noção de “time principal”.

## Itens já resolvidos nesta trilha

- autoria de `checkin` em patch e delete;
- alinhamento do validator de patch de `employee` com o tenant atual para validação de `LeaderId`.
- consolidação funcional de pertencimento a time via `EmployeeTeams`.

## Observação remanescente

Ainda existe um ponto contratual que pode ser revisado em rodada própria, se o time quiser eliminar toda ambiguidade de API:

- `CreateEmployeeRequest` ainda aceita `TeamId` como atalho para criar o primeiro vínculo em `EmployeeTeams`.

Esse campo não representa mais “time principal”; hoje ele funciona apenas como conveniência de criação inicial.

## Regras de implementação para o próximo agente

- manter todas as mensagens expostas ao usuário em `pt-BR`;
- não reverter as correções já aplicadas nesta rodada;
- tratar o worktree como potencialmente sujo e evitar sobrescrever mudanças alheias;
- atualizar testes junto com qualquer mudança de comportamento;
- se uma decisão de negócio estiver ausente, parar e registrar bloqueio em vez de inferir uma regra estrutural;
- ao concluir cada item, revisar também `specs/relatorio-avaliacao-estatica-entidades.md` se o status do achado deixar de ser pendente.

## Validação mínima esperada em cada entrega futura

- testes unitários dos casos de uso afetados;
- testes de integração dos endpoints afetados;
- revisão de contrato HTTP quando houver mudança de status code ou mensagem funcional;
- atualização da documentação técnica em `specs/` para refletir o novo estado real do código.
