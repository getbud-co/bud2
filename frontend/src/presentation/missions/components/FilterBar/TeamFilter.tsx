import type { RefObject } from "react";
import { FilterDropdown, Checkbox } from "@getbud-co/buds";

interface TeamFilterOption {
  id: string;
  label: string;
}

interface TeamFilterProps {
  open: boolean;
  onClose: () => void;
  anchorRef: RefObject<HTMLDivElement | null>;
  ignoreRefs: RefObject<HTMLDivElement | null>[];
  options: TeamFilterOption[];
  selectedTeams: string[];
  setSelectedTeams: React.Dispatch<React.SetStateAction<string[]>>;
  resolveTeamId: (legacyOrCanonicalId: string) => string;
}

export function TeamFilter({
  open,
  onClose,
  anchorRef,
  ignoreRefs,
  options,
  selectedTeams,
  setSelectedTeams,
  resolveTeamId,
}: TeamFilterProps) {
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
            ? selectedTeams.length === 0 || selectedTeams.includes("all")
            : selectedTeams.some((teamId) => resolveTeamId(teamId) === opt.id);
          return (
            <button
              key={opt.id}
              type="button"
              className={`flex items-center gap-2 w-full px-2 py-2 border-none bg-transparent rounded-[var(--radius-2xs)] font-[var(--font-label)] font-medium text-[var(--text-xs)] text-[var(--color-neutral-950)] cursor-pointer whitespace-nowrap transition-colors duration-[120ms] text-left hover:bg-[var(--color-caramel-100)] ${checked ? "bg-[var(--color-caramel-50)]" : ""}`}
              onClick={() => {
                if (isAll) {
                  setSelectedTeams(["all"]);
                } else {
                  setSelectedTeams((prev) => {
                    const withoutAll = prev
                      .filter((id) => id !== "all")
                      .map((id) => resolveTeamId(id));
                    const without = Array.from(new Set(withoutAll));
                    return without.includes(opt.id)
                      ? without.filter((id) => id !== opt.id)
                      : [...without, opt.id];
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
