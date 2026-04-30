"use client";

import { type RefObject, useMemo, useState } from "react";

interface FilterOption {
  id: string;
  label: string;
}

interface UseFilterChipsParams {
  /** Record mapping filter IDs to their chip wrapper refs.
   *  Used to build `ignoreChipRefs` so FilterDropdown's outside-click
   *  detection skips clicks on sibling chips. */
  chipRefs?: Record<string, RefObject<HTMLDivElement | null>>;
  onResetFilter?: (id: string) => void;
}

export function useFilterChips({
  chipRefs,
  onResetFilter,
}: UseFilterChipsParams = {}) {
  const [activeFilters, setActiveFilters] = useState<string[]>([]);
  const [openFilter, setOpenFilter] = useState<string | null>(null);

  /**
   * Memoised array of chip wrapper refs — pass as `ignoreRefs` to every
   * `<FilterDropdown>` / `<PopoverSelect>` so that clicking a sibling chip
   * never triggers the dropdown's outside-click `onClose`.
   */
  const ignoreChipRefs = useMemo(() => {
    if (!chipRefs) return undefined;
    return Object.values(chipRefs);
  }, [chipRefs]);

  function addFilter(id: string) {
    setActiveFilters((prev) => (prev.includes(id) ? prev : [...prev, id]));
  }

  function addFilterAndOpen(id: string) {
    addFilter(id);
    // Double rAF ensures the chip DOM node exists before we open the dropdown
    requestAnimationFrame(() => {
      requestAnimationFrame(() => {
        setOpenFilter(id);
      });
    });
  }

  function removeFilter(id: string) {
    setActiveFilters((prev) => prev.filter((filterId) => filterId !== id));
    if (openFilter === id) setOpenFilter(null);
    onResetFilter?.(id);
  }

  function clearAllFilters() {
    setActiveFilters((prev) => {
      prev.forEach((id) => onResetFilter?.(id));
      return [];
    });
    setOpenFilter(null);
  }

  function toggleFilterDropdown(id: string) {
    setOpenFilter((prev) => (prev === id ? null : id));
  }

  function getAvailableFilters(allFilters: FilterOption[]) {
    return allFilters.filter((filter) => !activeFilters.includes(filter.id));
  }

  return {
    activeFilters,
    setActiveFilters,
    openFilter,
    setOpenFilter,
    addFilter,
    addFilterAndOpen,
    removeFilter,
    clearAllFilters,
    toggleFilterDropdown,
    getAvailableFilters,
    ignoreChipRefs,
  };
}
