"use client";

import { useState, useRef, useEffect, useMemo } from "react";
import { useSearchParams, useRouter } from "next/navigation";
import { Card, CardDivider, toast } from "@getbud-co/buds";
import type { CalendarDate } from "@getbud-co/buds";

import { formatMultiLabel } from "@/components/PopoverSelect";
import { PageHeader } from "@/presentation/layout/page-header";
import { MissionDetailsDrawer } from "./components/MissionDetailsDrawer";
import { MissionsFilterBar } from "./components/FilterBar";
import { ResumeCards } from "./components/ResumeCards";
import { ViewListMode } from "./components/ViewListMode";
import { ViewCardsMode } from "./components/ViewCardsMode";
import { ViewKanbanMode, type KanbanItem } from "./components/ViewKanbanMode";
import { EditViewModal } from "./components/EditViewModal";
import { ExpandedMissionModal } from "./components/ExpandedMissionModal";
import { DeleteViewModal } from "./components/DeleteViewModal";
import { CreateMissionModal } from "./components/CreateMissionModal";
import { RemoveContribModal } from "./components/RemoveContribModal";
import { useMissionMentions } from "./hooks/useMissionMentions";
import { useMissionContributions } from "./hooks/useMissionContributions";
import {
  findParentMission,
  findIndicatorById,
  findTaskInMissions,
  flattenMissions,
} from "./utils/missionTree";
import {
  buildCheckInChartData,
  sortCheckInsDesc,
} from "./utils/checkinReadModels";
import { useSavedViews } from "@/contexts/SavedViewsContext";
import { useMissionsData } from "@/contexts/MissionsDataContext";
import { usePeopleData } from "@/contexts/PeopleDataContext";
import { useConfigData } from "@/contexts/ConfigDataContext";
import type { SavedView } from "@/contexts/SavedViewsContext";
import type {
  Mission,
  Indicator,
  MissionTask,
  KanbanStatus,
  ConfidenceLevel,
  CheckinPayload,
} from "@/types";
import {
  numVal,
  getMissionLabel,
  getOwnerName,
  getOwnerInitials,
  getIndicatorIcon,
} from "@/lib/tempStorage/missions";
import { isoToCalendarDate } from "./utils/utils";
import {
  CONFIDENCE_OPTIONS,
  CONTRIBUTION_OPTIONS,
  DRAWER_TASKS_BY_INDICATOR,
  INDICATOR_TYPE_OPTIONS,
  ITEM_TYPE_OPTIONS,
  MISSION_STATUS_OPTIONS,
  STATUS_OPTIONS,
  TASK_STATE_OPTIONS,
} from "./consts";
import type { MissionItemData } from "./types";
import { MissionItemInlineForm } from "./components/MissionItemInlineForm";

export function MyMissionsPage() {
  return <MissionsComponent mine />;
}

export function MissionsComponent({ mine = false }: { mine?: boolean }) {
  const {
    missions,
    setMissions,
    getCheckInsByIndicator,
    getCheckInSyncMeta,
    createCheckIn,
    updateCheckIn,
    deleteCheckIn,
    retryCheckInSync,
  } = useMissionsData();
  const {
    teamOptions,
    ownerOptions,
    mentionPeople,
    currentUser,
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

  const teamFilterOptions = useMemo(
    () => [{ id: "all", label: "Todos os times" }, ...teamOptions],
    [teamOptions],
  );
  const ownerFilterOptions = useMemo(
    () => [{ id: "all", label: "Todos", initials: "" }, ...ownerOptions],
    [ownerOptions],
  );
  const missionOwnerOptions = useMemo(
    () => ownerFilterOptions.filter((option) => option.id !== "all"),
    [ownerFilterOptions],
  );

  const visibilityOptions = useMemo(
    () => [
      { id: "org", label: "Toda a organização" },
      ...teamOptions.map((t) => ({ id: t.id, label: `Time — ${t.label}` })),
      { id: "person", label: "Somente eu" },
    ],
    [teamOptions],
  );

  const currentUserOption = useMemo(
    () => currentUser ?? ownerOptions[0] ?? null,
    [currentUser, ownerOptions],
  );
  const currentUserDefaultId = currentUserOption?.id ?? "all";
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
  const viewId = searchParams.get("view");
  const isNewViewMode = false;

  const [activeFilters, setActiveFilters] = useState<string[]>(
    mine ? ["owner", "period"] : ["team", "period"],
  );
  const [expandedMissions, setExpandedMissions] = useState<Set<string>>(
    new Set(),
  );
  const [expandedMissionId, setExpandedMissionId] = useState<string | null>(
    null,
  );
  const overlayOpenOrderRef = useRef(0);
  const [expandedMissionOverlayKey, setExpandedMissionOverlayKey] = useState(0);
  const [drawerOverlayKey, setDrawerOverlayKey] = useState(0);

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

  function claimNextOverlayKey() {
    overlayOpenOrderRef.current += 1;
    return overlayOpenOrderRef.current;
  }

  const setExpandedMission = (m: Mission | null) => {
    if (m && expandedMissionId === null) {
      setExpandedMissionOverlayKey(claimNextOverlayKey());
    }
    setExpandedMissionId(m?.id ?? null);
  };

  /* ——— View mode ——— */
  const [viewMode, setViewMode] = useState<"list" | "cards" | "kanban">("list");

  /* ——— Kanban statuses (per indicator/sub-mission) ——— */
  const [kanbanStatuses, setKanbanStatuses] = useState<
    Record<string, KanbanStatus>
  >({});
  const [kanbanMoveOpen, setKanbanMoveOpen] = useState<string | null>(null);
  const [kanbanExpanded, setKanbanExpanded] = useState<Set<string>>(new Set());
  const [draggedItemId, setDraggedItemId] = useState<string | null>(null);
  const [dragOverColumn, setDragOverColumn] = useState<KanbanStatus | null>(
    null,
  );
  const kanbanMoveBtnRefs = useRef<Record<string, HTMLButtonElement | null>>(
    {},
  );

  /* ——— Drawer edit mode ——— */
  const [drawerEditing, setDrawerEditing] = useState(false);

  function startDrawerEdit() {
    if (drawerMode === "indicator" && drawerIndicator) {
      const missionType = drawerIndicator.missionType;
      setEditingItem({
        id: drawerIndicator.id,
        name: drawerIndicator.title,
        description: getMissionLabel(drawerIndicator),
        measurementMode: "manual",
        manualType:
          missionType === "reach"
            ? "reach"
            : missionType === "above"
              ? "above"
              : missionType === "below"
                ? "below"
                : missionType === "between"
                  ? "between"
                  : null,
        period: [null, null],
        missionValue:
          missionType === "reach"
            ? String(numVal(drawerIndicator.targetValue))
            : "",
        missionValueMin:
          missionType === "above" || missionType === "between"
            ? String(numVal(drawerIndicator.lowThreshold))
            : "",
        missionValueMax:
          missionType === "below" || missionType === "between"
            ? String(numVal(drawerIndicator.highThreshold))
            : "",
        missionUnit: "%",
      });
      setIsEditingExisting(true);
    } else if (drawerMode === "task" && drawerTask) {
      setEditingItem({
        id: drawerTask.id,
        name: drawerTask.title,
        description: drawerTask.description ?? "",
        measurementMode: "task",
        manualType: null,
        period: [null, null],
        missionValue: "",
        missionValueMin: "",
        missionValueMax: "",
        missionUnit: "",
      });
      setIsEditingExisting(true);
    }
    setDrawerEditing(true);
  }

  function saveDrawerEdit() {
    if (!editingItem) return;
    if (drawerMode === "indicator" && drawerIndicator) {
      const updated = {
        ...drawerIndicator,
        title: editingItem.name,
        targetValue: String(
          Number(editingItem.missionValue) ||
            numVal(drawerIndicator.targetValue),
        ),
        lowThreshold: String(
          Number(editingItem.missionValueMin) ||
            numVal(drawerIndicator.lowThreshold),
        ),
        highThreshold: String(
          Number(editingItem.missionValueMax) ||
            numVal(drawerIndicator.highThreshold),
        ),
      };
      setDrawerIndicator(updated);
      setMissions((prev) => {
        function updateInList(list: Mission[]): Mission[] {
          return list.map((m) => ({
            ...m,
            indicators: (m.indicators ?? []).map((indicator) =>
              indicator.id === updated.id
                ? updated
                : {
                    ...indicator,
                    children: indicator.children?.map((s) =>
                      s.id === updated.id ? updated : s,
                    ),
                  },
            ),
            children: m.children ? updateInList(m.children) : undefined,
          }));
        }
        return updateInList(prev);
      });
      toast.success("Indicador atualizado");
    } else if (drawerMode === "task" && drawerTask) {
      const updated = {
        ...drawerTask,
        title: editingItem.name,
        description: editingItem.description,
      };
      setDrawerTask(updated);
      setMissions((prev) => {
        function updateTasks(list: Mission[]): Mission[] {
          return list.map((m) => ({
            ...m,
            tasks: m.tasks?.map((t) =>
              t.id === updated.id
                ? {
                    ...t,
                    title: updated.title,
                    description: updated.description,
                  }
                : t,
            ),
            indicators: (m.indicators ?? []).map((indicator) => ({
              ...indicator,
              tasks: indicator.tasks?.map((t) =>
                t.id === updated.id
                  ? {
                      ...t,
                      title: updated.title,
                      description: updated.description,
                    }
                  : t,
              ),
              children: indicator.children?.map((s) => ({
                ...s,
                tasks: s.tasks?.map((t) =>
                  t.id === updated.id
                    ? {
                        ...t,
                        title: updated.title,
                        description: updated.description,
                      }
                    : t,
                ),
              })),
            })),
            children: m.children ? updateTasks(m.children) : undefined,
          }));
        }
        return updateTasks(prev);
      });
      toast.success("Tarefa atualizada");
    }
    setDrawerEditing(false);
    setEditingItem(null);
    setIsEditingExisting(false);
  }

  function cancelDrawerEdit() {
    setDrawerEditing(false);
    setEditingItem(null);
    setIsEditingExisting(false);
  }

  /* ——— Drawer (indicator or task) ——— */
  const [drawerOpen, setDrawerOpen] = useState(false);
  const [drawerMode, setDrawerMode] = useState<"indicator" | "task">(
    "indicator",
  );
  const [drawerIndicator, setDrawerIndicator] = useState<Indicator | null>(
    null,
  );
  const [drawerTask, setDrawerTask] = useState<MissionTask | null>(null);
  const [drawerMissionTitle, setDrawerMissionTitle] = useState("");
  const [drawerValue, setDrawerValue] = useState("");
  const [drawerNote, setDrawerNote] = useState("");
  const [drawerConfidence, setDrawerConfidence] =
    useState<ConfidenceLevel | null>(null);
  const drawerNoteRef = useRef<HTMLTextAreaElement>(null);
  const [confidenceOpen, setConfidenceOpen] = useState(false);
  const confidenceBtnRef = useRef<HTMLButtonElement>(null);
  const [supportTeam, setSupportTeam] = useState<string[]>([]);
  const [addSupportOpen, setAddSupportOpen] = useState(false);
  const addSupportRef = useRef<HTMLDivElement>(null);
  const [supportSearch, setSupportSearch] = useState("");
  const [drawerContributesTo, setDrawerContributesTo] = useState<
    { missionId: string; missionTitle: string }[]
  >([]);
  const [drawerItemId, setDrawerItemId] = useState<string | null>(null);
  const [drawerSourceMissionId, setDrawerSourceMissionId] = useState<
    string | null
  >(null);
  const [drawerSourceMissionTitle, setDrawerSourceMissionTitle] = useState("");
  const [newlyCreatedCheckInId, setNewlyCreatedCheckInId] = useState<
    string | null
  >(null);
  const [drawerContribPickerOpen, setDrawerContribPickerOpen] = useState(false);
  const [drawerContribPickerSearch, setDrawerContribPickerSearch] =
    useState("");
  const addContribRef = useRef<HTMLButtonElement>(null);

  /* ——— Row menu (⋯) for indicators and tasks ——— */
  const [openRowMenu, setOpenRowMenu] = useState<string | null>(null);
  const [openContributeFor, setOpenContributeFor] = useState<string | null>(
    null,
  );
  const [contributePickerSearch, setContributePickerSearch] = useState("");
  const rowMenuBtnRefs = useRef<Record<string, HTMLButtonElement | null>>({});

  /* ——— Drawer tasks ——— */
  interface DrawerTask {
    id: string;
    title: string;
    isDone: boolean;
  }
  const [drawerTasks, setDrawerTasks] = useState<DrawerTask[]>([]);
  const [newTaskLabel, setNewTaskLabel] = useState("");
  const kanbanDragRef = useRef<{ itemId: string; value: number } | null>(null);

  useEffect(() => {
    function onPointerUp() {
      if (!kanbanDragRef.current) return;
      const { itemId, value } = kanbanDragRef.current;
      kanbanDragRef.current = null;
      const indicator = findIndicatorById(itemId, missions);
      if (!indicator) return;
      requestAnimationFrame(() => {
        handleOpenCheckin({
          indicator: indicator,
          currentValue: indicator.progress,
          newValue: value,
        });
      });
    }
    document.addEventListener("pointerup", onPointerUp);
    return () => document.removeEventListener("pointerup", onPointerUp);
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [missions]);

  function findMissionOfItem(
    itemId: string,
    missionList: Mission[],
  ): Mission | null {
    for (const m of missionList) {
      if (m.tasks?.some((t) => t.id === itemId)) return m;
      if (
        m.indicators?.some(
          (indicator) =>
            indicator.id === itemId ||
            indicator.children?.some((s) => s.id === itemId) ||
            indicator.tasks?.some((t) => t.id === itemId),
        )
      )
        return m;
      if (m.children) {
        const found = findMissionOfItem(itemId, m.children);
        if (found) return found;
      }
    }
    return null;
  }

  const {
    removeContribConfirm,
    setRemoveContribConfirm,
    handleRequestRemoveContribution,
    handleRemoveContribution,
    handleAddContribution,
  } = useMissionContributions({
    setMissions,
    setDrawerContributesTo,
    setOpenRowMenu,
    setOpenContributeFor,
    setContributePickerSearch,
  });

  const flatMissions = useMemo(() => flattenMissions(missions), [missions]);

  const drawerCheckIns = useMemo(() => {
    if (!drawerIndicator) return [];
    return sortCheckInsDesc(getCheckInsByIndicator(drawerIndicator.id));
  }, [drawerIndicator, getCheckInsByIndicator]);

  const drawerCheckInChartData = useMemo(
    () => buildCheckInChartData(drawerCheckIns),
    [drawerCheckIns],
  );

  const drawerCheckInSyncStateById = useMemo(() => {
    const stateById: Record<
      string,
      {
        syncStatus: "pending" | "synced" | "failed";
        error: string | null;
        nextRetryAt: string | null;
      }
    > = {};
    for (const checkIn of drawerCheckIns) {
      const meta = getCheckInSyncMeta(checkIn.id);
      if (!meta) continue;
      stateById[checkIn.id] = meta;
    }
    return stateById;
  }, [drawerCheckIns, getCheckInSyncMeta]);

  function handleOpenCheckin(payload: CheckinPayload) {
    setDrawerOverlayKey(claimNextOverlayKey());
    const parentTitle = findParentMission(payload.indicator.id, missions);
    setDrawerMode("indicator");
    setDrawerTask(null);
    setDrawerIndicator(payload.indicator);
    setDrawerMissionTitle(parentTitle);
    setDrawerValue(String(payload.newValue));
    setDrawerNote("");
    setDrawerConfidence(null);
    setNewlyCreatedCheckInId(null);
    // Pre-populate support team from check-in history participants
    const history = getCheckInsByIndicator(payload.indicator.id);
    const team: string[] = [];
    const ownerInitials = getOwnerInitials(payload.indicator.owner);
    const seen = new Set([ownerInitials]);
    for (const entry of history) {
      const entryInitials = getOwnerInitials(entry.author);
      if (!seen.has(entryInitials)) {
        seen.add(entryInitials);
        team.push(entryInitials);
      }
    }
    setSupportTeam(team);
    setDrawerContributesTo(payload.indicator.contributesTo ?? []);
    setDrawerItemId(payload.indicator.id);
    const srcM = findMissionOfItem(payload.indicator.id, missions);
    setDrawerSourceMissionId(srcM?.id ?? null);
    setDrawerSourceMissionTitle(srcM?.title ?? "");
    setDrawerTasks(DRAWER_TASKS_BY_INDICATOR[payload.indicator.id] ?? []);
    setNewTaskLabel("");
    setDrawerOpen(true);
  }

  function handleOpenTaskDrawer(task: MissionTask, parentLabel: string) {
    setDrawerOverlayKey(claimNextOverlayKey());
    setDrawerMode("task");
    setDrawerIndicator(null);
    setNewlyCreatedCheckInId(null);
    setDrawerTask(task);
    setDrawerMissionTitle(parentLabel);
    setSupportTeam([]);
    setAddSupportOpen(false);
    setDrawerContributesTo(task.contributesTo ?? []);
    setDrawerItemId(task.id);
    const srcM = findMissionOfItem(task.id, missions);
    setDrawerSourceMissionId(srcM?.id ?? null);
    setDrawerSourceMissionTitle(srcM?.title ?? "");
    setNewTaskLabel("");
    setDrawerOpen(true);
  }

  function handleCloseDrawer() {
    setDrawerOpen(false);
    setDrawerIndicator(null);
    setDrawerTask(null);
    setNewlyCreatedCheckInId(null);
    setDrawerItemId(null);
    setDrawerContribPickerOpen(false);
    setMentionQuery(null);
    if (drawerEditing) {
      setDrawerEditing(false);
      setEditingItem(null);
      setIsEditingExisting(false);
    }
  }

  function handleToggleTask(taskId: string) {
    setMissions((prev) => {
      function toggleIndicatorTasks(indicators: Indicator[]): Indicator[] {
        return indicators.map((indicator) => ({
          ...indicator,
          tasks: indicator.tasks?.map((t) =>
            t.id === taskId ? { ...t, isDone: !t.isDone } : t,
          ),
          children: indicator.children
            ? toggleIndicatorTasks(indicator.children)
            : undefined,
        }));
      }
      function toggleInList(list: Mission[]): Mission[] {
        return list.map((m) => ({
          ...m,
          tasks: m.tasks?.map((t) =>
            t.id === taskId ? { ...t, isDone: !t.isDone } : t,
          ),
          indicators: toggleIndicatorTasks(m.indicators ?? []),
          children: m.children ? toggleInList(m.children) : undefined,
        }));
      }
      return toggleInList(prev);
    });
    // Move task to appropriate kanban column
    setKanbanStatuses((prev) => {
      const next = { ...prev };
      const task = findTaskInMissions(taskId, missions);
      if (task) {
        next[taskId] = task.isDone ? "todo" : "done";
      }
      return next;
    });
  }

  const {
    mentionQuery,
    setMentionQuery,
    mentionIndex,
    mentionResults,
    insertMention,
    handleNoteChange,
    handleNoteKeyDown,
  } = useMissionMentions({
    people: mentionPeople,
    drawerNote,
    setDrawerNote,
    drawerNoteRef,
  });

  function handleConfirmCheckin() {
    if (!drawerIndicator) return;
    const currentIndicator = drawerIndicator;
    const checkInAuthor = currentUserOption ?? {
      id: "local-user",
      label: "Usuário local",
      initials: "UL",
    };
    const checkInAuthorFullName = checkInAuthor.label;
    const numValue = Number(drawerValue) || 0;
    const previousValue = String(currentIndicator.currentValue);
    const nowIso = new Date().toISOString();

    function deriveKrStatus(progress: number): Indicator["status"] {
      if (progress >= 100) return "completed";
      if (progress >= 75) return "on_track";
      if (progress >= 40) return "attention";
      return "off_track";
    }

    setMissions((prev) => {
      function updateIndicators(indicators: Indicator[]): Indicator[] {
        return indicators.map((indicator) => {
          const nextChildren = indicator.children
            ? updateIndicators(indicator.children)
            : undefined;
          if (indicator.id !== currentIndicator.id) {
            return nextChildren
              ? { ...indicator, children: nextChildren }
              : indicator;
          }

          return {
            ...indicator,
            progress: numValue,
            currentValue: String(numValue),
            status: deriveKrStatus(numValue),
            updatedAt: nowIso,
            children: nextChildren,
          };
        });
      }

      function recalcMission(mission: Mission): Mission {
        const nextChildren = mission.children?.map(recalcMission);
        const nextIndicators = updateIndicators(mission.indicators ?? []);
        const progressSources = [
          ...nextIndicators.map((indicator) => indicator.progress),
          ...(nextChildren ?? []).map((child) => child.progress),
        ];
        const nextProgress =
          progressSources.length > 0
            ? Math.round(
                progressSources.reduce((acc, value) => acc + value, 0) /
                  progressSources.length,
              )
            : mission.progress;

        return {
          ...mission,
          indicators: nextIndicators,
          children: nextChildren,
          progress: nextProgress,
          updatedAt: nowIso,
        };
      }

      return prev.map(recalcMission);
    });

    const createdCheckIn = createCheckIn({
      indicatorId: currentIndicator.id,
      authorId: checkInAuthor.id,
      value: String(numValue),
      previousValue,
      confidence: drawerConfidence,
      note: drawerNote.trim() || null,
      mentions: supportTeam.length > 0 ? supportTeam : null,
      createdAt: nowIso,
      author: {
        id: checkInAuthor.id,
        fullName: checkInAuthorFullName,
        initials: checkInAuthor.initials,
      },
    });

    setDrawerIndicator((prev) => {
      if (!prev) return prev;
      return {
        ...prev,
        progress: numValue,
        currentValue: String(numValue),
        status: deriveKrStatus(numValue),
        updatedAt: nowIso,
      };
    });
    setDrawerValue(String(numValue));
    setDrawerConfidence(null);
    setDrawerNote("");
    setMentionQuery(null);
    setNewlyCreatedCheckInId(createdCheckIn.id);

    toast.success("Check-in registrado com sucesso!");
  }

  function handleUpdateDrawerCheckIn(
    checkInId: string,
    patch: { note?: string | null; confidence?: ConfidenceLevel | null },
  ) {
    const updated = updateCheckIn(checkInId, patch);
    if (!updated) {
      toast.error("Nao foi possivel atualizar o check-in.");
      return;
    }
    toast.success("Check-in atualizado.");
  }

  function handleDeleteDrawerCheckIn(checkInId: string) {
    deleteCheckIn(checkInId);
    setNewlyCreatedCheckInId((current) =>
      current === checkInId ? null : current,
    );
    toast.success("Check-in excluido.");
  }

  function getKanbanStatus(itemId: string): KanbanStatus {
    if (kanbanStatuses[itemId]) return kanbanStatuses[itemId];
    // Auto-assign tasks based on done status
    const taskItem = kanbanItems.find(
      (ki) => ki.id === itemId && ki.type === "task",
    );
    if (taskItem) return taskItem.done ? "done" : "todo";
    return "uncategorized";
  }

  function moveToKanban(itemId: string, status: KanbanStatus) {
    setKanbanStatuses((prev) => ({ ...prev, [itemId]: status }));
    setKanbanMoveOpen(null);
  }

  function toggleKanbanExpand(itemId: string) {
    setKanbanExpanded((prev) => {
      const next = new Set(prev);
      if (next.has(itemId)) next.delete(itemId);
      else next.add(itemId);
      return next;
    });
  }

  /* ——— Filter values ——— */
  const [selectedTeams, setSelectedTeams] = useState<string[]>(["all"]);
  const [selectedPeriod, setSelectedPeriod] = useState<
    [CalendarDate | null, CalendarDate | null]
  >([null, null]);
  const [selectedStatus, setSelectedStatus] = useState("all");
  const [selectedOwners, setSelectedOwners] = useState<string[]>(
    mine && currentUserDefaultId !== "all" ? [currentUserDefaultId] : ["all"],
  );
  const [selectedItemTypes, setSelectedItemTypes] = useState<string[]>(["all"]);
  const [selectedIndicatorTypes, setSelectedIndicatorTypes] = useState<
    string[]
  >(["all"]);
  const [selectedContributions, setSelectedContributions] = useState<string[]>([
    "all",
  ]);
  const [selectedTaskState, setSelectedTaskState] = useState("all");
  const [selectedMissionStatuses, setSelectedMissionStatuses] = useState<
    string[]
  >(["all"]);
  const [selectedSupporters, setSelectedSupporters] = useState<string[]>([
    "all",
  ]);

  useEffect(() => {
    if (!mine || currentUserDefaultId === "all") return;
    setSelectedOwners((prev) =>
      prev.length === 0 || prev.includes("all") ? [currentUserDefaultId] : prev,
    );
  }, [mine, currentUserDefaultId]);

  /* ——— Pre-filter by owner when navigating from team health view ——— */
  // Pass filterOwnerUserId / filterSupporterUserId as search params (?filterOwnerUserId=...)
  // when navigating from a collaborator card.
  const filterOwnerUserId = searchParams.get("filterOwnerUserId");
  const filterSupporterUserId = searchParams.get("filterSupporterUserId");

  useEffect(() => {
    const hasOwner = !!filterOwnerUserId;
    const hasSupporter = !!filterSupporterUserId;
    if (!hasOwner && !hasSupporter) return;
    setActiveFilters((prev) => {
      let next = prev.filter((f) => f !== "team");
      if (hasOwner && !next.includes("owner")) next = ["owner", ...next];
      if (hasSupporter && !next.includes("supporter"))
        next = ["supporter", ...next];
      return next;
    });
    if (hasOwner) setSelectedOwners([filterOwnerUserId]);
    if (hasSupporter) setSelectedSupporters([filterSupporterUserId]);
    // Clear navigation state so a page refresh doesn't re-apply
    window.history.replaceState(
      {},
      "",
      window.location.pathname + window.location.search,
    );
  }, [filterOwnerUserId, filterSupporterUserId]);

  /* ——— Save view ——— */
  const { views, addView, updateView, deleteView } = useSavedViews();
  const [saveModalOpen, setSaveModalOpen] = useState(false);
  const [deleteModalOpen, setDeleteModalOpen] = useState(false);
  const [viewName, setViewName] = useState("");

  /* ——— Create / Edit mission ——— */
  const [createOpen, setCreateOpen] = useState(false);
  const [editingMission, setEditingMission] = useState<Mission | undefined>(
    undefined,
  );
  const [editingItem, setEditingItem] = useState<MissionItemData | null>(null);
  const [isEditingExisting, setIsEditingExisting] = useState(false);

  /* ——— Current saved view ——— */
  const currentView: SavedView | undefined = viewId
    ? views.find((v) => v.id === viewId)
    : undefined;

  /* ——— Load saved view filters when URL changes ——— */
  useEffect(() => {
    if (currentView) {
      const f = currentView.filters;
      setActiveFilters(f.activeFilters);
      setSelectedTeams(f.selectedTeams);
      setSelectedPeriod(f.selectedPeriod);
      setSelectedStatus(f.selectedStatus);
      setSelectedOwners(f.selectedOwners);
      setSelectedItemTypes(f.selectedItemTypes ?? ["all"]);
      setSelectedIndicatorTypes(f.selectedIndicatorTypes ?? ["all"]);
      setSelectedContributions(f.selectedContributions ?? ["all"]);
      setSelectedTaskState(f.selectedTaskState ?? "all");
      setSelectedMissionStatuses(f.selectedMissionStatuses ?? ["all"]);
      setSelectedSupporters(f.selectedSupporters ?? ["all"]);
    }
  }, [viewId]); // eslint-disable-line react-hooks/exhaustive-deps

  /* ——— New view mode: clear filters and open popover ——— */
  useEffect(() => {
    if (isNewViewMode) {
      setActiveFilters([]);
      setSelectedTeams(["all"]);
      setSelectedPeriod([null, null]);
      setSelectedStatus("all");
      setSelectedOwners(
        mine && currentUserDefaultId !== "all"
          ? [currentUserDefaultId]
          : ["all"],
      );
      setSelectedItemTypes(["all"]);
      setSelectedIndicatorTypes(["all"]);
      setSelectedContributions(["all"]);
      setSelectedTaskState("all");
      setSelectedMissionStatuses(["all"]);
      setSelectedSupporters(["all"]);
      // Clear navigation state so refresh doesn't re-trigger
      window.history.replaceState({}, "");
    }
  }, [isNewViewMode, mine, currentUserDefaultId]);

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

  const displayedMissions = useMemo(() => {
    const selectedTeamSet = new Set(
      selectedTeams.filter((id) => id !== "all").map((id) => resolveTeamId(id)),
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
    const selectedOwnerIds = new Set(
      selectedOwners
        .filter((id) => id !== "all")
        .map((id) => resolveUserId(id).toLowerCase()),
    );
    const selectedOwnerInitials = new Set(
      selectedOwners
        .filter((id) => id !== "all")
        .map(
          (id) =>
            ownerFilterOptions
              .find((option) => option.id === id)
              ?.initials?.toLowerCase() ?? id.toLowerCase(),
        )
        .filter((value) => value.length > 0),
    );
    const statusValue = selectedStatus.replace("-", "_");

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
      fullName: string;
      initials: string | null;
    }): boolean {
      if (!ownerFilterActive) return true;
      if (!owner) return false;

      const initials = getOwnerInitials(owner).toLowerCase();
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

    function indicatorHasContribution(indicator: Indicator): boolean {
      if ((indicator.contributesTo?.length ?? 0) > 0) return true;
      if (
        (indicator.tasks ?? []).some(
          (task) => (task.contributesTo?.length ?? 0) > 0,
        )
      )
        return true;
      if (
        (indicator.children ?? []).some((child) =>
          indicatorHasContribution(child),
        )
      )
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
      if (
        (mission.indicators ?? []).some((indicator) =>
          indicatorHasContribution(indicator),
        )
      )
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

    function indicatorContributionMatches(indicator: Indicator): boolean {
      if (!contributionFilterActive) return true;

      const hasContributing = (indicator.contributesTo?.length ?? 0) > 0;
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

    function indicatorTypeMatches(indicator: Indicator): boolean {
      if (!indicatorTypeFilterActive) return true;
      if (selectedIndicatorTypeSet.has(indicator.missionType)) return true;
      if (
        selectedIndicatorTypeSet.has("external") &&
        indicator.measurementMode === "external"
      )
        return true;
      if (
        selectedIndicatorTypeSet.has("linked_mission") &&
        indicator.measurementMode === "mission"
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
      indicator: Indicator,
      missionScopeMatches: boolean,
    ): Indicator | null {
      const nextChildren = (indicator.children ?? [])
        .map((child) => filterIndicatorNode(child, missionScopeMatches))
        .filter((child): child is Indicator => !!child);
      const nextTasks = (indicator.tasks ?? [])
        .map((task) => filterTaskNode(task, missionScopeMatches))
        .filter((task): task is MissionTask => !!task);

      const directMatch =
        missionScopeMatches &&
        (!itemTypeFilterActive || selectedItemTypeSet.has("indicator")) &&
        !taskStateFilterActive &&
        dateRangeMatches(indicator.periodStart, indicator.periodEnd) &&
        ownerMatches(indicator.owner) &&
        (!statusFilterActive || indicator.status === statusValue) &&
        indicatorTypeMatches(indicator) &&
        indicatorContributionMatches(indicator);

      if (!directMatch && nextChildren.length === 0 && nextTasks.length === 0) {
        return null;
      }

      return {
        ...indicator,
        children:
          nextChildren.length > 0
            ? nextChildren
            : indicator.children
              ? []
              : undefined,
        tasks:
          nextTasks.length > 0 ? nextTasks : indicator.tasks ? [] : undefined,
      };
    }

    function filterMissionNode(mission: Mission): Mission | null {
      const resolvedMissionTeamId = mission.teamId
        ? resolveTeamId(mission.teamId)
        : null;
      const missionTeamMatches =
        !teamFilterActive ||
        (resolvedMissionTeamId !== null &&
          selectedTeamSet.has(resolvedMissionTeamId));
      const missionScopeMatches =
        missionTeamMatches && missionStatusMatches(mission);

      const nextChildren = (mission.children ?? [])
        .map((child) => filterMissionNode(child))
        .filter((child): child is Mission => !!child);
      const nextIndicators = (mission.indicators ?? [])
        .map((indicator) => filterIndicatorNode(indicator, missionScopeMatches))
        .filter((indicator): indicator is Indicator => !!indicator);
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
    ownerFilterOptions,
    resolveTeamId,
    resolveUserId,
  ]);

  /* Flatten all indicators and sub-missions into kanban items */
  const kanbanItems: KanbanItem[] = [];
  function collectKanbanItems(missionList: Mission[]) {
    for (const m of missionList) {
      for (const indicator of m.indicators ?? []) {
        kanbanItems.push({
          id: indicator.id,
          label: indicator.title,
          missionTitle: m.title,
          missionId: m.id,
          value: indicator.progress,
          target: numVal(indicator.targetValue),
          missionLabel: getMissionLabel(indicator),
          ownerInitials: getOwnerInitials(indicator.owner),
          ownerName: getOwnerName(indicator.owner),
          period: indicator.periodLabel ?? "",
          type: "indicator",
          icon: getIndicatorIcon(indicator),
        });
        // Collect sub-Indicators
        if (indicator.children) {
          for (const sub of indicator.children) {
            kanbanItems.push({
              id: sub.id,
              label: sub.title,
              missionTitle: `${m.title} › ${indicator.title}`,
              missionId: m.id,
              value: sub.progress,
              target: numVal(sub.targetValue),
              missionLabel: getMissionLabel(sub),
              ownerInitials: getOwnerInitials(sub.owner),
              ownerName: getOwnerName(sub.owner),
              period: sub.periodLabel ?? "",
              type: "indicator",
              icon: getIndicatorIcon(sub),
            });
          }
        }
        // Collect Indicator tasks
        if (indicator.tasks) {
          for (const task of indicator.tasks) {
            kanbanItems.push({
              id: task.id,
              label: task.title,
              missionTitle: `${m.title} › ${indicator.title}`,
              missionId: m.id,
              value: task.isDone ? 100 : 0,
              target: 100,
              missionLabel: task.isDone ? "Concluída" : "Pendente",
              ownerInitials: getOwnerInitials(task.owner),
              ownerName: getOwnerName(task.owner),
              period: "",
              type: "task",
              done: task.isDone,
            });
          }
        }
      }
      if (m.tasks) {
        for (const task of m.tasks) {
          kanbanItems.push({
            id: task.id,
            label: task.title,
            missionTitle: m.title,
            missionId: m.id,
            value: task.isDone ? 100 : 0,
            target: 100,
            missionLabel: task.isDone ? "Concluída" : "Pendente",
            ownerInitials: getOwnerInitials(task.owner),
            ownerName: getOwnerName(task.owner),
            period: "",
            type: "task",
            done: task.isDone,
          });
        }
      }
      if (m.children) {
        const childIndicators = (child: Mission) => child.indicators ?? [];
        for (const child of m.children) {
          const cIndicators = childIndicators(child);
          kanbanItems.push({
            id: child.id,
            label: child.title,
            missionTitle: m.title,
            missionId: m.id,
            value: child.progress,
            target: 100,
            missionLabel: `${cIndicators.length} indicador${cIndicators.length !== 1 ? "es" : ""}`,
            ownerInitials: cIndicators[0]
              ? getOwnerInitials(cIndicators[0].owner)
              : "",
            ownerName: cIndicators[0] ? getOwnerName(cIndicators[0].owner) : "",
            period: cIndicators[0]?.periodLabel ?? "",
            type: "mission",
            children: cIndicators.map((ci) => ({
              id: ci.id,
              label: ci.title,
              value: ci.progress,
              target: numVal(ci.targetValue),
              missionLabel: getMissionLabel(ci),
              ownerInitials: getOwnerInitials(ci.owner),
              period: ci.periodLabel ?? "",
              icon: getIndicatorIcon(ci),
            })),
          });
        }
      }
    }
  }
  collectKanbanItems(displayedMissions);

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

  function formatPeriodLabel(): string {
    const [start, end] = selectedPeriod;
    if (!start && !end) return "Selecionar período";
    const fmt = (d: CalendarDate) =>
      `${String(d.day).padStart(2, "0")}/${String(d.month).padStart(2, "0")}`;
    if (start && end) {
      return `${fmt(start)} - ${fmt(end)}/${end.year}`;
    }
    if (start) return fmt(start);
    return "";
  }

  function handleOpenSaveModal() {
    setViewName(currentView?.name ?? "");
    setSaveModalOpen(true);
  }

  function getCurrentFilters() {
    return {
      activeFilters,
      selectedTeams,
      selectedPeriod,
      selectedStatus,
      selectedOwners,
      selectedItemTypes,
      selectedIndicatorTypes,
      selectedContributions,
      selectedSupporters,
      selectedTaskState,
      selectedMissionStatuses,
    };
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

  function handleEditMission(mission: Mission) {
    setEditingMission(mission);
    setExpandedMission(null);
    setCreateOpen(true);
  }

  function handleDeleteMission(mission: Mission) {
    const confirmed = window.confirm(
      `Excluir a missão "${mission.title}"? Esta ação não pode ser desfeita.`,
    );
    if (!confirmed) return;

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

    toast.success("Missão excluída com sucesso!");
  }

  function getFilterValueSummary(filterId: string): string {
    switch (filterId) {
      case "team":
        return formatMultiLabel(
          selectedTeams,
          teamFilterOptions,
          "Todos os times",
        );
      case "period":
        return formatPeriodLabel();
      case "status":
        return (
          STATUS_OPTIONS.find((s) => s.id === selectedStatus)?.label ?? "Todos"
        );
      case "owner":
        return formatMultiLabel(selectedOwners, ownerFilterOptions, "Todos");
      case "itemType":
        return formatMultiLabel(
          selectedItemTypes,
          ITEM_TYPE_OPTIONS,
          "Todos os itens",
        );
      case "indicatorType":
        return formatMultiLabel(
          selectedIndicatorTypes,
          INDICATOR_TYPE_OPTIONS,
          "Todos os tipos",
        );
      case "contribution":
        return formatMultiLabel(
          selectedContributions,
          CONTRIBUTION_OPTIONS,
          "Todas",
        );
      case "supporter":
        return formatMultiLabel(
          selectedSupporters,
          ownerFilterOptions,
          "Todos",
        );
      case "taskState":
        return (
          TASK_STATE_OPTIONS.find((s) => s.id === selectedTaskState)?.label ??
          "Todas"
        );
      case "missionStatus":
        return formatMultiLabel(
          selectedMissionStatuses,
          MISSION_STATUS_OPTIONS,
          "Todos",
        );
      default:
        return "";
    }
  }

  /* ——— Helper: render inline add/edit form (for drawer) ——— */
  function renderInlineForm() {
    if (!editingItem) return null;
    return (
      <MissionItemInlineForm
        editingItem={editingItem}
        setEditingItem={setEditingItem}
        isEditingExisting={isEditingExisting}
        onSave={saveDrawerEdit}
        onCancel={cancelDrawerEdit}
        drawerEditing
        missionOwnerOptions={missionOwnerOptions}
        missionTagOptions={missionTagOptions}
        visibilityOptions={visibilityOptions}
        presetPeriods={presetPeriods}
        createTag={createTag}
      />
    );
  }

  return (
    <div className="flex flex-col gap-2 w-full">
      <PageHeader
        title={
          currentView
            ? currentView.name
            : mine
              ? "Minhas missões"
              : "Todas as missões"
        }
      />

      <Card padding="sm">
        <MissionsFilterBar
          activeFilters={activeFilters}
          setActiveFilters={setActiveFilters}
          currentView={currentView}
          handleOpenSaveModal={handleOpenSaveModal}
          handleDeleteView={handleDeleteView}
          mine={mine}
          currentUserDefaultId={currentUserDefaultId}
          teamFilterOptions={teamFilterOptions}
          selectedTeams={selectedTeams}
          setSelectedTeams={setSelectedTeams}
          resolveTeamId={resolveTeamId}
          selectedStatus={selectedStatus}
          setSelectedStatus={setSelectedStatus}
          ownerFilterOptions={ownerFilterOptions}
          selectedOwners={selectedOwners}
          setSelectedOwners={setSelectedOwners}
          resolveUserId={resolveUserId}
          selectedItemTypes={selectedItemTypes}
          setSelectedItemTypes={setSelectedItemTypes}
          selectedIndicatorTypes={selectedIndicatorTypes}
          setSelectedIndicatorTypes={setSelectedIndicatorTypes}
          selectedContributions={selectedContributions}
          setSelectedContributions={setSelectedContributions}
          selectedSupporters={selectedSupporters}
          setSelectedSupporters={setSelectedSupporters}
          selectedTaskState={selectedTaskState}
          setSelectedTaskState={setSelectedTaskState}
          selectedMissionStatuses={selectedMissionStatuses}
          setSelectedMissionStatuses={setSelectedMissionStatuses}
          presetPeriods={presetPeriods}
          selectedPeriod={selectedPeriod}
          setSelectedPeriod={setSelectedPeriod}
          viewMode={viewMode}
          setViewMode={setViewMode}
          onCreateMission={() => {
            setEditingMission(undefined);
            setCreateOpen(true);
          }}
        />

        <CardDivider />

        <ResumeCards
          totalValue={totalValue}
          totalExpected={totalExpected}
          activeMissions={activeMissions}
          outdatedIndicators={outdatedIndicators}
        />

        <CardDivider />

        {viewMode === "list" && (
          <ViewListMode
            missions={displayedMissions}
            expandedMissions={expandedMissions}
            onToggle={toggleMission}
            onExpand={setExpandedMission}
            onEdit={handleEditMission}
            onDelete={handleDeleteMission}
            onCheckin={handleOpenCheckin}
            onToggleTask={handleToggleTask}
            onOpenTaskDrawer={handleOpenTaskDrawer}
            openRowMenu={openRowMenu}
            setOpenRowMenu={setOpenRowMenu}
            openContributeFor={openContributeFor}
            setOpenContributeFor={setOpenContributeFor}
            contributePickerSearch={contributePickerSearch}
            setContributePickerSearch={setContributePickerSearch}
            rowMenuBtnRefs={rowMenuBtnRefs}
            allMissions={flatMissions}
            onAddContribution={handleAddContribution}
            onRemoveContribution={handleRequestRemoveContribution}
          />
        )}

        {viewMode === "cards" && (
          <ViewCardsMode
            missions={displayedMissions}
            ownerFilterOptions={ownerFilterOptions}
            onExpand={setExpandedMission}
            onEdit={handleEditMission}
            onDelete={handleDeleteMission}
          />
        )}

        {viewMode === "kanban" && (
          <ViewKanbanMode
            kanbanItems={kanbanItems}
            getKanbanStatus={getKanbanStatus}
            moveToKanban={moveToKanban}
            dragOverColumn={dragOverColumn}
            setDragOverColumn={setDragOverColumn}
            draggedItemId={draggedItemId}
            setDraggedItemId={setDraggedItemId}
            kanbanExpanded={kanbanExpanded}
            toggleKanbanExpand={toggleKanbanExpand}
            kanbanDragRef={kanbanDragRef}
            kanbanMoveBtnRefs={kanbanMoveBtnRefs}
            kanbanMoveOpen={kanbanMoveOpen}
            setKanbanMoveOpen={setKanbanMoveOpen}
            missions={missions}
            handleOpenCheckin={handleOpenCheckin}
            handleOpenTaskDrawer={handleOpenTaskDrawer}
            handleToggleTask={handleToggleTask}
          />
        )}
      </Card>

      <EditViewModal
        open={saveModalOpen}
        onClose={() => setSaveModalOpen(false)}
        currentView={currentView}
        viewName={viewName}
        setViewName={setViewName}
        onSave={handleSaveView}
        activeFilters={activeFilters}
        getFilterValueSummary={getFilterValueSummary}
      />

      <ExpandedMissionModal
        overlayKey={expandedMissionOverlayKey}
        mission={expandedMission}
        onClose={() => setExpandedMission(null)}
        onExpand={setExpandedMission}
        onEdit={handleEditMission}
        onDelete={handleDeleteMission}
        onCheckin={handleOpenCheckin}
        onToggleTask={handleToggleTask}
        onOpenTaskDrawer={handleOpenTaskDrawer}
        onAddContribution={handleAddContribution}
        onRemoveContribution={handleRequestRemoveContribution}
        allMissions={flatMissions}
      />

      <DeleteViewModal
        open={deleteModalOpen}
        onClose={() => setDeleteModalOpen(false)}
        currentView={currentView}
        onConfirm={handleConfirmDelete}
      />

      <RemoveContribModal
        confirm={removeContribConfirm}
        onClose={() => setRemoveContribConfirm(null)}
        onConfirm={(itemId, itemType, targetMissionId) =>
          handleRemoveContribution(itemId, itemType, targetMissionId)
        }
      />

      <MissionDetailsDrawer
        key={`mission-details-drawer-${drawerOverlayKey}`}
        drawerOpen={drawerOpen}
        drawerMode={drawerMode}
        drawerIndicator={drawerIndicator}
        drawerTask={drawerTask}
        drawerMissionTitle={drawerMissionTitle}
        drawerEditing={drawerEditing}
        editingItem={editingItem}
        renderInlineForm={renderInlineForm}
        startDrawerEdit={startDrawerEdit}
        handleCloseDrawer={handleCloseDrawer}
        drawerContributesTo={drawerContributesTo}
        setDrawerContributesTo={setDrawerContributesTo}
        drawerItemId={drawerItemId}
        handleRequestRemoveContribution={handleRequestRemoveContribution}
        drawerContribPickerOpen={drawerContribPickerOpen}
        setDrawerContribPickerOpen={setDrawerContribPickerOpen}
        drawerContribPickerSearch={drawerContribPickerSearch}
        setDrawerContribPickerSearch={setDrawerContribPickerSearch}
        addContribRef={addContribRef}
        allMissions={flatMissions}
        drawerSourceMissionId={drawerSourceMissionId}
        drawerSourceMissionTitle={drawerSourceMissionTitle}
        handleAddContribution={handleAddContribution}
        supportTeam={supportTeam}
        setSupportTeam={setSupportTeam}
        addSupportOpen={addSupportOpen}
        setAddSupportOpen={setAddSupportOpen}
        addSupportRef={addSupportRef}
        supportSearch={supportSearch}
        setSupportSearch={setSupportSearch}
        ownerOptions={missionOwnerOptions}
        drawerValue={drawerValue}
        setDrawerValue={setDrawerValue}
        drawerConfidence={drawerConfidence}
        setDrawerConfidence={setDrawerConfidence}
        confidenceOpen={confidenceOpen}
        setConfidenceOpen={setConfidenceOpen}
        confidenceBtnRef={confidenceBtnRef}
        confidenceOptions={CONFIDENCE_OPTIONS}
        drawerNote={drawerNote}
        drawerNoteRef={drawerNoteRef}
        handleNoteChange={handleNoteChange}
        handleNoteKeyDown={handleNoteKeyDown}
        mentionQuery={mentionQuery}
        mentionIndex={mentionIndex}
        mentionResults={mentionResults}
        insertMention={insertMention}
        handleConfirmCheckin={handleConfirmCheckin}
        checkInHistoryForIndicator={drawerCheckIns}
        checkInChartDataForIndicator={drawerCheckInChartData}
        checkInSyncStateById={drawerCheckInSyncStateById}
        retryCheckInSync={retryCheckInSync}
        onUpdateCheckIn={handleUpdateDrawerCheckIn}
        onDeleteCheckIn={handleDeleteDrawerCheckIn}
        newlyCreatedCheckInId={newlyCreatedCheckInId}
        drawerTasks={drawerTasks}
        setDrawerTasks={setDrawerTasks}
        newTaskLabel={newTaskLabel}
        setNewTaskLabel={setNewTaskLabel}
        setDrawerTask={setDrawerTask}
      />

      {/* ——— Create mission modal ——— */}
      <CreateMissionModal
        open={createOpen}
        onClose={() => {
          setCreateOpen(false);
          setEditingMission(undefined);
        }}
        editingMission={editingMission}
        onSave={(mission) => {
          setMissions((prev) =>
            editingMission
              ? prev.map((m) =>
                  m.id === mission.id
                    ? { ...mission, status: "active" as const }
                    : m,
                )
              : [...prev, { ...mission, status: "active" as const }],
          );
          toast.success(
            editingMission
              ? "Missão atualizada com sucesso!"
              : "Missão criada com sucesso!",
          );
          setEditingMission(undefined);
        }}
        onDraft={(mission) => {
          setMissions((prev) => [
            ...prev,
            { ...mission, id: `draft-${Date.now()}`, status: "draft" as const },
          ]);
          toast.success("Rascunho salvo com sucesso!");
        }}
        activeOrgId={activeOrgId}
        missionsCount={missions.length}
        resolveTagId={resolveTagId}
        getTagById={getTagById}
        missionOwnerOptions={missionOwnerOptions}
        missionTagOptions={missionTagOptions}
        visibilityOptions={visibilityOptions}
        presetPeriods={presetPeriods}
        createTag={createTag}
      />
    </div>
  );
}
