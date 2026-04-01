# Relatório de Avaliação Estática por Entidade

## Contexto

Este documento foi revisado contra o código atual do repositório em 2026-03-30, após leitura estática, validação de fluxos e execução de testes direcionados.

Status possíveis usados nesta revisão:

- `confirmado`: o achado representa corretamente o estado atual do código.
- `parcial`: existe um problema real, mas a formulação original exagerava, estava incompleta ou atribuía impacto incorreto.
- `refutado`: o código atual contradiz o achado.
- `desatualizado`: o achado descrevia um estado anterior já corrigido no branch atual.

## Achados comuns

### 1. Contrato HTTP e comportamento real nem sempre estão alinhados

Status: `parcial`

Leitura revisada:

- havia drift real em parte dos endpoints, mas alguns itens do relatório já estavam corrigidos no código atual;
- `organization create` não documenta mais `404`;
- `task create` já aponta para `GET` canônico;
- o maior ponto de alinhamento contratual remanescente era `organization` sem tratamento explícito de duplicidade, agora corrigido para conflito.

### 2. Regras de negócio relevantes ficam concentradas no validator, não no domínio

Status: `parcial`

Leitura revisada:

- a crítica é válida em alguns pontos de borda, mas o texto original subestimava o papel do domínio;
- `indicator` já garantia boa parte das invariantes quantitativas no domínio via `IndicatorTargetDefinition`;
- `organization` não tinha esse problema, porque validator e domínio já compartilhavam a mesma regra de domínio válido.

### 3. Há acoplamento entre autorização contextual e contexto de tenant/colaborador

Status: `confirmado`

Leitura revisada:

- a estrutura controller -> use case -> regra contextual continua consistente;
- a semântica real segue distribuída entre middleware, tenant provider e regras de autorização;
- isso permanece um ponto de atenção, mas não motivou correção funcional nesta rodada.

### 4. Side effects pós-commit podem fazer a API responder erro após persistir o recurso principal

Status: `parcial`

Leitura revisada:

- a API realmente pode responder `500` se um handler de evento falhar;
- porém o recurso principal não fica persistido, porque o `EfUnitOfWork` mantém transação aberta até o dispatch e faz rollback se o handler falhar;
- o risco real é indisponibilidade do fluxo, não inconsistência “persistiu mas respondeu erro”.

### 5. Cobertura automatizada existe, mas deixa lacunas nas bordas mais sensíveis

Status: `confirmado`

Leitura revisada:

- o branch principal estava razoavelmente coberto;
- ainda havia lacuna de persistência real em `PatchCheckin`, que foi detectada pela revisão e coberta nesta rodada;
- contratos e leituras correlatas continuam merecendo testes de regressão dedicados.

## Achados específicos por entidade

### Organization

1. Contrato de criação exige domínio válido no validator, mas o aggregate aceita apenas um nome válido genérico.
   Status: `refutado`
   Leitura revisada: validator e domínio usam a mesma regra via `OrganizationDomainName`.

2. `POST /api/organizations` documenta `404`, embora o fluxo real não tenha caminho para esse status.
   Status: `desatualizado`
   Leitura revisada: o endpoint atual já não documenta `404`.

3. Não há evidência de proteção explícita contra duplicidade de nome/domínio no create.
   Status: `confirmado`
   Leitura revisada: o fluxo não tinha proteção explícita; nesta rodada foi adicionada checagem de unicidade e retorno `409 Conflict`.

### Mission

1. A missão pode ser persistida com sucesso e ainda assim a API responder `500` se a cadeia de eventos/notificações falhar após o `SaveChanges`.
   Status: `parcial`
   Leitura revisada: o `500` pode ocorrer, mas a missão não fica persistida porque a transação é revertida.

2. Para `GlobalAdmin`, um `X-Tenant-Id` inválido pode escapar da validação semântica e falhar apenas em infra/banco.
   Status: `refutado`
   Leitura revisada: o middleware valida o tenant antes do fluxo de negócio e o comportamento está coberto por teste.

3. `POST /api/missions` não documenta adequadamente todos os `403/404` possíveis do fluxo real.
   Status: `refutado`
   Leitura revisada: o endpoint já documenta os status relevantes e havia teste unitário de metadados cobrindo isso.

4. A regra de missão filha cobre início em relação ao pai, mas não há evidência equivalente para outros limites temporais do relacionamento pai-filho.
   Status: `refutado`
   Leitura revisada: a política cobre início e término em relação ao pai; a relação básica `end >= start` já é garantida em outras camadas.

### Indicator

1. As invariantes de indicador quantitativo dependem fortemente do validator HTTP; o domínio não garante integralmente o mesmo conjunto de regras.
   Status: `parcial`
   Leitura revisada: o domínio já cobre tipo quantitativo, unidade, obrigatoriedade de min/max, faixa válida e limpeza de campos; ainda restam regras de formato e higiene de payload na borda HTTP.

2. O create de indicador não dispara side effects adicionais.
   Status: `confirmado`
   Leitura revisada: continua sendo um fluxo mais simples e operacionalmente previsível.

### Checkin

1. `CreateCheckin` e `PatchCheckin` usam `DateTime.SpecifyKind(..., Utc)` em vez de normalização real para UTC.
   Status: `confirmado`
   Leitura revisada: o achado procedia e foi corrigido nesta rodada com `UtcDateTimeNormalizer.Normalize`.

2. O create de check-in herda o risco de “persistiu mas respondeu `500`” por causa do despacho de eventos e notificações pós-commit.
   Status: `parcial`
   Leitura revisada: o `500` pode acontecer, mas o recurso não fica persistido se o handler falhar.

3. A edição e exclusão se apoiam em permissão de escrita sobre o indicador; as mensagens de “apenas o autor” existem no catálogo, mas não aparecem implementadas no fluxo avaliado.
   Status: `desatualizado`
   Leitura revisada: o achado procedia na versão anterior, mas foi corrigido nesta rodada; `patch` e `delete` agora exigem identidade do colaborador autenticado e bloqueiam alteração por não autor com mensagem funcional explícita.

### Employee

1. `TeamId` entra no contrato e no comando de criação, mas não é aplicado no caso de uso de create.
   Status: `refutado`
   Leitura revisada: o caso de uso aplica `TeamId` como time primário e também cria vínculo em `EmployeeTeams`.

2. O controller documenta `404` por organização ou time não encontrado, mas o create não busca time nem organização por identificador explícito.
   Status: `desatualizado`
   Leitura revisada: o endpoint atual documenta `404` apenas para time, e o create valida a existência do time dentro da organização corrente.

3. A validação de `LeaderId` no patch perde o contexto de organização no validator e só é reforçada depois no use case.
   Status: `desatualizado`
   Leitura revisada: o achado procedia na versão anterior, mas foi corrigido nesta rodada; o validator passou a usar o tenant atual para validar `LeaderId` já na borda, reduzindo o drift em relação ao caso de uso.

### Task

1. O `201 Created` do create aponta `Location` para a action de `PATCH`, não para um `GET` canônico do recurso.
   Status: `refutado`
   Leitura revisada: o endpoint já usa `CreatedAtAction(nameof(GetById), ...)`.

2. Não há side effects adicionais no create.
   Status: `confirmado`

3. O domínio de tarefa está simples e consistente com o contrato atual.
   Status: `confirmado`

### Team

1. A proteção contra exclusão de time com metas associadas está implementada no caso de uso, mas anulada por um stub no repositório que sempre retorna `false`.
   Status: `confirmado`
   Leitura revisada: o problema procedia; nesta rodada a checagem foi implementada com base em colaboradores do time.

2. A semântica de “membros do time” diverge entre endpoints.
   Status: `desatualizado`
   Leitura revisada: o achado procedia parcialmente na versão anterior, mas foi resolvido nesta rodada; os fluxos críticos passaram a usar `EmployeeTeams` como fonte funcional de pertencimento.

3. O create sincroniza o líder em `EmployeeTeams`, mas isso não garante consistência automática com endpoints que dependem de `TeamId`.
   Status: `desatualizado`
   Leitura revisada: o risco funcional foi removido nesta rodada ao deixar `EmployeeTeams` como fonte de verdade nos fluxos críticos; `TeamId` deixou de orientar comportamento de negócio nessas áreas.

## Achado adicional identificado durante a implementação

### Checkin

1. `PatchCheckin` atualizava entidades carregadas sem rastreamento e podia responder `200` sem garantir persistência real.
   Status: `confirmado`
   Tratamento: corrigido nesta rodada com carregamento rastreado do `Checkin` no fluxo de update e reforço de teste de integração.

## Resumo executivo revisado

### Corrigidos nesta rodada

- conflito explícito por domínio duplicado em `organization`;
- normalização real para UTC em `checkin`;
- persistência real do `PatchCheckin`;
- autoria de `checkin` em patch e delete;
- validação contextual de `LeaderId` no patch de `employee`;
- bloqueio de exclusão de `team` quando houver metas associadas a membros do time;
- consolidação funcional de pertencimento a time via `EmployeeTeams`;
- reforço de testes de unidade, repositório, contrato e integração.

### Mantidos como decisão pendente


### Itens removidos do diagnóstico técnico como estado atual

- `organization create` documentando `404`;
- `employee create` ignorando `TeamId`;
- `task create` apontando `Location` para `PATCH`;
- `mission create` deixando `GlobalAdmin` com tenant inválido escapar para infra.
