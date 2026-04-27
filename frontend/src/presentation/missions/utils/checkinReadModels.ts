import type { CheckIn, ConfidenceLevel } from "@/types";
import { numVal } from "@/lib/missions";
import { formatDateShort } from "@/lib/date-format";

export interface CheckInChartPoint {
  date: string;
  value: number | null;
  /** Value of the previous point in this chart's ordered sequence (not historical previousValue) */
  visualPreviousValue?: number | null;
  createdAt?: string;
  authorName?: string;
  authorInitials?: string;
  confidence?: ConfidenceLevel | null;
  note?: string | null;
  isForecast?: boolean;
}

export function sortCheckInsDesc(checkIns: CheckIn[]): CheckIn[] {
  return [...checkIns].sort(
    (a, b) => new Date(b.createdAt).getTime() - new Date(a.createdAt).getTime(),
  );
}

export function buildCheckInChartData(checkIns: CheckIn[]): CheckInChartPoint[] {
  const ordered = [...checkIns].sort(
    (a, b) => new Date(a.createdAt).getTime() - new Date(b.createdAt).getTime(),
  );

  return ordered.map((entry, idx) => {
    const author = entry.author;
    const authorName = author ? `${author.firstName} ${author.lastName}` : undefined;
    const authorInitials = author
      ? (author.initials ?? `${author.firstName[0]}${author.lastName[0]}`.toUpperCase())
      : undefined;
    const prevEntry = idx > 0 ? ordered[idx - 1] : null;
    const visualPrev = prevEntry ? numVal(prevEntry.value) : null;
    return {
      date: formatDateShort(entry.createdAt),
      value: numVal(entry.value),
      createdAt: entry.createdAt,
      authorName,
      authorInitials,
      confidence: entry.confidence,
      visualPreviousValue: visualPrev,
      note: entry.note,
    };
  });
}
