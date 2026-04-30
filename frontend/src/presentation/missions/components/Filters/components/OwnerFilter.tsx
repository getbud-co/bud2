import { Avatar, Checkbox, FilterDropdown } from "@getbud-co/buds";
import { useMemo } from "react";
import { useEmployees } from "@/presentation/missions/components/Filters/hooks/useEmployees";

interface OwnerFilterProps {
  isOpen: boolean;
  ownerChipRef: React.RefObject<HTMLDivElement | null>;
  ignoreChipRefs: React.RefObject<HTMLDivElement | null>[];
  selectedOwners: string[];
  onClose: () => void;
  onChange: (id: string) => void;
}

export function OwnerFilter({
  isOpen,
  ownerChipRef,
  ignoreChipRefs,
  selectedOwners,
  onClose,
  onChange,
}: OwnerFilterProps) {
  const { data: employees = [] } = useEmployees();

  const options = useMemo(
    () => [
      { id: "all", label: "Todos", initials: "" },
      ...employees.map((e) => ({
        id: e.id,
        label: e.fullName,
        initials: e.initials,
      })),
    ],
    [employees],
  );

  return (
    <FilterDropdown
      open={isOpen}
      onClose={onClose}
      anchorRef={ownerChipRef}
      ignoreRefs={ignoreChipRefs}
    >
      <div className="flex flex-col max-h-[320px] p-[var(--sp-3xs)] overflow-y-auto">
        {options.map((opt) => {
          const isAll = opt.id === "all";
          const checked = isAll
            ? selectedOwners.length === 0 || selectedOwners.includes("all")
            : selectedOwners.some((name) => name === opt.label);
          return (
            <button
              key={opt.id}
              type="button"
              className={`flex items-center gap-[var(--sp-2xs)] w-full p-[var(--sp-2xs)] border-none bg-transparent rounded-[var(--radius-2xs)] [font-family:var(--font-label)] font-medium text-[var(--text-xs)] text-[var(--color-neutral-950)] cursor-pointer whitespace-nowrap transition-colors duration-[120ms] ease-in-out text-left hover:bg-[var(--color-caramel-100)] ${checked ? "bg-[var(--color-caramel-100)]" : ""}`}
              onClick={() => onChange(isAll ? "all" : opt.label)}
            >
              <Checkbox checked={checked} readOnly />
              {opt.initials && <Avatar initials={opt.initials} size="xs" />}
              <span>{opt.label}</span>
            </button>
          );
        })}
      </div>
    </FilterDropdown>
  );
}
