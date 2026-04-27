import {
  formatPeriodRange,
  getIndicatorIcon,
  getMissionLabel,
  getOwnerInitials,
  numVal,
} from "@/lib/tempStorage/missions";
import { ExternalContribution, Indicator, Mission, MissionTask } from "@/types";
import {
  Avatar,
  Badge,
  Button,
  Card,
  CardBody,
  Chart,
  Checkbox,
  FilterDropdown,
  GoalGaugeBar,
  GoalProgressBar,
} from "@getbud-co/buds";
import {
  ArrowsOutSimple,
  CaretDown,
  CaretUp,
  DotsThree,
  EyeSlash,
  Gauge,
  GitBranch,
  ListChecks,
  MagnifyingGlass,
  PencilSimple,
  Target,
  Trash,
  X,
} from "@phosphor-icons/react";
import { useRouter } from "next/router";
import { Fragment, useEffect, useRef, useState } from "react";

interface MissionItemProps {
  mission: Mission;
  isOpen: boolean;
  onToggle: (id: string) => void;
  onExpand: (mission: Mission) => void;
  onEdit: (mission: Mission) => void;
  onDelete?: (mission: Mission) => void;
  onCheckin?: (payload: {
    keyResult: Indicator;
    currentValue: number;
    newValue: number;
  }) => void;
  onToggleTask?: (taskId: string) => void;
  onOpenTaskDrawer?: (task: MissionTask, parentLabel: string) => void;
  expandedMissions: Set<string>;
  depth?: number;
  isLast?: boolean;
  isChild?: boolean;
  hideExpand?: boolean;
  openRowMenu?: string | null;
  setOpenRowMenu?: (id: string | null) => void;
  openContributeFor?: string | null;
  setOpenContributeFor?: (id: string | null) => void;
  contributePickerSearch?: string;
  setContributePickerSearch?: (s: string) => void;
  rowMenuBtnRefs?: React.MutableRefObject<
    Record<string, HTMLButtonElement | null>
  >;
  allMissions?: { id: string; title: string }[];
  onAddContribution?: (
    item: Indicator | MissionTask,
    itemType: "indicator" | "task",
    sourceMissionId: string,
    sourceMissionTitle: string,
    targetMissionId: string,
    targetMissionTitle: string,
  ) => void;
  onRemoveContribution?: (
    itemId: string,
    itemType: "indicator" | "task",
    targetMissionId: string,
    targetMissionTitle: string,
  ) => void;
  onOpenExternalContrib?: (ec: ExternalContribution) => void;
  onToggleSubtask?: (taskId: string, subtaskId: string) => void;
}

function indicatorRowCls(
  isFirst: boolean,
  isLast: boolean,
  clickable: boolean,
  hasBadge: boolean,
) {
  return [
    "relative flex items-center px-[var(--sp-sm)] py-[var(--sp-xs)] bg-[var(--color-caramel-50)] rounded-[var(--radius-2xs)] min-h-[48px]",
    hasBadge ? "flex-wrap" : "",
    clickable
      ? "cursor-pointer transition-colors duration-[120ms] ease-in-out hover:bg-[var(--color-caramel-100)]"
      : "",
    "before:content-[''] before:absolute before:left-[-28px] before:w-0 before:[border-left:1.5px_solid_var(--color-caramel-300)]",
    isFirst ? "before:top-0" : "before:top-[-8px]",
    isLast ? "before:bottom-[calc(50%+10px)]" : "before:bottom-0",
    "after:content-[''] after:absolute after:left-[-28px] after:top-[calc(50%-10px)] after:w-[28px] after:h-[10px] after:[border-left:1.5px_solid_var(--color-caramel-300)] after:[border-bottom:1.5px_solid_var(--color-caramel-300)] after:rounded-bl-[10px]",
  ]
    .filter(Boolean)
    .join(" ");
}

function nestedRowCls(isFirst: boolean, isLast: boolean) {
  return [
    "relative flex items-center px-[var(--sp-sm)] py-[var(--sp-2xs)] bg-[var(--color-caramel-50)] rounded-[var(--radius-2xs)] transition-colors duration-[120ms] ease-in-out cursor-pointer hover:bg-[var(--color-caramel-100)]",
    "before:content-[''] before:absolute before:left-[-20px] before:w-0 before:[border-left:1.5px_solid_var(--color-caramel-300)]",
    isFirst ? "before:top-0" : "before:top-[calc(-1*var(--sp-2xs))]",
    isLast ? "before:bottom-[calc(50%+8px)]" : "before:bottom-0",
    "after:content-[''] after:absolute after:left-[-20px] after:top-[calc(50%-8px)] after:w-[20px] after:h-[8px] after:[border-left:1.5px_solid_var(--color-caramel-300)] after:[border-bottom:1.5px_solid_var(--color-caramel-300)] after:rounded-bl-[8px]",
  ]
    .filter(Boolean)
    .join(" ");
}

function wrapperCls(isLast: boolean) {
  return [
    "flex flex-col relative",
    !isLast
      ? "before:content-[''] before:absolute before:left-[-28px] before:top-0 before:bottom-0 before:w-[1.5px] before:[background-image:repeating-linear-gradient(to_bottom,var(--color-caramel-300)_0,var(--color-caramel-300)_6px,transparent_6px,transparent_14px)]"
      : "",
  ]
    .filter(Boolean)
    .join(" ");
}

function childWrapperCls(isLast: boolean) {
  return [
    "relative",
    "before:content-[''] before:absolute before:left-[-28px] before:top-[-8px] before:w-0 before:[border-left:1.5px_solid_var(--color-caramel-300)]",
    isLast ? "before:bottom-[calc(100%-27px)]" : "before:bottom-0",
    "after:content-[''] after:absolute after:left-[-28px] after:top-[27px] after:w-[28px] after:h-[10px] after:[border-left:1.5px_solid_var(--color-caramel-300)] after:[border-bottom:1.5px_solid_var(--color-caramel-300)] after:rounded-bl-[10px]",
  ]
    .filter(Boolean)
    .join(" ");
}

function ContributeMenu({
  itemId,
  contributesTo,
  openRowMenu,
  openContributeFor,
  setOpenRowMenu,
  setOpenContributeFor,
  setContributePickerSearch,
  contributePickerSearch,
  allMissions,
  missionId,
  missionTitle,
  itemType,
  onAddContribution,
  onRemoveContribution,
  rowMenuBtnRefs,
  item,
}: {
  itemId: string;
  contributesTo?: { missionId: string; missionTitle: string }[];
  openRowMenu: string | null;
  openContributeFor: string | null;
  setOpenRowMenu?: (id: string | null) => void;
  setOpenContributeFor?: (id: string | null) => void;
  setContributePickerSearch?: (s: string) => void;
  contributePickerSearch: string;
  allMissions: { id: string; title: string }[];
  missionId: string;
  missionTitle: string;
  itemType: "indicator" | "task";
  onAddContribution?: (
    item: Indicator | MissionTask,
    itemType: "indicator" | "task",
    sourceMissionId: string,
    sourceMissionTitle: string,
    targetMissionId: string,
    targetMissionTitle: string,
  ) => void;
  onRemoveContribution?: (
    itemId: string,
    itemType: "indicator" | "task",
    targetMissionId: string,
    targetMissionTitle: string,
  ) => void;
  rowMenuBtnRefs?: React.MutableRefObject<
    Record<string, HTMLButtonElement | null>
  >;
  item: Indicator | MissionTask;
}) {
  return (
    <>
      <Button
        ref={(el: HTMLButtonElement | null) => {
          if (rowMenuBtnRefs) rowMenuBtnRefs.current[itemId] = el;
        }}
        variant="tertiary"
        size="sm"
        leftIcon={DotsThree}
        aria-label="Mais ações"
        onClick={() => {
          setOpenRowMenu?.(openRowMenu === itemId ? null : itemId);
          setOpenContributeFor?.(null);
        }}
      />
      <FilterDropdown
        open={openRowMenu === itemId && openContributeFor !== itemId}
        onClose={() => setOpenRowMenu?.(null)}
        anchorRef={{ current: rowMenuBtnRefs?.current[itemId] ?? null }}
        noOverlay
      >
        <div
          className={
            "flex flex-col p-[var(--sp-3xs)] max-h-[320px] overflow-y-auto"
          }
        >
          <button
            type="button"
            className={
              "flex items-center gap-[var(--sp-2xs)] w-full px-[var(--sp-2xs)] py-[var(--sp-2xs)] border-none bg-transparent rounded-[var(--radius-2xs)] [font-family:var(--font-label)] font-medium text-[var(--text-xs)] text-[var(--color-neutral-950)] cursor-pointer whitespace-nowrap transition-colors duration-[120ms] ease-in-out text-left hover:bg-[var(--color-caramel-100)]"
            }
            onClick={() => {
              setOpenContributeFor?.(itemId);
              setContributePickerSearch?.("");
            }}
          >
            <GitBranch size={14} />
            <span>Contribui para...</span>
          </button>
          {(contributesTo?.length ?? 0) > 0 && (
            <>
              <div className="h-[1px] bg-[var(--color-caramel-200)] my-[var(--sp-3xs)]" />
              {contributesTo!.map((ct) => (
                <button
                  key={ct.missionId}
                  type="button"
                  className={
                    "flex items-center gap-[var(--sp-2xs)] w-full px-[var(--sp-2xs)] py-[var(--sp-2xs)] border-none bg-transparent rounded-[var(--radius-2xs)] [font-family:var(--font-label)] font-medium text-[var(--text-xs)] text-[var(--color-red-600)] cursor-pointer whitespace-nowrap transition-colors duration-[120ms] ease-in-out text-left hover:bg-[var(--color-red-50)]"
                  }
                  onClick={() =>
                    onRemoveContribution?.(
                      itemId,
                      itemType,
                      ct.missionId,
                      ct.missionTitle,
                    )
                  }
                >
                  <X size={14} />
                  <span
                    className="overflow-hidden text-ellipsis whitespace-nowrap max-w-[220px]"
                    title={`Desconectar de ${ct.missionTitle}`}
                  >
                    Desconectar de {ct.missionTitle}
                  </span>
                </button>
              ))}
            </>
          )}
        </div>
      </FilterDropdown>
      <FilterDropdown
        open={openContributeFor === itemId}
        onClose={() => {
          setOpenContributeFor?.(null);
          setOpenRowMenu?.(null);
        }}
        anchorRef={{ current: rowMenuBtnRefs?.current[itemId] ?? null }}
        noOverlay
      >
        <div
          className={
            "flex flex-col p-[var(--sp-3xs)] max-h-[320px] overflow-y-auto"
          }
        >
          <div className="flex items-center gap-[var(--sp-2xs)] p-[var(--sp-2xs)] border-b border-[var(--color-caramel-200)] mb-[var(--sp-3xs)]">
            <MagnifyingGlass
              size={14}
              className="shrink-0 text-[var(--color-neutral-400)]"
            />
            <input
              type="text"
              className={
                "flex-1 min-w-0 border-none bg-transparent outline-none [font-family:var(--font-label)] font-medium text-[var(--text-xs)] text-[var(--color-neutral-950)] placeholder:text-[var(--color-neutral-400)]"
              }
              placeholder="Buscar missão..."
              value={contributePickerSearch}
              onChange={(e: React.ChangeEvent<HTMLInputElement>) =>
                setContributePickerSearch?.(e.target.value)
              }
            />
          </div>
          {allMissions
            .filter((m) => m.id !== missionId)
            .filter((m) => !contributesTo?.some((c) => c.missionId === m.id))
            .filter((m) =>
              m.title
                .toLowerCase()
                .includes(contributePickerSearch.toLowerCase()),
            )
            .map((m) => (
              <button
                key={m.id}
                type="button"
                className={
                  "flex items-center gap-[var(--sp-2xs)] w-full px-[var(--sp-2xs)] py-[var(--sp-2xs)] border-none bg-transparent rounded-[var(--radius-2xs)] [font-family:var(--font-label)] font-medium text-[var(--text-xs)] text-[var(--color-neutral-950)] cursor-pointer whitespace-nowrap transition-colors duration-[120ms] ease-in-out text-left hover:bg-[var(--color-caramel-100)]"
                }
                onClick={() =>
                  onAddContribution?.(
                    item,
                    itemType,
                    missionId,
                    missionTitle,
                    m.id,
                    m.title,
                  )
                }
              >
                <Target size={14} />
                <span
                  className="overflow-hidden text-ellipsis whitespace-nowrap max-w-[220px]"
                  title={m.title}
                >
                  {m.title}
                </span>
              </button>
            ))}
        </div>
      </FilterDropdown>
    </>
  );
}

function MissionItem({
  mission,
  isOpen,
  onToggle,
  onExpand,
  onEdit,
  onDelete,
  onCheckin,
  onToggleTask,
  onOpenTaskDrawer,
  expandedMissions,
  isChild = false,
  isLast = false,
  hideExpand = false,
  openRowMenu = null,
  setOpenRowMenu,
  openContributeFor = null,
  setOpenContributeFor,
  contributePickerSearch = "",
  setContributePickerSearch,
  rowMenuBtnRefs,
  allMissions = [],
  onAddContribution,
  onRemoveContribution,
  onOpenExternalContrib,
  onToggleSubtask,
}: MissionItemProps) {
  const router = useRouter();
  const [indicatorValues, setIndicatorValues] = useState<
    Record<string, number>
  >({});
  const [expandedIndicators, setExpandedIndicators] = useState<Set<string>>(
    new Set(),
  );
  const dragRef = useRef<{ indicator: Indicator; value: number } | null>(null);

  function getIndicatorValue(kr: Indicator) {
    return indicatorValues[kr.id] ?? kr.progress;
  }

  function handleIndicatorDrag(indicator: Indicator, newValue: number) {
    dragRef.current = { indicator: indicator, value: newValue };
    setIndicatorValues((prev) => ({ ...prev, [indicator.id]: newValue }));
  }

  useEffect(() => {
    if (!onCheckin) return;
    function onPointerUp() {
      if (!dragRef.current) return;
      const { indicator, value } = dragRef.current;
      dragRef.current = null;
      setIndicatorValues((prev) => {
        const next = { ...prev };
        delete next[indicator.id];
        return next;
      });
      requestAnimationFrame(() => {
        onCheckin!({
          keyResult: indicator,
          currentValue: indicator.progress,
          newValue: value,
        });
      });
    }
    document.addEventListener("pointerup", onPointerUp);
    return () => document.removeEventListener("pointerup", onPointerUp);
  }, [onCheckin]);

  function handleIndicatorClick(kr: Indicator) {
    if (onCheckin) {
      onCheckin({
        keyResult: kr,
        currentValue: getIndicatorValue(kr),
        newValue: getIndicatorValue(kr),
      });
    }
  }

  const keyResults = mission.indicators ?? [];
  const rs = mission.restrictedSummary;
  const hasRestricted =
    rs != null && (rs.indicators > 0 || rs.tasks > 0 || rs.children > 0);
  const extContribs = mission.externalContributions ?? [];
  const hasContent =
    keyResults.length > 0 ||
    (mission.tasks?.length ?? 0) > 0 ||
    (mission.children?.length ?? 0) > 0 ||
    hasRestricted ||
    extContribs.length > 0;
  const items: {
    type: "indicator" | "task" | "mission";
    data: Indicator | MissionTask | Mission;
  }[] = [
    ...keyResults.map((kr) => ({ type: "indicator" as const, data: kr })),
    ...(mission.tasks ?? []).map((task) => ({
      type: "task" as const,
      data: task,
    })),
    ...(mission.children ?? []).map((child) => ({
      type: "mission" as const,
      data: child,
    })),
  ];

  const cardClasses = [
    "group cursor-pointer transition-colors duration-[120ms] ease-in-out",
    mission.status === "draft"
      ? "border-dashed border-[var(--color-caramel-300)] opacity-85 hover:border-[var(--color-caramel-500)]"
      : "",
  ]
    .filter(Boolean)
    .join(" ");

  return (
    <div className={isChild ? childWrapperCls(isLast) : ""}>
      <Card
        padding="sm"
        className={cardClasses}
        onClick={() => hasContent && onToggle(mission.id)}
        role={hasContent ? "button" : undefined}
        tabIndex={hasContent ? 0 : undefined}
        onKeyDown={(e: React.KeyboardEvent) => {
          if (hasContent && (e.key === "Enter" || e.key === " ")) {
            e.preventDefault();
            onToggle(mission.id);
          }
        }}
      >
        <CardBody>
          <div className="flex items-center gap-[var(--sp-lg)]">
            <Chart value={mission.progress} size={40} />
            <span className="flex-1 min-w-0 [font-family:var(--font-heading)] text-[var(--text-sm)] font-bold text-[var(--color-neutral-950)] leading-[1.15]">
              {mission.title}
            </span>
            {mission.status === "draft" && (
              <Badge color="caramel">Rascunho</Badge>
            )}
            <div className="flex items-center gap-[2px] shrink-0">
              <Button
                variant="tertiary"
                size="sm"
                leftIcon={PencilSimple}
                aria-label="Editar missão"
                className="opacity-0 transition-opacity duration-[120ms] ease-in-out group-hover:opacity-100"
                onClick={(e: React.MouseEvent) => {
                  e.stopPropagation();
                  onEdit(mission);
                }}
              />
              <Button
                variant="tertiary"
                size="sm"
                leftIcon={Trash}
                aria-label="Excluir missão"
                className="opacity-0 transition-opacity duration-[120ms] ease-in-out group-hover:opacity-100"
                onClick={(e: React.MouseEvent) => {
                  e.stopPropagation();
                  onDelete?.(mission);
                }}
              />
              {!hideExpand && (
                <Button
                  variant="tertiary"
                  size="sm"
                  leftIcon={ArrowsOutSimple}
                  aria-label="Expandir missão"
                  onClick={(e: React.MouseEvent) => {
                    e.stopPropagation();
                    onExpand(mission);
                  }}
                />
              )}
            </div>
            {hasContent && (
              <span className="flex items-center justify-center text-[var(--color-neutral-500)]">
                {isOpen ? <CaretUp size={20} /> : <CaretDown size={20} />}
              </span>
            )}
          </div>

          <div
            className={`grid transition-[grid-template-rows] duration-200 ease-in-out ${isOpen ? "grid-rows-[1fr] mt-[var(--sp-sm)]" : "grid-rows-[0fr]"}`}
          >
            <div className="overflow-hidden pl-[40px]">
              {/* eslint-disable-next-line jsx-a11y/click-events-have-key-events, jsx-a11y/no-static-element-interactions */}
              <div
                className="flex flex-col gap-[var(--sp-2xs)]"
                onClick={(e) => e.stopPropagation()}
              >
                {items.map((item, idx) => {
                  const itemIsLast = idx === items.length - 1;
                  const isFirstItem = idx === 0;

                  if (item.type === "indicator") {
                    const kr = item.data as Indicator;
                    const Icon = getIndicatorIcon(kr);
                    const hasIndChildren = (kr.tasks?.length ?? 0) > 0;
                    const isIndExpanded = expandedIndicators.has(kr.id);
                    return (
                      <div key={kr.id} className={wrapperCls(itemIsLast)}>
                        <div
                          className={indicatorRowCls(
                            isFirstItem,
                            itemIsLast,
                            !!onCheckin,
                            hasIndChildren,
                          )}
                          onClick={() => handleIndicatorClick(kr)}
                          role={onCheckin ? "button" : undefined}
                          tabIndex={onCheckin ? 0 : undefined}
                          onKeyDown={(e) => {
                            if (
                              onCheckin &&
                              (e.key === "Enter" || e.key === " ")
                            ) {
                              e.preventDefault();
                              handleIndicatorClick(kr);
                            }
                          }}
                        >
                          {hasIndChildren && (
                            <div className="w-full mb-[var(--sp-3xs)]">
                              <Badge color="neutral">
                                {kr.tasks?.length ?? 0}{" "}
                                {(kr.tasks?.length ?? 0) === 1
                                  ? "tarefa"
                                  : "tarefas"}
                              </Badge>
                            </div>
                          )}
                          <div
                            className={
                              "flex items-center gap-[var(--sp-sm)] flex-[1_1_33.33%] min-w-0"
                            }
                          >
                            {hasIndChildren && (
                              <button
                                type="button"
                                className="shrink-0 flex items-center justify-center w-[var(--sp-md)] h-[var(--sp-md)] border-none bg-transparent p-0 cursor-pointer text-[var(--color-neutral-500)] rounded-[var(--radius-2xs)] transition-[background-color,color] duration-[120ms] ease-in-out hover:bg-[var(--color-caramel-200)] hover:text-[var(--color-neutral-700)]"
                                onClick={(e) => {
                                  e.stopPropagation();
                                  setExpandedIndicators((prev) => {
                                    const next = new Set(prev);
                                    if (next.has(kr.id)) next.delete(kr.id);
                                    else next.add(kr.id);
                                    return next;
                                  });
                                }}
                                aria-label={
                                  isIndExpanded ? "Recolher" : "Expandir"
                                }
                              >
                                {isIndExpanded ? (
                                  <CaretUp size={14} />
                                ) : (
                                  <CaretDown size={14} />
                                )}
                              </button>
                            )}
                            <Icon
                              size={24}
                              className="shrink-0 text-[var(--color-neutral-500)]"
                            />
                            <span
                              className={
                                "flex-1 min-w-0 [font-family:var(--font-heading)] text-[var(--text-sm)] font-medium text-[var(--color-neutral-950)] leading-[1.15] overflow-hidden text-ellipsis whitespace-nowrap"
                              }
                            >
                              {kr.title}
                            </span>
                          </div>
                          <div
                            className={
                              "flex items-center gap-[var(--sp-xl)] flex-[2_1_66.66%] justify-between"
                            }
                          >
                            <div className="shrink-0 flex flex-col gap-[2px] text-left whitespace-nowrap">
                              <span className="[font-family:var(--font-heading)] text-[var(--text-xs)] font-bold text-[var(--color-neutral-950)] leading-[1.15]">
                                {kr.periodLabel ?? ""}
                              </span>
                              <span className="[font-family:var(--font-body)] text-[var(--text-xs)] text-[var(--color-neutral-950)] leading-[1.15]">
                                {formatPeriodRange(
                                  kr.periodStart,
                                  kr.periodEnd,
                                )}
                              </span>
                            </div>
                            {/* eslint-disable-next-line jsx-a11y/click-events-have-key-events, jsx-a11y/no-static-element-interactions */}
                            <div
                              className="flex-[1_1_200px] min-w-[160px]"
                              onClick={(e) => e.stopPropagation()}
                            >
                              {(() => {
                                const val = getIndicatorValue(kr);
                                return kr.goalType === "reach" ? (
                                  <GoalProgressBar
                                    label={getMissionLabel(kr)}
                                    value={val}
                                    target={numVal(kr.targetValue)}
                                    expected={numVal(kr.expectedValue)}
                                    formattedValue={`${val}%`}
                                    onChange={(v: number) =>
                                      handleIndicatorDrag(kr, v)
                                    }
                                  />
                                ) : (
                                  <GoalGaugeBar
                                    label={getMissionLabel(kr)}
                                    value={val}
                                    goalType={
                                      kr.goalType as
                                        | "above"
                                        | "below"
                                        | "between"
                                    }
                                    low={numVal(kr.lowThreshold)}
                                    high={numVal(kr.highThreshold)}
                                    min={0}
                                    max={100}
                                    formattedValue={String(val)}
                                    onChange={(v: number) =>
                                      handleIndicatorDrag(kr, v)
                                    }
                                  />
                                );
                              })()}
                            </div>
                            <div
                              className={
                                "flex items-center gap-[var(--sp-2xs)] ml-auto shrink-0"
                              }
                            >
                              <Avatar
                                initials={getOwnerInitials(kr.owner)}
                                size="sm"
                              />
                              {/* eslint-disable-next-line jsx-a11y/click-events-have-key-events, jsx-a11y/no-static-element-interactions */}
                              <div
                                className="flex items-center"
                                onClick={(e) => e.stopPropagation()}
                                onPointerDown={(e) => e.stopPropagation()}
                              >
                                <ContributeMenu
                                  itemId={kr.id}
                                  contributesTo={kr.contributesTo}
                                  openRowMenu={openRowMenu}
                                  openContributeFor={openContributeFor}
                                  setOpenRowMenu={setOpenRowMenu}
                                  setOpenContributeFor={setOpenContributeFor}
                                  setContributePickerSearch={
                                    setContributePickerSearch
                                  }
                                  contributePickerSearch={
                                    contributePickerSearch
                                  }
                                  allMissions={allMissions}
                                  missionId={mission.id}
                                  missionTitle={mission.title}
                                  itemType="indicator"
                                  onAddContribution={onAddContribution}
                                  onRemoveContribution={onRemoveContribution}
                                  rowMenuBtnRefs={rowMenuBtnRefs}
                                  item={kr}
                                />
                              </div>
                            </div>
                          </div>
                        </div>
                        {hasIndChildren && isIndExpanded && (
                          <div className="flex flex-col gap-[var(--sp-2xs)] pt-[var(--sp-2xs)] pl-[var(--sp-xl)] ml-[var(--sp-sm)] overflow-hidden">
                            {kr.tasks?.map((task, taskIndex, tasks) => {
                              const isFirstTask = taskIndex === 0;
                              const isLastTask = taskIndex === tasks.length - 1;
                              return (
                                <Fragment key={task.id}>
                                  <div
                                    className={nestedRowCls(
                                      isFirstTask,
                                      isLastTask,
                                    )}
                                    style={{ cursor: "pointer" }}
                                    onClick={() =>
                                      onOpenTaskDrawer?.(
                                        task,
                                        `${mission.title} › ${kr.title}`,
                                      )
                                    }
                                  >
                                    <div
                                      className={
                                        "flex items-center gap-[var(--sp-sm)] flex-[1_1_33.33%] min-w-0"
                                      }
                                    >
                                      {/* eslint-disable-next-line jsx-a11y/click-events-have-key-events, jsx-a11y/no-static-element-interactions */}
                                      <div
                                        onClick={(e) => e.stopPropagation()}
                                        onPointerDown={(e) =>
                                          e.stopPropagation()
                                        }
                                      >
                                        <Checkbox
                                          checked={task.isDone}
                                          onChange={(
                                            e: React.ChangeEvent<HTMLInputElement>,
                                          ) => {
                                            e.stopPropagation();
                                            onToggleTask?.(task.id);
                                          }}
                                        />
                                      </div>
                                      <span
                                        className={`${"flex-1 min-w-0 [font-family:var(--font-heading)] text-[var(--text-sm)] font-medium text-[var(--color-neutral-950)] leading-[1.15] overflow-hidden text-ellipsis whitespace-nowrap"} ${task.isDone ? "line-through text-[var(--color-neutral-400)]" : ""}`}
                                      >
                                        {task.title}
                                      </span>
                                    </div>
                                    <div
                                      className={
                                        "flex items-center gap-[var(--sp-xl)] flex-[2_1_66.66%] justify-between"
                                      }
                                    >
                                      <Badge
                                        color={
                                          task.isDone ? "success" : "neutral"
                                        }
                                      >
                                        {task.isDone ? "Concluída" : "Pendente"}
                                      </Badge>
                                      <div
                                        className={
                                          "flex items-center gap-[var(--sp-2xs)] ml-auto shrink-0"
                                        }
                                      >
                                        <Avatar
                                          initials={getOwnerInitials(
                                            task.owner,
                                          )}
                                          size="sm"
                                        />
                                        {/* eslint-disable-next-line jsx-a11y/click-events-have-key-events, jsx-a11y/no-static-element-interactions */}
                                        <div
                                          className="flex items-center"
                                          onClick={(e) => e.stopPropagation()}
                                          onPointerDown={(e) =>
                                            e.stopPropagation()
                                          }
                                        >
                                          <ContributeMenu
                                            itemId={task.id}
                                            contributesTo={task.contributesTo}
                                            openRowMenu={openRowMenu}
                                            openContributeFor={
                                              openContributeFor
                                            }
                                            setOpenRowMenu={setOpenRowMenu}
                                            setOpenContributeFor={
                                              setOpenContributeFor
                                            }
                                            setContributePickerSearch={
                                              setContributePickerSearch
                                            }
                                            contributePickerSearch={
                                              contributePickerSearch
                                            }
                                            allMissions={allMissions}
                                            missionId={mission.id}
                                            missionTitle={mission.title}
                                            itemType="task"
                                            onAddContribution={
                                              onAddContribution
                                            }
                                            onRemoveContribution={
                                              onRemoveContribution
                                            }
                                            rowMenuBtnRefs={rowMenuBtnRefs}
                                            item={task}
                                          />
                                        </div>
                                      </div>
                                    </div>
                                  </div>
                                </Fragment>
                              );
                            })}
                          </div>
                        )}
                      </div>
                    );
                  }

                  if (item.type === "task") {
                    const task = item.data as MissionTask;
                    return (
                      <Fragment key={task.id}>
                        <div
                          className={indicatorRowCls(
                            isFirstItem,
                            itemIsLast,
                            false,
                            false,
                          )}
                          style={{ cursor: "pointer" }}
                          onClick={() =>
                            onOpenTaskDrawer?.(task, mission.title)
                          }
                        >
                          <div
                            className={
                              "flex items-center gap-[var(--sp-sm)] flex-[1_1_33.33%] min-w-0"
                            }
                          >
                            {/* eslint-disable-next-line jsx-a11y/click-events-have-key-events, jsx-a11y/no-static-element-interactions */}
                            <div
                              onClick={(e) => e.stopPropagation()}
                              onPointerDown={(e) => e.stopPropagation()}
                            >
                              <Checkbox
                                checked={task.isDone}
                                onChange={(
                                  e: React.ChangeEvent<HTMLInputElement>,
                                ) => {
                                  e.stopPropagation();
                                  onToggleTask?.(task.id);
                                }}
                              />
                            </div>
                            <span
                              className={`${"flex-1 min-w-0 [font-family:var(--font-heading)] text-[var(--text-sm)] font-medium text-[var(--color-neutral-950)] leading-[1.15] overflow-hidden text-ellipsis whitespace-nowrap"} ${task.isDone ? "line-through text-[var(--color-neutral-400)]" : ""}`}
                            >
                              {task.title}
                            </span>
                          </div>
                          <div
                            className={
                              "flex items-center gap-[var(--sp-xl)] flex-[2_1_66.66%] justify-between"
                            }
                          >
                            <Badge color={task.isDone ? "success" : "neutral"}>
                              {task.isDone ? "Concluída" : "Pendente"}
                            </Badge>
                            <div
                              className={
                                "flex items-center gap-[var(--sp-2xs)] ml-auto shrink-0"
                              }
                            >
                              <Avatar
                                initials={getOwnerInitials(task.owner)}
                                size="sm"
                              />
                              {/* eslint-disable-next-line jsx-a11y/click-events-have-key-events, jsx-a11y/no-static-element-interactions */}
                              <div
                                className="flex items-center"
                                onClick={(e) => e.stopPropagation()}
                                onPointerDown={(e) => e.stopPropagation()}
                              >
                                <ContributeMenu
                                  itemId={task.id}
                                  contributesTo={task.contributesTo}
                                  openRowMenu={openRowMenu}
                                  openContributeFor={openContributeFor}
                                  setOpenRowMenu={setOpenRowMenu}
                                  setOpenContributeFor={setOpenContributeFor}
                                  setContributePickerSearch={
                                    setContributePickerSearch
                                  }
                                  contributePickerSearch={
                                    contributePickerSearch
                                  }
                                  allMissions={allMissions}
                                  missionId={mission.id}
                                  missionTitle={mission.title}
                                  itemType="task"
                                  onAddContribution={onAddContribution}
                                  onRemoveContribution={onRemoveContribution}
                                  rowMenuBtnRefs={rowMenuBtnRefs}
                                  item={task}
                                />
                              </div>
                            </div>
                          </div>
                        </div>
                      </Fragment>
                    );
                  }

                  const child = item.data as Mission;
                  return (
                    <MissionItem
                      key={child.id}
                      mission={child}
                      isOpen={expandedMissions.has(child.id)}
                      onToggle={onToggle}
                      onExpand={onExpand}
                      onEdit={onEdit}
                      onDelete={onDelete}
                      onCheckin={onCheckin}
                      onToggleTask={onToggleTask}
                      onOpenTaskDrawer={onOpenTaskDrawer}
                      expandedMissions={expandedMissions}
                      isChild
                      isLast={itemIsLast}
                      openRowMenu={openRowMenu}
                      setOpenRowMenu={setOpenRowMenu}
                      openContributeFor={openContributeFor}
                      setOpenContributeFor={setOpenContributeFor}
                      contributePickerSearch={contributePickerSearch}
                      setContributePickerSearch={setContributePickerSearch}
                      rowMenuBtnRefs={rowMenuBtnRefs}
                      allMissions={allMissions}
                      onAddContribution={onAddContribution}
                      onRemoveContribution={onRemoveContribution}
                      onOpenExternalContrib={onOpenExternalContrib}
                      onToggleSubtask={onToggleSubtask}
                    />
                  );
                })}
                {extContribs.length > 0 && (
                  <>
                    <div className="flex items-center gap-[var(--sp-2xs)] px-[var(--sp-sm)] py-[var(--sp-3xs)] [font-family:var(--font-label)] text-[var(--text-xs)] text-[var(--color-neutral-500)] uppercase tracking-[0.03em]">
                      <GitBranch size={14} />
                      <span>Contribuições externas</span>
                      <Badge color="neutral" size="sm">
                        {extContribs.length}
                      </Badge>
                    </div>
                    {extContribs.map((ec: ExternalContribution) => (
                      <div
                        key={ec.id}
                        className="flex flex-row items-center gap-[var(--sp-xs)] py-[var(--sp-xs)] pl-[var(--sp-sm)] pr-[var(--sp-xs)] bg-[var(--color-neutral-0)] border-[1.5px] border-dashed border-[var(--color-caramel-200)] rounded-[var(--radius-2xs)] cursor-pointer transition-colors duration-[120ms] ease-in-out hover:bg-[var(--color-caramel-100)]"
                        onClick={() => onOpenExternalContrib?.(ec)}
                        role="button"
                        tabIndex={0}
                        onKeyDown={(e) => {
                          if (e.key === "Enter") onOpenExternalContrib?.(ec);
                        }}
                      >
                        <div className="flex-1 min-w-0 flex flex-col gap-[var(--sp-3xs)]">
                          <div className="flex items-center gap-[var(--sp-xs)]">
                            {ec.type === "indicator" ? (
                              <Gauge size={16} />
                            ) : (
                              <ListChecks size={16} />
                            )}
                            <span className="flex-1 min-w-0 overflow-hidden text-ellipsis whitespace-nowrap [font-family:var(--font-body)] text-[var(--text-sm)] text-[var(--color-neutral-900)]">
                              {ec.title}
                            </span>
                            {ec.type === "indicator" && ec.status && (
                              <Badge
                                color={
                                  ec.status === "on_track"
                                    ? "success"
                                    : ec.status === "attention"
                                      ? "warning"
                                      : ec.status === "off_track"
                                        ? "error"
                                        : "neutral"
                                }
                                size="sm"
                              >
                                {ec.status === "on_track"
                                  ? "No ritmo"
                                  : ec.status === "attention"
                                    ? "Atenção"
                                    : ec.status === "off_track"
                                      ? "Atrasado"
                                      : "Concluído"}
                              </Badge>
                            )}
                            {ec.type === "task" && (
                              <Badge
                                color={ec.isDone ? "success" : "neutral"}
                                size="sm"
                              >
                                {ec.isDone ? "Concluída" : "Pendente"}
                              </Badge>
                            )}
                            {ec.type === "indicator" && ec.progress != null && (
                              <span className="[font-family:var(--font-label)] text-[var(--text-sm)] text-[var(--color-neutral-500)] shrink-0">
                                {ec.progress}%
                              </span>
                            )}
                          </div>
                          <div className="flex items-center gap-[var(--sp-3xs)] pl-[calc(16px+var(--sp-xs))] [font-family:var(--font-body)] text-[var(--text-xs)] text-[var(--color-neutral-400)]">
                            <Target size={12} />
                            <span
                              className="text-[var(--color-orange-600)] cursor-pointer transition-colors duration-[120ms] ease-in-out hover:text-[var(--color-orange-700)] hover:underline"
                              onClick={(e) => {
                                e.stopPropagation();
                                router.push(`/missions/${ec.sourceMission.id}`);
                              }}
                              role="link"
                              tabIndex={0}
                              onKeyDown={(e) => {
                                if (e.key === "Enter") {
                                  e.stopPropagation();
                                  router.push(
                                    `/missions/${ec.sourceMission.id}`,
                                  );
                                }
                              }}
                            >
                              de {ec.sourceMission.title}
                            </span>
                          </div>
                        </div>
                        {/* eslint-disable-next-line jsx-a11y/click-events-have-key-events, jsx-a11y/no-static-element-interactions */}
                        <div onClick={(e) => e.stopPropagation()}>
                          <Button
                            variant="tertiary"
                            size="sm"
                            leftIcon={X}
                            aria-label="Remover contribuição"
                            onClick={() =>
                              onRemoveContribution?.(
                                ec.id,
                                ec.type,
                                mission.id,
                                mission.title,
                              )
                            }
                          />
                        </div>
                      </div>
                    ))}
                  </>
                )}
                {hasRestricted &&
                  (() => {
                    const parts: string[] = [];
                    if (rs!.indicators > 0)
                      parts.push(
                        `${rs!.indicators} indicador${rs!.indicators > 1 ? "es" : ""}`,
                      );
                    if (rs!.tasks > 0)
                      parts.push(
                        `${rs!.tasks} tarefa${rs!.tasks > 1 ? "s" : ""}`,
                      );
                    if (rs!.children > 0)
                      parts.push(
                        `${rs!.children} sub-miss${rs!.children > 1 ? "ões" : "ão"}`,
                      );
                    const joined =
                      parts.length > 1
                        ? parts.slice(0, -1).join(", ") +
                          " e " +
                          parts[parts.length - 1]
                        : parts[0];
                    return (
                      <div className="flex items-center gap-[var(--sp-xs)] px-[var(--sp-sm)] py-[var(--sp-xs)] bg-[var(--color-caramel-50)] border-[1.5px] border-dashed border-[var(--color-caramel-300)] rounded-[var(--radius-2xs)] [font-family:var(--font-body)] text-[var(--text-sm)] text-[var(--color-neutral-500)]">
                        <EyeSlash size={16} weight="regular" />
                        <span>
                          {joined} oculto
                          {rs!.indicators + rs!.tasks + rs!.children > 1
                            ? "s"
                            : ""}{" "}
                          contribuem para o progresso desta missão
                        </span>
                      </div>
                    );
                  })()}
              </div>
            </div>
          </div>
        </CardBody>
      </Card>
    </div>
  );
}

export default MissionItem;
