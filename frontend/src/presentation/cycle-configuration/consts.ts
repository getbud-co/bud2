import { CycleStatus, CycleType } from "@/types";

export const TYPE_OPTIONS = [
  { value: "quarterly" satisfies CycleType, label: "Trimestral" },
  { value: "semi_annual" satisfies CycleType, label: "Semestral" },
  { value: "annual" satisfies CycleType, label: "Anual" },
  { value: "custom" satisfies CycleType, label: "Personalizado" },
];

export const STATUS_BADGE: Partial<
  Record<
    CycleStatus,
    { label: string; color: "success" | "orange" | "neutral" }
  >
> = {
  active: { label: "Ativo", color: "success" },
  planning: { label: "Futuro", color: "orange" },
  ended: { label: "Encerrado", color: "neutral" },
  review: { label: "Em revisão", color: "orange" },
  archived: { label: "Arquivado", color: "neutral" },
};
