import type { RefObject } from "react";
import { FilterDropdown, Radio } from "@getbud-co/buds";

interface TaskStateOption {
  id: string;
  label: string;
}

interface TaskStateFilterProps {
  open: boolean;
  onClose: () => void;
  anchorRef: RefObject<HTMLDivElement | null>;
  ignoreRefs: RefObject<HTMLDivElement | null>[];
  options: TaskStateOption[];
  selectedTaskState: string;
  setSelectedTaskState: React.Dispatch<React.SetStateAction<string>>;
}

export function TaskStateFilter({
  open,
  onClose,
  anchorRef,
  ignoreRefs,
  options,
  selectedTaskState,
  setSelectedTaskState,
}: TaskStateFilterProps) {
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
            className={`flex items-center gap-2 w-full px-2 py-2 border-none bg-transparent rounded-[var(--radius-2xs)] font-[var(--font-label)] font-medium text-[var(--text-xs)] text-[var(--color-neutral-950)] cursor-pointer whitespace-nowrap transition-colors duration-[120ms] text-left hover:bg-[var(--color-caramel-100)] ${selectedTaskState === opt.id ? "bg-[var(--color-caramel-50)]" : ""}`}
            onClick={() => {
              setSelectedTaskState(opt.id);
              onClose();
            }}
          >
            <Radio checked={selectedTaskState === opt.id} readOnly />
            <span>{opt.label}</span>
          </button>
        ))}
      </div>
    </FilterDropdown>
  );
}
