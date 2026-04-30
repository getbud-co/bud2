import type { RefObject } from "react";
import { FilterDropdown, Checkbox, Avatar } from "@getbud-co/buds";
import type { OwnerOption } from "@/contexts/PeopleDataContext";

type OwnerFilterOption =
  | OwnerOption
  | { id: string; label: string; initials: string };

interface OwnerFilterProps {
  open: boolean;
  onClose: () => void;
  anchorRef: RefObject<HTMLDivElement | null>;
  ignoreRefs: RefObject<HTMLDivElement | null>[];
  options: OwnerFilterOption[];
  selectedOwners: string[];
  setSelectedOwners: React.Dispatch<React.SetStateAction<string[]>>;
  resolveUserId: (legacyOrCanonicalId: string) => string;
}

export function OwnerFilter({
  open,
  onClose,
  anchorRef,
  ignoreRefs,
  options,
  selectedOwners,
  setSelectedOwners,
  resolveUserId,
}: OwnerFilterProps) {
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
            ? selectedOwners.length === 0 || selectedOwners.includes("all")
            : selectedOwners.some(
                (ownerId) => resolveUserId(ownerId) === opt.id,
              );
          return (
            <button
              key={opt.id}
              type="button"
              className={`flex items-center gap-2 w-full px-2 py-2 border-none bg-transparent rounded-[var(--radius-2xs)] font-[var(--font-label)] font-medium text-[var(--text-xs)] text-[var(--color-neutral-950)] cursor-pointer whitespace-nowrap transition-colors duration-[120ms] text-left hover:bg-[var(--color-caramel-100)] ${checked ? "bg-[var(--color-caramel-50)]" : ""}`}
              onClick={() => {
                if (isAll) {
                  setSelectedOwners(["all"]);
                } else {
                  setSelectedOwners((prev) => {
                    const withoutAll = prev
                      .filter((id) => id !== "all")
                      .map((id) => resolveUserId(id));
                    const without = Array.from(new Set(withoutAll));
                    return without.includes(opt.id)
                      ? without.filter((id) => id !== opt.id)
                      : [...without, opt.id];
                  });
                }
              }}
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
