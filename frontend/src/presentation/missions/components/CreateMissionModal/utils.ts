import { Indicator } from "@/types";
import { CalendarDate } from "@getbud-co/buds";

/** Converts a CalendarDate to an ISO 8601 date string (YYYY-MM-DD), or null if no date is provided. */
export function toIsoDate(date: CalendarDate | null): string | null {
  if (!date) return null;
  return `${date.year}-${String(date.month).padStart(2, "0")}-${String(date.day).padStart(2, "0")}`;
}

export function unitFromValue(unit: string): Indicator["unit"] {
  if (unit === "%") return "percent";
  if (unit === "R$" || unit === "US$") return "currency";
  if (!unit || unit === "un") return "count";
  return "custom";
}
