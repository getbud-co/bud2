import { FilterDropdown, Radio } from "@getbud-co/buds";
import { STATUS_OPTIONS } from "@/presentation/missions/consts";

interface StatusFilterProps {
  isOpen: boolean;
  statusChipRef: React.RefObject<HTMLDivElement | null>;
  ignoreChipRefs: React.RefObject<HTMLDivElement | null>[];
  selectedStatus: string;
  onClose: () => void;
  onChange: (id: string) => void;
}

export function StatusFilter({
  isOpen,
  statusChipRef,
  ignoreChipRefs,
  selectedStatus,
  onClose,
  onChange,
}: StatusFilterProps) {
  return (
    <FilterDropdown
      open={isOpen}
      onClose={onClose}
      anchorRef={statusChipRef}
      ignoreRefs={ignoreChipRefs}
    >
      <div className="flex flex-col max-h-[320px] p-[var(--sp-3xs)] overflow-y-auto">
        {STATUS_OPTIONS.map((opt) => (
          <button
            key={opt.id}
            type="button"
            className={`flex items-center gap-[var(--sp-2xs)] w-full p-[var(--sp-2xs)] border-none bg-transparent rounded-[var(--radius-2xs)] [font-family:var(--font-label)] font-medium text-[var(--text-xs)] text-[var(--color-neutral-950)] cursor-pointer whitespace-nowrap transition-colors duration-[120ms] ease-in-out text-left hover:bg-[var(--color-caramel-100)] ${selectedStatus === opt.id ? "bg-[var(--color-caramel-50)]" : ""}`}
            onClick={() => {
              onChange(opt.id);
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
