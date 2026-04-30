import { Dispatch, SetStateAction, useMemo, useRef, useState } from "react";
import type { ComponentType, RefObject } from "react";
import { FilterBar, FilterChip } from "@getbud-co/buds";
import type { CalendarDate } from "@getbud-co/buds";
import { Trash } from "@phosphor-icons/react";
import type { SavedView } from "@/contexts/SavedViewsContext";
import { formatMultiLabel } from "@/components/PopoverSelect";
import {
  FILTER_OPTIONS,
  STATUS_OPTIONS,
  ITEM_TYPE_OPTIONS,
  INDICATOR_TYPE_OPTIONS,
  CONTRIBUTION_OPTIONS,
  TASK_STATE_OPTIONS,
  MISSION_STATUS_OPTIONS,
} from "../../consts";
import { TeamFilter } from "./components/TeamFilter";
import { StatusFilter } from "./components/StatusFilter";
import { OwnerFilter } from "./components/OwnerFilter";
import { ItemTypeFilter } from "./components/ItemTypeFilter";
import { IndicatorTypeFilter } from "./components/IndicatorTypeFilter";
import { ContributionFilter } from "./components/ContributionFilter";
import { SupporterFilter } from "./components/SupporterFilter";
import { TaskStateFilter } from "./components/TaskStateFilter";
import { MissionStatusFilter } from "./components/MissionStatusFilter";
import { PeriodFilter } from "./components/PeriodFilter";
import { filterChipIcons } from "../../utils";

// ── Types ─────────────────────────────────────────────────────────────────────

export interface FilterValues {
  selectedTeams: string[];
  selectedPeriod: [CalendarDate | null, CalendarDate | null];
  selectedStatus: string;
  selectedOwners: string[];
  selectedItemTypes: string[];
  selectedIndicatorTypes: string[];
  selectedContributions: string[];
  selectedTaskState: string;
  selectedMissionStatuses: string[];
  selectedSupporters: string[];
}

interface FilterSectionProps {
  activeFilters: string[];
  setActiveFilters: Dispatch<SetStateAction<string[]>>;
  filterBarDefaultOpen: boolean;
  setFilterBarDefaultOpen: (v: boolean) => void;
  currentView?: SavedView;
  onSaveView?: () => void;
  onDeleteView?: () => void;
  mine: boolean;
  currentUserDefaultName: string;
  ownerFilterOptions: { id: string; label: string }[];
  filters: FilterValues;
  setFilters: Dispatch<SetStateAction<FilterValues>>;
}

// ── Helpers ───────────────────────────────────────────────────────────────────

function checklistFilterOnChange(
  key: keyof FilterValues,
  setFilters: Dispatch<SetStateAction<FilterValues>>,
) {
  return (id: string) => {
    if (id === "all") {
      setFilters((prev) => ({ ...prev, [key]: ["all"] }));
    } else {
      setFilters((prev) => {
        const current = prev[key] as string[];
        const without = current.filter((x) => x !== "all");
        const next = without.includes(id)
          ? without.filter((x) => x !== id)
          : [...without, id];
        return { ...prev, [key]: next };
      });
    }
  };
}

// ── Component ─────────────────────────────────────────────────────────────────

export function FilterSection({
  activeFilters,
  setActiveFilters,
  filterBarDefaultOpen,
  setFilterBarDefaultOpen,
  currentView,
  onSaveView,
  onDeleteView,
  mine,
  currentUserDefaultName,
  ownerFilterOptions,
  filters,
  setFilters,
}: FilterSectionProps) {
  const {
    selectedTeams,
    selectedPeriod,
    selectedStatus,
    selectedOwners,
    selectedItemTypes,
    selectedIndicatorTypes,
    selectedContributions,
    selectedTaskState,
    selectedMissionStatuses,
    selectedSupporters,
  } = filters;

  const [openFilter, setOpenFilter] = useState<string | null>(null);

  const teamChipRef = useRef<HTMLDivElement>(null);
  const periodChipRef = useRef<HTMLDivElement>(null);
  const statusChipRef = useRef<HTMLDivElement>(null);
  const ownerChipRef = useRef<HTMLDivElement>(null);
  const itemTypeChipRef = useRef<HTMLDivElement>(null);
  const indicatorTypeChipRef = useRef<HTMLDivElement>(null);
  const contributionChipRef = useRef<HTMLDivElement>(null);
  const supporterChipRef = useRef<HTMLDivElement>(null);
  const taskStateChipRef = useRef<HTMLDivElement>(null);
  const missionStatusChipRef = useRef<HTMLDivElement>(null);

  const chipRefs: Record<string, RefObject<HTMLDivElement | null>> = {
    team: teamChipRef,
    period: periodChipRef,
    status: statusChipRef,
    owner: ownerChipRef,
    itemType: itemTypeChipRef,
    indicatorType: indicatorTypeChipRef,
    contribution: contributionChipRef,
    supporter: supporterChipRef,
    taskState: taskStateChipRef,
    missionStatus: missionStatusChipRef,
  };

  const ignoreChipRefs = useMemo(
    () => [
      teamChipRef,
      periodChipRef,
      statusChipRef,
      ownerChipRef,
      itemTypeChipRef,
      indicatorTypeChipRef,
      contributionChipRef,
      supporterChipRef,
      taskStateChipRef,
      missionStatusChipRef,
    ],
    [],
  );

  function handleAddFilter(filterId: string) {
    if (!activeFilters.includes(filterId)) {
      setActiveFilters((prev) => [...prev, filterId]);
      setTimeout(() => setOpenFilter(filterId), 0);
    }
  }

  function resetFilterSelection(filterId: string) {
    switch (filterId) {
      case "team":
        setFilters((prev) => ({ ...prev, selectedTeams: ["all"] }));
        break;
      case "period":
        setFilters((prev) => ({ ...prev, selectedPeriod: [null, null] }));
        break;
      case "status":
        setFilters((prev) => ({ ...prev, selectedStatus: "all" }));
        break;
      case "owner":
        setFilters((prev) => ({
          ...prev,
          selectedOwners:
            mine && currentUserDefaultName !== "all"
              ? [currentUserDefaultName]
              : ["all"],
        }));
        break;
      case "itemType":
        setFilters((prev) => ({ ...prev, selectedItemTypes: ["all"] }));
        break;
      case "indicatorType":
        setFilters((prev) => ({ ...prev, selectedIndicatorTypes: ["all"] }));
        break;
      case "contribution":
        setFilters((prev) => ({ ...prev, selectedContributions: ["all"] }));
        break;
      case "supporter":
        setFilters((prev) => ({ ...prev, selectedSupporters: ["all"] }));
        break;
      case "taskState":
        setFilters((prev) => ({ ...prev, selectedTaskState: "all" }));
        break;
      case "missionStatus":
        setFilters((prev) => ({ ...prev, selectedMissionStatuses: ["all"] }));
        break;
    }
  }

  function handleRemoveFilter(filterId: string) {
    setActiveFilters((prev) => prev.filter((f) => f !== filterId));
    resetFilterSelection(filterId);
    setOpenFilter(null);
  }

  function handleClearAll() {
    activeFilters.forEach((filterId) => resetFilterSelection(filterId));
    setActiveFilters([]);
    setOpenFilter(null);
  }

  function formatPeriodLabel(): string {
    const [start, end] = selectedPeriod;
    if (!start && !end) return "Selecionar período";
    const fmt = (d: CalendarDate) =>
      `${String(d.day).padStart(2, "0")}/${String(d.month).padStart(2, "0")}`;
    if (start && end) return `${fmt(start)} - ${fmt(end)}/${end.year}`;
    if (start) return fmt(start);
    return "";
  }

  function getFilterLabel(filterId: string): string {
    const prefixed = (
      prefix: string,
      ids: string[],
      options: { id: string; label: string }[],
    ) => {
      if (ids.length === 0) return prefix;
      return `${prefix}: ${formatMultiLabel(ids, options, prefix)}`;
    };

    switch (filterId) {
      case "team": {
        const selected = selectedTeams.filter((n) => n !== "all");
        if (selected.length === 0) return "Time";
        const [first, ...rest] = selected;
        return `Time: ${first}${rest.length > 0 ? ` +${rest.length}` : ""}`;
      }
      case "period": {
        const periodLabel = formatPeriodLabel();
        return periodLabel === "Selecionar período"
          ? "Período"
          : `Período: ${periodLabel}`;
      }
      case "status":
        return selectedStatus === "all"
          ? "Status"
          : `Status: ${STATUS_OPTIONS.find((s) => s.id === selectedStatus)?.label ?? "Todos"}`;
      case "owner": {
        const selected = selectedOwners.filter((n) => n !== "all");
        if (selected.length === 0) return "Responsável";
        const [first, ...rest] = selected;
        return `Responsável: ${first}${rest.length > 0 ? ` +${rest.length}` : ""}`;
      }
      case "itemType":
        return prefixed("Tipo", selectedItemTypes, ITEM_TYPE_OPTIONS);
      case "indicatorType":
        return prefixed(
          "Indicador",
          selectedIndicatorTypes,
          INDICATOR_TYPE_OPTIONS,
        );
      case "contribution":
        return prefixed(
          "Contribuição",
          selectedContributions,
          CONTRIBUTION_OPTIONS,
        );
      case "supporter":
        return prefixed("Apoio", selectedSupporters, ownerFilterOptions);
      case "taskState":
        return selectedTaskState === "all"
          ? "Tarefa"
          : `Tarefa: ${TASK_STATE_OPTIONS.find((s) => s.id === selectedTaskState)?.label ?? ""}`;
      case "missionStatus":
        return selectedMissionStatuses.includes("all")
          ? "Missão"
          : `Missão: ${formatMultiLabel(selectedMissionStatuses, MISSION_STATUS_OPTIONS, "Todos")}`;
      default:
        return filterId;
    }
  }

  return (
    <>
      <FilterBar
        key={filterBarDefaultOpen ? "open" : "default"}
        filters={FILTER_OPTIONS.filter((f) => !activeFilters.includes(f.id))}
        onAddFilter={(id: string) => {
          setFilterBarDefaultOpen(false);
          handleAddFilter(id);
        }}
        onClearAll={activeFilters.length > 0 ? handleClearAll : undefined}
        onSaveView={activeFilters.length > 0 ? onSaveView : undefined}
        saveViewLabel={
          currentView ? "Atualizar visualização" : "Salvar visualização"
        }
        defaultOpen={filterBarDefaultOpen}
        primaryAction={
          currentView ? (
            <button
              type="button"
              className="inline-flex items-center gap-[var(--sp-3xs)] min-h-[28px] py-[var(--sp-3xs)] pr-[var(--sp-2xs)] pl-[var(--sp-3xs)] bg-transparent border border-transparent rounded-[var(--radius-xs)] [font-family:var(--font-label)] font-medium text-[var(--text-xs)] leading-[1.05] text-[var(--color-red-600)] cursor-pointer whitespace-nowrap transition-colors duration-[120ms] ease-in-out hover:bg-[var(--color-red-50)] hover:text-[var(--color-red-700)] focus-visible:outline focus-visible:outline-2 focus-visible:outline-[var(--color-red-600)] focus-visible:outline-offset-2"
              onClick={onDeleteView}
              aria-label="Excluir visualização"
            >
              <Trash size={14} />
              <span>Excluir</span>
            </button>
          ) : undefined
        }
      >
        {activeFilters.map((filterId) => (
          <div
            key={filterId}
            ref={chipRefs[filterId]}
            style={{ display: "inline-flex" }}
          >
            <FilterChip
              label={getFilterLabel(filterId)}
              icon={filterChipIcons[filterId]}
              active={openFilter === filterId}
              onClick={() =>
                setOpenFilter(openFilter === filterId ? null : filterId)
              }
              onRemove={() => handleRemoveFilter(filterId)}
            />
          </div>
        ))}
      </FilterBar>

      <TeamFilter
        isOpen={openFilter === "team"}
        teamChipRef={teamChipRef}
        ignoreChipRefs={ignoreChipRefs}
        selectedTeams={selectedTeams}
        onClose={() => setOpenFilter(null)}
        onChange={checklistFilterOnChange("selectedTeams", setFilters)}
      />

      <StatusFilter
        isOpen={openFilter === "status"}
        statusChipRef={statusChipRef}
        ignoreChipRefs={ignoreChipRefs}
        selectedStatus={selectedStatus}
        onClose={() => setOpenFilter(null)}
        onChange={(val) =>
          setFilters((prev) => ({ ...prev, selectedStatus: val }))
        }
      />

      <OwnerFilter
        isOpen={openFilter === "owner"}
        ownerChipRef={ownerChipRef}
        ignoreChipRefs={ignoreChipRefs}
        selectedOwners={selectedOwners}
        onClose={() => setOpenFilter(null)}
        onChange={checklistFilterOnChange("selectedOwners", setFilters)}
      />

      <ItemTypeFilter
        isOpen={openFilter === "itemType"}
        itemTypeChipRef={itemTypeChipRef}
        ignoreChipRefs={ignoreChipRefs}
        selectedItemTypes={selectedItemTypes}
        onClose={() => setOpenFilter(null)}
        onChange={checklistFilterOnChange("selectedItemTypes", setFilters)}
      />

      <IndicatorTypeFilter
        isOpen={openFilter === "indicatorType"}
        indicatorTypeChipRef={indicatorTypeChipRef}
        ignoreChipRefs={ignoreChipRefs}
        selectedIndicatorTypes={selectedIndicatorTypes}
        onClose={() => setOpenFilter(null)}
        onChange={checklistFilterOnChange("selectedIndicatorTypes", setFilters)}
      />

      <ContributionFilter
        isOpen={openFilter === "contribution"}
        contributionChipRef={contributionChipRef}
        ignoreChipRefs={ignoreChipRefs}
        selectedContributions={selectedContributions}
        onClose={() => setOpenFilter(null)}
        onChange={checklistFilterOnChange("selectedContributions", setFilters)}
      />

      <SupporterFilter
        isOpen={openFilter === "supporter"}
        supporterChipRef={supporterChipRef}
        ignoreChipRefs={ignoreChipRefs}
        selectedSupporters={selectedSupporters}
        onClose={() => setOpenFilter(null)}
        onChange={checklistFilterOnChange("selectedSupporters", setFilters)}
      />

      <TaskStateFilter
        isOpen={openFilter === "taskState"}
        taskStateChipRef={taskStateChipRef}
        ignoreChipRefs={ignoreChipRefs}
        selectedTaskState={selectedTaskState}
        onClose={() => setOpenFilter(null)}
        onChange={(val) =>
          setFilters((prev) => ({ ...prev, selectedTaskState: val }))
        }
      />

      <MissionStatusFilter
        isOpen={openFilter === "missionStatus"}
        missionStatusChipRef={missionStatusChipRef}
        ignoreChipRefs={ignoreChipRefs}
        selectedMissionStatuses={selectedMissionStatuses}
        onClose={() => setOpenFilter(null)}
        onChange={checklistFilterOnChange(
          "selectedMissionStatuses",
          setFilters,
        )}
      />

      <PeriodFilter
        isOpen={openFilter === "period"}
        periodChipRef={periodChipRef}
        ignoreChipRefs={ignoreChipRefs}
        selectedPeriod={selectedPeriod}
        onClose={() => setOpenFilter(null)}
        onChange={(val) =>
          setFilters((prev) => ({ ...prev, selectedPeriod: val }))
        }
      />
    </>
  );
}
