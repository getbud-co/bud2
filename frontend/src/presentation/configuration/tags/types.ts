import type { Tag } from "@/types";

export interface TagView extends Tag {
  linkedItems: number;
}

export const COLOR_OPTIONS = [
  { value: "neutral", label: "Cinza" },
  { value: "orange", label: "Laranja" },
  { value: "wine", label: "Vinho" },
  { value: "caramel", label: "Caramelo" },
  { value: "success", label: "Verde" },
  { value: "warning", label: "Amarelo" },
  { value: "error", label: "Vermelho" },
] as const;
