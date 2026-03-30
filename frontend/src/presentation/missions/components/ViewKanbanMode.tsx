import type {
  MutableRefObject,
  Dispatch,
  SetStateAction,
  ComponentType,
} from "react";
import type { IconProps } from "@phosphor-icons/react";
import {
  Card,
  CardBody,
  Badge,
  GoalProgressBar,
  AvatarGroup,
  Button,
  Checkbox,
} from "@mdonangelo/bud-ds";
import {
  ArrowRight,
  CaretDown,
  CaretUp,
  Target,
  PencilSimple,
} from "@phosphor-icons/react";
import { FilterDropdown } from "@mdonangelo/bud-ds";
import type {
  Mission,
  KanbanStatus,
  MissionTask,
  CheckinPayload,
} from "@/types";
import {
  findIndicatorById,
  findTaskById,
} from "@/presentation/missions/utils/missionTree";
import { KANBAN_COLUMNS } from "@/presentation/missions/consts";

export interface KanbanChildItem {
  id: string;
  label: string;
  value: number;
  target: number;
  missionLabel: string;
  ownerInitials: string;
  period: string;
  icon?: ComponentType<IconProps>;
}

export interface KanbanItem {
  id: string;
  label: string;
  missionTitle: string;
  missionId: string;
  value: number;
  target: number;
  missionLabel: string;
  ownerInitials: string;
  ownerName: string;
  period: string;
  type: "indicator" | "mission" | "task";
  icon?: ComponentType<IconProps>;
  children?: KanbanChildItem[];
  done?: boolean;
}

interface ViewKanbanModeProps {
  kanbanItems: KanbanItem[];
  getKanbanStatus: (itemId: string) => KanbanStatus;
  moveToKanban: (itemId: string, status: KanbanStatus) => void;
  dragOverColumn: KanbanStatus | null;
  setDragOverColumn: Dispatch<SetStateAction<KanbanStatus | null>>;
  draggedItemId: string | null;
  setDraggedItemId: Dispatch<SetStateAction<string | null>>;
  kanbanExpanded: Set<string>;
  toggleKanbanExpand: (itemId: string) => void;
  kanbanDragRef: MutableRefObject<{ itemId: string; value: number } | null>;
  kanbanMoveBtnRefs: MutableRefObject<Record<string, HTMLButtonElement | null>>;
  kanbanMoveOpen: string | null;
  setKanbanMoveOpen: Dispatch<SetStateAction<string | null>>;
  missions: Mission[];
  handleOpenCheckin: (payload: CheckinPayload) => void;
  handleOpenTaskDrawer: (task: MissionTask, parentLabel: string) => void;
  handleToggleTask: (taskId: string) => void;
}

export function ViewKanbanMode({
  kanbanItems,
  getKanbanStatus,
  moveToKanban,
  dragOverColumn,
  setDragOverColumn,
  draggedItemId,
  setDraggedItemId,
  kanbanExpanded,
  toggleKanbanExpand,
  kanbanDragRef,
  kanbanMoveBtnRefs,
  kanbanMoveOpen,
  setKanbanMoveOpen,
  missions,
  handleOpenCheckin,
  handleOpenTaskDrawer,
  handleToggleTask,
}: ViewKanbanModeProps) {
  return (
    <CardBody>
      <div className="grid grid-cols-4 gap-4 min-h-[300px]">
        {KANBAN_COLUMNS.map((col) => {
          const colItems = kanbanItems.filter(
            (item) => getKanbanStatus(item.id) === col.id,
          );
          const isDropTarget =
            dragOverColumn === col.id && draggedItemId !== null;

          return (
            <div
              key={col.id}
              className={`flex flex-col gap-3 min-w-0 ${isDropTarget ? "kanban-drop-target" : ""}`}
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
              <div className="flex items-center gap-2 pb-3 border-b-[1.5px] border-[var(--color-caramel-200)]">
                <span
                  className="w-2 h-2 rounded-full flex-shrink-0"
                  style={{ backgroundColor: col.color }}
                />
                <span className="font-[var(--font-heading)] text-[var(--text-xs)] font-semibold text-[var(--color-neutral-950)] leading-[1.15]">
                  {col.label}
                </span>
                <span className="font-[var(--font-body)] text-[var(--text-xs)] text-[var(--color-neutral-400)] ml-auto">
                  {colItems.length}
                </span>
              </div>
              <div className="flex flex-col gap-2 min-h-[60px] p-1 rounded-[var(--radius-xs)] transition-colors duration-200">
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
                      className={`transition-[transform,opacity,box-shadow] duration-200 ${isDragging ? "opacity-25 scale-[0.97] drop-shadow-[0_8px_20px_rgba(0,0,0,0.12)]" : ""}`}
                    >
                      <Card
                        padding="sm"
                        className="transition-colors duration-[120ms] hover:border-[var(--color-caramel-300)] cursor-pointer hover:bg-[var(--color-caramel-100)]"
                        onClick={() => {
                          if (item.type === "task") {
                            const task = findTaskById(item.id, missions);
                            if (task)
                              handleOpenTaskDrawer(task.task, task.parentLabel);
                          } else {
                            const ind = findIndicatorById(item.id, missions);
                            if (ind)
                              handleOpenCheckin({
                                indicator: ind,
                                currentValue: ind.progress,
                                newValue: ind.progress,
                              });
                          }
                        }}
                      >
                        <CardBody>
                          <div className="flex items-start gap-2">
                            {item.type === "task" && (
                              <span
                                className="flex-shrink-0 flex items-center"
                                onClick={(e) => e.stopPropagation()}
                              >
                                <Checkbox
                                  checked={item.done ?? false}
                                  onChange={() => handleToggleTask(item.id)}
                                />
                              </span>
                            )}
                            {item.type !== "task" && Icon && (
                              <Icon
                                size={20}
                                className="flex-shrink-0 text-[var(--color-neutral-500)] mt-[1px]"
                              />
                            )}
                            {item.type === "mission" && !Icon && (
                              <Target
                                size={20}
                                className="flex-shrink-0 text-[var(--color-neutral-500)] mt-[1px]"
                              />
                            )}
                            <div className="flex-1 min-w-0 flex flex-col gap-[2px]">
                              <span
                                className={`font-[var(--font-heading)] text-[var(--text-xs)] font-semibold text-[var(--color-neutral-950)] leading-[1.25] line-clamp-2 ${item.type === "task" && item.done ? "line-through text-[var(--color-neutral-400)]" : ""}`}
                              >
                                {item.label}
                              </span>
                              <span className="font-[var(--font-body)] text-[10px] text-[var(--color-neutral-400)] leading-[1.3] overflow-hidden text-ellipsis whitespace-nowrap">
                                {item.missionTitle}
                              </span>
                            </div>
                            {hasChildren && (
                              <button
                                type="button"
                                className="flex items-center justify-center flex-shrink-0 w-[22px] h-[22px] border-none bg-transparent rounded-[var(--radius-2xs)] text-[var(--color-neutral-500)] cursor-pointer transition-colors duration-[120ms] hover:bg-[var(--color-caramel-200)]"
                                onClick={() => toggleKanbanExpand(item.id)}
                                aria-label={
                                  isExpanded ? "Recolher" : "Expandir"
                                }
                              >
                                {isExpanded ? (
                                  <CaretUp size={14} />
                                ) : (
                                  <CaretDown size={14} />
                                )}
                              </button>
                            )}
                          </div>
                          {item.type === "task" ? (
                            <div className="mt-2">
                              <Badge color={item.done ? "success" : "neutral"}>
                                {item.done ? "Concluída" : "Pendente"}
                              </Badge>
                            </div>
                          ) : (
                            <>
                              {/* eslint-disable-next-line jsx-a11y/click-events-have-key-events, jsx-a11y/no-static-element-interactions */}
                              <div
                                className="mt-2"
                                onClick={(e) => e.stopPropagation()}
                              >
                                <GoalProgressBar
                                  label=""
                                  value={item.value}
                                  target={item.target}
                                  formattedValue={`${item.value}%`}
                                  onChange={(v: number) => {
                                    kanbanDragRef.current = {
                                      itemId: item.id,
                                      value: v,
                                    };
                                  }}
                                />
                              </div>
                            </>
                          )}
                          <div className="flex items-center justify-between mt-2">
                            <div className="flex items-center gap-1">
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
                                  current:
                                    kanbanMoveBtnRefs.current[item.id] ?? null,
                                }}
                                noOverlay
                              >
                                <div className="flex flex-col p-1 max-h-80 overflow-y-auto">
                                  <span className="font-[var(--font-label)] font-medium text-[10px] text-[var(--color-neutral-400)] uppercase tracking-[0.5px] px-1 py-1">
                                    Mover para
                                  </span>
                                  {KANBAN_COLUMNS.filter(
                                    (c) => c.id !== col.id,
                                  ).map((target) => (
                                    <button
                                      key={target.id}
                                      type="button"
                                      className="flex items-center gap-2 w-full px-2 py-2 border-none bg-transparent rounded-[var(--radius-2xs)] font-[var(--font-label)] font-medium text-[var(--text-xs)] text-[var(--color-neutral-950)] cursor-pointer whitespace-nowrap transition-colors duration-[120ms] text-left hover:bg-[var(--color-caramel-100)]"
                                      onClick={() =>
                                        moveToKanban(item.id, target.id)
                                      }
                                    >
                                      <span
                                        className="w-2 h-2 rounded-full flex-shrink-0"
                                        style={{
                                          backgroundColor: target.color,
                                        }}
                                      />
                                      <span>{target.label}</span>
                                    </button>
                                  ))}
                                </div>
                              </FilterDropdown>
                              {item.type !== "task" && (
                                <Button
                                  variant="tertiary"
                                  size="sm"
                                  leftIcon={PencilSimple}
                                  aria-label="Editar indicador"
                                  onClick={() => {
                                    const ind = findIndicatorById(
                                      item.id,
                                      missions,
                                    );
                                    if (ind)
                                      handleOpenCheckin({
                                        indicator: ind,
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
                            <div className="flex flex-col gap-1 mt-2 pt-2 border-t border-[var(--color-caramel-200)]">
                              {item.children!.map((child) => {
                                const ChildIcon = child.icon;
                                return (
                                  <div
                                    key={child.id}
                                    className="flex items-start gap-2 px-1 py-1 bg-[var(--color-caramel-50)] rounded-[var(--radius-2xs)]"
                                  >
                                    {ChildIcon && (
                                      <ChildIcon
                                        size={14}
                                        className="flex-shrink-0 text-[var(--color-neutral-400)] mt-[2px]"
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
                {colItems.length === 0 && (
                  <span className="font-[var(--font-body)] text-[var(--text-xs)] text-[var(--color-neutral-400)] italic px-4 py-3 text-center">
                    Nenhum item
                  </span>
                )}
              </div>
            </div>
          );
        })}
      </div>
    </CardBody>
  );
}
