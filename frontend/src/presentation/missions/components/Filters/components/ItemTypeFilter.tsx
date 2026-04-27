import { Checkbox, FilterDropdown } from "@getbud-co/buds";
import { ITEM_TYPE_OPTIONS } from "@/presentation/missions/consts";

interface ItemTypeFilterProps {
  isOpen: boolean;
  itemTypeChipRef: React.RefObject<HTMLDivElement | null>;
  ignoreChipRefs: React.RefObject<HTMLDivElement | null>[];
  selectedItemTypes: string[];
  onClose: () => void;
  onChange: (id: string) => void;
}

export function ItemTypeFilter({
  isOpen,
  itemTypeChipRef,
  ignoreChipRefs,
  selectedItemTypes,
  onClose,
  onChange,
}: ItemTypeFilterProps) {
  return (
    <FilterDropdown
      open={isOpen}
      onClose={onClose}
      anchorRef={itemTypeChipRef}
      ignoreRefs={ignoreChipRefs}
    >
      <div className="flex flex-col max-h-[320px] p-[var(--sp-3xs)] overflow-y-auto">
        {ITEM_TYPE_OPTIONS.map((opt) => {
          const isAll = opt.id === "all";
          const checked = isAll
            ? selectedItemTypes.length === 0 || selectedItemTypes.includes("all")
            : selectedItemTypes.includes(opt.id);
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
