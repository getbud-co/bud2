import { TemplateConfig } from "@/types";
import { Mission } from "@/types/mission/mission";
import { TEMPLATE_CONFIGS } from "../consts";
import { CalendarDate } from "@getbud-co/buds";

export function getTemplateConfig(
  template: string | undefined,
): TemplateConfig {
  const key = template ?? "scratch";
  return (
    TEMPLATE_CONFIGS[key] ?? (TEMPLATE_CONFIGS["scratch"] as TemplateConfig)
  );
}

export function isoToCalendarDate(iso: string): CalendarDate {
  const [year = 0, month = 1, day = 1] = iso.split("-").map(Number);
  return { year, month, day };
}

export function collectMissionIds(mission: Mission): string[] {
  const ids = [mission.id];
  for (const child of mission.children ?? []) {
    ids.push(...collectMissionIds(child));
  }
  return ids;
}
