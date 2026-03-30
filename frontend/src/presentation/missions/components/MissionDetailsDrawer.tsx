"use client";

import {
  useEffect,
  useRef,
  useState,
  type ChangeEvent,
  type KeyboardEvent,
  type MutableRefObject,
  type ReactNode,
} from "react";
import {
  FilterDropdown,
  Button,
  GoalProgressBar,
  GoalGaugeBar,
  Avatar,
  AvatarGroup,
  Input,
  Checkbox,
  Badge,
  Textarea,
  ChartTooltipContent,
  Drawer,
  DrawerHeader,
  DrawerBody,
} from "@mdonangelo/bud-ds";
import {
  Target,
  GitBranch,
  X,
  Plus,
  MagnifyingGlass,
  PencilSimple,
  Trash,
  ArrowRight,
  CaretDown,
  Fire,
} from "@phosphor-icons/react";
import {
  ResponsiveContainer,
  LineChart,
  Line,
  XAxis,
  YAxis,
  CartesianGrid,
  Tooltip as RechartsTooltip,
} from "recharts";
import type { CheckIn, ConfidenceLevel, Indicator, MissionTask } from "@/types";
import {
  numVal,
  getMissionLabel,
  getOwnerName,
  getOwnerInitials,
  formatCheckinDate,
} from "@/lib/tempStorage/missions";
import type { CheckInChartPoint } from "../utils/checkinReadModels";

interface OwnerOption {
  id: string;
  label: string;
  initials: string;
}

interface ConfidenceOption {
  id: ConfidenceLevel;
  label: string;
  description: string;
  color: string;
}

interface MentionPerson {
  id: string;
  label: string;
  initials: string;
}

interface DrawerTask {
  id: string;
  title: string;
  isDone: boolean;
}

interface CheckInSyncState {
  syncStatus: "pending" | "synced" | "failed";
  error: string | null;
  nextRetryAt: string | null;
}

interface UpdateCheckInPatch {
  note?: string | null;
  confidence?: ConfidenceLevel | null;
}

interface MissionDetailsDrawerProps {
  drawerOpen: boolean;
  drawerMode: "indicator" | "task";
  drawerIndicator: Indicator | null;
  drawerTask: MissionTask | null;
  drawerMissionTitle: string;
  drawerEditing: boolean;
  editingItem: unknown;
  renderInlineForm: () => ReactNode;
  startDrawerEdit: () => void;
  handleCloseDrawer: () => void;
  drawerContributesTo: { missionId: string; missionTitle: string }[];
  setDrawerContributesTo: (
    updater: (
      prev: { missionId: string; missionTitle: string }[],
    ) => { missionId: string; missionTitle: string }[],
  ) => void;
  drawerItemId: string | null;
  handleRequestRemoveContribution: (
    itemId: string,
    itemType: "indicator" | "task",
    targetMissionId: string,
    targetMissionTitle: string,
  ) => void;
  drawerContribPickerOpen: boolean;
  setDrawerContribPickerOpen: (open: boolean) => void;
  drawerContribPickerSearch: string;
  setDrawerContribPickerSearch: (value: string) => void;
  addContribRef: MutableRefObject<HTMLButtonElement | null>;
  allMissions: { id: string; title: string }[];
  drawerSourceMissionId: string | null;
  drawerSourceMissionTitle: string;
  handleAddContribution: (
    item: Indicator | MissionTask,
    itemType: "indicator" | "task",
    sourceMissionId: string,
    sourceMissionTitle: string,
    targetMissionId: string,
    targetMissionTitle: string,
  ) => void;
  supportTeam: string[];
  setSupportTeam: (updater: (prev: string[]) => string[]) => void;
  addSupportOpen: boolean;
  setAddSupportOpen: (updater: (prev: boolean) => boolean) => void;
  addSupportRef: MutableRefObject<HTMLDivElement | null>;
  supportSearch: string;
  setSupportSearch: (value: string) => void;
  ownerOptions: OwnerOption[];
  drawerValue: string;
  setDrawerValue: (value: string) => void;
  drawerConfidence: ConfidenceLevel | null;
  setDrawerConfidence: (value: ConfidenceLevel) => void;
  confidenceOpen: boolean;
  setConfidenceOpen: (updater: (prev: boolean) => boolean) => void;
  confidenceBtnRef: MutableRefObject<HTMLButtonElement | null>;
  confidenceOptions: ConfidenceOption[];
  drawerNote: string;
  drawerNoteRef: MutableRefObject<HTMLTextAreaElement | null>;
  handleNoteChange: (e: ChangeEvent<HTMLTextAreaElement>) => void;
  handleNoteKeyDown: (e: KeyboardEvent<HTMLTextAreaElement>) => void;
  mentionQuery: string | null;
  mentionIndex: number;
  mentionResults: MentionPerson[];
  insertMention: (person: MentionPerson) => void;
  handleConfirmCheckin: () => void;
  checkInHistoryForIndicator: CheckIn[];
  checkInChartDataForIndicator: CheckInChartPoint[];
  checkInSyncStateById: Record<string, CheckInSyncState>;
  retryCheckInSync: (checkInId: string) => void;
  onUpdateCheckIn: (checkInId: string, patch: UpdateCheckInPatch) => void;
  onDeleteCheckIn: (checkInId: string) => void;
  newlyCreatedCheckInId: string | null;
  drawerTasks: DrawerTask[];
  setDrawerTasks: (updater: (prev: DrawerTask[]) => DrawerTask[]) => void;
  newTaskLabel: string;
  setNewTaskLabel: (value: string) => void;
  setDrawerTask: (
    updater: (prev: MissionTask | null) => MissionTask | null,
  ) => void;
}

export function MissionDetailsDrawer({
  drawerOpen,
  drawerMode,
  drawerIndicator,
  drawerTask,
  drawerMissionTitle,
  drawerEditing,
  editingItem,
  renderInlineForm,
  startDrawerEdit,
  handleCloseDrawer,
  drawerContributesTo,
  setDrawerContributesTo,
  drawerItemId,
  handleRequestRemoveContribution,
  drawerContribPickerOpen,
  setDrawerContribPickerOpen,
  drawerContribPickerSearch,
  setDrawerContribPickerSearch,
  addContribRef,
  allMissions,
  drawerSourceMissionId,
  drawerSourceMissionTitle,
  handleAddContribution,
  supportTeam,
  setSupportTeam,
  addSupportOpen,
  setAddSupportOpen,
  addSupportRef,
  supportSearch,
  setSupportSearch,
  ownerOptions,
  drawerValue,
  setDrawerValue,
  drawerConfidence,
  setDrawerConfidence,
  confidenceOpen,
  setConfidenceOpen,
  confidenceBtnRef,
  confidenceOptions,
  drawerNote,
  drawerNoteRef,
  handleNoteChange,
  handleNoteKeyDown,
  mentionQuery,
  mentionIndex,
  mentionResults,
  insertMention,
  handleConfirmCheckin,
  checkInHistoryForIndicator,
  checkInChartDataForIndicator,
  checkInSyncStateById,
  retryCheckInSync,
  onUpdateCheckIn,
  onDeleteCheckIn,
  newlyCreatedCheckInId,
  drawerTasks,
  setDrawerTasks,
  newTaskLabel,
  setNewTaskLabel,
  setDrawerTask,
}: MissionDetailsDrawerProps) {
  const newlyCreatedCheckInRef = useRef<HTMLDivElement | null>(null);
  const [highlightedCheckInId, setHighlightedCheckInId] = useState<
    string | null
  >(null);
  const [editingCheckInId, setEditingCheckInId] = useState<string | null>(null);
  const [editingCheckInNote, setEditingCheckInNote] = useState("");
  const [editingCheckInConfidence, setEditingCheckInConfidence] = useState<
    ConfidenceLevel | ""
  >("");

  function startEditingCheckIn(entry: CheckIn) {
    setEditingCheckInId(entry.id);
    setEditingCheckInNote(entry.note ?? "");
    setEditingCheckInConfidence(entry.confidence ?? "");
  }

  function cancelEditingCheckIn() {
    setEditingCheckInId(null);
    setEditingCheckInNote("");
    setEditingCheckInConfidence("");
  }

  function saveEditingCheckIn() {
    if (!editingCheckInId) return;

    const trimmed = editingCheckInNote.trim();
    onUpdateCheckIn(editingCheckInId, {
      note: trimmed.length > 0 ? trimmed : null,
      confidence:
        editingCheckInConfidence === "" ? null : editingCheckInConfidence,
    });
    cancelEditingCheckIn();
  }

  function confirmDeleteCheckIn(checkInId: string) {
    if (!window.confirm("Deseja excluir este check-in?")) {
      return;
    }
    onDeleteCheckIn(checkInId);
    if (editingCheckInId === checkInId) {
      cancelEditingCheckIn();
    }
  }

  useEffect(() => {
    if (!drawerOpen || drawerMode !== "indicator" || !newlyCreatedCheckInId) {
      setHighlightedCheckInId(null);
      return;
    }

    setHighlightedCheckInId(null);
    let cancelled = false;
    let observer: IntersectionObserver | null = null;
    let fallbackTimerId: number | null = null;

    const activateHighlight = () => {
      if (cancelled) return;
      setHighlightedCheckInId(newlyCreatedCheckInId);
      if (observer) {
        observer.disconnect();
        observer = null;
      }
      if (fallbackTimerId !== null) {
        window.clearTimeout(fallbackTimerId);
        fallbackTimerId = null;
      }
    };

    const frameId = requestAnimationFrame(() => {
      const targetElement = newlyCreatedCheckInRef.current;
      if (!targetElement) return;

      targetElement.scrollIntoView({
        behavior: "smooth",
        block: "center",
      });

      observer = new IntersectionObserver(
        (entries) => {
          const targetEntry = entries[0];
          if (!targetEntry) return;
          if (
            targetEntry.isIntersecting &&
            targetEntry.intersectionRatio >= 0.7
          ) {
            activateHighlight();
          }
        },
        {
          threshold: [0.7],
        },
      );

      observer.observe(targetElement);
      fallbackTimerId = window.setTimeout(activateHighlight, 650);
    });

    return () => {
      cancelled = true;
      cancelAnimationFrame(frameId);
      if (observer) observer.disconnect();
      if (fallbackTimerId !== null) {
        window.clearTimeout(fallbackTimerId);
      }
    };
  }, [
    drawerOpen,
    drawerMode,
    newlyCreatedCheckInId,
    checkInHistoryForIndicator.length,
  ]);

  useEffect(() => {
    if (!highlightedCheckInId) return;

    const timerId = window.setTimeout(() => {
      setHighlightedCheckInId((current) =>
        current === highlightedCheckInId ? null : current,
      );
    }, 1000);

    return () => {
      window.clearTimeout(timerId);
    };
  }, [highlightedCheckInId]);

  useEffect(() => {
    if (!drawerOpen || drawerMode !== "indicator") {
      cancelEditingCheckIn();
    }
  }, [drawerOpen, drawerMode]);

  return (
    <Drawer
      open={drawerOpen}
      onClose={handleCloseDrawer}
      size="md"
      aria-label={
        drawerMode === "task" ? "Detalhe da tarefa" : "Detalhe do indicador"
      }
    >
      {drawerMode === "task" && drawerTask && (
        <>
          <DrawerHeader
            title={drawerTask.title}
            onClose={handleCloseDrawer}
            afterTitle={
              <>
                <div className="flex items-center gap-[var(--sp-3xs)] font-[var(--font-body)] text-[var(--text-xs)] text-[var(--color-neutral-500)] leading-[1.3]">
                  <Target size={14} />
                  <span>{drawerMissionTitle}</span>
                </div>
                <div className="flex items-start gap-[var(--sp-3xs)] mt-[var(--sp-3xs)] font-[var(--font-body)] text-[var(--text-xs)] text-[var(--color-neutral-400)] leading-[1.4]">
                  <GitBranch size={12} />
                  <div className="flex flex-wrap items-center gap-[var(--sp-3xs)]">
                    {drawerContributesTo.map((ct) => (
                      <span
                        key={ct.missionId}
                        className="inline-flex items-center gap-[3px] bg-[var(--color-caramel-50)] border border-[var(--color-caramel-200)] rounded-full py-[1px] pr-[var(--sp-3xs)] pl-[var(--sp-2xs)] font-[var(--font-label)] text-[var(--text-xs)] text-[var(--color-neutral-600)] whitespace-nowrap"
                      >
                        <span>{ct.missionTitle}</span>
                        <button
                          type="button"
                          className="inline-flex items-center justify-center p-0 border-none bg-transparent text-[var(--color-neutral-400)] cursor-pointer rounded-full transition-colors duration-[120ms] leading-[1] hover:text-[var(--color-error-600)] hover:bg-[var(--color-error-50)]"
                          aria-label={`Remover contribuição para ${ct.missionTitle}`}
                          onClick={() =>
                            drawerItemId &&
                            handleRequestRemoveContribution(
                              drawerItemId,
                              "task",
                              ct.missionId,
                              ct.missionTitle,
                            )
                          }
                        >
                          <X size={10} />
                        </button>
                      </span>
                    ))}
                    <button
                      ref={addContribRef}
                      type="button"
                      className="inline-flex items-center gap-[3px] py-[1px] px-[var(--sp-2xs)] pl-[var(--sp-3xs)] border border-dashed border-[var(--color-caramel-300)] rounded-full bg-transparent font-[var(--font-label)] text-[var(--text-xs)] text-[var(--color-neutral-400)] cursor-pointer whitespace-nowrap leading-[1.4] hover:text-[var(--color-orange-600)] hover:border-[var(--color-orange-300)] hover:bg-[var(--color-orange-50)]"
                      onClick={() => {
                        setDrawerContribPickerOpen(true);
                        setDrawerContribPickerSearch("");
                      }}
                    >
                      <Plus size={10} />
                      <span>Contribui para...</span>
                    </button>
                  </div>
                </div>
              </>
            }
          >
            {drawerTask.dueDate && (
              <Badge color="neutral">{drawerTask.dueDate}</Badge>
            )}
            <Badge color={drawerTask.isDone ? "success" : "neutral"}>
              {drawerTask.isDone ? "Concluída" : "Pendente"}
            </Badge>
            {!drawerEditing && (
              <Button
                variant="secondary"
                size="sm"
                leftIcon={PencilSimple}
                onClick={startDrawerEdit}
              >
                Editar
              </Button>
            )}
          </DrawerHeader>

          <DrawerBody>
            <FilterDropdown
              open={drawerContribPickerOpen}
              onClose={() => setDrawerContribPickerOpen(false)}
              anchorRef={addContribRef}
              noOverlay
            >
              <div className="flex flex-col p-[var(--sp-3xs)] max-h-[320px] overflow-y-auto">
                <div className="flex items-center gap-[var(--sp-2xs)] p-[var(--sp-2xs)] border-b border-[var(--color-caramel-200)] mb-[var(--sp-3xs)]">
                  <MagnifyingGlass
                    size={14}
                    className="flex-shrink-0 text-[var(--color-neutral-400)]"
                  />
                  <input
                    type="text"
                    className="flex-1 min-w-0 border-none bg-transparent outline-none font-[var(--font-label)] font-medium text-[var(--text-xs)] text-[var(--color-neutral-950)] placeholder:text-[var(--color-neutral-400)]"
                    placeholder="Buscar missão..."
                    value={drawerContribPickerSearch}
                    onChange={(e: ChangeEvent<HTMLInputElement>) =>
                      setDrawerContribPickerSearch(e.target.value)
                    }
                  />
                </div>
                {allMissions
                  .filter((m) => m.id !== drawerSourceMissionId)
                  .filter(
                    (m) =>
                      !drawerContributesTo.some((ct) => ct.missionId === m.id),
                  )
                  .filter((m) =>
                    m.title
                      .toLowerCase()
                      .includes(drawerContribPickerSearch.toLowerCase()),
                  )
                  .map((m) => (
                    <button
                      key={m.id}
                      type="button"
                      className="flex items-center gap-[var(--sp-2xs)] w-full p-[var(--sp-2xs)] border-none bg-transparent rounded-[var(--radius-2xs)] font-[var(--font-label)] font-medium text-[var(--text-xs)] text-[var(--color-neutral-950)] cursor-pointer whitespace-nowrap transition-colors duration-[120ms] text-left hover:bg-[var(--color-caramel-100)]"
                      onClick={() => {
                        if (
                          !drawerItemId ||
                          !drawerSourceMissionId ||
                          !drawerTask
                        )
                          return;
                        handleAddContribution(
                          drawerTask,
                          "task",
                          drawerSourceMissionId,
                          drawerSourceMissionTitle,
                          m.id,
                          m.title,
                        );
                        setDrawerContributesTo((prev) => [
                          ...prev,
                          { missionId: m.id, missionTitle: m.title },
                        ]);
                        setDrawerContribPickerOpen(false);
                      }}
                    >
                      <Target size={14} />
                      <span>{m.title}</span>
                    </button>
                  ))}
              </div>
            </FilterDropdown>

            {drawerEditing && editingItem ? (
              <div className="flex flex-col gap-[var(--sp-xs)] px-[var(--sp-lg)] py-[var(--sp-sm)] border-b border-[var(--color-caramel-100)] last:border-b-0">
                {renderInlineForm()}
              </div>
            ) : (
              <>
                {drawerTask.description && (
                  <div className="flex flex-col gap-[var(--sp-xs)] px-[var(--sp-lg)] py-[var(--sp-sm)] border-b border-[var(--color-caramel-100)] last:border-b-0">
                    <span className="font-[var(--font-label)] font-medium text-[var(--text-xs)] text-[var(--color-neutral-500)] uppercase tracking-[0.5px] leading-[1.15]">
                      Descrição
                    </span>
                    <p className="font-[var(--font-body)] text-[var(--text-sm)] text-[var(--color-neutral-700)] leading-[1.5] m-0">
                      {drawerTask.description}
                    </p>
                  </div>
                )}

                <div className="flex flex-col gap-[var(--sp-xs)] px-[var(--sp-lg)] py-[var(--sp-sm)] border-b border-[var(--color-caramel-100)] last:border-b-0">
                  <span className="font-[var(--font-label)] font-medium text-[var(--text-xs)] text-[var(--color-neutral-500)] uppercase tracking-[0.5px] leading-[1.15]">
                    Participantes
                  </span>
                  <div className="grid grid-cols-2 gap-[var(--sp-sm)]">
                    <div className="flex flex-col gap-[var(--sp-2xs)]">
                      <span className="font-[var(--font-label)] font-medium text-[10px] text-[var(--color-neutral-400)] uppercase tracking-[0.3px] leading-[1.15]">
                        Responsável
                      </span>
                      <div className="flex items-center gap-[var(--sp-2xs)]">
                        <Avatar
                          initials={getOwnerInitials(drawerTask.owner)}
                          size="sm"
                        />
                        <span className="font-[var(--font-label)] font-medium text-[var(--text-xs)] text-[var(--color-neutral-950)] leading-[1.15]">
                          {getOwnerName(drawerTask.owner)}
                        </span>
                      </div>
                    </div>
                    <div className="flex flex-col gap-[var(--sp-2xs)]">
                      <span className="font-[var(--font-label)] font-medium text-[10px] text-[var(--color-neutral-400)] uppercase tracking-[0.3px] leading-[1.15]">
                        Time de apoio
                      </span>
                      <div ref={addSupportRef}>
                        <AvatarGroup
                          size="sm"
                          avatars={supportTeam.map((initials) => ({
                            initials,
                            alt:
                              ownerOptions.find((o) => o.initials === initials)
                                ?.label ?? initials,
                          }))}
                          maxVisible={5}
                          showAddButton
                          onAddClick={() => {
                            setAddSupportOpen((v) => !v);
                            setSupportSearch("");
                          }}
                        />
                      </div>
                      <FilterDropdown
                        open={addSupportOpen}
                        onClose={() => setAddSupportOpen(() => false)}
                        anchorRef={addSupportRef}
                      >
                        <div className="flex flex-col p-[var(--sp-3xs)] max-h-[320px] overflow-y-auto">
                          <div className="flex items-center gap-[var(--sp-2xs)] p-[var(--sp-2xs)] border-b border-[var(--color-caramel-200)] mb-[var(--sp-3xs)]">
                            <MagnifyingGlass
                              size={14}
                              className="flex-shrink-0 text-[var(--color-neutral-400)]"
                            />
                            <input
                              className="flex-1 min-w-0 border-none bg-transparent outline-none font-[var(--font-label)] font-medium text-[var(--text-xs)] text-[var(--color-neutral-950)] placeholder:text-[var(--color-neutral-400)]"
                              placeholder="Buscar pessoa..."
                              value={supportSearch}
                              onChange={(e) => setSupportSearch(e.target.value)}
                              autoFocus
                            />
                          </div>
                          {(() => {
                            const candidates = ownerOptions
                              .filter(
                                (o) =>
                                  o.id !== "all" &&
                                  o.initials !==
                                    getOwnerInitials(drawerTask.owner),
                              )
                              .filter(
                                (o) =>
                                  !supportSearch ||
                                  o.label
                                    .toLowerCase()
                                    .includes(supportSearch.toLowerCase()),
                              );
                            const active = candidates.filter((o) =>
                              supportTeam.includes(o.initials),
                            );
                            const inactive = candidates
                              .filter((o) => !supportTeam.includes(o.initials))
                              .sort((a, b) =>
                                a.label.localeCompare(b.label, "pt-BR"),
                              );
                            const sorted = [...active, ...inactive];
                            return sorted.map((o) => {
                              const isIn = supportTeam.includes(o.initials);
                              return (
                                <button
                                  key={o.id}
                                  className={`flex items-center gap-[var(--sp-2xs)] w-full p-[var(--sp-2xs)] border-none bg-transparent rounded-[var(--radius-2xs)] font-[var(--font-label)] font-medium text-[var(--text-xs)] text-[var(--color-neutral-950)] cursor-pointer whitespace-nowrap transition-colors duration-[120ms] text-left hover:bg-[var(--color-caramel-100)] ${isIn ? "bg-[var(--color-caramel-50)]" : ""}`}
                                  onClick={() =>
                                    setSupportTeam((prev) =>
                                      isIn
                                        ? prev.filter((i) => i !== o.initials)
                                        : [...prev, o.initials],
                                    )
                                  }
                                >
                                  <Checkbox
                                    checked={isIn}
                                    readOnly
                                    tabIndex={-1}
                                  />
                                  <Avatar initials={o.initials} size="xs" />
                                  {o.label}
                                </button>
                              );
                            });
                          })()}
                        </div>
                      </FilterDropdown>
                    </div>
                  </div>
                </div>

                <div className="flex flex-col gap-[var(--sp-xs)] px-[var(--sp-lg)] py-[var(--sp-sm)] border-b border-[var(--color-caramel-100)] last:border-b-0">
                  <div className="flex items-center justify-between">
                    <span className="font-[var(--font-label)] font-medium text-[var(--text-xs)] text-[var(--color-neutral-500)] uppercase tracking-[0.5px] leading-[1.15]">
                      Subtarefas
                    </span>
                    {drawerTask.subtasks && drawerTask.subtasks.length > 0 && (
                      <span className="font-[var(--font-label)] font-medium text-[var(--text-xs)] text-[var(--color-neutral-400)]">
                        {drawerTask.subtasks.filter((s) => s.isDone).length}/
                        {drawerTask.subtasks.length}
                      </span>
                    )}
                  </div>
                  {drawerTask.subtasks && drawerTask.subtasks.length > 0 && (
                    <div className="h-[4px] bg-[var(--color-caramel-200)] rounded-full overflow-hidden">
                      <div
                        className="h-full bg-[var(--color-green-500)] rounded-full transition-[width] duration-300"
                        style={{
                          width: `${(drawerTask.subtasks.filter((s) => s.isDone).length / drawerTask.subtasks.length) * 100}%`,
                        }}
                      />
                    </div>
                  )}
                  <ul className="list-none m-0 p-0 flex flex-col">
                    {[...(drawerTask.subtasks ?? [])]
                      .sort((a, b) => Number(a.isDone) - Number(b.isDone))
                      .map((sub) => (
                        <li
                          key={sub.id}
                          className="flex items-start gap-[var(--sp-2xs)] py-[var(--sp-2xs)] border-b border-[var(--color-caramel-100)] last:border-b-0"
                        >
                          <Checkbox
                            checked={sub.isDone}
                            onChange={() => {
                              setDrawerTask((prev) => {
                                if (!prev) return prev;
                                return {
                                  ...prev,
                                  subtasks: prev.subtasks?.map((s) =>
                                    s.id === sub.id
                                      ? { ...s, isDone: !s.isDone }
                                      : s,
                                  ),
                                };
                              });
                            }}
                          />
                          <span
                            className={`flex-1 min-w-0 font-[var(--font-body)] text-[var(--text-xs)] text-[var(--color-neutral-950)] leading-[1.5] pt-[2px] ${sub.isDone ? "line-through text-[var(--color-neutral-400)]" : ""}`}
                          >
                            {sub.title}
                          </span>
                        </li>
                      ))}
                  </ul>
                  <form
                    className="flex items-center gap-[var(--sp-2xs)] pt-[var(--sp-3xs)]"
                    onSubmit={(e) => {
                      e.preventDefault();
                      if (!newTaskLabel.trim()) return;
                      setDrawerTask((prev) => {
                        if (!prev) return prev;
                        const newSub = {
                          id: `st-${Date.now()}`,
                          taskId: prev.id,
                          title: newTaskLabel.trim(),
                          isDone: false,
                          sortOrder: prev.subtasks?.length ?? 0,
                        };
                        return {
                          ...prev,
                          subtasks: [...(prev.subtasks ?? []), newSub],
                        };
                      });
                      setNewTaskLabel("");
                    }}
                  >
                    <Plus
                      size={16}
                      className="flex-shrink-0 text-[var(--color-neutral-400)]"
                    />
                    <input
                      className="flex-1 min-w-0 border-none bg-transparent outline-none font-[var(--font-body)] text-[var(--text-xs)] text-[var(--color-neutral-950)] leading-[1.5] placeholder:text-[var(--color-neutral-400)]"
                      placeholder="Adicionar subtarefa..."
                      value={newTaskLabel}
                      onChange={(e) => setNewTaskLabel(e.target.value)}
                    />
                  </form>
                </div>
              </>
            )}
          </DrawerBody>
        </>
      )}

      {drawerMode === "indicator" && drawerIndicator && (
        <>
          <DrawerHeader
            title={drawerIndicator.title}
            onClose={handleCloseDrawer}
            afterTitle={
              <>
                <div className="flex items-center gap-[var(--sp-3xs)] font-[var(--font-body)] text-[var(--text-xs)] text-[var(--color-neutral-500)] leading-[1.3]">
                  <Target size={14} />
                  <span>{drawerMissionTitle}</span>
                </div>
                <div className="flex items-start gap-[var(--sp-3xs)] mt-[var(--sp-3xs)] font-[var(--font-body)] text-[var(--text-xs)] text-[var(--color-neutral-400)] leading-[1.4]">
                  <GitBranch size={12} />
                  <div className="flex flex-wrap items-center gap-[var(--sp-3xs)]">
                    {drawerContributesTo.map((ct) => (
                      <span
                        key={ct.missionId}
                        className="inline-flex items-center gap-[3px] bg-[var(--color-caramel-50)] border border-[var(--color-caramel-200)] rounded-full py-[1px] pr-[var(--sp-3xs)] pl-[var(--sp-2xs)] font-[var(--font-label)] text-[var(--text-xs)] text-[var(--color-neutral-600)] whitespace-nowrap"
                      >
                        <span>{ct.missionTitle}</span>
                        <button
                          type="button"
                          className="inline-flex items-center justify-center p-0 border-none bg-transparent text-[var(--color-neutral-400)] cursor-pointer rounded-full transition-colors duration-[120ms] leading-[1] hover:text-[var(--color-error-600)] hover:bg-[var(--color-error-50)]"
                          aria-label={`Remover contribuição para ${ct.missionTitle}`}
                          onClick={() =>
                            drawerItemId &&
                            handleRequestRemoveContribution(
                              drawerItemId,
                              "indicator",
                              ct.missionId,
                              ct.missionTitle,
                            )
                          }
                        >
                          <X size={10} />
                        </button>
                      </span>
                    ))}
                    <button
                      ref={addContribRef}
                      type="button"
                      className="inline-flex items-center gap-[3px] py-[1px] px-[var(--sp-2xs)] pl-[var(--sp-3xs)] border border-dashed border-[var(--color-caramel-300)] rounded-full bg-transparent font-[var(--font-label)] text-[var(--text-xs)] text-[var(--color-neutral-400)] cursor-pointer whitespace-nowrap leading-[1.4] hover:text-[var(--color-orange-600)] hover:border-[var(--color-orange-300)] hover:bg-[var(--color-orange-50)]"
                      onClick={() => {
                        setDrawerContribPickerOpen(true);
                        setDrawerContribPickerSearch("");
                      }}
                    >
                      <Plus size={10} />
                      <span>Contribui para...</span>
                    </button>
                  </div>
                </div>
              </>
            }
          >
            <Badge color="neutral">{drawerIndicator.periodLabel ?? ""}</Badge>
            {!drawerEditing && (
              <Button
                variant="secondary"
                size="sm"
                leftIcon={PencilSimple}
                onClick={startDrawerEdit}
              >
                Editar
              </Button>
            )}
          </DrawerHeader>

          <DrawerBody>
            <FilterDropdown
              open={drawerContribPickerOpen}
              onClose={() => setDrawerContribPickerOpen(false)}
              anchorRef={addContribRef}
              noOverlay
            >
              <div className="flex flex-col p-[var(--sp-3xs)] max-h-[320px] overflow-y-auto">
                <div className="flex items-center gap-[var(--sp-2xs)] p-[var(--sp-2xs)] border-b border-[var(--color-caramel-200)] mb-[var(--sp-3xs)]">
                  <MagnifyingGlass
                    size={14}
                    className="flex-shrink-0 text-[var(--color-neutral-400)]"
                  />
                  <input
                    type="text"
                    className="flex-1 min-w-0 border-none bg-transparent outline-none font-[var(--font-label)] font-medium text-[var(--text-xs)] text-[var(--color-neutral-950)] placeholder:text-[var(--color-neutral-400)]"
                    placeholder="Buscar missão..."
                    value={drawerContribPickerSearch}
                    onChange={(e: ChangeEvent<HTMLInputElement>) =>
                      setDrawerContribPickerSearch(e.target.value)
                    }
                  />
                </div>
                {allMissions
                  .filter((m) => m.id !== drawerSourceMissionId)
                  .filter(
                    (m) =>
                      !drawerContributesTo.some((ct) => ct.missionId === m.id),
                  )
                  .filter((m) =>
                    m.title
                      .toLowerCase()
                      .includes(drawerContribPickerSearch.toLowerCase()),
                  )
                  .map((m) => (
                    <button
                      key={m.id}
                      type="button"
                      className="flex items-center gap-[var(--sp-2xs)] w-full p-[var(--sp-2xs)] border-none bg-transparent rounded-[var(--radius-2xs)] font-[var(--font-label)] font-medium text-[var(--text-xs)] text-[var(--color-neutral-950)] cursor-pointer whitespace-nowrap transition-colors duration-[120ms] text-left hover:bg-[var(--color-caramel-100)]"
                      onClick={() => {
                        if (
                          !drawerItemId ||
                          !drawerSourceMissionId ||
                          !drawerIndicator
                        )
                          return;
                        handleAddContribution(
                          drawerIndicator,
                          "indicator",
                          drawerSourceMissionId,
                          drawerSourceMissionTitle,
                          m.id,
                          m.title,
                        );
                        setDrawerContributesTo((prev) => [
                          ...prev,
                          { missionId: m.id, missionTitle: m.title },
                        ]);
                        setDrawerContribPickerOpen(false);
                      }}
                    >
                      <Target size={14} />
                      <span>{m.title}</span>
                    </button>
                  ))}
              </div>
            </FilterDropdown>

            {drawerEditing && editingItem ? (
              <div className="flex flex-col gap-[var(--sp-xs)] px-[var(--sp-lg)] py-[var(--sp-sm)] border-b border-[var(--color-caramel-100)] last:border-b-0">
                {renderInlineForm()}
              </div>
            ) : (
              <>
                <div className="flex flex-col gap-[var(--sp-xs)] px-[var(--sp-lg)] py-[var(--sp-sm)] border-b border-[var(--color-caramel-100)] last:border-b-0">
                  <div className="py-[var(--sp-3xs)]">
                    {drawerIndicator.missionType === "reach" ? (
                      <GoalProgressBar
                        label={getMissionLabel(drawerIndicator)}
                        value={drawerIndicator.progress}
                        target={numVal(drawerIndicator.targetValue)}
                        expected={numVal(drawerIndicator.expectedValue)}
                        formattedValue={`${drawerIndicator.progress}%`}
                      />
                    ) : (
                      <GoalGaugeBar
                        label={getMissionLabel(drawerIndicator)}
                        value={drawerIndicator.progress}
                        goalType={
                          drawerIndicator.missionType as
                            | "above"
                            | "below"
                            | "between"
                        }
                        low={numVal(drawerIndicator.lowThreshold)}
                        high={numVal(drawerIndicator.highThreshold)}
                        min={0}
                        max={100}
                        formattedValue={String(drawerIndicator.progress)}
                      />
                    )}
                  </div>
                </div>

                {checkInChartDataForIndicator.length > 0 && (
                  <div className="flex flex-col gap-[var(--sp-xs)] px-[var(--sp-lg)] py-[var(--sp-sm)] border-b border-[var(--color-caramel-100)] last:border-b-0">
                    <span className="font-[var(--font-label)] font-medium text-[var(--text-xs)] text-[var(--color-neutral-500)] uppercase tracking-[0.5px] leading-[1.15]">
                      Evolução
                    </span>
                    <div className="bg-[var(--color-caramel-50)] rounded-[var(--radius-xs)] px-[var(--sp-2xs)] py-[var(--sp-xs)]">
                      <ResponsiveContainer width="100%" height={160}>
                        <LineChart
                          data={checkInChartDataForIndicator}
                          margin={{ top: 8, right: 8, bottom: 0, left: -16 }}
                        >
                          <CartesianGrid
                            strokeDasharray="3 3"
                            stroke="var(--color-caramel-200)"
                            vertical={false}
                          />
                          <XAxis
                            dataKey="date"
                            tick={{
                              fontFamily: "var(--font-label)",
                              fontSize: 11,
                              fill: "var(--color-neutral-500)",
                            }}
                            axisLine={false}
                            tickLine={false}
                          />
                          <YAxis
                            tick={{
                              fontFamily: "var(--font-label)",
                              fontSize: 11,
                              fill: "var(--color-neutral-500)",
                            }}
                            axisLine={false}
                            tickLine={false}
                            domain={[0, 100]}
                          />
                          <RechartsTooltip
                            content={<ChartTooltipContent />}
                            animationDuration={150}
                            animationEasing="ease-out"
                          />
                          <Line
                            type="monotone"
                            dataKey="value"
                            stroke="var(--color-orange-500)"
                            strokeWidth={2}
                            dot={{ r: 3, fill: "var(--color-orange-500)" }}
                            activeDot={{ r: 5 }}
                            name="Valor"
                          />
                        </LineChart>
                      </ResponsiveContainer>
                    </div>
                  </div>
                )}

                <div className="flex flex-col gap-[var(--sp-xs)] px-[var(--sp-lg)] py-[var(--sp-sm)] border-b border-[var(--color-caramel-100)] last:border-b-0">
                  <span className="font-[var(--font-label)] font-medium text-[var(--text-xs)] text-[var(--color-neutral-500)] uppercase tracking-[0.5px] leading-[1.15]">
                    Registrar check-in
                  </span>
                  <div className="flex flex-col gap-[var(--sp-xs)]">
                    <div className="flex items-end gap-[var(--sp-sm)] [&>*]:flex-1 [&>*]:min-w-0">
                      <Input
                        label="Valor atual"
                        type="number"
                        value={String(drawerIndicator.progress)}
                        disabled
                      />
                      <div className="flex items-center justify-center text-[var(--color-neutral-400)] h-[36px] flex-shrink-0 flex-[0_0_auto]">
                        <ArrowRight size={16} />
                      </div>
                      <Input
                        label="Novo valor"
                        type="number"
                        value={drawerValue}
                        onChange={(e: ChangeEvent<HTMLInputElement>) =>
                          setDrawerValue(e.target.value)
                        }
                      />
                    </div>
                    <div className="flex flex-col gap-[var(--sp-3xs)] relative">
                      <span className="font-[var(--font-label)] font-medium text-[var(--text-xs)] text-[var(--color-neutral-500)] leading-[1.15]">
                        Confiança
                      </span>
                      <button
                        ref={confidenceBtnRef}
                        className="flex items-center gap-[var(--sp-2xs)] w-full min-h-[36px] px-[var(--sp-xs)] py-[var(--sp-2xs)] bg-white border border-[var(--color-caramel-300)] rounded-[var(--radius-xs)] cursor-pointer transition-colors duration-[120ms] text-left hover:border-[var(--color-caramel-500)] focus-visible:outline-2 focus-visible:outline-[var(--color-orange-500)] focus-visible:outline-offset-2"
                        onClick={() => setConfidenceOpen((v) => !v)}
                        type="button"
                      >
                        {drawerConfidence ? (
                          <>
                            <span
                              className="w-[10px] h-[10px] rounded-full flex-shrink-0"
                              style={{
                                backgroundColor: confidenceOptions.find(
                                  (o) => o.id === drawerConfidence,
                                )?.color,
                              }}
                            />
                            <span className="flex-1 min-w-0 font-[var(--font-label)] font-medium text-[var(--text-xs)] text-[var(--color-neutral-950)] leading-[1.15] overflow-hidden text-ellipsis whitespace-nowrap">
                              {
                                confidenceOptions.find(
                                  (o) => o.id === drawerConfidence,
                                )?.label
                              }
                            </span>
                          </>
                        ) : (
                          <span className="flex-1 font-[var(--font-body)] text-[var(--text-xs)] text-[var(--color-neutral-400)] leading-[1.15]">
                            Selecione o nível de confiança
                          </span>
                        )}
                        <CaretDown
                          size={14}
                          className="flex-shrink-0 text-[var(--color-neutral-400)] transition-transform duration-[150ms]"
                        />
                      </button>
                      <FilterDropdown
                        open={confidenceOpen}
                        onClose={() => setConfidenceOpen(() => false)}
                        anchorRef={confidenceBtnRef}
                      >
                        <div className="flex flex-col p-[var(--sp-3xs)] max-h-[320px] overflow-y-auto">
                          {confidenceOptions.map((opt) => (
                            <button
                              key={opt.id}
                              className={`flex items-start gap-[var(--sp-2xs)] w-full px-[var(--sp-xs)] py-[var(--sp-2xs)] border-none bg-transparent rounded-[var(--radius-2xs)] cursor-pointer text-left transition-colors duration-[120ms] hover:bg-[var(--color-caramel-100)] ${drawerConfidence === opt.id ? "bg-[var(--color-caramel-50)]" : ""}`}
                              onClick={() => {
                                setDrawerConfidence(opt.id);
                                setConfidenceOpen(() => false);
                              }}
                            >
                              <span
                                className="w-[10px] h-[10px] rounded-full flex-shrink-0"
                                style={{ backgroundColor: opt.color }}
                              />
                              <div className="flex flex-col gap-[2px] flex-1 min-w-0">
                                <span className="font-[var(--font-label)] font-medium text-[var(--text-xs)] text-[var(--color-neutral-950)] leading-[1.15]">
                                  {opt.label}
                                </span>
                                <span className="font-[var(--font-body)] text-[10px] text-[var(--color-neutral-500)] leading-[1.4]">
                                  {opt.description}
                                </span>
                              </div>
                            </button>
                          ))}
                        </div>
                      </FilterDropdown>
                    </div>
                    <div className="relative">
                      <Textarea
                        ref={drawerNoteRef}
                        label="Comentário"
                        placeholder="O que mudou desde o último check-in? Para marcar alguém, use @"
                        value={drawerNote}
                        onChange={handleNoteChange}
                        onKeyDown={handleNoteKeyDown}
                        rows={3}
                      />
                      {mentionQuery !== null && mentionResults.length > 0 && (
                        <ul className="absolute bottom-full left-0 right-0 m-0 p-[var(--sp-3xs)] list-none bg-white border border-[var(--color-caramel-200)] rounded-[var(--radius-xs)] shadow-[var(--shadow-md,0_4px_12px_rgba(0,0,0,0.08))] max-h-[200px] overflow-y-auto z-10">
                          {mentionResults.map((person, idx) => (
                            <li key={person.id}>
                              <button
                                className={`flex items-center gap-[var(--sp-2xs)] w-full p-[var(--sp-2xs)] border-none bg-transparent rounded-[var(--radius-2xs)] font-[var(--font-label)] font-medium text-[var(--text-xs)] text-[var(--color-neutral-950)] cursor-pointer text-left transition-colors duration-[120ms] hover:bg-[var(--color-caramel-100)] ${idx === mentionIndex ? "bg-[var(--color-caramel-100)]" : ""}`}
                                onMouseDown={(e) => {
                                  e.preventDefault();
                                  insertMention(person);
                                }}
                              >
                                <Avatar initials={person.initials} size="xs" />
                                <span>{person.label}</span>
                              </button>
                            </li>
                          ))}
                        </ul>
                      )}
                    </div>
                    <Button
                      variant="primary"
                      size="md"
                      onClick={handleConfirmCheckin}
                      style={{ alignSelf: "flex-end" }}
                    >
                      Registrar check-in
                    </Button>
                  </div>
                </div>

                <div className="flex flex-col gap-[var(--sp-xs)] px-[var(--sp-lg)] py-[var(--sp-sm)] border-b border-[var(--color-caramel-100)] last:border-b-0">
                  <span className="font-[var(--font-label)] font-medium text-[var(--text-xs)] text-[var(--color-neutral-500)] uppercase tracking-[0.5px] leading-[1.15]">
                    Participantes
                  </span>
                  <div className="grid grid-cols-2 gap-[var(--sp-sm)]">
                    <div className="flex flex-col gap-[var(--sp-2xs)]">
                      <span className="font-[var(--font-label)] font-medium text-[10px] text-[var(--color-neutral-400)] uppercase tracking-[0.3px] leading-[1.15]">
                        Responsável
                      </span>
                      <div className="flex items-center gap-[var(--sp-2xs)]">
                        <Avatar
                          initials={getOwnerInitials(drawerIndicator.owner)}
                          size="sm"
                        />
                        <span className="font-[var(--font-label)] font-medium text-[var(--text-xs)] text-[var(--color-neutral-950)] leading-[1.15]">
                          {getOwnerName(drawerIndicator.owner)}
                        </span>
                      </div>
                    </div>
                    <div className="flex flex-col gap-[var(--sp-2xs)]">
                      <span className="font-[var(--font-label)] font-medium text-[10px] text-[var(--color-neutral-400)] uppercase tracking-[0.3px] leading-[1.15]">
                        Time de apoio
                      </span>
                      <div ref={addSupportRef}>
                        <AvatarGroup
                          size="sm"
                          avatars={supportTeam.map((initials) => ({
                            initials,
                            alt:
                              ownerOptions.find((o) => o.initials === initials)
                                ?.label ?? initials,
                          }))}
                          maxVisible={5}
                          showAddButton
                          onAddClick={() => {
                            setAddSupportOpen((v) => !v);
                            setSupportSearch("");
                          }}
                        />
                      </div>
                      <FilterDropdown
                        open={addSupportOpen}
                        onClose={() => setAddSupportOpen(() => false)}
                        anchorRef={addSupportRef}
                      >
                        <div className="flex flex-col p-[var(--sp-3xs)] max-h-[320px] overflow-y-auto">
                          <div className="flex items-center gap-[var(--sp-2xs)] p-[var(--sp-2xs)] border-b border-[var(--color-caramel-200)] mb-[var(--sp-3xs)]">
                            <MagnifyingGlass
                              size={14}
                              className="flex-shrink-0 text-[var(--color-neutral-400)]"
                            />
                            <input
                              className="flex-1 min-w-0 border-none bg-transparent outline-none font-[var(--font-label)] font-medium text-[var(--text-xs)] text-[var(--color-neutral-950)] placeholder:text-[var(--color-neutral-400)]"
                              placeholder="Buscar pessoa..."
                              value={supportSearch}
                              onChange={(e) => setSupportSearch(e.target.value)}
                              autoFocus
                            />
                          </div>
                          {(() => {
                            const candidates = ownerOptions
                              .filter(
                                (o) =>
                                  o.id !== "all" &&
                                  o.initials !==
                                    getOwnerInitials(drawerIndicator.owner),
                              )
                              .filter(
                                (o) =>
                                  !supportSearch ||
                                  o.label
                                    .toLowerCase()
                                    .includes(supportSearch.toLowerCase()),
                              );
                            const active = candidates.filter((o) =>
                              supportTeam.includes(o.initials),
                            );
                            const inactive = candidates
                              .filter((o) => !supportTeam.includes(o.initials))
                              .sort((a, b) =>
                                a.label.localeCompare(b.label, "pt-BR"),
                              );
                            const sorted = [...active, ...inactive];
                            return sorted.map((o) => {
                              const isIn = supportTeam.includes(o.initials);
                              return (
                                <button
                                  key={o.id}
                                  className={`flex items-center gap-[var(--sp-2xs)] w-full p-[var(--sp-2xs)] border-none bg-transparent rounded-[var(--radius-2xs)] font-[var(--font-label)] font-medium text-[var(--text-xs)] text-[var(--color-neutral-950)] cursor-pointer whitespace-nowrap transition-colors duration-[120ms] text-left hover:bg-[var(--color-caramel-100)] ${isIn ? "bg-[var(--color-caramel-50)]" : ""}`}
                                  onClick={() =>
                                    setSupportTeam((prev) =>
                                      isIn
                                        ? prev.filter((i) => i !== o.initials)
                                        : [...prev, o.initials],
                                    )
                                  }
                                >
                                  <Checkbox
                                    checked={isIn}
                                    readOnly
                                    tabIndex={-1}
                                  />
                                  <Avatar initials={o.initials} size="xs" />
                                  {o.label}
                                </button>
                              );
                            });
                          })()}
                        </div>
                      </FilterDropdown>
                    </div>
                  </div>
                </div>

                <div className="flex flex-col gap-[var(--sp-xs)] px-[var(--sp-lg)] py-[var(--sp-sm)] border-b border-[var(--color-caramel-100)] last:border-b-0">
                  <div className="flex items-center justify-between">
                    <span className="font-[var(--font-label)] font-medium text-[var(--text-xs)] text-[var(--color-neutral-500)] uppercase tracking-[0.5px] leading-[1.15]">
                      Tarefas
                    </span>
                    <span className="font-[var(--font-label)] font-medium text-[var(--text-xs)] text-[var(--color-neutral-400)]">
                      {drawerTasks.filter((t) => t.isDone).length}/
                      {drawerTasks.length}
                    </span>
                  </div>
                  {drawerTasks.length > 0 && (
                    <div className="h-[4px] bg-[var(--color-caramel-200)] rounded-full overflow-hidden">
                      <div
                        className="h-full bg-[var(--color-green-500)] rounded-full transition-[width] duration-300"
                        style={{
                          width: `${(drawerTasks.filter((t) => t.isDone).length / drawerTasks.length) * 100}%`,
                        }}
                      />
                    </div>
                  )}
                  <ul className="list-none m-0 p-0 flex flex-col">
                    {[...drawerTasks]
                      .sort((a, b) => Number(a.isDone) - Number(b.isDone))
                      .map((task) => (
                        <li
                          key={task.id}
                          className="flex items-start gap-[var(--sp-2xs)] py-[var(--sp-2xs)] border-b border-[var(--color-caramel-100)] last:border-b-0"
                        >
                          <Checkbox
                            checked={task.isDone}
                            onChange={() =>
                              setDrawerTasks((prev) =>
                                prev.map((t) =>
                                  t.id === task.id
                                    ? { ...t, isDone: !t.isDone }
                                    : t,
                                ),
                              )
                            }
                          />
                          <span
                            className={`flex-1 min-w-0 font-[var(--font-body)] text-[var(--text-xs)] text-[var(--color-neutral-950)] leading-[1.5] pt-[2px] ${task.isDone ? "line-through text-[var(--color-neutral-400)]" : ""}`}
                          >
                            {task.title}
                          </span>
                        </li>
                      ))}
                  </ul>
                  <form
                    className="flex items-center gap-[var(--sp-2xs)] pt-[var(--sp-3xs)]"
                    onSubmit={(e) => {
                      e.preventDefault();
                      if (!newTaskLabel.trim()) return;
                      setDrawerTasks((prev) => [
                        ...prev,
                        {
                          id: `t-${Date.now()}`,
                          title: newTaskLabel.trim(),
                          isDone: false,
                        },
                      ]);
                      setNewTaskLabel("");
                    }}
                  >
                    <Plus
                      size={16}
                      className="flex-shrink-0 text-[var(--color-neutral-400)]"
                    />
                    <input
                      className="flex-1 min-w-0 border-none bg-transparent outline-none font-[var(--font-body)] text-[var(--text-xs)] text-[var(--color-neutral-950)] leading-[1.5] placeholder:text-[var(--color-neutral-400)]"
                      placeholder="Adicionar tarefa..."
                      value={newTaskLabel}
                      onChange={(e) => setNewTaskLabel(e.target.value)}
                    />
                  </form>
                </div>

                {(() => {
                  const checkins = checkInHistoryForIndicator;
                  if (checkins.length === 0) return null;
                  return (
                    <div className="flex flex-col gap-[var(--sp-xs)] px-[var(--sp-lg)] py-[var(--sp-sm)] border-b border-[var(--color-caramel-100)] last:border-b-0">
                      <span className="font-[var(--font-label)] font-medium text-[var(--text-xs)] text-[var(--color-neutral-500)] uppercase tracking-[0.5px] leading-[1.15]">
                        Histórico de check-ins
                      </span>
                      <div className="flex flex-col">
                        {checkins.map((entry: CheckIn, idx: number) => {
                          const syncState = checkInSyncStateById[entry.id];
                          const isSyncPending =
                            syncState?.syncStatus === "pending";
                          const isSyncFailed =
                            syncState?.syncStatus === "failed";

                          return (
                            <div
                              key={entry.id}
                              ref={
                                entry.id === newlyCreatedCheckInId
                                  ? newlyCreatedCheckInRef
                                  : null
                              }
                              className={`flex gap-[var(--sp-xs)] min-h-0 ${entry.id === highlightedCheckInId ? "scroll-mt-[var(--sp-lg)]" : ""}`}
                            >
                              <div className="flex flex-col items-center">
                                <span
                                  className={`w-[10px] h-[10px] rounded-full bg-[var(--color-caramel-300)] flex-shrink-0 ${idx === 0 ? "bg-[var(--color-orange-500)]" : ""} ${entry.id === highlightedCheckInId ? "animate-pulse" : ""}`}
                                />
                                {idx < checkins.length - 1 && (
                                  <span className="flex-1 w-[1.5px] bg-[var(--color-caramel-200)] mx-auto mt-[2px]" />
                                )}
                              </div>
                              <div className="flex flex-col gap-[var(--sp-2xs)] flex-1 min-w-0 pb-[var(--sp-sm)]">
                                <div className="flex items-center gap-[var(--sp-2xs)]">
                                  <Avatar
                                    initials={getOwnerInitials(entry.author)}
                                    size="xs"
                                  />
                                  <span className="font-[var(--font-label)] font-medium text-[var(--text-xs)] text-[var(--color-neutral-950)] leading-[1.15]">
                                    {getOwnerName(entry.author)}
                                  </span>
                                  <div className="flex items-center gap-[var(--sp-2xs)] ml-auto">
                                    <span className="font-[var(--font-body)] text-[var(--text-xs)] text-[var(--color-neutral-400)]">
                                      {formatCheckinDate(entry.createdAt)}
                                    </span>
                                    <div className="flex items-center gap-[2px]">
                                      <button
                                        type="button"
                                        className="inline-flex items-center justify-center w-[22px] h-[22px] border-none bg-transparent rounded-[var(--radius-2xs)] text-[var(--color-neutral-400)] cursor-pointer transition-colors duration-[120ms] hover:bg-[var(--color-caramel-100)] hover:text-[var(--color-neutral-700)]"
                                        aria-label="Editar check-in"
                                        onClick={() =>
                                          startEditingCheckIn(entry)
                                        }
                                      >
                                        <PencilSimple size={12} />
                                      </button>
                                      <button
                                        type="button"
                                        className="inline-flex items-center justify-center w-[22px] h-[22px] border-none bg-transparent rounded-[var(--radius-2xs)] text-[var(--color-neutral-400)] cursor-pointer transition-colors duration-[120ms] hover:bg-[var(--color-error-50)] hover:text-[var(--color-error-600)]"
                                        aria-label="Excluir check-in"
                                        onClick={() =>
                                          confirmDeleteCheckIn(entry.id)
                                        }
                                      >
                                        <Trash size={12} />
                                      </button>
                                    </div>
                                  </div>
                                </div>
                                <div className="flex items-center gap-[var(--sp-2xs)] flex-wrap">
                                  <span className="font-[var(--font-label)] text-[var(--text-xs)] text-[var(--color-neutral-400)] line-through">
                                    {numVal(entry.previousValue)}%
                                  </span>
                                  <ArrowRight size={12} />
                                  <span className="font-[var(--font-label)] font-medium text-[var(--text-xs)] text-[var(--color-neutral-950)]">
                                    {numVal(entry.value)}%
                                  </span>
                                  {entry.confidence && (
                                    <span
                                      className="inline-flex items-center gap-[var(--sp-3xs)] font-[var(--font-label)] text-[var(--text-xs)] text-[var(--color-neutral-500)]"
                                      style={{
                                        color: confidenceOptions.find(
                                          (c) => c.id === entry.confidence,
                                        )?.color,
                                      }}
                                    >
                                      <Fire size={12} />
                                      {
                                        confidenceOptions.find(
                                          (c) => c.id === entry.confidence,
                                        )?.label
                                      }
                                    </span>
                                  )}
                                </div>
                                {(isSyncPending || isSyncFailed) && (
                                  <div className="flex items-center gap-[var(--sp-2xs)]">
                                    {isSyncPending && (
                                      <Badge color="neutral">
                                        Sincronizando
                                      </Badge>
                                    )}
                                    {isSyncFailed && (
                                      <Badge color="error">Falha de sync</Badge>
                                    )}
                                    {isSyncFailed && (
                                      <button
                                        type="button"
                                        className="inline-flex items-center gap-[var(--sp-3xs)] border-none bg-transparent font-[var(--font-label)] font-medium text-[var(--text-xs)] text-[var(--color-orange-600)] cursor-pointer underline p-0"
                                        onClick={() =>
                                          retryCheckInSync(entry.id)
                                        }
                                      >
                                        Tentar novamente
                                      </button>
                                    )}
                                  </div>
                                )}
                                {isSyncFailed && syncState?.error && (
                                  <p className="font-[var(--font-body)] text-[var(--text-xs)] text-[var(--color-error-600)] m-0">
                                    {syncState.error}
                                  </p>
                                )}
                                {isSyncFailed && syncState?.nextRetryAt && (
                                  <p className="font-[var(--font-body)] text-[var(--text-xs)] text-[var(--color-neutral-400)] m-0">
                                    Nova tentativa automatica as{" "}
                                    {new Date(
                                      syncState.nextRetryAt,
                                    ).toLocaleTimeString("pt-BR", {
                                      hour: "2-digit",
                                      minute: "2-digit",
                                    })}
                                    .
                                  </p>
                                )}
                                {editingCheckInId === entry.id ? (
                                  <div className="flex flex-col gap-[var(--sp-xs)] bg-[var(--color-caramel-50)] rounded-[var(--radius-xs)] p-[var(--sp-xs)]">
                                    <div className="flex items-end gap-[var(--sp-2xs)]">
                                      <label
                                        className="font-[var(--font-label)] font-medium text-[var(--text-xs)] text-[var(--color-neutral-500)] leading-[1.15]"
                                        htmlFor={`checkin-confidence-${entry.id}`}
                                      >
                                        Confianca
                                      </label>
                                      <select
                                        id={`checkin-confidence-${entry.id}`}
                                        className="flex-1 min-w-0"
                                        value={editingCheckInConfidence}
                                        onChange={(
                                          event: ChangeEvent<HTMLSelectElement>,
                                        ) =>
                                          setEditingCheckInConfidence(
                                            event.target.value as
                                              | ConfidenceLevel
                                              | "",
                                          )
                                        }
                                      >
                                        <option value="">Sem confianca</option>
                                        {confidenceOptions.map((opt) => (
                                          <option key={opt.id} value={opt.id}>
                                            {opt.label}
                                          </option>
                                        ))}
                                      </select>
                                    </div>
                                    <textarea
                                      className="w-full border-none bg-transparent outline-none font-[var(--font-body)] text-[var(--text-xs)] text-[var(--color-neutral-950)] leading-[1.5] resize-none placeholder:text-[var(--color-neutral-400)]"
                                      value={editingCheckInNote}
                                      onChange={(
                                        event: ChangeEvent<HTMLTextAreaElement>,
                                      ) =>
                                        setEditingCheckInNote(
                                          event.target.value,
                                        )
                                      }
                                      rows={2}
                                      placeholder="Comentário do check-in"
                                    />
                                    <div className="flex items-center justify-end gap-[var(--sp-2xs)]">
                                      <Button
                                        variant="secondary"
                                        size="sm"
                                        onClick={cancelEditingCheckIn}
                                      >
                                        Cancelar
                                      </Button>
                                      <Button
                                        variant="primary"
                                        size="sm"
                                        onClick={saveEditingCheckIn}
                                      >
                                        Salvar
                                      </Button>
                                    </div>
                                  </div>
                                ) : (
                                  entry.note && (
                                    <p className="font-[var(--font-body)] text-[var(--text-xs)] text-[var(--color-neutral-700)] leading-[1.5] m-0">
                                      {entry.note}
                                    </p>
                                  )
                                )}
                                {entry.mentions &&
                                  entry.mentions.length > 0 && (
                                    <div className="flex flex-wrap gap-[var(--sp-3xs)]">
                                      {entry.mentions.map((m: string) => (
                                        <span
                                          key={m}
                                          className="font-[var(--font-label)] font-medium text-[var(--text-xs)] text-[var(--color-orange-600)]"
                                        >
                                          @{m}
                                        </span>
                                      ))}
                                    </div>
                                  )}
                              </div>
                            </div>
                          );
                        })}
                      </div>
                    </div>
                  );
                })()}
              </>
            )}
          </DrawerBody>
        </>
      )}
    </Drawer>
  );
}
