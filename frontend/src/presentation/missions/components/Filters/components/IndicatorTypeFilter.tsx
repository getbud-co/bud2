import { Checkbox, FilterDropdown } from "@getbud-co/buds";
import { INDICATOR_TYPE_OPTIONS } from "@/presentation/missions/consts";

interface IndicatorTypeFilterProps {
  isOpen: boolean;
  indicatorTypeChipRef: React.RefObject<HTMLDivElement | null>;
  ignoreChipRefs: React.RefObject<HTMLDivElement | null>[];
  selectedIndicatorTypes: string[];
  onClose: () => void;
  onChange: (id: string) => void;
}

export function IndicatorTypeFilter({
  isOpen,
  indicatorTypeChipRef,
  ignoreChipRefs,
  selectedIndicatorTypes,
  onClose,
  onChange,
}: IndicatorTypeFilterProps) {
  return (
    <FilterDropdown
      open={isOpen}
      onClose={onClose}
      anchorRef={indicatorTypeChipRef}
      ignoreRefs={ignoreChipRefs}
    >
      <div className="flex flex-col max-h-[320px] p-[var(--sp-3xs)] overflow-y-auto">
        {INDICATOR_TYPE_OPTIONS.map((opt) => {
          const isAll = opt.id === "all";
          const checked = isAll
            ? selectedIndicatorTypes.length === 0 || selectedIndicatorTypes.includes("all")
            : selectedIndicatorTypes.includes(opt.id);
          return (
            <button
              key={opt.id}
              type="button"
              className={`flex items-center gap-[var(--sp-2xs)] w-full p-[var(--sp-2xs)] border-none bg-transparent rounded-[var(--radius-2xs)] [font-family:var(--font-label)] font-medium text-[var(--text-xs)] text-[var(--color-neutral-950)] cursor-pointer whitespace-nowrap transition-colors duration-[120ms] ease-in-out text-left hover:bg-[var(--color-caramel-100)] ${checked ? "bg-[var(--color-caramel-100)]" : ""}`}
              onClick={() => onChange(opt.id)}
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
