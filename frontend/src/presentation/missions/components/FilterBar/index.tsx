import { useState, useRef, useMemo } from "react";
import type { RefObject, Dispatch, SetStateAction } from "react";
import { FilterBar, FilterChip, CardBody, Button } from "@mdonangelo/bud-ds";
import type { CalendarDate } from "@mdonangelo/bud-ds";
import {
  Users,
  CalendarBlank,
  FunnelSimple,
  User,
  ListBullets,
  Crosshair,
  GitBranch,
  UsersThree,
  ListChecks,
  Target,
  Trash,
  CaretDown,
  Plus,
  SquaresFour,
  Kanban,
} from "@phosphor-icons/react";
import type { SavedView } from "@/contexts/SavedViewsContext";
import type { OwnerOption } from "@/contexts/PeopleDataContext";
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
import { TeamFilter } from "./TeamFilter";
import { StatusFilter } from "./StatusFilter";
import { OwnerFilter } from "./OwnerFilter";
import { ItemTypeFilter } from "./ItemTypeFilter";
import { IndicatorTypeFilter } from "./IndicatorTypeFilter";
import { ContributionFilter } from "./ContributionFilter";
import { SupporterFilter } from "./SupporterFilter";
import { TaskStateFilter } from "./TaskStateFilter";
import { MissionStatusFilter } from "./MissionStatusFilter";
import { PeriodFilter } from "./PeriodFilter";
import { ViewModeFilter } from "./ViewModeFilter";

type ViewMode = "list" | "cards" | "kanban";

const filterChipIcons: Record<string, typeof Users | undefined> = {
  team: Users,
  period: CalendarBlank,
  status: FunnelSimple,
  owner: User,
  itemType: ListBullets,
  indicatorType: Crosshair,
  contribution: GitBranch,
  supporter: UsersThree,
  taskState: ListChecks,
  missionStatus: Target,
};

interface MissionsFilterBarProps {
  // Filter list state (kept in parent — used for filtering missions)
  activeFilters: string[];
  setActiveFilters: Dispatch<SetStateAction<string[]>>;
  currentView: SavedView | undefined;
  // Parent handlers (open modals that live in parent)
  handleOpenSaveModal: () => void;
  handleDeleteView: () => void;
  // Needed for owner filter reset default
  mine: boolean;
  currentUserDefaultId: string;
  // Team
  teamFilterOptions: Array<{ id: string; label: string }>;
  selectedTeams: string[];
  setSelectedTeams: Dispatch<SetStateAction<string[]>>;
  resolveTeamId: (id: string) => string;
  // Status
  selectedStatus: string;
  setSelectedStatus: Dispatch<SetStateAction<string>>;
  // Owner
  ownerFilterOptions: Array<
    OwnerOption | { id: string; label: string; initials: string }
  >;
  selectedOwners: string[];
  setSelectedOwners: Dispatch<SetStateAction<string[]>>;
  resolveUserId: (id: string) => string;
  // Item type
  selectedItemTypes: string[];
  setSelectedItemTypes: Dispatch<SetStateAction<string[]>>;
  // Indicator type
  selectedIndicatorTypes: string[];
  setSelectedIndicatorTypes: Dispatch<SetStateAction<string[]>>;
  // Contribution
  selectedContributions: string[];
  setSelectedContributions: Dispatch<SetStateAction<string[]>>;
  // Supporter
  selectedSupporters: string[];
  setSelectedSupporters: Dispatch<SetStateAction<string[]>>;
  // Task state
  selectedTaskState: string;
  setSelectedTaskState: Dispatch<SetStateAction<string>>;
  // Mission status
  selectedMissionStatuses: string[];
  setSelectedMissionStatuses: Dispatch<SetStateAction<string[]>>;
  // Period
  presetPeriods: Array<{
    id: string;
    label: string;
    start: CalendarDate;
    end: CalendarDate;
  }>;
  selectedPeriod: [CalendarDate | null, CalendarDate | null];
  setSelectedPeriod: Dispatch<
    SetStateAction<[CalendarDate | null, CalendarDate | null]>
  >;
  // View mode
  viewMode: ViewMode;
  setViewMode: Dispatch<SetStateAction<ViewMode>>;
  // Create button
  onCreateMission: () => void;
}

export function MissionsFilterBar({
  activeFilters,
  setActiveFilters,
  currentView,
  handleOpenSaveModal,
  handleDeleteView,
  mine,
  currentUserDefaultId,
  teamFilterOptions,
  selectedTeams,
  setSelectedTeams,
  resolveTeamId,
  selectedStatus,
  setSelectedStatus,
  ownerFilterOptions,
  selectedOwners,
  setSelectedOwners,
  resolveUserId,
  selectedItemTypes,
  setSelectedItemTypes,
  selectedIndicatorTypes,
  setSelectedIndicatorTypes,
  selectedContributions,
  setSelectedContributions,
  selectedSupporters,
  setSelectedSupporters,
  selectedTaskState,
  setSelectedTaskState,
  selectedMissionStatuses,
  setSelectedMissionStatuses,
  presetPeriods,
  selectedPeriod,
  setSelectedPeriod,
  viewMode,
  setViewMode,
  onCreateMission,
}: MissionsFilterBarProps) {
  /* ——— Internal UI state ——— */
  const [filterBarDefaultOpen, setFilterBarDefaultOpen] = useState(false);
  const [openFilter, setOpenFilter] = useState<string | null>(null);
  const [viewModeOpen, setViewModeOpen] = useState(false);
  const [filterPeriodCustom, setFilterPeriodCustom] = useState(false);

  /* ——— Chip anchor refs ——— */
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
  const filterPeriodCustomBtnRef = useRef<HTMLButtonElement>(null);
  const viewModeBtnRef = useRef<HTMLButtonElement>(null);

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

  const ignoreChipRefs = useMemo(() => Object.values(chipRefs), []);

  /* ——— Helpers ——— */
  function formatPeriodLabel(): string {
    const [start, end] = selectedPeriod;
    if (!start && !end) return "Selecionar período";
    const fmt = (d: CalendarDate) =>
      `${String(d.day).padStart(2, "0")}/${String(d.month).padStart(2, "0")}`;
    if (start && end) return `${fmt(start)} - ${fmt(end)}/${end.year}`;
    if (start) return fmt(start);
    return "";
  }

  function resetFilterSelection(filterId: string) {
    switch (filterId) {
      case "team":
        setSelectedTeams(["all"]);
        break;
      case "period":
        setSelectedPeriod([null, null]);
        setFilterPeriodCustom(false);
        break;
      case "status":
        setSelectedStatus("all");
        break;
      case "owner":
        setSelectedOwners(
          mine && currentUserDefaultId !== "all"
            ? [currentUserDefaultId]
            : ["all"],
        );
        break;
      case "itemType":
        setSelectedItemTypes(["all"]);
        break;
      case "indicatorType":
        setSelectedIndicatorTypes(["all"]);
        break;
      case "contribution":
        setSelectedContributions(["all"]);
        break;
      case "supporter":
        setSelectedSupporters(["all"]);
        break;
      case "taskState":
        setSelectedTaskState("all");
        break;
      case "missionStatus":
        setSelectedMissionStatuses(["all"]);
        break;
      default:
        break;
    }
  }

  function handleAddFilter(filterId: string) {
    if (!activeFilters.includes(filterId)) {
      setActiveFilters((prev) => [...prev, filterId]);
      setTimeout(() => setOpenFilter(filterId), 0);
    }
    setFilterBarDefaultOpen(false);
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
      case "team":
        return prefixed("Time", selectedTeams, teamFilterOptions);
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
      case "owner":
        return prefixed("Responsável", selectedOwners, ownerFilterOptions);
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

  const closeFilter = () => setOpenFilter(null);

  return (
    <CardBody>
      <FilterBar
        key={filterBarDefaultOpen ? "open" : "default"}
        filters={FILTER_OPTIONS.filter((f) => !activeFilters.includes(f.id))}
        onAddFilter={(id: string) => handleAddFilter(id)}
        onClearAll={activeFilters.length > 0 ? handleClearAll : undefined}
        onSaveView={
          activeFilters.length > 0
            ? () => {
                setOpenFilter(null);
                handleOpenSaveModal();
              }
            : undefined
        }
        saveViewLabel={
          currentView ? "Atualizar visualização" : "Salvar visualização"
        }
        defaultOpen={filterBarDefaultOpen}
        primaryAction={
          currentView ? (
            <button
              type="button"
              className="inline-flex items-center gap-1 min-h-[28px] px-2 py-1 pl-1 bg-transparent border border-transparent rounded-[var(--radius-xs)] font-[var(--font-label)] font-medium text-[var(--text-xs)] text-[var(--color-red-600)] cursor-pointer whitespace-nowrap transition-colors duration-[120ms] hover:bg-[var(--color-red-50)] hover:text-[var(--color-red-700)] focus-visible:outline focus-visible:outline-2 focus-visible:outline-[var(--color-red-600)] focus-visible:outline-offset-2"
              onClick={handleDeleteView}
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

      {/* ——— Filter dropdowns ——— */}

      <TeamFilter
        open={openFilter === "team"}
        onClose={closeFilter}
        anchorRef={teamChipRef}
        ignoreRefs={ignoreChipRefs}
        options={teamFilterOptions}
        selectedTeams={selectedTeams}
        setSelectedTeams={setSelectedTeams}
        resolveTeamId={resolveTeamId}
      />

      <StatusFilter
        open={openFilter === "status"}
        onClose={closeFilter}
        anchorRef={statusChipRef}
        ignoreRefs={ignoreChipRefs}
        options={STATUS_OPTIONS}
        selectedStatus={selectedStatus}
        setSelectedStatus={setSelectedStatus}
      />

      <OwnerFilter
        open={openFilter === "owner"}
        onClose={closeFilter}
        anchorRef={ownerChipRef}
        ignoreRefs={ignoreChipRefs}
        options={ownerFilterOptions}
        selectedOwners={selectedOwners}
        setSelectedOwners={setSelectedOwners}
        resolveUserId={resolveUserId}
      />

      <ItemTypeFilter
        open={openFilter === "itemType"}
        onClose={closeFilter}
        anchorRef={itemTypeChipRef}
        ignoreRefs={ignoreChipRefs}
        options={ITEM_TYPE_OPTIONS}
        selectedItemTypes={selectedItemTypes}
        setSelectedItemTypes={setSelectedItemTypes}
      />

      <IndicatorTypeFilter
        open={openFilter === "indicatorType"}
        onClose={closeFilter}
        anchorRef={indicatorTypeChipRef}
        ignoreRefs={ignoreChipRefs}
        options={INDICATOR_TYPE_OPTIONS}
        selectedIndicatorTypes={selectedIndicatorTypes}
        setSelectedIndicatorTypes={setSelectedIndicatorTypes}
      />

      <ContributionFilter
        open={openFilter === "contribution"}
        onClose={closeFilter}
        anchorRef={contributionChipRef}
        ignoreRefs={ignoreChipRefs}
        options={CONTRIBUTION_OPTIONS}
        selectedContributions={selectedContributions}
        setSelectedContributions={setSelectedContributions}
      />

      <SupporterFilter
        open={openFilter === "supporter"}
        onClose={closeFilter}
        anchorRef={supporterChipRef}
        ignoreRefs={ignoreChipRefs}
        options={ownerFilterOptions}
        selectedSupporters={selectedSupporters}
        setSelectedSupporters={setSelectedSupporters}
      />

      <TaskStateFilter
        open={openFilter === "taskState"}
        onClose={closeFilter}
        anchorRef={taskStateChipRef}
        ignoreRefs={ignoreChipRefs}
        options={TASK_STATE_OPTIONS}
        selectedTaskState={selectedTaskState}
        setSelectedTaskState={setSelectedTaskState}
      />

      <MissionStatusFilter
        open={openFilter === "missionStatus"}
        onClose={closeFilter}
        anchorRef={missionStatusChipRef}
        ignoreRefs={ignoreChipRefs}
        options={MISSION_STATUS_OPTIONS}
        selectedMissionStatuses={selectedMissionStatuses}
        setSelectedMissionStatuses={setSelectedMissionStatuses}
      />

      {/* Period filter dropdown — presets */}
      <PeriodFilter
        open={openFilter === "period"}
        onClose={closeFilter}
        anchorRef={periodChipRef}
        ignoreRefs={ignoreChipRefs}
        presetPeriods={presetPeriods}
        selectedPeriod={selectedPeriod}
        setSelectedPeriod={setSelectedPeriod}
        filterPeriodCustom={filterPeriodCustom}
        setFilterPeriodCustom={setFilterPeriodCustom}
        filterPeriodCustomBtnRef={filterPeriodCustomBtnRef}
      />

      <div className="flex items-center justify-end gap-2 mt-3">
        <Button
          ref={viewModeBtnRef}
          variant="secondary"
          size="md"
          leftIcon={
            viewMode === "list"
              ? ListBullets
              : viewMode === "cards"
                ? SquaresFour
                : Kanban
          }
          rightIcon={CaretDown}
          onClick={() => setViewModeOpen((v) => !v)}
        >
          {viewMode === "list"
            ? "Vendo em lista"
            : viewMode === "cards"
              ? "Vendo em cartões"
              : "Vendo em kanban"}
        </Button>
        <ViewModeFilter
          open={viewModeOpen}
          onClose={() => setViewModeOpen(false)}
          anchorRef={viewModeBtnRef}
          viewMode={viewMode}
          setViewMode={setViewMode}
        />
        <Button
          variant="primary"
          size="md"
          leftIcon={Plus}
          onClick={onCreateMission}
        >
          Criar missão
        </Button>
      </div>
    </CardBody>
  );
}
