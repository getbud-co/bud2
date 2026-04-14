import type { RefObject } from "react";
import { FilterDropdown, Radio } from "@getbud-co/buds";

interface StatusOption {
  id: string;
  label: string;
}

interface StatusFilterProps {
  open: boolean;
  onClose: () => void;
  anchorRef: RefObject<HTMLDivElement | null>;
  ignoreRefs: RefObject<HTMLDivElement | null>[];
  options: StatusOption[];
  selectedStatus: string;
  setSelectedStatus: React.Dispatch<React.SetStateAction<string>>;
}

export function StatusFilter({
  open,
  onClose,
  anchorRef,
  ignoreRefs,
  options,
  selectedStatus,
  setSelectedStatus,
}: StatusFilterProps) {
  return (
    <FilterDropdown
      open={open}
      onClose={onClose}
      anchorRef={anchorRef}
      ignoreRefs={ignoreRefs}
    >
      <div className="flex flex-col p-1 max-h-80 overflow-y-auto">
        {options.map((opt) => (
          <button
            key={opt.id}
            type="button"
            className={`flex items-center gap-2 w-full px-2 py-2 border-none bg-transparent rounded-[var(--radius-2xs)] font-[var(--font-label)] font-medium text-[var(--text-xs)] text-[var(--color-neutral-950)] cursor-pointer whitespace-nowrap transition-colors duration-[120ms] text-left hover:bg-[var(--color-caramel-100)] ${selectedStatus === opt.id ? "bg-[var(--color-caramel-50)]" : ""}`}
            onClick={() => {
              setSelectedStatus(opt.id);
              onClose();
            }}
          >
            <Radio checked={selectedStatus === opt.id} readOnly />
            <span>{opt.label}</span>
          </button>
        ))}
      </div>
    </FilterDropdown>
  );
}
