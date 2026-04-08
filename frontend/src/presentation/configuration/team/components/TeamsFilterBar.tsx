import { useRef, type RefObject } from "react";
import {
  FilterBar,
  FilterChip,
  FilterDropdown,
  Radio,
} from "@mdonangelo/bud-ds";
import { useFilterChips } from "@/hooks/useFilterChips";
import { STATUS_FILTER, FILTER_OPTIONS } from "../consts";

interface TeamsFilterBarProps {
  filterStatus: string;
  onFilterStatusChange: (value: string) => void;
}

export function TeamsFilterBar({
  filterStatus,
  onFilterStatusChange,
}: TeamsFilterBarProps) {
  const statusChipRef = useRef<HTMLDivElement>(null);
  const chipRefs: Record<string, RefObject<HTMLDivElement | null>> = {
    status: statusChipRef,
  };

  const {
    activeFilters,
    openFilter,
    setOpenFilter,
    addFilterAndOpen,
    removeFilter,
    clearAllFilters,
    toggleFilterDropdown,
    getAvailableFilters,
    ignoreChipRefs,
  } = useFilterChips({
    chipRefs,
    onResetFilter: (id) => {
      if (id === "status") onFilterStatusChange("all");
    },
  });

  function getFilterLabel(id: string): string {
    if (id === "status")
      return STATUS_FILTER.find((s) => s.id === filterStatus)?.label ?? "Status";
    return id;
  }

  return (
    <>
      <div className="flex flex-col gap-[var(--sp-sm)] px-[var(--sp-sm)] py-[var(--sp-lg)]">
        <FilterBar
          filters={getAvailableFilters(FILTER_OPTIONS)}
          onAddFilter={addFilterAndOpen}
          onClearAll={activeFilters.length > 0 ? clearAllFilters : undefined}
        >
          {activeFilters.map((filterId) => (
            <div
              key={filterId}
              ref={filterId === "status" ? statusChipRef : undefined}
              style={{ display: "inline-flex" }}
            >
              <FilterChip
                label={getFilterLabel(filterId)}
                active={openFilter === filterId}
                onClick={() => toggleFilterDropdown(filterId)}
                onRemove={() => removeFilter(filterId)}
              />
            </div>
          ))}
        </FilterBar>
      </div>

      <FilterDropdown
        open={openFilter === "status"}
        onClose={() => setOpenFilter(null)}
        anchorRef={statusChipRef}
        ignoreRefs={ignoreChipRefs}
      >
        <div className="flex flex-col max-h-80 overflow-y-auto">
          {STATUS_FILTER.map((opt) => (
            <button
              key={opt.id}
              type="button"
              className={`flex items-center gap-[var(--sp-2xs)] px-[var(--sp-sm)] py-[var(--sp-2xs)] border-none bg-transparent font-[var(--font-body)] text-[var(--text-sm)] cursor-pointer text-left w-full transition-[background] duration-100 hover:bg-[var(--color-caramel-50)] ${filterStatus === opt.id ? "bg-[var(--color-orange-50)] text-[var(--color-orange-700)]" : "text-[var(--color-neutral-700)]"}`}
              onClick={() => {
                onFilterStatusChange(opt.id);
                setOpenFilter(null);
              }}
            >
              <Radio checked={filterStatus === opt.id} readOnly />
              <span>{opt.label}</span>
            </button>
          ))}
        </div>
      </FilterDropdown>
    </>
  );
}
