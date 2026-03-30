/**
 * Tests for useFilterChips hook
 *
 * This hook manages filter chip state for FilterBar components.
 * It handles adding/removing filters, toggling dropdowns, and tracking active filters.
 */

import { describe, it, expect, vi, beforeEach } from "vitest";
import { renderHook, act } from "@testing-library/react";
import { useFilterChips } from "./useFilterChips";
import { createRef, type RefObject } from "react";

// ─── Test Helpers ───

const FILTER_OPTIONS = [
  { id: "status", label: "Status" },
  { id: "team", label: "Time" },
  { id: "owner", label: "Responsável" },
  { id: "period", label: "Período" },
  { id: "type", label: "Tipo" },
];

// ─── Tests ───

describe("useFilterChips", () => {
  beforeEach(() => {
    vi.useFakeTimers();
  });

  // ═══════════════════════════════════════════════════════════════════════════
  // Initial State
  // ═══════════════════════════════════════════════════════════════════════════

  describe("initial state", () => {
    it("starts with empty active filters", () => {
      const { result } = renderHook(() => useFilterChips());
      expect(result.current.activeFilters).toEqual([]);
    });

    it("starts with no open filter", () => {
      const { result } = renderHook(() => useFilterChips());
      expect(result.current.openFilter).toBeNull();
    });

    it("starts with undefined ignoreChipRefs when no refs provided", () => {
      const { result } = renderHook(() => useFilterChips());
      expect(result.current.ignoreChipRefs).toBeUndefined();
    });
  });

  // ═══════════════════════════════════════════════════════════════════════════
  // addFilter
  // ═══════════════════════════════════════════════════════════════════════════

  describe("addFilter", () => {
    it("adds a filter to active filters", () => {
      const { result } = renderHook(() => useFilterChips());

      act(() => {
        result.current.addFilter("status");
      });

      expect(result.current.activeFilters).toEqual(["status"]);
    });

    it("adds multiple filters", () => {
      const { result } = renderHook(() => useFilterChips());

      act(() => {
        result.current.addFilter("status");
        result.current.addFilter("team");
        result.current.addFilter("owner");
      });

      expect(result.current.activeFilters).toEqual(["status", "team", "owner"]);
    });

    it("does not duplicate filters", () => {
      const { result } = renderHook(() => useFilterChips());

      act(() => {
        result.current.addFilter("status");
        result.current.addFilter("status");
        result.current.addFilter("status");
      });

      expect(result.current.activeFilters).toEqual(["status"]);
    });

    it("does not open the filter dropdown", () => {
      const { result } = renderHook(() => useFilterChips());

      act(() => {
        result.current.addFilter("status");
      });

      expect(result.current.openFilter).toBeNull();
    });
  });

  // ═══════════════════════════════════════════════════════════════════════════
  // addFilterAndOpen
  // ═══════════════════════════════════════════════════════════════════════════

  describe("addFilterAndOpen", () => {
    it("adds filter and schedules dropdown open", async () => {
      const { result } = renderHook(() => useFilterChips());

      act(() => {
        result.current.addFilterAndOpen("status");
      });

      expect(result.current.activeFilters).toEqual(["status"]);

      // Initially null (waiting for double rAF)
      expect(result.current.openFilter).toBeNull();

      // Simulate double requestAnimationFrame
      await act(async () => {
        vi.advanceTimersByTime(32); // ~2 frames
      });

      // Now should be open
      expect(result.current.openFilter).toBe("status");
    });

    it("does not duplicate filter when adding and opening", async () => {
      const { result } = renderHook(() => useFilterChips());

      act(() => {
        result.current.addFilter("status");
      });

      act(() => {
        result.current.addFilterAndOpen("status");
      });

      expect(result.current.activeFilters).toEqual(["status"]);

      await act(async () => {
        vi.advanceTimersByTime(32);
      });

      expect(result.current.openFilter).toBe("status");
    });
  });

  // ═══════════════════════════════════════════════════════════════════════════
  // removeFilter
  // ═══════════════════════════════════════════════════════════════════════════

  describe("removeFilter", () => {
    it("removes a filter from active filters", () => {
      const { result } = renderHook(() => useFilterChips());

      act(() => {
        result.current.addFilter("status");
        result.current.addFilter("team");
      });

      act(() => {
        result.current.removeFilter("status");
      });

      expect(result.current.activeFilters).toEqual(["team"]);
    });

    it("closes dropdown if removed filter is open", () => {
      const { result } = renderHook(() => useFilterChips());

      act(() => {
        result.current.addFilter("status");
        result.current.setOpenFilter("status");
      });

      expect(result.current.openFilter).toBe("status");

      act(() => {
        result.current.removeFilter("status");
      });

      expect(result.current.openFilter).toBeNull();
    });

    it("does not affect other open filters", () => {
      const { result } = renderHook(() => useFilterChips());

      act(() => {
        result.current.addFilter("status");
        result.current.addFilter("team");
        result.current.setOpenFilter("team");
      });

      act(() => {
        result.current.removeFilter("status");
      });

      expect(result.current.openFilter).toBe("team");
    });

    it("calls onResetFilter callback when provided", () => {
      const onResetFilter = vi.fn();
      const { result } = renderHook(() => useFilterChips({ onResetFilter }));

      act(() => {
        result.current.addFilter("status");
      });

      act(() => {
        result.current.removeFilter("status");
      });

      expect(onResetFilter).toHaveBeenCalledWith("status");
      expect(onResetFilter).toHaveBeenCalledTimes(1);
    });

    it("handles removing non-existent filter gracefully", () => {
      const { result } = renderHook(() => useFilterChips());

      act(() => {
        result.current.addFilter("status");
      });

      act(() => {
        result.current.removeFilter("nonexistent");
      });

      expect(result.current.activeFilters).toEqual(["status"]);
    });
  });

  // ═══════════════════════════════════════════════════════════════════════════
  // clearAllFilters
  // ═══════════════════════════════════════════════════════════════════════════

  describe("clearAllFilters", () => {
    it("removes all active filters", () => {
      const { result } = renderHook(() => useFilterChips());

      act(() => {
        result.current.addFilter("status");
        result.current.addFilter("team");
        result.current.addFilter("owner");
      });

      act(() => {
        result.current.clearAllFilters();
      });

      expect(result.current.activeFilters).toEqual([]);
    });

    it("closes any open dropdown", () => {
      const { result } = renderHook(() => useFilterChips());

      act(() => {
        result.current.addFilter("status");
        result.current.setOpenFilter("status");
      });

      act(() => {
        result.current.clearAllFilters();
      });

      expect(result.current.openFilter).toBeNull();
    });

    it("calls onResetFilter for each filter", () => {
      const onResetFilter = vi.fn();
      const { result } = renderHook(() => useFilterChips({ onResetFilter }));

      act(() => {
        result.current.addFilter("status");
        result.current.addFilter("team");
        result.current.addFilter("owner");
      });

      act(() => {
        result.current.clearAllFilters();
      });

      expect(onResetFilter).toHaveBeenCalledTimes(3);
      expect(onResetFilter).toHaveBeenCalledWith("status");
      expect(onResetFilter).toHaveBeenCalledWith("team");
      expect(onResetFilter).toHaveBeenCalledWith("owner");
    });

    it("handles empty filters gracefully", () => {
      const onResetFilter = vi.fn();
      const { result } = renderHook(() => useFilterChips({ onResetFilter }));

      act(() => {
        result.current.clearAllFilters();
      });

      expect(result.current.activeFilters).toEqual([]);
      expect(onResetFilter).not.toHaveBeenCalled();
    });
  });

  // ═══════════════════════════════════════════════════════════════════════════
  // toggleFilterDropdown
  // ═══════════════════════════════════════════════════════════════════════════

  describe("toggleFilterDropdown", () => {
    it("opens a closed dropdown", () => {
      const { result } = renderHook(() => useFilterChips());

      act(() => {
        result.current.toggleFilterDropdown("status");
      });

      expect(result.current.openFilter).toBe("status");
    });

    it("closes an open dropdown", () => {
      const { result } = renderHook(() => useFilterChips());

      act(() => {
        result.current.setOpenFilter("status");
      });

      act(() => {
        result.current.toggleFilterDropdown("status");
      });

      expect(result.current.openFilter).toBeNull();
    });

    it("switches to different dropdown", () => {
      const { result } = renderHook(() => useFilterChips());

      act(() => {
        result.current.setOpenFilter("status");
      });

      act(() => {
        result.current.toggleFilterDropdown("team");
      });

      expect(result.current.openFilter).toBe("team");
    });
  });

  // ═══════════════════════════════════════════════════════════════════════════
  // getAvailableFilters
  // ═══════════════════════════════════════════════════════════════════════════

  describe("getAvailableFilters", () => {
    it("returns all filters when none are active", () => {
      const { result } = renderHook(() => useFilterChips());

      const available = result.current.getAvailableFilters(FILTER_OPTIONS);

      expect(available).toEqual(FILTER_OPTIONS);
    });

    it("excludes active filters", () => {
      const { result } = renderHook(() => useFilterChips());

      act(() => {
        result.current.addFilter("status");
        result.current.addFilter("team");
      });

      const available = result.current.getAvailableFilters(FILTER_OPTIONS);

      expect(available).toEqual([
        { id: "owner", label: "Responsável" },
        { id: "period", label: "Período" },
        { id: "type", label: "Tipo" },
      ]);
    });

    it("returns empty array when all filters are active", () => {
      const { result } = renderHook(() => useFilterChips());

      act(() => {
        FILTER_OPTIONS.forEach((filter) => {
          result.current.addFilter(filter.id);
        });
      });

      const available = result.current.getAvailableFilters(FILTER_OPTIONS);

      expect(available).toEqual([]);
    });
  });

  // ═══════════════════════════════════════════════════════════════════════════
  // setActiveFilters (direct state setter)
  // ═══════════════════════════════════════════════════════════════════════════

  describe("setActiveFilters", () => {
    it("allows direct state setting", () => {
      const { result } = renderHook(() => useFilterChips());

      act(() => {
        result.current.setActiveFilters(["status", "team", "owner"]);
      });

      expect(result.current.activeFilters).toEqual(["status", "team", "owner"]);
    });

    it("allows functional update", () => {
      const { result } = renderHook(() => useFilterChips());

      act(() => {
        result.current.setActiveFilters(["status"]);
      });

      act(() => {
        result.current.setActiveFilters((prev) => [...prev, "team"]);
      });

      expect(result.current.activeFilters).toEqual(["status", "team"]);
    });
  });

  // ═══════════════════════════════════════════════════════════════════════════
  // setOpenFilter (direct state setter)
  // ═══════════════════════════════════════════════════════════════════════════

  describe("setOpenFilter", () => {
    it("allows direct state setting", () => {
      const { result } = renderHook(() => useFilterChips());

      act(() => {
        result.current.setOpenFilter("status");
      });

      expect(result.current.openFilter).toBe("status");
    });

    it("allows setting to null", () => {
      const { result } = renderHook(() => useFilterChips());

      act(() => {
        result.current.setOpenFilter("status");
      });

      act(() => {
        result.current.setOpenFilter(null);
      });

      expect(result.current.openFilter).toBeNull();
    });
  });

  // ═══════════════════════════════════════════════════════════════════════════
  // ignoreChipRefs
  // ═══════════════════════════════════════════════════════════════════════════

  describe("ignoreChipRefs", () => {
    it("returns array of refs when chipRefs provided", () => {
      const statusRef = createRef<HTMLDivElement>();
      const teamRef = createRef<HTMLDivElement>();

      const chipRefs: Record<string, RefObject<HTMLDivElement | null>> = {
        status: statusRef,
        team: teamRef,
      };

      const { result } = renderHook(() => useFilterChips({ chipRefs }));

      expect(result.current.ignoreChipRefs).toEqual([statusRef, teamRef]);
    });

    it("is memoized and stable", () => {
      const statusRef = createRef<HTMLDivElement>();
      const teamRef = createRef<HTMLDivElement>();

      const chipRefs: Record<string, RefObject<HTMLDivElement | null>> = {
        status: statusRef,
        team: teamRef,
      };

      const { result, rerender } = renderHook(() =>
        useFilterChips({ chipRefs }),
      );

      const firstValue = result.current.ignoreChipRefs;

      // Rerender without changing chipRefs
      rerender();

      const secondValue = result.current.ignoreChipRefs;

      expect(firstValue).toBe(secondValue);
    });
  });

  // ═══════════════════════════════════════════════════════════════════════════
  // Integration scenarios
  // ═══════════════════════════════════════════════════════════════════════════

  describe("integration scenarios", () => {
    it("typical filter workflow: add, configure, remove", () => {
      const onResetFilter = vi.fn();
      const { result } = renderHook(() => useFilterChips({ onResetFilter }));

      // User clicks "Add filter" and selects "Status"
      act(() => {
        result.current.addFilter("status");
        result.current.setOpenFilter("status");
      });

      expect(result.current.activeFilters).toEqual(["status"]);
      expect(result.current.openFilter).toBe("status");

      // User configures the filter and closes dropdown
      act(() => {
        result.current.setOpenFilter(null);
      });

      expect(result.current.openFilter).toBeNull();

      // User adds another filter
      act(() => {
        result.current.addFilter("team");
      });

      expect(result.current.activeFilters).toEqual(["status", "team"]);

      // User removes the first filter
      act(() => {
        result.current.removeFilter("status");
      });

      expect(result.current.activeFilters).toEqual(["team"]);
      expect(onResetFilter).toHaveBeenCalledWith("status");
    });

    it("filter bar with available filters", () => {
      const { result } = renderHook(() => useFilterChips());

      // Check initial available filters
      let available = result.current.getAvailableFilters(FILTER_OPTIONS);
      expect(available).toHaveLength(5);

      // Add first filter
      act(() => {
        result.current.addFilter("status");
      });

      available = result.current.getAvailableFilters(FILTER_OPTIONS);
      expect(available).toHaveLength(4);
      expect(available.find((f) => f.id === "status")).toBeUndefined();

      // Add second filter
      act(() => {
        result.current.addFilter("team");
      });

      available = result.current.getAvailableFilters(FILTER_OPTIONS);
      expect(available).toHaveLength(3);

      // Clear all
      act(() => {
        result.current.clearAllFilters();
      });

      available = result.current.getAvailableFilters(FILTER_OPTIONS);
      expect(available).toHaveLength(5);
    });
  });
});
