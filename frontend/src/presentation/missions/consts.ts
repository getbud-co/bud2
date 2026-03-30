import { ConfidenceLevel, KanbanStatus, TemplateConfig } from "@/types";
import { BreadcrumbItem, FilterOption, MissionItem } from "@mdonangelo/bud-ds";
import {
  ArrowsInLineHorizontal,
  CalendarBlank,
  ChartBar,
  ChartLineUp,
  Crosshair,
  Eye,
  FunnelSimple,
  Gauge,
  GitBranch,
  ListBullets,
  ListChecks,
  Tag,
  Target,
  TrendDown,
  UploadSimple,
  User,
  Users,
  UsersThree,
} from "@phosphor-icons/react";

export const FILTER_OPTIONS: FilterOption[] = [
  { id: "team", label: "Time", icon: Users },
  { id: "period", label: "Período", icon: CalendarBlank },
  { id: "status", label: "Status", icon: FunnelSimple },
  { id: "owner", label: "Responsável", icon: User },
  { id: "itemType", label: "Tipo", icon: ListBullets },
  { id: "indicatorType", label: "Indicador", icon: Crosshair },
  { id: "contribution", label: "Contribuição", icon: GitBranch },
  { id: "supporter", label: "Apoio", icon: UsersThree },
  { id: "taskState", label: "Tarefa", icon: ListChecks },
  { id: "missionStatus", label: "Missão", icon: Target },
];

export const STATUS_OPTIONS = [
  { id: "all", label: "Todos" },
  { id: "on-track", label: "Dentro do previsto" },
  { id: "attention", label: "Atenção" },
  { id: "off-track", label: "Atrasado" },
  { id: "completed", label: "Concluído" },
];

export const ITEM_TYPE_OPTIONS = [
  { id: "all", label: "Todos os itens" },
  { id: "mission", label: "Missão" },
  { id: "indicator", label: "Indicador" },
  { id: "task", label: "Tarefa" },
];

export const INDICATOR_TYPE_OPTIONS = [
  { id: "all", label: "Todos os tipos" },
  { id: "reach", label: "Atingir" },
  { id: "above", label: "Manter acima" },
  { id: "below", label: "Manter abaixo" },
  { id: "between", label: "Manter entre" },
  { id: "reduce", label: "Reduzir" },
  { id: "survey", label: "Pesquisa" },
  { id: "external", label: "Fonte externa" },
  { id: "linked_mission", label: "Missão vinculada" },
];

export const CONTRIBUTION_OPTIONS = [
  { id: "all", label: "Todas" },
  { id: "contributing", label: "Contribuindo para outras" },
  { id: "receiving", label: "Recebendo contribuição" },
  { id: "none", label: "Sem contribuição" },
];

export const TASK_STATE_OPTIONS = [
  { id: "all", label: "Todas" },
  { id: "pending", label: "Pendentes" },
  { id: "done", label: "Concluídas" },
];

export const MISSION_STATUS_OPTIONS = [
  { id: "all", label: "Todos" },
  { id: "draft", label: "Rascunho" },
  { id: "active", label: "Ativa" },
  { id: "paused", label: "Pausada" },
  { id: "completed", label: "Concluída" },
  { id: "cancelled", label: "Cancelada" },
];

/* ——— Mission templates ——— */

export const MISSION_TEMPLATES = [
  { value: "okr", title: "OKR", description: "Objetivo + Key Results" },
  {
    value: "pdi",
    title: "PDI",
    description: "Plano de Desenvolvimento Individual",
  },
  {
    value: "bsc",
    title: "BSC",
    description: "Balanced Scorecard — 4 perspectivas",
  },
  {
    value: "kpi",
    title: "KPI",
    description: "Indicador chave com alvo e frequência",
  },
  {
    value: "meta",
    title: "Meta simples",
    description: "Título, descrição, alvo e prazo",
  },
  {
    value: "scratch",
    title: "Criar do zero",
    description: "Monte sua estrutura com campos livres",
  },
];

export const TEMPLATE_CONFIGS: Record<string, TemplateConfig> = {
  okr: {
    stepTitle: "Definir objetivo",
    namePlaceholder: "Nome do objetivo",
    descPlaceholder: "Descrição do objetivo",
    addItemLabel: "Adicionar resultado-chave (KR)",
    addItemFormTitle: "Adicionar resultado-chave",
    editItemFormTitle: "Editar resultado-chave",
    itemTitlePlaceholder: "Título do resultado-chave",
    itemDescPlaceholder: "Descrição",
    allowedModes: ["manual", "task", "external"],
  },
  pdi: {
    stepTitle: "Montar plano de desenvolvimento",
    namePlaceholder: "Nome do PDI",
    descPlaceholder: "Descreva o foco de desenvolvimento",
    addItemLabel: "Adicionar ação de desenvolvimento",
    addItemFormTitle: "Adicionar ação de desenvolvimento",
    editItemFormTitle: "Editar ação de desenvolvimento",
    itemTitlePlaceholder: "Título da ação",
    itemDescPlaceholder: "Descrição",
    allowedModes: ["task", "manual"],
  },
  bsc: {
    stepTitle: "Objetivo estratégico BSC",
    namePlaceholder: "Nome do BSC",
    descPlaceholder: "Descreva onde deseja chegar com o objetivo",
    addItemLabel: "Adicionar indicador",
    addItemFormTitle: "Adicionar indicador",
    editItemFormTitle: "Editar indicador",
    itemTitlePlaceholder: "Título do indicador",
    itemDescPlaceholder: "Descrição",
    allowedModes: ["mission", "manual", "task", "external"],
  },
  kpi: {
    stepTitle: "Definir KPI",
    namePlaceholder: "Nome do KPI",
    descPlaceholder: "Descrição do indicador",
    addItemLabel: "Adicionar meta do KPI",
    addItemFormTitle: "Adicionar meta",
    editItemFormTitle: "Editar meta",
    itemTitlePlaceholder: "Título da meta",
    itemDescPlaceholder: "Descrição",
    allowedModes: ["manual", "external"],
  },
  meta: {
    stepTitle: "Definir meta",
    namePlaceholder: "Nome da meta",
    descPlaceholder: "Descrição da meta",
    addItemLabel: "Adicionar sub-meta",
    addItemFormTitle: "Adicionar sub-meta",
    editItemFormTitle: "Editar sub-meta",
    itemTitlePlaceholder: "Título da sub-meta",
    itemDescPlaceholder: "Descrição",
    allowedModes: ["manual", "task", "external"],
  },
  scratch: {
    stepTitle: "Construir missão",
    namePlaceholder: "Nome da missão",
    descPlaceholder: "Descrição",
    addItemLabel: "Adicionar item",
    addItemFormTitle: "Adicionar item",
    editItemFormTitle: "Editar item",
    itemTitlePlaceholder: "Título",
    itemDescPlaceholder: "Descrição",
    allowedModes: null,
  },
};

export const CREATE_STEPS: BreadcrumbItem[] = [
  { label: "Escolher template" },
  { label: "Construir missão" },
  { label: "Revisão" },
];

export const MORE_MISSION_OPTIONS = [
  { id: "team-support", label: "Time de apoio", icon: Users },
  { id: "organizers", label: "Tags", icon: Tag },
  { id: "visibility", label: "Quem pode ver", icon: Eye },
];

// visibilityOptions is now generated dynamically inside MissionsPage using teamOptions

export const ASSISTANT_MISSIONS: MissionItem[] = [
  { id: "okr-1", label: "Reduzir churn do produto de Crédito Imobiliário" },
  { id: "okr-2", label: "Aumentar NPS do onboarding para 75+" },
  { id: "okr-3", label: "Lançar feature Y até final do Q1" },
];

/* ——— Measurement mode options ——— */

export const MEASUREMENT_MODES = [
  {
    id: "mission",
    label: "Nova missão",
    description: "Missão com indicadores dentro",
    icon: Target,
  },
  {
    id: "task",
    label: "Tarefa",
    description: "Pendente, em andamento e concluída",
    icon: ListChecks,
  },
  {
    id: "manual",
    label: "Indicador manual",
    description: "Defina meta e acompanhe manualmente",
    icon: Gauge,
  },
  {
    id: "external",
    label: "Importar de fonte externa",
    description: "Google Sheets, Power BI, etc.",
    icon: UploadSimple,
  },
];

export const MANUAL_INDICATOR_TYPES = [
  { id: "reach", label: "Atingir", icon: Crosshair },
  { id: "above", label: "Manter acima", icon: ChartLineUp },
  { id: "below", label: "Manter abaixo", icon: TrendDown },
  { id: "between", label: "Manter entre", icon: ArrowsInLineHorizontal },
  { id: "reduce", label: "Reduzir", icon: TrendDown },
  { id: "survey", label: "De uma pesquisa", icon: ChartBar },
];

export const UNIT_OPTIONS = [
  { value: "%", label: "% (Percentual)" },
  { value: "R$", label: "R$ (Reais)" },
  { value: "US$", label: "US$ (Dólar)" },
  { value: "un", label: "Unidades" },
  { value: "pessoas", label: "Pessoas" },
  { value: "pts", label: "Pontos" },
  { value: "NPS", label: "NPS" },
  { value: "nota-10", label: "Nota (1-10)" },
  { value: "nota-5", label: "Nota (1-5)" },
  { value: "dias", label: "Dias" },
  { value: "hrs", label: "Horas" },
  { value: "min", label: "Minutos" },
  { value: "bool", label: "Sim/Não" },
];

export const KANBAN_COLUMNS: {
  id: KanbanStatus;
  label: string;
  color: string;
}[] = [
  {
    id: "uncategorized",
    label: "Não categorizado",
    color: "var(--color-neutral-400)",
  },
  { id: "todo", label: "Para fazer", color: "var(--color-caramel-400)" },
  { id: "doing", label: "Fazendo", color: "var(--color-orange-500)" },
  { id: "done", label: "Feito", color: "var(--color-green-500)" },
];

/* (Mock data and types imported from @/types and @/lib/missions) */

export const CONFIDENCE_OPTIONS: {
  id: ConfidenceLevel;
  label: string;
  description: string;
  color: string;
}[] = [
  {
    id: "high",
    label: "Alta confiança",
    description: "Se tudo continuar assim, esperamos alcançar o resultado",
    color: "var(--color-green-500)",
  },
  {
    id: "medium",
    label: "Média confiança",
    description:
      "Existe um risco de não alcançarmos o resultado-chave, mas seguimos otimistas",
    color: "var(--color-yellow-500)",
  },
  {
    id: "low",
    label: "Baixa confiança",
    description:
      "Não vamos alcançar o resultado a não ser que a gente mude nossa abordagem",
    color: "var(--color-red-500)",
  },
  {
    id: "barrier",
    label: "Com barreira",
    description:
      "Existe um fator externo impedindo o progresso desse resultado-chave",
    color: "var(--color-wine-500)",
  },
  {
    id: "deprioritized",
    label: "Despriorizado",
    description:
      "Este resultado-chave foi despriorizado e deixado de lado por enquanto",
    color: "var(--color-neutral-400)",
  },
];

export const DRAWER_TASKS_BY_INDICATOR: Record<
  string,
  { id: string; title: string; isDone: boolean }[]
> = {
  i1: [
    {
      id: "t1",
      title: "Revisar contratos pendentes com jurídico",
      isDone: true,
    },
    { id: "t2", title: "Agendar reunião com time comercial", isDone: true },
    { id: "t3", title: "Preparar relatório de pipeline Q1", isDone: false },
  ],
  i5: [
    {
      id: "t4",
      title: "Definir escopo do módulo de pesquisas v2",
      isDone: true,
    },
    { id: "t5", title: "Criar protótipos de alta fidelidade", isDone: false },
    { id: "t6", title: "Validar fluxo com 3 clientes beta", isDone: false },
  ],
};
