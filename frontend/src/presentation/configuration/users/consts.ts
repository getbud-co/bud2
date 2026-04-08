import { Gender, UserStatus } from "@/types";

export const LANGUAGE_OPTIONS = [
  { value: "pt-br", label: "Português (Brasil)" },
  { value: "en", label: "English" },
  { value: "es", label: "Español" },
];

export const GENDER_OPTIONS: { value: Gender; label: string }[] = [
  { value: "feminino", label: "Feminino" },
  { value: "masculino", label: "Masculino" },
  { value: "nao-binario", label: "Não-binário" },
  { value: "prefiro-nao-dizer", label: "Prefiro não dizer" },
];

export const STATUS_FILTER: { id: string; label: string }[] = [
  { id: "all", label: "Todos" },
  { id: "active", label: "Ativo" },
  { id: "inactive", label: "Inativo" },
  { id: "invited", label: "Convidado" },
  { id: "suspended", label: "Suspenso" },
];

export const STATUS_BADGE: Record<
  UserStatus,
  { label: string; color: "success" | "neutral" | "warning" | "error" }
> = {
  active: { label: "Ativo", color: "success" },
  inactive: { label: "Inativo", color: "neutral" },
  invited: { label: "Convidado", color: "warning" },
  suspended: { label: "Suspenso", color: "error" },
};

/** Fallback role slug when no default role is configured */
export const DEFAULT_ROLE_SLUG = "colaborador";
