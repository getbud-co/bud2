"use client";

import type { RefObject } from "react";
import {
  FilterBar,
  FilterChip,
  FilterDropdown,
  Radio,
} from "@mdonangelo/bud-ds";
import { STATUS_FILTER } from "../consts";

interface FilterOption {
  id: string;
  label: string;
}

interface UsersFilterBarProps {
  availableFilters: FilterOption[];
  activeFilters: string[];
  openFilter: string | null;
  filterStatus: string;
  filterRole: string;
  roleFilterOptions: FilterOption[];
  statusChipRef: RefObject<HTMLDivElement | null>;
  roleChipRef: RefObject<HTMLDivElement | null>;
  ignoreChipRefs: RefObject<HTMLElement | null>[];
  getFilterLabel: (id: string) => string;
  onAddFilter: (id: string) => void;
  onClearAll: (() => void) | undefined;
  onToggleFilter: (id: string) => void;
  onRemoveFilter: (id: string) => void;
  onSetOpenFilter: (id: string | null) => void;
  onStatusChange: (id: string) => void;
  onRoleChange: (id: string) => void;
}

export function UsersFilterBar({
  availableFilters,
  activeFilters,
  openFilter,
  filterStatus,
  filterRole,
  roleFilterOptions,
  statusChipRef,
  roleChipRef,
  ignoreChipRefs,
  getFilterLabel,
  onAddFilter,
  onClearAll,
  onToggleFilter,
  onRemoveFilter,
  onSetOpenFilter,
  onStatusChange,
  onRoleChange,
}: UsersFilterBarProps) {
  const chipRefs: Record<string, RefObject<HTMLDivElement | null>> = {
    status: statusChipRef,
    role: roleChipRef,
  };

  return (
    <div className="flex flex-col gap-[var(--sp-sm)] px-[var(--sp-lg)] py-[var(--sp-sm)]">
      <FilterBar
        filters={availableFilters}
        onAddFilter={onAddFilter}
        onClearAll={onClearAll}
      >
        {activeFilters.map((filterId) => (
          <div key={filterId} ref={chipRefs[filterId]} className="inline-flex">
            <FilterChip
              label={getFilterLabel(filterId)}
              active={openFilter === filterId}
              onClick={() => onToggleFilter(filterId)}
              onRemove={() => onRemoveFilter(filterId)}
            />
          </div>
        ))}
      </FilterBar>

      <FilterDropdown
        open={openFilter === "status"}
        onClose={() => onSetOpenFilter(null)}
        anchorRef={statusChipRef}
        ignoreRefs={ignoreChipRefs}
      >
        <div className="flex flex-col max-h-[320px] overflow-y-auto">
          {STATUS_FILTER.map((opt) => (
            <button
              key={opt.id}
              type="button"
              className={`flex items-center gap-[var(--sp-2xs)] px-[var(--sp-sm)] py-[var(--sp-2xs)] border-0 bg-transparent font-[var(--font-body)] text-[var(--text-sm)] text-[var(--color-neutral-700)] cursor-pointer text-left w-full transition-[background] duration-100 hover:bg-[var(--color-caramel-50)] ${filterStatus === opt.id ? "bg-[var(--color-orange-50)] text-[var(--color-orange-700)]" : ""}`}
              onClick={() => {
                onStatusChange(opt.id);
                onSetOpenFilter(null);
              }}
            >
              <Radio checked={filterStatus === opt.id} readOnly />
              <span>{opt.label}</span>
            </button>
          ))}
        </div>
      </FilterDropdown>

      <FilterDropdown
        open={openFilter === "role"}
        onClose={() => onSetOpenFilter(null)}
        anchorRef={roleChipRef}
        ignoreRefs={ignoreChipRefs}
      >
        <div className="flex flex-col max-h-[320px] overflow-y-auto">
          {roleFilterOptions.map((opt) => (
            <button
              key={opt.id}
              type="button"
              className={`flex items-center gap-[var(--sp-2xs)] px-[var(--sp-sm)] py-[var(--sp-2xs)] border-0 bg-transparent font-[var(--font-body)] text-[var(--text-sm)] text-[var(--color-neutral-700)] cursor-pointer text-left w-full transition-[background] duration-100 hover:bg-[var(--color-caramel-50)] ${filterRole === opt.id ? "bg-[var(--color-orange-50)] text-[var(--color-orange-700)]" : ""}`}
              onClick={() => {
                onRoleChange(opt.id);
                onSetOpenFilter(null);
              }}
            >
              <Radio checked={filterRole === opt.id} readOnly />
              <span>{opt.label}</span>
            </button>
          ))}
        </div>
      </FilterDropdown>
    </div>
  );
}
