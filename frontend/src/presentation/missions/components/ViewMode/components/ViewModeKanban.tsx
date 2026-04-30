import { useEffect, useRef, useState } from "react";
import type { ComponentType } from "react";
import {
  AvatarGroup,
  Badge,
  Button,
  Card,
  CardBody,
  Checkbox,
  FilterDropdown,
  GoalProgressBar,
} from "@getbud-co/buds";
import {
  ArrowRight,
  CaretDown,
  CaretUp,
  PencilSimple,
  Target,
} from "@phosphor-icons/react";
import type { IconProps } from "@phosphor-icons/react";
import type { Mission, KanbanStatus } from "@/types";
import {
  numVal,
  getOwnerName,
  getOwnerInitials,
  getIndicatorIcon,
  getMissionLabel,
} from "@/lib/tempStorage/missions";
import { findIndicatorById, findTaskById } from "../../../utils/missionTree";
import { KANBAN_COLUMNS } from "../../../consts";
import { useMissionDrawer } from "../../../contexts/MissionDrawerContext";

// ── Types ─────────────────────────────────────────────────────────────────────

interface KanbanChildItem {
  id: string;
  label: string;
  value: number;
  target: number;
  goalLabel: string;
  ownerInitials: string;
  period: string;
  icon?: ComponentType<IconProps>;
}

interface KanbanItem {
  id: string;
  label: string;
  missionTitle: string;
  missionId: string;
  value: number;
  target: number;
  goalLabel: string;
  ownerInitials: string;
  ownerName: string;
  period: string;
  type: "indicator" | "mission" | "task";
  icon?: ComponentType<IconProps>;
  children?: KanbanChildItem[];
  done?: boolean;
  teamName?: string;
  teamColor?: string;
}

// ── Utils ─────────────────────────────────────────────────────────────────────

function collectKanbanItems(missionList: Mission[]): KanbanItem[] {
  const items: KanbanItem[] = [];

  for (const m of missionList) {
    for (const kr of m.indicators ?? []) {
      items.push({
        id: kr.id,
        label: kr.title,
        missionTitle: m.title,
        missionId: m.id,
        value: kr.progress,
        target: numVal(kr.targetValue),
        goalLabel: getMissionLabel(kr),
        ownerInitials: getOwnerInitials(kr.owner),
        ownerName: getOwnerName(kr.owner),
        period: kr.periodLabel ?? "",
        type: "indicator",
        icon: getIndicatorIcon(kr),
        teamName: m.team?.name,
        teamColor: m.team?.color,
      });
      if (kr.children) {
        for (const sub of kr.children) {
          items.push({
            id: sub.id,
            label: sub.title,
            missionTitle: `${m.title} › ${kr.title}`,
            missionId: m.id,
            value: sub.progress,
            target: numVal(sub.targetValue),
            goalLabel: getMissionLabel(sub),
            ownerInitials: getOwnerInitials(sub.owner),
            ownerName: getOwnerName(sub.owner),
            period: sub.periodLabel ?? "",
            type: "indicator",
            icon: getIndicatorIcon(sub),
            teamName: m.team?.name,
            teamColor: m.team?.color,
          });
        }
      }
      if (kr.tasks) {
        for (const task of kr.tasks) {
          items.push({
            id: task.id,
            label: task.title,
            missionTitle: `${m.title} › ${kr.title}`,
            missionId: m.id,
            value: task.isDone ? 100 : 0,
            target: 100,
            goalLabel: task.isDone ? "Concluída" : "Pendente",
            ownerInitials: getOwnerInitials(task.owner),
            ownerName: getOwnerName(task.owner),
            period: "",
            type: "task",
            done: task.isDone,
            teamName: m.team?.name,
            teamColor: m.team?.color,
          });
        }
      }
    }
    if (m.tasks) {
      for (const task of m.tasks) {
        items.push({
          id: task.id,
          label: task.title,
          missionTitle: m.title,
          missionId: m.id,
          value: task.isDone ? 100 : 0,
          target: 100,
          goalLabel: task.isDone ? "Concluída" : "Pendente",
          ownerInitials: getOwnerInitials(task.owner),
          ownerName: getOwnerName(task.owner),
          period: "",
          type: "task",
          done: task.isDone,
          teamName: m.team?.name,
          teamColor: m.team?.color,
        });
      }
    }
    if (m.children) {
      for (const child of m.children) {
        const cIndicators = child.indicators ?? [];
        items.push({
          id: child.id,
          label: child.title,
          missionTitle: m.title,
          missionId: m.id,
          value: child.progress,
          target: 100,
          goalLabel: `${cIndicators.length} indicador${cIndicators.length !== 1 ? "es" : ""}`,
          ownerInitials: cIndicators[0] ? getOwnerInitials(cIndicators[0].owner) : "",
          ownerName: cIndicators[0] ? getOwnerName(cIndicators[0].owner) : "",
          period: cIndicators[0]?.periodLabel ?? "",
          type: "mission",
          teamName: m.team?.name,
          teamColor: m.team?.color,
          children: cIndicators.map((ci) => ({
            id: ci.id,
            label: ci.title,
            value: ci.progress,
            target: numVal(ci.targetValue),
            goalLabel: getMissionLabel(ci),
            ownerInitials: getOwnerInitials(ci.owner),
            period: ci.periodLabel ?? "",
            icon: getIndicatorIcon(ci),
          })),
        });
      }
    }
  }

  return items;
}

// ── Component ─────────────────────────────────────────────────────────────────

interface ViewModeKanbanProps {
  displayedMissions: Mission[];
  missions: Mission[];
  isMultiTeam: boolean;
  onToggleTask: (taskId: string) => void;
}

export function ViewModeKanban({
  displayedMissions,
  missions,
  isMultiTeam,
  onToggleTask,
}: ViewModeKanbanProps) {
  const { openCheckin, openTaskDrawer } = useMissionDrawer();

  const [kanbanStatuses, setKanbanStatuses] = useState<Record<string, KanbanStatus>>({});
  const [kanbanMoveOpen, setKanbanMoveOpen] = useState<string | null>(null);
  const [kanbanExpanded, setKanbanExpanded] = useState<Set<string>>(new Set());
  const [draggedItemId, setDraggedItemId] = useState<string | null>(null);
  const [dragOverColumn, setDragOverColumn] = useState<KanbanStatus | null>(null);

  const kanbanMoveBtnRefs = useRef<Record<string, HTMLButtonElement | null>>({});
  const kanbanDragRef = useRef<{ itemId: string; value: number } | null>(null);

  useEffect(() => {
    function onPointerUp() {
      if (!kanbanDragRef.current) return;
      const { itemId, value } = kanbanDragRef.current;
      kanbanDragRef.current = null;
      const kr = findIndicatorById(itemId, missions);
      if (!kr) return;
      requestAnimationFrame(() => {
        openCheckin({ keyResult: kr, currentValue: kr.progress, newValue: value });
      });
    }
    document.addEventListener("pointerup", onPointerUp);
    return () => document.removeEventListener("pointerup", onPointerUp);
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [missions]);

  const kanbanItems = collectKanbanItems(displayedMissions);

  function getKanbanStatus(itemId: string): KanbanStatus {
    if (kanbanStatuses[itemId]) return kanbanStatuses[itemId];
    const taskItem = kanbanItems.find((ki) => ki.id === itemId && ki.type === "task");
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

  return (
    <div className="grid grid-cols-4 gap-[var(--sp-sm)] min-h-[300px] max-[960px]:grid-cols-2">
      {KANBAN_COLUMNS.map((col) => {
        const colItems = kanbanItems.filter(
          (item) => getKanbanStatus(item.id) === col.id,
        );
        const isDropTarget = dragOverColumn === col.id && draggedItemId !== null;

        return (
          <div
            key={col.id}
            className="flex flex-col gap-[var(--sp-xs)] min-w-0"
            onDragOver={(e) => {
              e.preventDefault();
              e.dataTransfer.dropEffect = "move";
              setDragOverColumn(col.id);
            }}
            onDragLeave={(e) => {
              if (!e.currentTarget.contains(e.relatedTarget as Node)) {
                setDragOverColumn(null);
              }
            }}
            onDrop={(e) => {
              e.preventDefault();
              const itemId = e.dataTransfer.getData("text/plain");
              if (itemId) moveToKanban(itemId, col.id);
              setDragOverColumn(null);
              setDraggedItemId(null);
            }}
          >
            {/* Column header */}
            <div className="flex items-center gap-[var(--sp-2xs)] pb-[var(--sp-xs)] border-b-[1.5px] border-[var(--color-caramel-200)]">
              <span
                className="w-2 h-2 rounded-full shrink-0"
                style={{ backgroundColor: col.color }}
              />
              <span className="font-[var(--font-heading)] text-[var(--text-xs)] font-semibold text-[var(--color-neutral-950)] leading-[1.15]">
                {col.label}
              </span>
              <span className="font-[var(--font-body)] text-[var(--text-xs)] text-[var(--color-neutral-400)] ml-auto">
                {colItems.length}
              </span>
            </div>

            {/* Column body */}
            <div
              className={`flex flex-col gap-[var(--sp-2xs)] min-h-[60px] p-[var(--sp-3xs)] rounded-[var(--radius-xs)] transition-[background-color,box-shadow] duration-200 ease-in-out${isDropTarget ? " bg-[var(--color-caramel-50)] shadow-[inset_0_0_0_1.5px_var(--color-caramel-300),inset_0_2px_8px_rgba(0,0,0,0.04)]" : ""}`}
            >
              {colItems.map((item) => {
                const Icon = item.icon;
                const hasChildren =
                  item.type === "mission" && (item.children?.length ?? 0) > 0;
                const isExpanded = kanbanExpanded.has(item.id);
                const isDragging = draggedItemId === item.id;

                return (
                  <div
                    key={item.id}
                    draggable
                    onDragStart={(e) => {
                      e.dataTransfer.setData("text/plain", item.id);
                      e.dataTransfer.effectAllowed = "move";
                      setDraggedItemId(item.id);
                    }}
                    onDragEnd={() => {
                      setDraggedItemId(null);
                      setDragOverColumn(null);
                    }}
                    className={`transition-[transform,opacity,box-shadow] duration-200 ease-in-out [&[draggable=true]]:cursor-grab [&[draggable=true]:active]:cursor-grabbing${isDragging ? " opacity-25 scale-[0.97] drop-shadow-[0_8px_20px_rgba(0,0,0,0.12)]" : ""}`}
                  >
                    <Card
                      padding="sm"
                      className="transition-[border-color,box-shadow] duration-[120ms] ease-in-out hover:border-[var(--color-caramel-300)] cursor-pointer transition-colors hover:bg-[var(--color-caramel-100)]"
                      onClick={() => {
                        if (item.type === "task") {
                          const task = findTaskById(item.id, missions);
                          if (task) openTaskDrawer(task.task, task.parentLabel);
                        } else {
                          const ind = findIndicatorById(item.id, missions);
                          if (ind)
                            openCheckin({
                              keyResult: ind,
                              currentValue: ind.progress,
                              newValue: ind.progress,
                            });
                        }
                      }}
                    >
                      <CardBody>
                        {/* Card row */}
                        <div className="flex items-start gap-[var(--sp-2xs)]">
                          {item.type === "task" && (
                            <span
                              className="shrink-0 flex items-center"
                              onClick={(e) => e.stopPropagation()}
                            >
                              <Checkbox
                                checked={item.done ?? false}
                                onChange={() => onToggleTask(item.id)}
                              />
                            </span>
                          )}
                          {item.type !== "task" && Icon && (
                            <Icon
                              size={20}
                              className="shrink-0 text-[var(--color-neutral-500)] mt-[1px]"
                            />
                          )}
                          {item.type === "mission" && !Icon && (
                            <Target
                              size={20}
                              className="shrink-0 text-[var(--color-neutral-500)] mt-[1px]"
                            />
                          )}
                          <div className="flex-1 min-w-0 flex flex-col gap-[2px]">
                            <span
                              className={`font-[var(--font-heading)] text-[var(--text-xs)] font-semibold text-[var(--color-neutral-950)] leading-[1.25] line-clamp-2${item.type === "task" && item.done ? " line-through text-[var(--color-neutral-400)]" : ""}`}
                            >
                              {item.label}
                            </span>
                            <span className="font-[var(--font-body)] text-[10px] text-[var(--color-neutral-400)] leading-[1.3] overflow-hidden text-ellipsis whitespace-nowrap">
                              {item.missionTitle}
                            </span>
                            {isMultiTeam && item.teamName && (
                              <span className="inline-flex items-center gap-[var(--sp-3xs)] font-[var(--font-label)] text-[var(--text-xs)] text-[var(--color-neutral-500)] mt-[var(--sp-3xs)]">
                                <span
                                  className="w-[6px] h-[6px] rounded-full shrink-0"
                                  style={{
                                    backgroundColor: `var(--color-${item.teamColor ?? "neutral"}-500)`,
                                  }}
                                />
                                {item.teamName}
                              </span>
                            )}
                          </div>
                          {hasChildren && (
                            <button
                              type="button"
                              className="flex items-center justify-center shrink-0 w-[22px] h-[22px] border-none bg-transparent rounded-[var(--radius-2xs)] text-[var(--color-neutral-500)] cursor-pointer transition-colors duration-[120ms] ease-in-out hover:bg-[var(--color-caramel-200)]"
                              onClick={() => toggleKanbanExpand(item.id)}
                              aria-label={isExpanded ? "Recolher" : "Expandir"}
                            >
                              {isExpanded ? (
                                <CaretUp size={14} />
                              ) : (
                                <CaretDown size={14} />
                              )}
                            </button>
                          )}
                        </div>

                        {/* Progress / status */}
                        {item.type === "task" ? (
                          <div className="mt-[var(--sp-2xs)]">
                            <Badge color={item.done ? "success" : "neutral"}>
                              {item.done ? "Concluída" : "Pendente"}
                            </Badge>
                          </div>
                        ) : (
                          // eslint-disable-next-line jsx-a11y/click-events-have-key-events, jsx-a11y/no-static-element-interactions
                          <div
                            className="mt-[var(--sp-2xs)]"
                            onClick={(e) => e.stopPropagation()}
                          >
                            <GoalProgressBar
                              label=""
                              value={item.value}
                              target={item.target}
                              formattedValue={`${item.value}%`}
                              onChange={(v: number) => {
                                kanbanDragRef.current = { itemId: item.id, value: v };
                              }}
                            />
                          </div>
                        )}

                        {/* Footer */}
                        <div className="flex items-center justify-between mt-[var(--sp-2xs)]">
                          <div className="flex items-center gap-[var(--sp-3xs)]">
                            <AvatarGroup
                              size="xs"
                              avatars={[
                                {
                                  initials: item.ownerInitials,
                                  alt: item.ownerName,
                                },
                              ]}
                              maxVisible={3}
                            />
                            {item.period && (
                              <span className="font-[var(--font-body)] text-[10px] text-[var(--color-neutral-400)] whitespace-nowrap">
                                {item.period}
                              </span>
                            )}
                          </div>
                          {/* eslint-disable-next-line jsx-a11y/click-events-have-key-events, jsx-a11y/no-static-element-interactions */}
                          <div
                            className="flex items-center gap-[2px]"
                            onClick={(e) => e.stopPropagation()}
                          >
                            <Button
                              ref={(el: HTMLButtonElement | null) => {
                                kanbanMoveBtnRefs.current[item.id] = el;
                              }}
                              variant="tertiary"
                              size="sm"
                              leftIcon={ArrowRight}
                              aria-label="Mover"
                              onClick={() =>
                                setKanbanMoveOpen((prev) =>
                                  prev === item.id ? null : item.id,
                                )
                              }
                            />
                            <FilterDropdown
                              open={kanbanMoveOpen === item.id}
                              onClose={() => setKanbanMoveOpen(null)}
                              anchorRef={{
                                current: kanbanMoveBtnRefs.current[item.id] ?? null,
                              }}
                              noOverlay
                            >
                              <div className="flex flex-col p-[var(--sp-3xs)] max-h-[320px] overflow-y-auto">
                                <span className="font-[var(--font-label)] font-medium text-[10px] text-[var(--color-neutral-400)] uppercase tracking-[0.5px] px-[var(--sp-2xs)] py-[var(--sp-3xs)]">
                                  Mover para
                                </span>
                                {KANBAN_COLUMNS.filter((c) => c.id !== col.id).map(
                                  (target) => (
                                    <button
                                      key={target.id}
                                      type="button"
                                      className="flex items-center gap-[var(--sp-2xs)] w-full p-[var(--sp-2xs)] border-none bg-transparent rounded-[var(--radius-2xs)] font-[var(--font-label)] font-medium text-[var(--text-xs)] text-[var(--color-neutral-950)] cursor-pointer whitespace-nowrap transition-colors duration-[120ms] ease-in-out text-left hover:bg-[var(--color-caramel-100)]"
                                      onClick={() => moveToKanban(item.id, target.id)}
                                    >
                                      <span
                                        className="w-2 h-2 rounded-full shrink-0"
                                        style={{ backgroundColor: target.color }}
                                      />
                                      <span>{target.label}</span>
                                    </button>
                                  ),
                                )}
                              </div>
                            </FilterDropdown>
                            {item.type !== "task" && (
                              <Button
                                variant="tertiary"
                                size="sm"
                                leftIcon={PencilSimple}
                                aria-label="Editar indicador"
                                onClick={() => {
                                  const ind = findIndicatorById(item.id, missions);
                                  if (ind)
                                    openCheckin({
                                      keyResult: ind,
                                      currentValue: ind.progress,
                                      newValue: ind.progress,
                                    });
                                }}
                              />
                            )}
                          </div>
                        </div>

                        {/* Expandable children */}
                        {hasChildren && isExpanded && (
                          <div className="flex flex-col gap-[var(--sp-3xs)] mt-[var(--sp-2xs)] pt-[var(--sp-2xs)] border-t border-[var(--color-caramel-200)]">
                            {item.children!.map((child) => {
                              const ChildIcon = child.icon;
                              return (
                                <div
                                  key={child.id}
                                  className="flex items-start gap-[var(--sp-2xs)] px-[var(--sp-2xs)] py-[var(--sp-3xs)] bg-[var(--color-caramel-50)] rounded-[var(--radius-2xs)]"
                                >
                                  {ChildIcon && (
                                    <ChildIcon
                                      size={14}
                                      className="shrink-0 text-[var(--color-neutral-400)] mt-[2px]"
                                    />
                                  )}
                                  <div className="flex-1 min-w-0 flex flex-col gap-[2px]">
                                    <span className="font-[var(--font-label)] font-medium text-[10px] text-[var(--color-neutral-700)] leading-[1.3] overflow-hidden text-ellipsis whitespace-nowrap">
                                      {child.label}
                                    </span>
                                    <div className="mt-[2px]">
                                      <GoalProgressBar
                                        label=""
                                        value={child.value}
                                        target={child.target}
                                        formattedValue={`${child.value}%`}
                                      />
                                    </div>
                                  </div>
                                </div>
                              );
                            })}
                          </div>
                        )}
                      </CardBody>
                    </Card>
                  </div>
                );
              })}

              {/* Drop target placeholder */}
              {isDropTarget && (
                <div className="min-h-[56px] border-[1.5px] border-dashed border-[var(--color-caramel-400)] rounded-[var(--radius-xs)] bg-[var(--color-caramel-100)] opacity-60" />
              )}

              {colItems.length === 0 && !isDropTarget && (
                <span className="font-[var(--font-body)] text-[var(--text-xs)] text-[var(--color-neutral-400)] italic px-[var(--sp-xs)] py-[var(--sp-sm)] text-center">
                  Nenhum item
                </span>
              )}
            </div>
          </div>
        );
      })}
    </div>
  );
}
