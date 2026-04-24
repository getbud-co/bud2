"use client";

import { useState } from "react";

type SortDirection = "asc" | "desc";

export function useDataTable<SortKey extends string>() {
  const [selectedRows, setSelectedRows] = useState<Set<string>>(new Set());
  const [sortKey, setSortKey] = useState<SortKey | null>(null);
  const [sortDir, setSortDir] = useState<SortDirection>("asc");

  function handleSort(key: SortKey) {
    if (sortKey === key) {
      setSortDir((prev) => (prev === "asc" ? "desc" : "asc"));
      return;
    }
    setSortKey(key);
    setSortDir("asc");
  }

  function getSortDirection(key: SortKey): SortDirection | undefined {
    return sortKey === key ? sortDir : undefined;
  }

  function handleSelectRow(id: string, checked: boolean) {
    setSelectedRows((prev) => {
      const next = new Set(prev);
      if (checked) next.add(id);
      else next.delete(id);
      return next;
    });
  }

  function handleSelectAll(checked: boolean, rowIds: string[]) {
    setSelectedRows(checked ? new Set(rowIds) : new Set());
  }

  function clearSelection() {
    setSelectedRows(new Set());
  }

  return {
    selectedRows,
    setSelectedRows,
    clearSelection,
    sortKey,
    sortDir,
    handleSort,
    getSortDirection,
    handleSelectRow,
    handleSelectAll,
  };
}
