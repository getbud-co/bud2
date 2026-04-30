import { useState, useRef, useEffect, useMemo } from "react";
import { useSearchParams, useRouter, useParams } from "next/navigation";
import { FILTER_OPTIONS } from "./consts";
import { useTranslation } from "react-i18next";
import {
  Button,
  Card,
  CardBody,
  CardDivider,
  GoalProgressBar,
  Tooltip,
  Breadcrumb,
  toast,
} from "@getbud-co/buds";
import type { CalendarDate } from "@getbud-co/buds";
import { Plus, Info } from "@phosphor-icons/react";
import { PageHeader } from "@/presentation/layout/page-header";
import { MissionDetailsDrawer } from "./components/MissionDetailsDrawer";
import { useMissionContributions } from "./hooks/useMissionContributions";
import { findIndicatorById, flattenMissions } from "./utils/missionTree";
import { useSavedViews } from "@/contexts/SavedViewsContext";
import { useMissionsData } from "@/contexts/MissionsDataContext";
import { usePeopleData } from "@/contexts/PeopleDataContext";
import { useConfigData } from "@/contexts/ConfigDataContext";
import type { SavedView } from "@/contexts/SavedViewsContext";
import type { Mission, Indicator, MissionTask } from "@/types";
import styles from "./MissionsPage.module.css";
import { isoToCalendarDate } from "./utils";
import { CreateMissionModal } from "./components/CreateMissionModal";
import { DeleteMissionModal } from "./components/DeleteMissionModal";
import { DeleteViewModal } from "./components/DeleteViewModal";
import { RemoveContribModal } from "./components/RemoveContribModal";
import { SaveViewModal } from "./components/SaveViewModal";
import {
  ViewModeKanban,
  ViewModeCards,
  ViewModeList,
} from "./components/ViewMode";
import {
  MissionDrawerProvider,
  useMissionDrawer,
} from "./contexts/MissionDrawerContext";
import MissionItem from "./components/MissionItem";
import { FilterSection } from "./components/Filters";
import type { FilterValues } from "./components/Filters";
import { ViewModeFilter } from "./components/Filters/components/ViewModeFilter";
import {
  ExpandedMissionModal,
  collectMissionIds,
} from "./components/ExpandedMissionModal";

/* ——— Component ——— */

export function MyMissionsPage() {
  return <MissionsPage mine />;
}

export function AnnualMissionsPage() {
  const year = new Date().getFullYear();
  return (
    <MissionsPage
      customTitle={`Ano ${year}`}
      initialPeriod={[
        { year, month: 1, day: 1 },
        { year, month: 12, day: 31 },
      ]}
    />
  );
}

export function QuarterlyMissionsPage() {
  const now = new Date();
  const year = now.getFullYear();
  const q = Math.ceil((now.getMonth() + 1) / 3);
  const startMonth = (q - 1) * 3 + 1;
  const endMonth = q * 3;
  const endDay = new Date(year, endMonth, 0).getDate();
  return (
    <MissionsPage
      customTitle={`Q${q} ${year}`}
      initialPeriod={[
        { year, month: startMonth, day: 1 },
        { year, month: endMonth, day: endDay },
      ]}
    />
  );
}

export function MissionDetailPage() {
  const { missionId } = useParams() as { missionId: string };
  return <MissionsPage focusMissionId={missionId} />;
}

function MissionsPageContent({
  mine = false,
  customTitle,
  initialPeriod,
  focusMissionId,
}: {
  mine?: boolean;
  customTitle?: string;
  initialPeriod?: [CalendarDate, CalendarDate];
  focusMissionId?: string;
}) {
  const { missions, setMissions } = useMissionsData();
  const {
    teamOptions,
    ownerOptions,
    currentUser,
    users,
    resolveUserId,
    resolveTeamId,
  } = usePeopleData();
  const {
    activeOrgId,
    tagOptions,
    cyclePresetOptions,
    createTag,
    getTagById,
    resolveTagId,
  } = useConfigData();
  const { openCheckin, drawerOverlayKey } = useMissionDrawer();

  const ownerFilterOptions = useMemo(
    () => [{ id: "all", label: "Todos", initials: "" }, ...ownerOptions],
    [ownerOptions],
  );
  const missionOwnerOptions = useMemo(
    () => ownerFilterOptions.filter((option) => option.id !== "all"),
    [ownerFilterOptions],
  );
  const currentUserOption = useMemo(
    () => currentUser ?? ownerOptions[0] ?? null,
    [currentUser, ownerOptions],
  );
  const currentUserDefaultName = currentUserOption?.label ?? "all";
  const missionTagOptions = useMemo(
    () => tagOptions.map((tag) => ({ id: tag.id, label: tag.label })),
    [tagOptions],
  );
  const presetPeriods = useMemo(
    () =>
      cyclePresetOptions.map((cycle) => ({
        id: cycle.id,
        label: cycle.label,
        start: isoToCalendarDate(cycle.startDate),
        end: isoToCalendarDate(cycle.endDate),
      })),
    [cyclePresetOptions],
  );

  const searchParams = useSearchParams();
  const router = useRouter();
  const { t } = useTranslation("missions");
  const viewId = searchParams.get("view");

  /* ——— "New view" mode from sidebar ——— */
  const isNewViewMode = searchParams.get("newView") === "true";
  const [filterBarDefaultOpen, setFilterBarDefaultOpen] =
    useState(isNewViewMode);

  const [activeFilters, setActiveFilters] = useState<string[]>(
    mine ? ["owner", "period"] : ["team", "period"],
  );
  const [expandedMissions, setExpandedMissions] = useState<Set<string>>(
    new Set(),
  );
  const [expandedMissionId] = useState<string | null>(null);

  function findMissionById(id: string, list: Mission[]): Mission | null {
    for (const m of list) {
      if (m.id === id) return m;
      if (m.children) {
        const found = findMissionById(id, m.children);
        if (found) return found;
      }
    }
    return null;
  }
  const expandedMission = expandedMissionId
    ? findMissionById(expandedMissionId, missions)
    : null;

  const setExpandedMission = (m: Mission | null) => {
    if (m) {
      router.push(`/missions/${m.id}`);
    }
  };

  /* ——— View mode ——— */
  const [viewMode, setViewMode] = useState<"list" | "cards" | "kanban">("list");
  /* ——— Row menu (⋯) for indicators and tasks ——— */
  const [openRowMenu, setOpenRowMenu] = useState<string | null>(null);
  const [openContributeFor, setOpenContributeFor] = useState<string | null>(
    null,
  );
  const [contributePickerSearch, setContributePickerSearch] = useState("");
  const rowMenuBtnRefs = useRef<Record<string, HTMLButtonElement | null>>({});

  const {
    removeContribConfirm,
    setRemoveContribConfirm,
    handleRemoveContribution,
  } = useMissionContributions({
    setMissions,
    setOpenRowMenu,
    setOpenContributeFor,
    setContributePickerSearch,
  });

  const flatMissions = useMemo(() => flattenMissions(missions), [missions]);

  // ── Focus mode: single mission detail view ──
  const focusMission = useMemo(() => {
    if (!focusMissionId) return null;
    function findById(list: Mission[]): Mission | null {
      for (const m of list) {
        if (m.id === focusMissionId) return m;
        if (m.children) {
          const found = findById(m.children);
          if (found) return found;
        }
      }
      return null;
    }
    return findById(missions);
  }, [focusMissionId, missions]);

  const focusBreadcrumb = useMemo(() => {
    if (!focusMission) return [];
    const allFlat: Mission[] = [];
    function collect(list: Mission[]) {
      for (const m of list) {
        allFlat.push(m);
        if (m.children?.length) collect(m.children);
      }
    }
    collect(missions);
    return focusMission.path.map((id) => {
      const m = allFlat.find((x) => x.id === id);
      return {
        label: m?.title ?? id,
        onClick:
          id !== focusMission.id
            ? () => router.push(`/missions/${id}`)
            : undefined,
      };
    });
  }, [focusMission, missions, router]);

  // Auto-expand all items in focus mode (only on initial mount / ID change)
  useEffect(() => {
    if (!focusMissionId) return;
    // Find the mission fresh from current missions state
    function findById(list: Mission[]): Mission | null {
      for (const m of list) {
        if (m.id === focusMissionId) return m;
        if (m.children) {
          const f = findById(m.children);
          if (f) return f;
        }
      }
      return null;
    }
    const m = findById(missions);
    if (m) setExpandedMissions(new Set(collectMissionIds(m)));
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [focusMissionId]);

  function handleToggleTask(taskId: string) {
    setMissions((prev) => {
      function toggleKRTasks(krs: Indicator[]): Indicator[] {
        return krs.map((kr) => ({
          ...kr,
          tasks: kr.tasks?.map((t) =>
            t.id === taskId ? { ...t, isDone: !t.isDone } : t,
          ),
          children: kr.children ? toggleKRTasks(kr.children) : undefined,
        }));
      }
      function toggleInList(list: Mission[]): Mission[] {
        return list.map((m) => ({
          ...m,
          tasks: m.tasks?.map((t) =>
            t.id === taskId ? { ...t, isDone: !t.isDone } : t,
          ),
          indicators: toggleKRTasks(m.indicators ?? []),
          children: m.children ? toggleInList(m.children) : undefined,
        }));
      }
      return toggleInList(prev);
    });
  }

  function handleToggleSubtask(taskId: string, subtaskId: string) {
    setMissions((prev) => {
      function toggleSub(t: MissionTask): MissionTask {
        if (t.id !== taskId) return t;
        return {
          ...t,
          subtasks: t.subtasks?.map((s) =>
            s.id === subtaskId ? { ...s, isDone: !s.isDone } : s,
          ),
        };
      }
      function updateKRs(krs: Indicator[]): Indicator[] {
        return krs.map((kr) => ({
          ...kr,
          tasks: kr.tasks?.map(toggleSub),
          children: kr.children ? updateKRs(kr.children) : undefined,
        }));
      }
      function updateMissions(list: Mission[]): Mission[] {
        return list.map((m) => ({
          ...m,
          tasks: m.tasks?.map(toggleSub),
          indicators: updateKRs(m.indicators ?? []),
          children: m.children ? updateMissions(m.children) : undefined,
        }));
      }
      return updateMissions(prev);
    });
  }

  /* ——— Filter values ——— */
  const [filters, setFilters] = useState<FilterValues>({
    selectedTeams: ["all"],
    selectedPeriod: initialPeriod ?? [null, null],
    selectedStatus: "all",
    selectedOwners:
      mine && currentUserDefaultName !== "all"
        ? [currentUserDefaultName]
        : ["all"],
    selectedItemTypes: ["all"],
    selectedIndicatorTypes: ["all"],
    selectedContributions: ["all"],
    selectedTaskState: "all",
    selectedMissionStatuses: ["all"],
    selectedSupporters: ["all"],
  });
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

  useEffect(() => {
    if (!mine || currentUserDefaultName === "all") return;
    setFilters((prev) => ({
      ...prev,
      selectedOwners:
        prev.selectedOwners.length === 0 || prev.selectedOwners.includes("all")
          ? [currentUserDefaultName]
          : prev.selectedOwners,
    }));
  }, [mine, currentUserDefaultName]);

  /* ——— Pre-filter by owner when navigating from team health view ——— */
  // CollaboratorProfileModal passes filter params via search params
  // when the manager clicks "Ver missões" on a collaborator card.
  const filterOwnerUserId = searchParams.get("filterOwnerUserId");
  const filterSupporterUserId = searchParams.get("filterSupporterUserId");
  const openCheckinKrId = searchParams.get("openCheckinKrId");
  const filterPeriodStart = searchParams.get("filterPeriodStart");
  const filterPeriodEnd = searchParams.get("filterPeriodEnd");
  const filterPeriod =
    filterPeriodStart && filterPeriodEnd
      ? { startDate: filterPeriodStart, endDate: filterPeriodEnd }
      : null;

  useEffect(() => {
    const hasOwner = !!filterOwnerUserId;
    const hasSupporter = !!filterSupporterUserId;
    const hasPeriod = !!filterPeriod;
    if (!hasOwner && !hasSupporter && !hasPeriod) return;
    setActiveFilters((prev) => {
      let next = prev.filter((f) => f !== "team");
      if (hasOwner && !next.includes("owner")) next = ["owner", ...next];
      if (hasSupporter && !next.includes("supporter"))
        next = ["supporter", ...next];
      if (hasPeriod && !next.includes("period")) next = [...next, "period"];
      return next;
    });
    if (hasOwner) {
      const ownerName = ownerOptions.find(
        (o) => o.id === filterOwnerUserId,
      )?.label;
      setFilters((prev) => ({
        ...prev,
        selectedOwners: ownerName ? [ownerName] : ["all"],
      }));
    }
    if (hasSupporter && filterSupporterUserId) {
      setFilters((prev) => ({
        ...prev,
        selectedSupporters: [filterSupporterUserId],
      }));
    }
    if (filterPeriod) {
      const [sy, sm, sd] = filterPeriod.startDate.split("-").map(Number);
      const [ey, em, ed] = filterPeriod.endDate.split("-").map(Number);
      setFilters((prev) => ({
        ...prev,
        selectedPeriod: [
          { year: sy!, month: sm!, day: sd! },
          { year: ey!, month: em!, day: ed! },
        ],
      }));
    }
    // Clear params so a page refresh doesn't re-apply filters
    router.replace(window.location.pathname);
  }, [filterOwnerUserId, filterSupporterUserId, filterPeriod]); // eslint-disable-line react-hooks/exhaustive-deps

  // Open drawer for a specific KR when navigating from Home activities
  useEffect(() => {
    if (!openCheckinKrId || missions.length === 0) return;
    const kr = findIndicatorById(openCheckinKrId, missions);
    if (kr) {
      openCheckin({
        keyResult: kr,
        currentValue: kr.progress,
        newValue: kr.progress,
      });
    }
    router.replace(window.location.pathname);
  }, [openCheckinKrId, missions]); // eslint-disable-line react-hooks/exhaustive-deps

  /* ——— Save view ——— */
  const { views, addView, updateView, deleteView } = useSavedViews();
  const [saveModalOpen, setSaveModalOpen] = useState(false);
  const [deleteModalOpen, setDeleteModalOpen] = useState(false);
  const [viewName, setViewName] = useState("");

  /* ——— Create / Edit mission ——— */
  const [editingMissionId, setEditingMissionId] = useState<string | null>(null);
  const [createOpen, setCreateOpen] = useState(false);

  /* ——— Current saved view ——— */
  const currentView: SavedView | undefined = viewId
    ? views.find((v) => v.id === viewId)
    : undefined;

  /* ——— Load saved view filters when URL changes ——— */
  useEffect(() => {
    if (currentView) {
      const f = currentView.filters;
      setActiveFilters(f.activeFilters);
      setFilters({
        selectedTeams: f.selectedTeams,
        selectedPeriod: f.selectedPeriod,
        selectedStatus: f.selectedStatus,
        selectedOwners: f.selectedOwners,
        selectedItemTypes: f.selectedItemTypes ?? ["all"],
        selectedIndicatorTypes: f.selectedIndicatorTypes ?? ["all"],
        selectedContributions: f.selectedContributions ?? ["all"],
        selectedTaskState: f.selectedTaskState ?? "all",
        selectedMissionStatuses: f.selectedMissionStatuses ?? ["all"],
        selectedSupporters: f.selectedSupporters ?? ["all"],
      });
    }
  }, [viewId]); // eslint-disable-line react-hooks/exhaustive-deps

  /* ——— New view mode: clear filters and open popover ——— */
  useEffect(() => {
    if (isNewViewMode) {
      setActiveFilters([]);
      setFilters({
        selectedTeams: ["all"],
        selectedPeriod: [null, null],
        selectedStatus: "all",
        selectedOwners:
          mine && currentUserDefaultName !== "all"
            ? [currentUserDefaultName]
            : ["all"],
        selectedItemTypes: ["all"],
        selectedIndicatorTypes: ["all"],
        selectedContributions: ["all"],
        selectedTaskState: "all",
        selectedMissionStatuses: ["all"],
        selectedSupporters: ["all"],
      });
      setFilterBarDefaultOpen(true);
      // Clear navigation state so refresh doesn't re-trigger
      window.history.replaceState({}, "");
    }
  }, [isNewViewMode, mine, currentUserDefaultName]);

  /* ——— Filtered missions ——— */
  const ownerFilterActive =
    activeFilters.includes("owner") &&
    !selectedOwners.includes("all") &&
    selectedOwners.length > 0;
  const teamFilterActive =
    activeFilters.includes("team") &&
    !selectedTeams.includes("all") &&
    selectedTeams.length > 0;
  const periodFilterActive =
    activeFilters.includes("period") &&
    (!!selectedPeriod[0] || !!selectedPeriod[1]);
  const statusFilterActive =
    activeFilters.includes("status") && selectedStatus !== "all";
  const itemTypeFilterActive =
    activeFilters.includes("itemType") &&
    !selectedItemTypes.includes("all") &&
    selectedItemTypes.length > 0;
  const indicatorTypeFilterActive =
    activeFilters.includes("indicatorType") &&
    !selectedIndicatorTypes.includes("all") &&
    selectedIndicatorTypes.length > 0;
  const contributionFilterActive =
    activeFilters.includes("contribution") &&
    !selectedContributions.includes("all") &&
    selectedContributions.length > 0;
  const taskStateFilterActive =
    activeFilters.includes("taskState") && selectedTaskState !== "all";
  const missionStatusFilterActive =
    activeFilters.includes("missionStatus") &&
    !selectedMissionStatuses.includes("all") &&
    selectedMissionStatuses.length > 0;
  const supporterFilterActive =
    activeFilters.includes("supporter") &&
    !selectedSupporters.includes("all") &&
    selectedSupporters.length > 0;

  const userTeamsMap = useMemo(() => {
    // user.teams contains team names, not IDs — resolve to IDs
    const nameToId = new Map(teamOptions.map((t) => [t.label, t.id]));
    const map = new Map<string, Set<string>>();
    for (const user of users) {
      const resolvedTeams = new Set(
        user.teams
          .map((name) => nameToId.get(name))
          .filter((id): id is string => !!id),
      );
      map.set(user.id, resolvedTeams);
    }
    return map;
  }, [users, teamOptions]);

  const displayedMissions = useMemo(() => {
    const nameToTeamId = new Map(teamOptions.map((t) => [t.label, t.id]));
    const selectedTeamSet = new Set(
      selectedTeams
        .filter((n) => n !== "all")
        .map((n) => resolveTeamId(nameToTeamId.get(n) ?? n)),
    );
    const selectedItemTypeSet = new Set(
      selectedItemTypes.filter((id) => id !== "all"),
    );
    const selectedIndicatorTypeSet = new Set(
      selectedIndicatorTypes.filter((id) => id !== "all"),
    );
    const selectedContributionSet = new Set(
      selectedContributions.filter((id) => id !== "all"),
    );
    const selectedMissionStatusSet = new Set(
      selectedMissionStatuses.filter((id) => id !== "all"),
    );
    const nameToOwner = new Map(ownerOptions.map((o) => [o.label, o]));
    const selectedOwnerIds = new Set(
      selectedOwners
        .filter((n) => n !== "all")
        .map((n) => resolveUserId(nameToOwner.get(n)?.id ?? n).toLowerCase()),
    );
    const selectedOwnerInitials = new Set(
      selectedOwners
        .filter((n) => n !== "all")
        .map((n) => nameToOwner.get(n)?.initials?.toLowerCase() ?? "")
        .filter((v) => v.length > 0),
    );
    const statusValue = selectedStatus.replace("-", "_");

    function ownerBelongsToSelectedTeam(
      ownerId: string | undefined | null,
    ): boolean {
      if (!ownerId) return false;
      const ownerTeams = userTeamsMap.get(resolveUserId(ownerId));
      if (!ownerTeams) return false;
      for (const tid of selectedTeamSet) {
        if (ownerTeams.has(tid)) return true;
      }
      return false;
    }

    function toTimestampFromCalendar(
      value: CalendarDate | null,
    ): number | null {
      if (!value) return null;
      return new Date(value.year, value.month - 1, value.day).getTime();
    }

    function toTimestampFromIso(
      value: string | null | undefined,
    ): number | null {
      if (!value) return null;
      const parsed = new Date(value).getTime();
      return Number.isNaN(parsed) ? null : parsed;
    }

    function dateRangeMatches(
      startIso: string | null | undefined,
      endIso: string | null | undefined,
    ): boolean {
      if (!periodFilterActive) return true;

      const filterStart = toTimestampFromCalendar(selectedPeriod[0]);
      const filterEnd = toTimestampFromCalendar(selectedPeriod[1]);
      const normalizedFilterStart = filterStart ?? filterEnd;
      const normalizedFilterEnd = filterEnd ?? filterStart;

      if (normalizedFilterStart === null || normalizedFilterEnd === null) {
        return true;
      }

      const start = toTimestampFromIso(startIso);
      const end = toTimestampFromIso(endIso);
      const normalizedStart = start ?? end;
      const normalizedEnd = end ?? start;

      if (normalizedStart === null || normalizedEnd === null) {
        return false;
      }

      return (
        normalizedStart <= normalizedFilterEnd &&
        normalizedEnd >= normalizedFilterStart
      );
    }

    function ownerMatches(owner?: {
      id: string;
      firstName: string;
      lastName: string;
      initials: string | null;
    }): boolean {
      if (!ownerFilterActive) return true;
      if (!owner) return false;

      const initials = owner.initials || "";
      return (
        selectedOwnerInitials.has(initials) ||
        selectedOwnerIds.has(resolveUserId(owner.id).toLowerCase())
      );
    }

    // Matches missions where the selected users are in the support team (role="supporter")
    const selectedSupporterIds = new Set(
      selectedSupporters
        .filter((id) => id !== "all")
        .map((id) => resolveUserId(id).toLowerCase()),
    );

    function missionSupporterMatches(mission: Mission): boolean {
      if (!supporterFilterActive) return true;
      return (mission.members ?? []).some(
        (m) =>
          m.role === "supporter" &&
          selectedSupporterIds.has(resolveUserId(m.userId).toLowerCase()),
      );
    }

    function keyResultHasContribution(kr: Indicator): boolean {
      if ((kr.contributesTo?.length ?? 0) > 0) return true;
      if (
        (kr.tasks ?? []).some((task) => (task.contributesTo?.length ?? 0) > 0)
      )
        return true;
      if ((kr.children ?? []).some((child) => keyResultHasContribution(child)))
        return true;
      return false;
    }

    function missionHasContribution(mission: Mission): boolean {
      if (
        (mission.tasks ?? []).some(
          (task) => (task.contributesTo?.length ?? 0) > 0,
        )
      )
        return true;
      if ((mission.indicators ?? []).some((kr) => keyResultHasContribution(kr)))
        return true;
      return false;
    }

    function missionContributionMatches(mission: Mission): boolean {
      if (!contributionFilterActive) return true;

      const hasContributing = missionHasContribution(mission);
      const hasReceiving = (mission.externalContributions?.length ?? 0) > 0;
      const hasNone = !hasContributing && !hasReceiving;

      if (selectedContributionSet.has("contributing") && hasContributing)
        return true;
      if (selectedContributionSet.has("receiving") && hasReceiving) return true;
      if (selectedContributionSet.has("none") && hasNone) return true;
      return false;
    }

    function keyResultContributionMatches(kr: Indicator): boolean {
      if (!contributionFilterActive) return true;

      const hasContributing = (kr.contributesTo?.length ?? 0) > 0;
      if (selectedContributionSet.has("contributing") && hasContributing)
        return true;
      if (selectedContributionSet.has("none") && !hasContributing) return true;
      return false;
    }

    function taskContributionMatches(task: MissionTask): boolean {
      if (!contributionFilterActive) return true;

      const hasContributing = (task.contributesTo?.length ?? 0) > 0;
      if (selectedContributionSet.has("contributing") && hasContributing)
        return true;
      if (selectedContributionSet.has("none") && !hasContributing) return true;
      return false;
    }

    function indicatorTypeMatches(kr: Indicator): boolean {
      if (!indicatorTypeFilterActive) return true;
      if (selectedIndicatorTypeSet.has(kr.goalType)) return true;
      if (
        selectedIndicatorTypeSet.has("external") &&
        kr.measurementMode === "external"
      )
        return true;
      if (
        selectedIndicatorTypeSet.has("linked_mission") &&
        kr.measurementMode === "mission"
      )
        return true;
      return false;
    }

    function taskStateMatches(task: MissionTask): boolean {
      if (!taskStateFilterActive) return true;
      if (selectedTaskState === "done") return task.isDone;
      if (selectedTaskState === "pending") return !task.isDone;
      return true;
    }

    function missionStatusMatches(mission: Mission): boolean {
      if (!missionStatusFilterActive) return true;
      return selectedMissionStatusSet.has(mission.status);
    }

    function filterTaskNode(
      task: MissionTask,
      missionScopeMatches: boolean,
    ): MissionTask | null {
      const directMatch =
        missionScopeMatches &&
        (!itemTypeFilterActive || selectedItemTypeSet.has("task")) &&
        !indicatorTypeFilterActive &&
        !statusFilterActive &&
        dateRangeMatches(task.dueDate, task.dueDate) &&
        ownerMatches(task.owner) &&
        taskContributionMatches(task) &&
        taskStateMatches(task);

      return directMatch ? task : null;
    }

    function filterIndicatorNode(
      kr: Indicator,
      missionScopeMatches: boolean,
    ): Indicator | null {
      const nextChildren = (kr.children ?? [])
        .map((child) => filterIndicatorNode(child, missionScopeMatches))
        .filter((child): child is Indicator => !!child);
      const nextTasks = (kr.tasks ?? [])
        .map((task) => filterTaskNode(task, missionScopeMatches))
        .filter((task): task is MissionTask => !!task);

      const directMatch =
        missionScopeMatches &&
        (!itemTypeFilterActive || selectedItemTypeSet.has("indicator")) &&
        !taskStateFilterActive &&
        dateRangeMatches(kr.periodStart, kr.periodEnd) &&
        ownerMatches(kr.owner) &&
        (!statusFilterActive || kr.status === statusValue) &&
        indicatorTypeMatches(kr) &&
        keyResultContributionMatches(kr);

      if (!directMatch && nextChildren.length === 0 && nextTasks.length === 0) {
        return null;
      }

      return {
        ...kr,
        children:
          nextChildren.length > 0 ? nextChildren : kr.children ? [] : undefined,
        tasks: nextTasks.length > 0 ? nextTasks : kr.tasks ? [] : undefined,
      };
    }

    function filterMissionNode(mission: Mission): Mission | null {
      const missionTeamMatches =
        !teamFilterActive || ownerBelongsToSelectedTeam(mission.ownerId);
      const missionScopeMatches =
        missionTeamMatches && missionStatusMatches(mission);

      const nextChildren = (mission.children ?? [])
        .map((child) => filterMissionNode(child))
        .filter((child): child is Mission => !!child);
      const nextIndicators = (mission.indicators ?? [])
        .map((kr) => filterIndicatorNode(kr, missionScopeMatches))
        .filter((kr): kr is Indicator => !!kr);
      const nextTasks = (mission.tasks ?? [])
        .map((task) => filterTaskNode(task, missionScopeMatches))
        .filter((task): task is MissionTask => !!task);

      const directMatch =
        missionScopeMatches &&
        (!itemTypeFilterActive || selectedItemTypeSet.has("mission")) &&
        !indicatorTypeFilterActive &&
        !statusFilterActive &&
        !taskStateFilterActive &&
        dateRangeMatches(mission.dueDate, mission.dueDate) &&
        ownerMatches(mission.owner) &&
        missionContributionMatches(mission) &&
        missionSupporterMatches(mission);

      if (
        !directMatch &&
        nextChildren.length === 0 &&
        nextIndicators.length === 0 &&
        nextTasks.length === 0
      ) {
        return null;
      }

      return {
        ...mission,
        children:
          nextChildren.length > 0
            ? nextChildren
            : mission.children
              ? []
              : undefined,
        indicators:
          nextIndicators.length > 0
            ? nextIndicators
            : mission.indicators
              ? []
              : undefined,
        tasks:
          nextTasks.length > 0 ? nextTasks : mission.tasks ? [] : undefined,
      };
    }

    return missions
      .map((mission) => filterMissionNode(mission))
      .filter((mission): mission is Mission => !!mission);
  }, [
    missions,
    ownerFilterActive,
    teamFilterActive,
    periodFilterActive,
    statusFilterActive,
    itemTypeFilterActive,
    indicatorTypeFilterActive,
    contributionFilterActive,
    supporterFilterActive,
    taskStateFilterActive,
    missionStatusFilterActive,
    selectedOwners,
    selectedTeams,
    selectedPeriod,
    selectedStatus,
    selectedItemTypes,
    selectedIndicatorTypes,
    selectedContributions,
    selectedSupporters,
    selectedTaskState,
    selectedMissionStatuses,
    ownerOptions,
    teamOptions,
    resolveTeamId,
    resolveUserId,
    userTeamsMap,
  ]);

  /* ——— Team context for filtered view ——— */
  const activeTeamNames = selectedTeams.filter((n) => n !== "all");
  const nameToTeamId = new Map(teamOptions.map((t) => [t.label, t.id]));
  const activeTeamIds = activeTeamNames.map((n) =>
    resolveTeamId(nameToTeamId.get(n) ?? n),
  );
  const isSingleTeam = teamFilterActive && activeTeamIds.length === 1;
  const isMultiTeam = teamFilterActive && activeTeamIds.length > 1;

  const singleTeamName = isSingleTeam ? (activeTeamNames[0] ?? null) : null;

  const groupedMissions = useMemo(() => {
    if (!isMultiTeam) return null;

    const activeTeamSet = new Set(activeTeamIds);
    const groups = new Map<
      string,
      { teamName: string; teamColor: string; missions: Mission[] }
    >();

    for (const m of displayedMissions) {
      // Find the owner's team that matches one of the active filter teams
      const ownerTeams = userTeamsMap.get(resolveUserId(m.ownerId));
      let matchedTeamId: string | null = null;
      if (ownerTeams) {
        for (const tid of ownerTeams) {
          if (activeTeamSet.has(tid)) {
            matchedTeamId = tid;
            break;
          }
        }
      }
      const key = matchedTeamId ?? "__no_team__";
      if (!groups.has(key)) {
        const teamOpt = matchedTeamId
          ? teamOptions.find((t) => t.id === matchedTeamId)
          : null;
        groups.set(key, {
          teamName: teamOpt?.label ?? "Sem time",
          teamColor: m.team?.color ?? "neutral",
          missions: [],
        });
      }
      groups.get(key)!.missions.push(m);
    }

    return [...groups.values()].sort((a, b) => {
      if (a.teamName === "Sem time") return 1;
      if (b.teamName === "Sem time") return -1;
      return a.teamName.localeCompare(b.teamName, "pt-BR");
    });
  }, [
    displayedMissions,
    isMultiTeam,
    activeTeamIds,
    userTeamsMap,
    resolveUserId,
    teamOptions,
  ]);

  const totalValue =
    displayedMissions.length > 0
      ? Math.round(
          displayedMissions.reduce((acc, m) => acc + m.progress, 0) /
            displayedMissions.length,
        )
      : 0;
  const totalExpected = 40;
  const activeMissions = displayedMissions.length;
  const outdatedIndicators = displayedMissions.reduce(
    (acc, m) =>
      acc +
      (m.indicators ?? []).filter(
        (indicator) => indicator.status === "off_track",
      ).length,
    0,
  );

  function toggleMission(id: string) {
    setExpandedMissions((prev) => {
      const next = new Set(prev);
      if (next.has(id)) next.delete(id);
      else next.add(id);
      return next;
    });
  }

  function handleOpenSaveModal() {
    setViewName(currentView?.name ?? "");
    setSaveModalOpen(true);
  }

  function getCurrentFilters() {
    return { activeFilters, ...filters };
  }

  function handleSaveView() {
    if (!viewName.trim()) return;
    if (currentView) {
      updateView(currentView.id, {
        name: viewName.trim(),
        filters: getCurrentFilters(),
      });
      setSaveModalOpen(false);
      toast.success(
        `Visualização "${viewName.trim()}" atualizada com sucesso.`,
      );
    } else {
      const newId = addView({
        name: viewName.trim(),
        module: "missions",
        filters: getCurrentFilters(),
      });
      setSaveModalOpen(false);
      toast.success(`Visualização "${viewName.trim()}" salva com sucesso.`);
      router.push(`/missions?view=${newId}`);
    }
    setViewName("");
  }

  function handleDeleteView() {
    setDeleteModalOpen(true);
  }

  function handleConfirmDelete() {
    if (!currentView) return;
    const name = currentView.name;
    deleteView(currentView.id);
    setDeleteModalOpen(false);
    router.push("/missions");
    toast.success(`Visualização "${name}" excluída.`);
  }

  function resetCreateForm() {
    setCreateOpen(false);
    setEditingMissionId(null);
  }

  function handleMissionSubmit(mission: Mission) {
    if (editingMissionId) {
      setMissions((prev) =>
        prev.map((m) =>
          m.id === editingMissionId
            ? { ...mission, status: "active" as const }
            : m,
        ),
      );
      toast.success("Missão atualizada com sucesso!");
    } else {
      setMissions((prev) => [
        ...prev,
        { ...mission, status: "active" as const },
      ]);
      toast.success("Missão criada com sucesso!");
    }
    resetCreateForm();
  }

  function handleMissionDraft(mission: Mission) {
    setMissions((prev) => [...prev, { ...mission, status: "draft" as const }]);
    toast.success("Rascunho salvo!");
    resetCreateForm();
  }

  function handleEditMission(mission: Mission) {
    setEditingMissionId(mission.id);
    setCreateOpen(true);
  }

  const [deleteMissionTarget, setDeleteMissionTarget] =
    useState<Mission | null>(null);

  function handleDeleteMission(mission: Mission) {
    setDeleteMissionTarget(mission);
  }

  function confirmDeleteMission() {
    if (!deleteMissionTarget) return;
    const mission = deleteMissionTarget;

    setMissions((prev) => {
      function removeFromTree(list: Mission[]): Mission[] {
        return list
          .filter((item) => item.id !== mission.id)
          .map((item) => ({
            ...item,
            children: item.children ? removeFromTree(item.children) : undefined,
          }));
      }
      return removeFromTree(prev);
    });

    setExpandedMissions((prev) => {
      const next = new Set(prev);
      next.delete(mission.id);
      return next;
    });

    if (expandedMissionId === mission.id) {
      setExpandedMission(null);
    }

    if (focusMissionId === mission.id) {
      router.push("/missions");
    }

    setDeleteMissionTarget(null);
    toast.success("Missão excluída com sucesso!");
  }

  return (
    <div className={styles.page}>
      <PageHeader
        title={
          focusMission
            ? focusMission.title
            : currentView
              ? currentView.name
              : customTitle
                ? customTitle
                : mine
                  ? t("pageTitle.mine")
                  : isSingleTeam && singleTeamName
                    ? t("pageTitle.team", { team: singleTeamName })
                    : t("pageTitle.all")
        }
      />

      {/* ── Focus mode: single mission detail ── */}
      {focusMission ? (
        <Card padding="sm">
          {focusBreadcrumb.length > 0 && (
            <Breadcrumb
              items={[
                {
                  label: t("pageTitle.all"),
                  onClick: () => router.push("/missions"),
                },
                ...focusBreadcrumb,
              ]}
              current={focusBreadcrumb.length}
            />
          )}
          <CardBody>
            <div className={styles.missionList}>
              <MissionItem
                mission={focusMission}
                isOpen
                hideExpand
                onToggle={toggleMission}
                onExpand={setExpandedMission}
                onEdit={handleEditMission}
                onDelete={handleDeleteMission}
                onToggleTask={handleToggleTask}
                expandedMissions={expandedMissions}
                openRowMenu={openRowMenu}
                setOpenRowMenu={setOpenRowMenu}
                openContributeFor={openContributeFor}
                setOpenContributeFor={setOpenContributeFor}
                contributePickerSearch={contributePickerSearch}
                setContributePickerSearch={setContributePickerSearch}
                rowMenuBtnRefs={rowMenuBtnRefs}
                allMissions={flatMissions}
                onToggleSubtask={handleToggleSubtask}
              />
            </div>
          </CardBody>
        </Card>
      ) : (
        <>
          <Card padding="sm">
            <CardBody>
              <FilterSection
                activeFilters={activeFilters}
                setActiveFilters={setActiveFilters}
                filterBarDefaultOpen={filterBarDefaultOpen}
                setFilterBarDefaultOpen={setFilterBarDefaultOpen}
                currentView={currentView}
                onSaveView={handleOpenSaveModal}
                onDeleteView={handleDeleteView}
                mine={mine}
                currentUserDefaultName={currentUserDefaultName}
                ownerFilterOptions={ownerFilterOptions}
                filters={filters}
                setFilters={setFilters}
              />

              <div className={styles.actionBar}>
                <ViewModeFilter viewMode={viewMode} onChange={setViewMode} />
                <Button
                  variant="primary"
                  size="md"
                  leftIcon={Plus}
                  onClick={() => setCreateOpen(true)}
                >
                  Criar missão
                </Button>
              </div>
            </CardBody>

            <CardDivider />

            <CardBody>
              <div className={styles.summaryRow}>
                <Card padding="sm">
                  <CardBody>
                    <div className={styles.summaryCard}>
                      <span className={styles.summaryLabel}>
                        Progresso geral
                      </span>
                      <GoalProgressBar
                        label=""
                        value={totalValue}
                        target={100}
                        expected={totalExpected}
                        formattedValue={`${totalValue}%`}
                      />
                      <span className={styles.summaryExpected}>
                        Esperado {totalExpected}%
                      </span>
                    </div>
                  </CardBody>
                </Card>

                <Card padding="sm">
                  <CardBody>
                    <div className={styles.summaryCard}>
                      <div className={styles.summaryMetric}>
                        <span className={styles.summaryValue}>
                          {activeMissions}
                        </span>
                        <span className={styles.summaryLabel}>
                          Missões ativas
                        </span>
                      </div>
                      <Tooltip content="Total de missões em andamento no período selecionado">
                        <Info size={16} className={styles.infoIcon} />
                      </Tooltip>
                    </div>
                  </CardBody>
                </Card>

                <Card padding="sm">
                  <CardBody>
                    <div className={styles.summaryCard}>
                      <div className={styles.summaryMetric}>
                        <span className={styles.summaryValueWarning}>
                          {outdatedIndicators}
                        </span>
                        <span className={styles.summaryLabel}>
                          Indicadores desatualizados
                        </span>
                      </div>
                      <Tooltip content="Indicadores com status 'Atrasado' que precisam de atenção">
                        <Info size={16} className={styles.infoIcon} />
                      </Tooltip>
                    </div>
                  </CardBody>
                </Card>
              </div>
            </CardBody>

            <CardDivider />

            {viewMode === "list" && (
              <CardBody>
                <ViewModeList
                  displayedMissions={displayedMissions}
                  groupedMissions={groupedMissions}
                  expandedMissions={expandedMissions}
                  flatMissions={flatMissions}
                  openRowMenu={openRowMenu}
                  setOpenRowMenu={setOpenRowMenu}
                  openContributeFor={openContributeFor}
                  setOpenContributeFor={setOpenContributeFor}
                  contributePickerSearch={contributePickerSearch}
                  setContributePickerSearch={setContributePickerSearch}
                  rowMenuBtnRefs={rowMenuBtnRefs}
                  onToggle={toggleMission}
                  onExpand={setExpandedMission}
                  onEdit={handleEditMission}
                  onDelete={handleDeleteMission}
                  onToggleTask={handleToggleTask}
                  onToggleSubtask={handleToggleSubtask}
                />
              </CardBody>
            )}

            {viewMode === "cards" && (
              <CardBody>
                <ViewModeCards
                  displayedMissions={displayedMissions}
                  groupedMissions={groupedMissions}
                  ownerFilterOptions={ownerFilterOptions}
                  onExpand={setExpandedMission}
                  onEdit={handleEditMission}
                  onDelete={handleDeleteMission}
                />
              </CardBody>
            )}

            {viewMode === "kanban" && (
              <CardBody>
                <ViewModeKanban
                  displayedMissions={displayedMissions}
                  missions={missions}
                  isMultiTeam={isMultiTeam}
                  onToggleTask={handleToggleTask}
                />
              </CardBody>
            )}
          </Card>
        </>
      )}

      <SaveViewModal
        open={saveModalOpen}
        filters={activeFilters}
        isUpdate={!!currentView}
        viewName={viewName}
        onViewNameChange={setViewName}
        activeFilters={activeFilters}
        filterOptions={FILTER_OPTIONS}
        onClose={() => setSaveModalOpen(false)}
        onConfirm={handleSaveView}
      />

      <ExpandedMissionModal
        mission={expandedMission}
        onClose={() => setExpandedMission(null)}
        onExpand={setExpandedMission}
        onEdit={handleEditMission}
        onDelete={handleDeleteMission}
        onToggleTask={handleToggleTask}
        onToggleSubtask={handleToggleSubtask}
        allMissions={flatMissions}
      />

      <DeleteViewModal
        open={deleteModalOpen}
        viewName={currentView?.name}
        onClose={() => setDeleteModalOpen(false)}
        onConfirm={handleConfirmDelete}
      />

      <RemoveContribModal
        target={removeContribConfirm}
        onClose={() => setRemoveContribConfirm(null)}
        onConfirm={handleRemoveContribution}
      />
      <MissionDetailsDrawer
        key={`mission-details-drawer-${drawerOverlayKey}`}
      />

      <DeleteMissionModal
        mission={deleteMissionTarget}
        onClose={() => setDeleteMissionTarget(null)}
        onConfirm={confirmDeleteMission}
      />

      <CreateMissionModal
        open={createOpen}
        editingMission={
          editingMissionId ? findMissionById(editingMissionId, missions) : null
        }
        onClose={resetCreateForm}
        onSubmit={handleMissionSubmit}
        onDraft={handleMissionDraft}
        missionOwnerOptions={missionOwnerOptions}
        missionTagOptions={missionTagOptions}
        presetPeriods={presetPeriods}
        currentUserOption={currentUserOption}
        activeOrgId={activeOrgId}
        teamOptions={teamOptions}
        missionsCount={missions.length}
        createTag={createTag}
        resolveTagId={resolveTagId}
        getTagById={getTagById}
      />
    </div>
  );
}

export function MissionsPage(props: {
  mine?: boolean;
  customTitle?: string;
  initialPeriod?: [CalendarDate, CalendarDate];
  focusMissionId?: string;
}) {
  return (
    <MissionDrawerProvider>
      <MissionsPageContent {...props} />
    </MissionDrawerProvider>
  );
}
