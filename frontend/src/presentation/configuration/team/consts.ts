import type { TeamColor, TeamMember } from "@/types";

export const COLOR_OPTIONS: { value: TeamColor; label: string }[] = [
  { value: "neutral", label: "Cinza" },
  { value: "orange", label: "Laranja" },
  { value: "wine", label: "Vinho" },
  { value: "caramel", label: "Caramelo" },
  { value: "success", label: "Verde" },
  { value: "warning", label: "Amarelo" },
  { value: "error", label: "Vermelho" },
];

export const ROLE_OPTIONS: { id: TeamMember["roleInTeam"]; label: string }[] = [
  { id: "leader", label: "Líder" },
  { id: "member", label: "Membro" },
  { id: "observer", label: "Observador" },
];

export const ROLE_BADGE_COLOR: Record<
  TeamMember["roleInTeam"],
  "wine" | "caramel" | "neutral"
> = {
  leader: "wine",
  member: "caramel",
  observer: "neutral",
};

export const ROLE_LABEL: Record<TeamMember["roleInTeam"], string> = {
  leader: "Líder",
  member: "Membro",
  observer: "Observador",
};

export const SEARCH_FILTERS = [
  { id: "membership", label: "Situação" },
  { id: "role", label: "Papel" },
  { id: "cargo", label: "Cargo" },
  { id: "time", label: "Time" },
];

export const MEMBERS_FILTERS = [{ id: "role", label: "Papel" }];

export const MEMBERSHIP_OPTIONS = [
  { id: "all", label: "Todos" },
  { id: "out", label: "Fora deste time" },
  { id: "in", label: "Neste time" },
] as const;

export const STATUS_FILTER = [
  { id: "all", label: "Todos" },
  { id: "active", label: "Ativo" },
  { id: "archived", label: "Arquivado" },
];

export const STATUS_BADGE: Record<
  string,
  { label: string; color: "success" | "neutral" }
> = {
  active: { label: "Ativo", color: "success" },
  archived: { label: "Arquivado", color: "neutral" },
};

export const FILTER_OPTIONS = [{ id: "status", label: "Status" }];

export const filterDropdownBodyCls =
  "flex flex-col min-w-[180px] py-[var(--sp-3xs)] max-h-80 overflow-y-auto";

export function filterActionItemCls(active: boolean) {
  return `flex items-center gap-[var(--sp-2xs)] px-[var(--sp-sm)] py-[var(--sp-2xs)] border-0 bg-transparent font-[var(--font-body)] text-[var(--text-sm)] cursor-pointer text-left w-full transition-[background-color] duration-100 hover:bg-[var(--color-caramel-50)] ${active ? "bg-[var(--color-orange-50)] text-[var(--color-orange-700)]" : "text-[var(--color-neutral-700)]"}`;
}
