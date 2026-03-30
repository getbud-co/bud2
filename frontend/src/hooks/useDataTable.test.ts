/**
 * Tests for useDataTable hook
 *
 * This hook manages table state including row selection and column sorting.
 * It's designed to work with the Table component from the Design System.
 */

import { describe, it, expect } from "vitest";
import { renderHook, act } from "@testing-library/react";
import { useDataTable } from "./useDataTable";

// ─── Test Helpers ───

type TestSortKey = "name" | "email" | "status" | "createdAt";

const TEST_ROW_IDS = ["row-1", "row-2", "row-3", "row-4", "row-5"];

// ─── Tests ───

describe("useDataTable", () => {
  // ═══════════════════════════════════════════════════════════════════════════
  // Initial State
  // ═══════════════════════════════════════════════════════════════════════════

  describe("initial state", () => {
    it("starts with empty selection", () => {
      const { result } = renderHook(() => useDataTable<TestSortKey>());
      expect(result.current.selectedRows.size).toBe(0);
    });

    it("starts with no sort key", () => {
      const { result } = renderHook(() => useDataTable<TestSortKey>());
      expect(result.current.sortKey).toBeNull();
    });

    it("starts with ascending sort direction", () => {
      const { result } = renderHook(() => useDataTable<TestSortKey>());
      expect(result.current.sortDir).toBe("asc");
    });
  });

  // ═══════════════════════════════════════════════════════════════════════════
  // Row Selection - Single Row
  // ═══════════════════════════════════════════════════════════════════════════

  describe("handleSelectRow", () => {
    it("selects a single row", () => {
      const { result } = renderHook(() => useDataTable<TestSortKey>());

      act(() => {
        result.current.handleSelectRow("row-1", true);
      });

      expect(result.current.selectedRows.has("row-1")).toBe(true);
      expect(result.current.selectedRows.size).toBe(1);
    });

    it("selects multiple rows", () => {
      const { result } = renderHook(() => useDataTable<TestSortKey>());

      act(() => {
        result.current.handleSelectRow("row-1", true);
        result.current.handleSelectRow("row-2", true);
        result.current.handleSelectRow("row-3", true);
      });

      expect(result.current.selectedRows.has("row-1")).toBe(true);
      expect(result.current.selectedRows.has("row-2")).toBe(true);
      expect(result.current.selectedRows.has("row-3")).toBe(true);
      expect(result.current.selectedRows.size).toBe(3);
    });

    it("deselects a row", () => {
      const { result } = renderHook(() => useDataTable<TestSortKey>());

      act(() => {
        result.current.handleSelectRow("row-1", true);
        result.current.handleSelectRow("row-2", true);
      });

      act(() => {
        result.current.handleSelectRow("row-1", false);
      });

      expect(result.current.selectedRows.has("row-1")).toBe(false);
      expect(result.current.selectedRows.has("row-2")).toBe(true);
      expect(result.current.selectedRows.size).toBe(1);
    });

    it("does not error when deselecting non-selected row", () => {
      const { result } = renderHook(() => useDataTable<TestSortKey>());

      act(() => {
        result.current.handleSelectRow("row-1", false);
      });

      expect(result.current.selectedRows.size).toBe(0);
    });

    it("does not duplicate when selecting already selected row", () => {
      const { result } = renderHook(() => useDataTable<TestSortKey>());

      act(() => {
        result.current.handleSelectRow("row-1", true);
        result.current.handleSelectRow("row-1", true);
        result.current.handleSelectRow("row-1", true);
      });

      expect(result.current.selectedRows.size).toBe(1);
    });
  });

  // ═══════════════════════════════════════════════════════════════════════════
  // Row Selection - Select All
  // ═══════════════════════════════════════════════════════════════════════════

  describe("handleSelectAll", () => {
    it("selects all rows", () => {
      const { result } = renderHook(() => useDataTable<TestSortKey>());

      act(() => {
        result.current.handleSelectAll(true, TEST_ROW_IDS);
      });

      expect(result.current.selectedRows.size).toBe(5);
      TEST_ROW_IDS.forEach((id) => {
        expect(result.current.selectedRows.has(id)).toBe(true);
      });
    });

    it("deselects all rows", () => {
      const { result } = renderHook(() => useDataTable<TestSortKey>());

      act(() => {
        result.current.handleSelectAll(true, TEST_ROW_IDS);
      });

      act(() => {
        result.current.handleSelectAll(false, TEST_ROW_IDS);
      });

      expect(result.current.selectedRows.size).toBe(0);
    });

    it("replaces current selection when selecting all", () => {
      const { result } = renderHook(() => useDataTable<TestSortKey>());

      // Select some rows manually
      act(() => {
        result.current.handleSelectRow("other-row", true);
      });

      // Select all with different row IDs
      act(() => {
        result.current.handleSelectAll(true, TEST_ROW_IDS);
      });

      // Should only have the rows from handleSelectAll
      expect(result.current.selectedRows.size).toBe(5);
      expect(result.current.selectedRows.has("other-row")).toBe(false);
    });

    it("handles empty row array", () => {
      const { result } = renderHook(() => useDataTable<TestSortKey>());

      act(() => {
        result.current.handleSelectAll(true, []);
      });

      expect(result.current.selectedRows.size).toBe(0);
    });
  });

  // ═══════════════════════════════════════════════════════════════════════════
  // Clear Selection
  // ═══════════════════════════════════════════════════════════════════════════

  describe("clearSelection", () => {
    it("clears all selected rows", () => {
      const { result } = renderHook(() => useDataTable<TestSortKey>());

      act(() => {
        result.current.handleSelectAll(true, TEST_ROW_IDS);
      });

      expect(result.current.selectedRows.size).toBe(5);

      act(() => {
        result.current.clearSelection();
      });

      expect(result.current.selectedRows.size).toBe(0);
    });

    it("handles clearing empty selection", () => {
      const { result } = renderHook(() => useDataTable<TestSortKey>());

      act(() => {
        result.current.clearSelection();
      });

      expect(result.current.selectedRows.size).toBe(0);
    });
  });

  // ═══════════════════════════════════════════════════════════════════════════
  // Sorting - handleSort
  // ═══════════════════════════════════════════════════════════════════════════

  describe("handleSort", () => {
    it("sets sort key on first click", () => {
      const { result } = renderHook(() => useDataTable<TestSortKey>());

      act(() => {
        result.current.handleSort("name");
      });

      expect(result.current.sortKey).toBe("name");
      expect(result.current.sortDir).toBe("asc");
    });

    it("toggles direction on same column click", () => {
      const { result } = renderHook(() => useDataTable<TestSortKey>());

      act(() => {
        result.current.handleSort("name");
      });

      expect(result.current.sortDir).toBe("asc");

      act(() => {
        result.current.handleSort("name");
      });

      expect(result.current.sortKey).toBe("name");
      expect(result.current.sortDir).toBe("desc");
    });

    it("toggles back to asc on third click", () => {
      const { result } = renderHook(() => useDataTable<TestSortKey>());

      act(() => {
        result.current.handleSort("name");
      });

      act(() => {
        result.current.handleSort("name");
      });

      act(() => {
        result.current.handleSort("name");
      });

      expect(result.current.sortKey).toBe("name");
      expect(result.current.sortDir).toBe("asc");
    });

    it("resets to asc when switching columns", () => {
      const { result } = renderHook(() => useDataTable<TestSortKey>());

      act(() => {
        result.current.handleSort("name");
      });

      act(() => {
        result.current.handleSort("name"); // Now desc
      });

      expect(result.current.sortDir).toBe("desc");

      act(() => {
        result.current.handleSort("email"); // Switch column
      });

      expect(result.current.sortKey).toBe("email");
      expect(result.current.sortDir).toBe("asc");
    });

    it("works with different sort keys", () => {
      const { result } = renderHook(() => useDataTable<TestSortKey>());

      act(() => {
        result.current.handleSort("name");
      });

      expect(result.current.sortKey).toBe("name");

      act(() => {
        result.current.handleSort("createdAt");
      });

      expect(result.current.sortKey).toBe("createdAt");

      act(() => {
        result.current.handleSort("status");
      });

      expect(result.current.sortKey).toBe("status");
    });
  });

  // ═══════════════════════════════════════════════════════════════════════════
  // Sorting - getSortDirection
  // ═══════════════════════════════════════════════════════════════════════════

  describe("getSortDirection", () => {
    it("returns undefined for non-sorted column", () => {
      const { result } = renderHook(() => useDataTable<TestSortKey>());

      const direction = result.current.getSortDirection("name");
      expect(direction).toBeUndefined();
    });

    it("returns asc for ascending sorted column", () => {
      const { result } = renderHook(() => useDataTable<TestSortKey>());

      act(() => {
        result.current.handleSort("name");
      });

      expect(result.current.getSortDirection("name")).toBe("asc");
    });

    it("returns desc for descending sorted column", () => {
      const { result } = renderHook(() => useDataTable<TestSortKey>());

      act(() => {
        result.current.handleSort("name");
      });

      act(() => {
        result.current.handleSort("name");
      });

      expect(result.current.getSortDirection("name")).toBe("desc");
    });

    it("returns undefined for other columns when one is sorted", () => {
      const { result } = renderHook(() => useDataTable<TestSortKey>());

      act(() => {
        result.current.handleSort("name");
      });

      expect(result.current.getSortDirection("name")).toBe("asc");
      expect(result.current.getSortDirection("email")).toBeUndefined();
      expect(result.current.getSortDirection("status")).toBeUndefined();
      expect(result.current.getSortDirection("createdAt")).toBeUndefined();
    });
  });

  // ═══════════════════════════════════════════════════════════════════════════
  // Direct State Setters
  // ═══════════════════════════════════════════════════════════════════════════

  describe("setSelectedRows", () => {
    it("allows direct state setting", () => {
      const { result } = renderHook(() => useDataTable<TestSortKey>());

      act(() => {
        result.current.setSelectedRows(new Set(["row-1", "row-2"]));
      });

      expect(result.current.selectedRows.size).toBe(2);
      expect(result.current.selectedRows.has("row-1")).toBe(true);
      expect(result.current.selectedRows.has("row-2")).toBe(true);
    });

    it("allows functional update", () => {
      const { result } = renderHook(() => useDataTable<TestSortKey>());

      act(() => {
        result.current.setSelectedRows(new Set(["row-1"]));
      });

      act(() => {
        result.current.setSelectedRows((prev) => {
          const next = new Set(prev);
          next.add("row-2");
          return next;
        });
      });

      expect(result.current.selectedRows.size).toBe(2);
    });
  });

  // ═══════════════════════════════════════════════════════════════════════════
  // Integration Scenarios
  // ═══════════════════════════════════════════════════════════════════════════

  describe("integration scenarios", () => {
    it("typical table workflow: sort, select some, bulk action, clear", () => {
      const { result } = renderHook(() => useDataTable<TestSortKey>());

      // User clicks column header to sort
      act(() => {
        result.current.handleSort("name");
      });

      expect(result.current.sortKey).toBe("name");
      expect(result.current.sortDir).toBe("asc");

      // User selects some rows
      act(() => {
        result.current.handleSelectRow("row-1", true);
        result.current.handleSelectRow("row-3", true);
      });

      expect(result.current.selectedRows.size).toBe(2);

      // After bulk action, clear selection
      act(() => {
        result.current.clearSelection();
      });

      expect(result.current.selectedRows.size).toBe(0);
      // Sort should still be active
      expect(result.current.sortKey).toBe("name");
    });

    it("select all, then deselect some", () => {
      const { result } = renderHook(() => useDataTable<TestSortKey>());

      // Select all
      act(() => {
        result.current.handleSelectAll(true, TEST_ROW_IDS);
      });

      expect(result.current.selectedRows.size).toBe(5);

      // Deselect specific rows
      act(() => {
        result.current.handleSelectRow("row-2", false);
        result.current.handleSelectRow("row-4", false);
      });

      expect(result.current.selectedRows.size).toBe(3);
      expect(result.current.selectedRows.has("row-1")).toBe(true);
      expect(result.current.selectedRows.has("row-2")).toBe(false);
      expect(result.current.selectedRows.has("row-3")).toBe(true);
      expect(result.current.selectedRows.has("row-4")).toBe(false);
      expect(result.current.selectedRows.has("row-5")).toBe(true);
    });

    it("sorting multiple columns in sequence", () => {
      const { result } = renderHook(() => useDataTable<TestSortKey>());

      // Sort by name ascending
      act(() => {
        result.current.handleSort("name");
      });

      expect(result.current.getSortDirection("name")).toBe("asc");
      expect(result.current.getSortDirection("email")).toBeUndefined();

      // Sort by email (should reset to asc)
      act(() => {
        result.current.handleSort("email");
      });

      expect(result.current.getSortDirection("name")).toBeUndefined();
      expect(result.current.getSortDirection("email")).toBe("asc");

      // Toggle email to desc
      act(() => {
        result.current.handleSort("email");
      });

      expect(result.current.getSortDirection("email")).toBe("desc");

      // Back to name
      act(() => {
        result.current.handleSort("name");
      });

      expect(result.current.getSortDirection("name")).toBe("asc");
      expect(result.current.getSortDirection("email")).toBeUndefined();
    });
  });

  // ═══════════════════════════════════════════════════════════════════════════
  // Type Safety
  // ═══════════════════════════════════════════════════════════════════════════

  describe("type safety", () => {
    it("works with custom sort key type", () => {
      type CustomSortKey = "alpha" | "beta" | "gamma";
      const { result } = renderHook(() => useDataTable<CustomSortKey>());

      act(() => {
        result.current.handleSort("alpha");
      });

      expect(result.current.sortKey).toBe("alpha");
      expect(result.current.getSortDirection("alpha")).toBe("asc");
      expect(result.current.getSortDirection("beta")).toBeUndefined();
    });

    it("works with string literal sort keys", () => {
      const { result } = renderHook(() => useDataTable<"a" | "b" | "c">());

      act(() => {
        result.current.handleSort("a");
      });

      act(() => {
        result.current.handleSort("a");
      });

      expect(result.current.sortKey).toBe("a");
      expect(result.current.sortDir).toBe("desc");
    });
  });
});
