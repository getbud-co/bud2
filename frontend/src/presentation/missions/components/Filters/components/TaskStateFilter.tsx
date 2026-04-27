import { FilterDropdown, Radio } from "@getbud-co/buds";
import { TASK_STATE_OPTIONS } from "@/presentation/missions/consts";

interface TaskStateFilterProps {
  isOpen: boolean;
  taskStateChipRef: React.RefObject<HTMLDivElement | null>;
  ignoreChipRefs: React.RefObject<HTMLDivElement | null>[];
  selectedTaskState: string;
  onClose: () => void;
  onChange: (id: string) => void;
}

export function TaskStateFilter({
  isOpen,
  taskStateChipRef,
  ignoreChipRefs,
  selectedTaskState,
  onClose,
  onChange,
}: TaskStateFilterProps) {
  return (
    <FilterDropdown
      open={isOpen}
      onClose={onClose}
      anchorRef={taskStateChipRef}
      ignoreRefs={ignoreChipRefs}
    >
      <div className="flex flex-col max-h-[320px] p-[var(--sp-3xs)] overflow-y-auto">
        {TASK_STATE_OPTIONS.map((opt) => (
          <button
            key={opt.id}
            type="button"
            className={`flex items-center gap-[var(--sp-2xs)] w-full p-[var(--sp-2xs)] border-none bg-transparent rounded-[var(--radius-2xs)] [font-family:var(--font-label)] font-medium text-[var(--text-xs)] text-[var(--color-neutral-950)] cursor-pointer whitespace-nowrap transition-colors duration-[120ms] ease-in-out text-left hover:bg-[var(--color-caramel-100)] ${selectedTaskState === opt.id ? "bg-[var(--color-caramel-50)]" : ""}`}
            onClick={() => {
              onChange(opt.id);
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
