import type { CycleCadence, CycleStatus } from "@/types";

export const TYPE_OPTIONS: { value: CycleCadence; label: string }[] = [
  { value: "Quarterly", label: "Trimestral" },
  { value: "SemiAnnual", label: "Semestral" },
  { value: "Annual", label: "Anual" },
  { value: "Custom", label: "Personalizado" },
];

export const STATUS_BADGE: Record<
  CycleStatus,
  { label: string; color: "success" | "orange" | "neutral" }
> = {
  Active: { label: "Ativo", color: "success" },
  Planning: { label: "Futuro", color: "orange" },
  Ended: { label: "Encerrado", color: "neutral" },
  Review: { label: "Em revisão", color: "orange" },
  Archived: { label: "Arquivado", color: "neutral" },
};
