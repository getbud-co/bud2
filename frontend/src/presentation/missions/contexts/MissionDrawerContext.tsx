"use client";

import {
  createContext,
  useContext,
  useMemo,
  useRef,
  useState,
  type ChangeEvent,
  type Dispatch,
  type KeyboardEvent,
  type MutableRefObject,
  type ReactNode,
  type SetStateAction,
} from "react";
import type {
  CheckIn,
  ConfidenceLevel,
  ExternalContribution,
  Indicator,
  Mission,
  MissionTask,
} from "@/types";
import { useMissionsData } from "@/contexts/MissionsDataContext";
import { usePeopleData } from "@/contexts/PeopleDataContext";
import { useMissionMentions } from "@/presentation/missions/hooks/useMissionMentions";
import {
  buildCheckInChartData,
  sortCheckInsDesc,
} from "@/presentation/missions/utils/checkinReadModels";
import type { CheckInChartPoint } from "@/presentation/missions/utils/checkinReadModels";
import {
  flattenMissions,
  findParentMission,
  findIndicatorById,
  findTaskById,
} from "@/presentation/missions/utils/missionTree";
import {
  CONFIDENCE_OPTIONS,
  DRAWER_TASKS_BY_INDICATOR,
} from "@/presentation/missions/consts";
import { getOwnerInitials } from "@/lib/tempStorage/missions";

// ── Shared types ─────────────────────────────────────────────────────────────

export interface DrawerTask {
  id: string;
  title: string;
  isDone: boolean;
  ownerId: string | null;
}

export interface CheckInSyncState {
  syncStatus: "pending" | "synced" | "failed";
  error: string | null;
  nextRetryAt: string | null;
}

export interface UpdateCheckInPatch {
  note?: string | null;
  confidence?: ConfidenceLevel | null;
}

export type CheckinPayload = {
  keyResult: Indicator;
  currentValue: number;
  newValue: number;
};

// ── Context interface ─────────────────────────────────────────────────────────

interface MissionDrawerContextValue {
  // Drawer state
  drawerOpen: boolean;
  drawerMode: "indicator" | "task";
  drawerIndicator: Indicator | null;
  drawerTask: MissionTask | null;
  setDrawerTask: Dispatch<SetStateAction<MissionTask | null>>;
  drawerMissionTitle: string;
  drawerEditing: boolean;
  drawerOverlayKey: number;

  // Check-in form
  drawerValue: string;
  setDrawerValue: Dispatch<SetStateAction<string>>;
  drawerNote: string;
  drawerNoteRef: MutableRefObject<HTMLTextAreaElement | null>;
  drawerConfidence: ConfidenceLevel | null;
  setDrawerConfidence: Dispatch<SetStateAction<ConfidenceLevel | null>>;
  confidenceOpen: boolean;
  setConfidenceOpen: Dispatch<SetStateAction<boolean>>;
  confidenceBtnRef: MutableRefObject<HTMLButtonElement | null>;
  confidenceOptions: typeof CONFIDENCE_OPTIONS;

  // Support team
  supportTeam: string[];
  setSupportTeam: Dispatch<SetStateAction<string[]>>;
  addSupportOpen: boolean;
  setAddSupportOpen: Dispatch<SetStateAction<boolean>>;
  addSupportRef: MutableRefObject<HTMLDivElement | null>;
  supportSearch: string;
  setSupportSearch: Dispatch<SetStateAction<string>>;

  // Contributions
  drawerContributesTo: { missionId: string; missionTitle: string }[];
  setDrawerContributesTo: Dispatch<
    SetStateAction<{ missionId: string; missionTitle: string }[]>
  >;
  drawerItemId: string | null;
  drawerSourceMissionId: string | null;
  drawerSourceMissionTitle: string;
  drawerContribPickerOpen: boolean;
  setDrawerContribPickerOpen: Dispatch<SetStateAction<boolean>>;
  drawerContribPickerSearch: string;
  setDrawerContribPickerSearch: Dispatch<SetStateAction<string>>;
  addContribRef: MutableRefObject<HTMLButtonElement | null>;
  allMissions: { id: string; title: string }[];

  // Tasks
  drawerTasks: DrawerTask[];
  setDrawerTasks: Dispatch<SetStateAction<DrawerTask[]>>;
  newTaskLabel: string;
  setNewTaskLabel: Dispatch<SetStateAction<string>>;
  newlyCreatedCheckInId: string | null;

  // Note / mention
  handleNoteChange: (e: ChangeEvent<HTMLTextAreaElement>) => void;
  handleNoteKeyDown: (e: KeyboardEvent<HTMLTextAreaElement>) => void;
  mentionQuery: string | null;
  mentionIndex: number;
  mentionResults: { id: string; label: string; initials: string }[];
  insertMention: (person: {
    id: string;
    label: string;
    initials: string;
  }) => void;

  // Check-in history
  checkInHistoryForIndicator: CheckIn[];
  checkInChartDataForIndicator: CheckInChartPoint[];
  checkInSyncStateById: Record<string, CheckInSyncState>;
  retryCheckInSync: (checkInId: string) => void;

  // Owner metadata
  ownerOptions: { id: string; label: string; initials: string }[];
  currentUserId: string | null;

  // Inline edit (TODO)
  editingItem: unknown;
  renderInlineForm: () => ReactNode;

  // Handlers
  openCheckin: (payload: CheckinPayload) => void;
  openTaskDrawer: (task: MissionTask, parentLabel: string) => void;
  openExternalContrib: (ec: ExternalContribution) => void;
  closeDrawer: () => void;
  startEdit: () => void;
  confirmCheckin: () => void;
  updateCheckIn: (checkInId: string, patch: UpdateCheckInPatch) => void;
  deleteCheckIn: (checkInId: string) => void;
  requestRemoveContribution: (
    itemId: string,
    itemType: "indicator" | "task",
    targetMissionId: string,
    targetMissionTitle: string,
  ) => void;
  addContribution: (
    item: Indicator | MissionTask,
    itemType: "indicator" | "task",
    sourceMissionId: string,
    sourceMissionTitle: string,
    targetMissionId: string,
    targetMissionTitle: string,
  ) => void;
  openTaskFromIndicator: (taskId: string) => void;
  changeTaskOwner: (ownerId: string) => void;
}

// ── Context ───────────────────────────────────────────────────────────────────

const MissionDrawerContext = createContext<MissionDrawerContextValue | null>(
  null,
);

// ── Helper ────────────────────────────────────────────────────────────────────

function findMissionOfItem(
  itemId: string,
  missionList: Mission[],
): Mission | null {
  for (const m of missionList) {
    if (m.tasks?.some((t) => t.id === itemId)) return m;
    if (
      m.indicators?.some(
        (ind) =>
          ind.id === itemId ||
          ind.children?.some((s) => s.id === itemId) ||
          ind.tasks?.some((t) => t.id === itemId),
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

// ── Provider ──────────────────────────────────────────────────────────────────

export function MissionDrawerProvider({ children }: { children: ReactNode }) {
  const {
    missions,
    getCheckInsByIndicator,
    getCheckInSyncMeta,
    retryCheckInSync,
  } = useMissionsData();
  const { ownerOptions, mentionPeople, currentUser } = usePeopleData();

  // Drawer state
  const [drawerOpen, setDrawerOpen] = useState(false);
  const [drawerMode, setDrawerMode] = useState<"indicator" | "task">(
    "indicator",
  );
  const [drawerIndicator, setDrawerIndicator] = useState<Indicator | null>(
    null,
  );
  const [drawerTask, setDrawerTask] = useState<MissionTask | null>(null);
  const [drawerMissionTitle, setDrawerMissionTitle] = useState("");
  const [drawerEditing, setDrawerEditing] = useState(false);
  const overlayOrderRef = useRef(0);
  const [drawerOverlayKey, setDrawerOverlayKey] = useState(0);

  // Check-in form
  const [drawerValue, setDrawerValue] = useState("");
  const [drawerNote, setDrawerNote] = useState("");
  const [drawerConfidence, setDrawerConfidence] =
    useState<ConfidenceLevel | null>(null);
  const drawerNoteRef = useRef<HTMLTextAreaElement>(null);
  const [confidenceOpen, setConfidenceOpen] = useState(false);
  const confidenceBtnRef = useRef<HTMLButtonElement>(null);

  // Support team
  const [supportTeam, setSupportTeam] = useState<string[]>([]);
  const [addSupportOpen, setAddSupportOpen] = useState(false);
  const addSupportRef = useRef<HTMLDivElement>(null);
  const [supportSearch, setSupportSearch] = useState("");

  // Contributions
  const [drawerContributesTo, setDrawerContributesTo] = useState<
    { missionId: string; missionTitle: string }[]
  >([]);
  const [drawerItemId, setDrawerItemId] = useState<string | null>(null);
  const [drawerSourceMissionId, setDrawerSourceMissionId] = useState<
    string | null
  >(null);
  const [drawerSourceMissionTitle, setDrawerSourceMissionTitle] = useState("");
  const [drawerContribPickerOpen, setDrawerContribPickerOpen] = useState(false);
  const [drawerContribPickerSearch, setDrawerContribPickerSearch] =
    useState("");
  const addContribRef = useRef<HTMLButtonElement>(null);

  // Tasks
  const [drawerTasks, setDrawerTasks] = useState<DrawerTask[]>([]);
  const [newTaskLabel, setNewTaskLabel] = useState("");
  const [newlyCreatedCheckInId, setNewlyCreatedCheckInId] = useState<
    string | null
  >(null);

  // Mentions
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

  // Derived: missions list for contribution picker
  const allMissions = useMemo(
    () => flattenMissions(missions).map((m) => ({ id: m.id, title: m.title })),
    [missions],
  );

  // Derived: check-in history for current indicator
  const drawerCheckIns = useMemo(() => {
    if (!drawerIndicator) return [];
    return sortCheckInsDesc(getCheckInsByIndicator(drawerIndicator.id));
  }, [drawerIndicator, getCheckInsByIndicator]);

  const drawerCheckInChartData = useMemo(
    () => buildCheckInChartData(drawerCheckIns),
    [drawerCheckIns],
  );

  const drawerCheckInSyncStateById = useMemo(() => {
    const stateById: Record<string, CheckInSyncState> = {};
    for (const checkIn of drawerCheckIns) {
      const meta = getCheckInSyncMeta(checkIn.id);
      if (!meta) continue;
      stateById[checkIn.id] = meta;
    }
    return stateById;
  }, [drawerCheckIns, getCheckInSyncMeta]);

  // ── Handlers ────────────────────────────────────────────────────────────────

  function openCheckin(payload: CheckinPayload) {
    overlayOrderRef.current += 1;
    setDrawerOverlayKey(overlayOrderRef.current);
    const parentTitle = findParentMission(payload.keyResult.id, missions);
    setDrawerMode("indicator");
    setDrawerTask(null);
    setDrawerIndicator(payload.keyResult);
    setDrawerMissionTitle(parentTitle);
    setDrawerValue(String(payload.newValue));
    setDrawerNote("");
    setDrawerConfidence(null);
    setNewlyCreatedCheckInId(null);
    const history = getCheckInsByIndicator(payload.keyResult.id);
    const team: string[] = [];
    const ownerInitials = getOwnerInitials(payload.keyResult.owner);
    const seen = new Set([ownerInitials]);
    for (const entry of history) {
      const entryInitials = getOwnerInitials(entry.author);
      if (!seen.has(entryInitials)) {
        seen.add(entryInitials);
        team.push(entryInitials);
      }
    }
    setSupportTeam(team);
    setDrawerContributesTo(payload.keyResult.contributesTo ?? []);
    setDrawerItemId(payload.keyResult.id);
    const srcM = findMissionOfItem(payload.keyResult.id, missions);
    setDrawerSourceMissionId(srcM?.id ?? null);
    setDrawerSourceMissionTitle(srcM?.title ?? "");
    const krTasks: DrawerTask[] =
      payload.keyResult.tasks?.map((t) => ({
        id: t.id,
        title: t.title,
        isDone: t.isDone,
        ownerId: t.ownerId,
      })) ??
      DRAWER_TASKS_BY_INDICATOR[payload.keyResult.id]?.map((t) => ({
        id: t.id,
        title: t.title,
        isDone: t.isDone,
        ownerId: null,
      })) ??
      [];
    setDrawerTasks(krTasks);
    setNewTaskLabel("");
    setDrawerOpen(true);
  }

  function openTaskDrawer(task: MissionTask, parentLabel: string) {
    overlayOrderRef.current += 1;
    setDrawerOverlayKey(overlayOrderRef.current);
    setDrawerMode("task");
    setDrawerIndicator(null);
    setNewlyCreatedCheckInId(null);
    setDrawerTask(task);
    // TODO: sync task to missions tree
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

  function openExternalContrib(ec: ExternalContribution) {
    if (ec.type === "indicator") {
      const kr = findIndicatorById(ec.id, missions);
      if (kr)
        openCheckin({
          keyResult: kr,
          currentValue: kr.progress,
          newValue: kr.progress,
        });
    } else {
      const result = findTaskById(ec.id, missions);
      if (result) openTaskDrawer(result.task, result.parentLabel);
    }
  }

  function closeDrawer() {
    setDrawerOpen(false);
    setDrawerIndicator(null);
    setDrawerTask(null);
    setNewlyCreatedCheckInId(null);
    setDrawerItemId(null);
    setDrawerContribPickerOpen(false);
    setMentionQuery(null);
    if (drawerEditing) {
      setDrawerEditing(false);
      // TODO: reset editingItem and isEditingExisting
    }
  }

  function startEdit() {
    // TODO: implement via drawer edit state
  }

  function confirmCheckin() {
    // TODO: implement via BFF
  }

  function updateCheckIn(_checkInId: string, _patch: UpdateCheckInPatch) {
    // TODO: implement via BFF
  }

  function deleteCheckIn(_checkInId: string) {
    // TODO: implement via BFF
  }

  function requestRemoveContribution(
    _itemId: string,
    _itemType: "indicator" | "task",
    _targetMissionId: string,
    _targetMissionTitle: string,
  ) {
    // TODO: implement via BFF
  }

  function addContribution(
    _item: Indicator | MissionTask,
    _itemType: "indicator" | "task",
    _sourceMissionId: string,
    _sourceMissionTitle: string,
    _targetMissionId: string,
    _targetMissionTitle: string,
  ) {
    // TODO: implement via BFF
  }

  function openTaskFromIndicator(taskId: string) {
    const result = findTaskById(taskId, missions);
    if (result) openTaskDrawer(result.task, result.parentLabel);
  }

  function changeTaskOwner(_ownerId: string) {
    // TODO: implement via BFF
  }

  // ── Context value ────────────────────────────────────────────────────────────

  const value: MissionDrawerContextValue = {
    drawerOpen,
    drawerMode,
    drawerIndicator,
    drawerTask,
    setDrawerTask,
    drawerMissionTitle,
    drawerEditing,
    drawerOverlayKey,
    drawerValue,
    setDrawerValue,
    drawerNote,
    drawerNoteRef,
    drawerConfidence,
    setDrawerConfidence,
    confidenceOpen,
    setConfidenceOpen,
    confidenceBtnRef,
    confidenceOptions: CONFIDENCE_OPTIONS,
    supportTeam,
    setSupportTeam,
    addSupportOpen,
    setAddSupportOpen,
    addSupportRef,
    supportSearch,
    setSupportSearch,
    drawerContributesTo,
    setDrawerContributesTo,
    drawerItemId,
    drawerSourceMissionId,
    drawerSourceMissionTitle,
    drawerContribPickerOpen,
    setDrawerContribPickerOpen,
    drawerContribPickerSearch,
    setDrawerContribPickerSearch,
    addContribRef,
    allMissions,
    drawerTasks,
    setDrawerTasks,
    newTaskLabel,
    setNewTaskLabel,
    newlyCreatedCheckInId,
    handleNoteChange,
    handleNoteKeyDown,
    mentionQuery,
    mentionIndex,
    mentionResults,
    insertMention,
    checkInHistoryForIndicator: drawerCheckIns,
    checkInChartDataForIndicator: drawerCheckInChartData,
    checkInSyncStateById: drawerCheckInSyncStateById,
    retryCheckInSync,
    ownerOptions,
    currentUserId: currentUser?.id ?? null,
    editingItem: null, // TODO: move inline edit state here
    renderInlineForm: () => null, // TODO: implement
    openCheckin,
    openTaskDrawer,
    openExternalContrib,
    closeDrawer,
    startEdit,
    confirmCheckin,
    updateCheckIn,
    deleteCheckIn,
    requestRemoveContribution,
    addContribution,
    openTaskFromIndicator,
    changeTaskOwner,
  };

  return (
    <MissionDrawerContext.Provider value={value}>
      {children}
    </MissionDrawerContext.Provider>
  );
}

// ── Hook ──────────────────────────────────────────────────────────────────────

export function useMissionDrawer() {
  const ctx = useContext(MissionDrawerContext);
  if (!ctx) {
    throw new Error(
      "useMissionDrawer must be used within MissionDrawerProvider",
    );
  }
  return ctx;
}
