import type { RefObject } from "react";
import { FilterDropdown, Checkbox } from "@getbud-co/buds";

interface ItemTypeOption {
  id: string;
  label: string;
}

interface ItemTypeFilterProps {
  open: boolean;
  onClose: () => void;
  anchorRef: RefObject<HTMLDivElement | null>;
  ignoreRefs: RefObject<HTMLDivElement | null>[];
  options: ItemTypeOption[];
  selectedItemTypes: string[];
  setSelectedItemTypes: React.Dispatch<React.SetStateAction<string[]>>;
}

export function ItemTypeFilter({
  open,
  onClose,
  anchorRef,
  ignoreRefs,
  options,
  selectedItemTypes,
  setSelectedItemTypes,
}: ItemTypeFilterProps) {
  return (
    <FilterDropdown
      open={open}
      onClose={onClose}
      anchorRef={anchorRef}
      ignoreRefs={ignoreRefs}
    >
      <div className="flex flex-col p-1 max-h-80 overflow-y-auto">
        {options.map((opt) => {
          const isAll = opt.id === "all";
          const checked = isAll
            ? selectedItemTypes.length === 0 ||
              selectedItemTypes.includes("all")
            : selectedItemTypes.includes(opt.id);
          return (
            <button
              key={opt.id}
              type="button"
              className={`flex items-center gap-2 w-full px-2 py-2 border-none bg-transparent rounded-[var(--radius-2xs)] font-[var(--font-label)] font-medium text-[var(--text-xs)] text-[var(--color-neutral-950)] cursor-pointer whitespace-nowrap transition-colors duration-[120ms] text-left hover:bg-[var(--color-caramel-100)] ${checked ? "bg-[var(--color-caramel-50)]" : ""}`}
              onClick={() => {
                if (isAll) {
                  setSelectedItemTypes(["all"]);
                } else {
                  setSelectedItemTypes((prev) => {
                    const without = prev.filter((id) => id !== "all");
                    const next = without.includes(opt.id)
                      ? without.filter((id) => id !== opt.id)
                      : [...without, opt.id];
                    return next.length > 0 ? next : ["all"];
                  });
                }
              }}
            >
              <Checkbox checked={checked} readOnly />
              <span>{opt.label}</span>
            </button>
          );
        })}
      </div>
    </FilterDropdown>
  );
}
