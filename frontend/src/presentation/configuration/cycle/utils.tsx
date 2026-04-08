import { formatDateBR } from "@/lib/tempStorage/date-format";
import type { CalendarDate } from "@mdonangelo/bud-ds";

/** Converts an ISO date string (YYYY-MM-DD) to a CalendarDate for the DatePicker */
export function isoToCalendarDate(iso: string): CalendarDate {
  const [y = 0, m = 1, d = 1] = iso.split("-").map(Number);
  return { year: y, month: m, day: d };
}

/** Converts a CalendarDate to an ISO date string (YYYY-MM-DD) */
export function calendarDateToIso(cd: CalendarDate): string {
  return `${cd.year}-${String(cd.month).padStart(2, "0")}-${String(cd.day).padStart(2, "0")}`;
}

/** Formats an ISO date string as DD/MM/YYYY for display */
export function formatDateDisplay(iso: string): string {
  return formatDateBR(iso) || "-";
}
