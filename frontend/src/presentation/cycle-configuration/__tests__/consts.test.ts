import { describe, it, expect } from "vitest";
import type { CycleType, CycleStatus } from "@/types";
import { TYPE_OPTIONS, STATUS_BADGE } from "../consts";

const ALL_TYPES: CycleType[] = ["quarterly", "semi_annual", "annual", "custom"];
const ALL_STATUSES: CycleStatus[] = ["active", "planning", "ended", "review", "archived"];
const VALID_COLORS = ["success", "orange", "neutral"] as const;

// ─── TYPE_OPTIONS ───

describe("TYPE_OPTIONS", () => {
  it("covers all CycleType values", () => {
    const values = TYPE_OPTIONS.map((o) => o.value);
    ALL_TYPES.forEach((type) => expect(values).toContain(type));
  });

  it("has no duplicate values", () => {
    const values = TYPE_OPTIONS.map((o) => o.value);
    expect(new Set(values).size).toBe(values.length);
  });

  it("each entry has a non-empty label", () => {
    TYPE_OPTIONS.forEach(({ label }) => expect(label.trim()).not.toBe(""));
  });
});

// ─── STATUS_BADGE ───

describe("STATUS_BADGE", () => {
  it("covers all CycleStatus values", () => {
    ALL_STATUSES.forEach((status) => {
      expect(STATUS_BADGE).toHaveProperty(status);
    });
  });

  it("each entry has a valid color", () => {
    Object.values(STATUS_BADGE).forEach((badge) => {
      expect(VALID_COLORS as readonly string[]).toContain(badge?.color);
    });
  });

  it("each entry has a non-empty label", () => {
    Object.values(STATUS_BADGE).forEach((badge) => {
      expect(badge?.label.trim()).not.toBe("");
    });
  });

  it("maps known statuses to expected labels", () => {
    expect(STATUS_BADGE.active?.label).toBe("Ativo");
    expect(STATUS_BADGE.ended?.label).toBe("Encerrado");
    expect(STATUS_BADGE.planning?.label).toBe("Futuro");
    expect(STATUS_BADGE.archived?.label).toBe("Arquivado");
    expect(STATUS_BADGE.review?.label).toBe("Em revisão");
  });
});
