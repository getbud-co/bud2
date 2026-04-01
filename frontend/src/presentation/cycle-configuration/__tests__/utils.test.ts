import { describe, it, expect, vi } from "vitest";
import { isoToCalendarDate, calendarDateToIso, formatDateDisplay } from "../utils";

vi.mock("@/lib/tempStorage/date-format", () => ({
  formatDateBR: (iso: string | null | undefined) => {
    if (!iso) return "";
    const parts = iso.split("-");
    if (parts.length !== 3 || parts.some((p) => isNaN(Number(p)))) return "";
    const [year, month, day] = parts;
    return `${day}/${month}/${year}`;
  },
}));

// ─── isoToCalendarDate ───

describe("isoToCalendarDate()", () => {
  it("converts YYYY-MM-DD to CalendarDate object", () => {
    expect(isoToCalendarDate("2025-03-31")).toEqual({ year: 2025, month: 3, day: 31 });
  });

  it("handles single-digit month and day", () => {
    expect(isoToCalendarDate("2025-01-05")).toEqual({ year: 2025, month: 1, day: 5 });
  });

  it("handles year boundary dates", () => {
    expect(isoToCalendarDate("2025-12-31")).toEqual({ year: 2025, month: 12, day: 31 });
    expect(isoToCalendarDate("2025-01-01")).toEqual({ year: 2025, month: 1, day: 1 });
  });

  it("returns defaults for missing month/day segments", () => {
    // Only year provided — month and day fall back to defaults
    expect(isoToCalendarDate("2025")).toEqual({ year: 2025, month: 1, day: 1 });
  });
});

// ─── calendarDateToIso ───

describe("calendarDateToIso()", () => {
  it("converts CalendarDate to ISO string", () => {
    expect(calendarDateToIso({ year: 2025, month: 3, day: 31 })).toBe("2025-03-31");
  });

  it("pads single-digit month and day with leading zero", () => {
    expect(calendarDateToIso({ year: 2025, month: 1, day: 5 })).toBe("2025-01-05");
  });

  it("does not pad month or day when already two digits", () => {
    expect(calendarDateToIso({ year: 2025, month: 12, day: 25 })).toBe("2025-12-25");
  });

  it("is the inverse of isoToCalendarDate", () => {
    const iso = "2025-07-15";
    expect(calendarDateToIso(isoToCalendarDate(iso))).toBe(iso);
  });
});

// ─── formatDateDisplay ───

describe("formatDateDisplay()", () => {
  it("formats ISO date string to DD/MM/YYYY", () => {
    expect(formatDateDisplay("2025-03-31")).toBe("31/03/2025");
  });

  it("returns '-' for invalid input", () => {
    expect(formatDateDisplay("invalid")).toBe("-");
  });

  it("returns '-' for empty string", () => {
    expect(formatDateDisplay("")).toBe("-");
  });
});
