import { useOrganization } from "@/contexts/OrganizationContext";
import { useTeams } from "@/presentation/missions/components/Filters/hooks/useTeams";
import { Checkbox, FilterDropdown } from "@getbud-co/buds";
import { useMemo } from "react";

interface TeamFilterProps {
  isOpen: boolean;
  teamChipRef: React.RefObject<HTMLDivElement | null>;
  ignoreChipRefs: React.RefObject<HTMLDivElement | null>[];
  selectedTeams: string[];
  onClose: () => void;
  onChange: (id: string) => void;
}

export function TeamFilter({
  isOpen,
  teamChipRef,
  ignoreChipRefs,
  selectedTeams,
  onChange,
  onClose,
}: TeamFilterProps) {
  const { activeOrgId } = useOrganization();
  const { data: teams = [] } = useTeams(activeOrgId);

  const teamOptions = teams.map((t) => ({ id: t.id, label: t.name }));
  const teamFilterOptions = useMemo(
    () => [{ id: "all", label: "Todos os times" }, ...teamOptions],
    [teamOptions],
  );
  return (
    <FilterDropdown
      open={isOpen}
      onClose={onClose}
      anchorRef={teamChipRef}
      ignoreRefs={ignoreChipRefs}
    >
      <div
        className={
          "flex flex-col max-h-[320px] p-[var(--sp-3xs)] overflow-y-auto"
        }
      >
        {teamFilterOptions.map((opt) => {
          const isAll = opt.id === "all";
          const checked = isAll
            ? selectedTeams.length === 0 || selectedTeams.includes("all")
            : selectedTeams.some((teamId) => teamId === opt.id);
          return (
            <button
              key={opt.id}
              type="button"
              className={`${"flex items-center gap-[var(--sp-2xs)] w-full p-[var(--sp-2xs)] border-none bg-transparent rounded-[var(--radius-2xs)] [font-family:var(--font-label)] font-medium text-[var(--text-xs)] text-[var(--color-neutral-950)] cursor-pointer whitespace-nowrap transition-colors duration-[120ms] ease-in-out text-left hover:bg-[var(--color-caramel-100)]"} ${checked ? "bg-[var(--color-caramel-100)]" : ""}`}
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
