import { TemplateConfig } from "@/types";
import { TEMPLATE_CONFIGS } from "./consts";
import { CalendarDate } from "@/lib/tempStorage/date-format";
import {
  Crosshair,
  GitBranch,
  ListChecks,
  Target,
  User,
  Users,
} from "lucide-react";
import {
  CalendarBlank,
  FunnelSimple,
  ListBullets,
  UsersThree,
} from "@phosphor-icons/react";

export function getTemplateConfig(
  template: string | undefined,
): TemplateConfig {
  const key = template ?? "scratch";
  return (
    TEMPLATE_CONFIGS[key] ?? (TEMPLATE_CONFIGS["scratch"] as TemplateConfig)
  );
}

export function generateItemId(): string {
  return `item-${Date.now()}-${Math.random().toString(36).slice(2, 8)}`;
}

/**
 * Parse a key result string to extract manualType, goalValue and goalUnit.
 * E.g. "Reduzir churn mensal de 5% para 2,5%" → { manualType: "reduce", goalValue: "2.5", goalUnit: "%" }
 *      "NPS ≥ 70"                             → { manualType: "above", goalValue: "70", goalUnit: "NPS" }
 */
export function parseKeyResultGoal(kr: string): {
  manualType: string;
  goalValue: string;
  goalUnit: string;
} {
  const defaults = { manualType: "reach", goalValue: "", goalUnit: "" };

  // Detect manualType from keywords
  let manualType = "reach";
  if (/reduzir|diminuir/i.test(kr)) manualType = "reduce";
  else if (/manter acima|≥|>=|mínimo/i.test(kr)) manualType = "above";
  else if (/manter abaixo|≤|<=|máximo|< \d/i.test(kr)) manualType = "below";
  else if (/zero /i.test(kr)) manualType = "below";

  // Try to find numeric target — prefer the last number in "para X" or "para < X" patterns
  const paraMatch = kr.match(/para\s*<?[≤≥]?\s*([\d.,]+)/i);
  const gteMatch = kr.match(/[≥>=]\s*([\d.,]+)/);
  const lteMatch = kr.match(/[≤<=<]\s*([\d.,]+)/);
  const percentMatch = kr.match(/([\d.,]+)\s*%/);
  const plainNumMatch = kr.match(/\b([\d.,]+)\b/);

  let rawValue = "";
  if (paraMatch) rawValue = paraMatch[1] ?? "";
  else if (gteMatch) {
    rawValue = gteMatch[1] ?? "";
    manualType = "above";
  } else if (lteMatch) {
    rawValue = lteMatch[1] ?? "";
    manualType = "below";
  } else if (percentMatch) rawValue = percentMatch[1] ?? "";
  else if (plainNumMatch) rawValue = plainNumMatch[1] ?? "";

  if (/^zero\b/i.test(kr)) rawValue = "0";

  // Normalize comma decimal
  const goalValue = rawValue.replace(",", ".");

  // Detect unit
  let goalUnit = "";
  if (/%/.test(kr)) goalUnit = "%";
  else if (/R\$/.test(kr)) goalUnit = "R$";
  else if (/US\$/.test(kr)) goalUnit = "US$";
  else if (/\bNPS\b/i.test(kr)) goalUnit = "NPS";
  else if (/\bdias?\b/i.test(kr)) goalUnit = "dias";
  else if (/\bhoras?\b/i.test(kr)) goalUnit = "hrs";
  else if (/\bminutos?\b|\bmin\b/i.test(kr)) goalUnit = "min";
  else if (/\bpontos?\b|\bpts\b/i.test(kr)) goalUnit = "pts";
  else if (/\bpessoas?\b|\bcolaborador/i.test(kr)) goalUnit = "pessoas";
  else if (/\bnota\b/i.test(kr)) goalUnit = "nota-10";
  else if (goalValue) goalUnit = "un";

  if (!goalValue) return defaults;
  return { manualType, goalValue, goalUnit };
}

export function splitFullName(label: string): {
  firstName: string;
  lastName: string;
} {
  const [firstName = "", ...rest] = label.trim().split(" ");
  return {
    firstName,
    lastName: rest.join(" "),
  };
}

export function isoToCalendarDate(iso: string): CalendarDate {
  const [year = 0, month = 1, day = 1] = iso.split("-").map(Number);
  return { year, month, day };
}

export const filterChipIcons: Record<string, typeof Users | undefined> = {
  team: Users,
  period: CalendarBlank,
  status: FunnelSimple,
  owner: User,
  itemType: ListBullets,
  indicatorType: Crosshair,
  contribution: GitBranch,
  supporter: UsersThree,
  taskState: ListChecks,
  missionStatus: Target,
};
