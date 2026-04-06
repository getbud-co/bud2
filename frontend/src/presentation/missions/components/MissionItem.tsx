import {
  formatPeriodRange,
  getMissionLabel,
  getIndicatorIcon,
  getOwnerInitials,
  numVal,
} from "@/lib/tempStorage/missions";
import {
  CheckinPayload,
  ExternalContribution,
  Indicator,
  Mission,
  MissionTask,
} from "@/types";
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
} from "@mdonangelo/bud-ds";
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
import { useEffect, useRef, useState } from "react";

export interface MissionItemProps {
  mission: Mission;
  isOpen: boolean;
  onToggle: (id: string) => void;
  onExpand: (mission: Mission) => void;
  onEdit: (mission: Mission) => void;
  onDelete?: (mission: Mission) => void;
  onCheckin?: (payload: CheckinPayload) => void;
  onToggleTask?: (taskId: string) => void;
  onOpenTaskDrawer?: (task: MissionTask, parentLabel: string) => void;
  expandedMissions: Set<string>;
  depth?: number;
  isLast?: boolean;
  isChild?: boolean;
  hideExpand?: boolean;
  /* row menu (⋯) for indicators and tasks */
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
}

export function MissionItem({
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
}: MissionItemProps) {
  const [indicatorValues, setIndicatorValues] = useState<
    Record<string, number>
  >({});
  const [expandedIndicators, setExpandedIndicators] = useState<Set<string>>(
    new Set(),
  );
  const dragRef = useRef<{ indicator: Indicator; value: number } | null>(null);

  function getIndicatorValue(indicator: Indicator) {
    return indicatorValues[indicator.id] ?? indicator.progress;
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
      // Reset bar to original value
      setIndicatorValues((prev) => {
        const next = { ...prev };
        delete next[indicator.id];
        return next;
      });
      requestAnimationFrame(() => {
        onCheckin!({
          indicator,
          currentValue: indicator.progress,
          newValue: value,
        });
      });
    }
    document.addEventListener("pointerup", onPointerUp);
    return () => document.removeEventListener("pointerup", onPointerUp);
  }, [onCheckin]);

  function handleIndicatorClick(indicator: Indicator) {
    if (onCheckin) {
      onCheckin({
        indicator: indicator,
        currentValue: getIndicatorValue(indicator),
        newValue: getIndicatorValue(indicator),
      });
    }
  }

  const indicators = mission.indicators ?? [];
  const rs = mission.restrictedSummary;
  const hasRestricted =
    rs != null && (rs.indicators > 0 || rs.tasks > 0 || rs.children > 0);
  const extContribs = mission.externalContributions ?? [];
  const hasContent =
    indicators.length > 0 ||
    (mission.tasks?.length ?? 0) > 0 ||
    (mission.children?.length ?? 0) > 0 ||
    hasRestricted ||
    extContribs.length > 0;
  const items: {
    type: "indicator" | "task" | "mission";
    data: Indicator | MissionTask | Mission;
  }[] = [
    ...indicators.map((indicator) => ({
      type: "indicator" as const,
      data: indicator,
    })),
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
    "cursor-pointer transition-colors duration-[120ms] hover:border-[var(--color-caramel-300)]",
    mission.status === "draft"
      ? "border-dashed border-[var(--color-caramel-300)] opacity-85"
      : "",
  ]
    .filter(Boolean)
    .join(" ");

  return (
    <div
      className={`${isChild ? "relative" : ""} ${isChild && isLast ? "" : ""}`}
    >
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
          <div className="flex items-center gap-6">
            <Chart value={mission.progress} size={40} />
            <span className="flex-1 min-w-0 font-[var(--font-heading)] text-[var(--text-sm)] font-bold text-[var(--color-neutral-950)] leading-[1.15] truncate">
              {mission.title}
            </span>
            {mission.status === "draft" && (
              <Badge color="caramel">Rascunho</Badge>
            )}
            <div className="flex items-center gap-[2px] flex-shrink-0">
              <Button
                variant="tertiary"
                size="sm"
                leftIcon={PencilSimple}
                aria-label="Editar missão"
                className="opacity-0 transition-opacity duration-[120ms] group-hover:opacity-100"
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
                className="opacity-0 transition-opacity duration-[120ms] group-hover:opacity-100"
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
            className={`grid transition-[grid-template-rows] duration-200 ${isOpen ? "grid-rows-[1fr] mt-4" : "grid-rows-[0fr]"}`}
          >
            <div className="overflow-hidden pl-10">
              {/* eslint-disable-next-line jsx-a11y/click-events-have-key-events, jsx-a11y/no-static-element-interactions */}
              <div
                className="flex flex-col gap-2"
                onClick={(e) => e.stopPropagation()}
              >
                {items.map((item, idx) => {
                  const itemIsLast = idx === items.length - 1;

                  if (item.type === "indicator") {
                    const indicator = item.data as Indicator;
                    const Icon = getIndicatorIcon(indicator);
                    const hasIndChildren =
                      (indicator.children?.length ?? 0) > 0 ||
                      (indicator.tasks?.length ?? 0) > 0;
                    const isIndExpanded = expandedIndicators.has(indicator.id);
                    return (
                      <div key={indicator.id} className="flex flex-col">
                        <div
                          className={`relative flex items-center px-4 py-3 bg-[var(--color-caramel-50)] rounded-[var(--radius-2xs)] min-h-[48px] ${hasIndChildren ? "flex-wrap" : ""} ${onCheckin ? "cursor-pointer transition-colors duration-[120ms] hover:bg-[var(--color-caramel-100)]" : ""}`}
                          onClick={() => handleIndicatorClick(indicator)}
                          role={onCheckin ? "button" : undefined}
                          tabIndex={onCheckin ? 0 : undefined}
                          onKeyDown={(e) => {
                            if (
                              onCheckin &&
                              (e.key === "Enter" || e.key === " ")
                            ) {
                              e.preventDefault();
                              handleIndicatorClick(indicator);
                            }
                          }}
                        >
                          {hasIndChildren && (
                            <div className="w-full mb-1">
                              <Badge color="neutral">
                                {(indicator.children?.length ?? 0) +
                                  (indicator.tasks?.length ?? 0)}{" "}
                                {(indicator.children?.length ?? 0) +
                                  (indicator.tasks?.length ?? 0) ===
                                1
                                  ? "item"
                                  : "itens"}
                              </Badge>
                            </div>
                          )}
                          <div className="flex items-center gap-4 flex-[1_1_33.33%] min-w-0">
                            {hasIndChildren && (
                              <button
                                type="button"
                                className="flex-shrink-0 flex items-center justify-center w-5 h-5 border-none bg-none p-0 cursor-pointer text-[var(--color-neutral-500)] rounded-[var(--radius-2xs)] transition-colors duration-[120ms] hover:bg-[var(--color-caramel-200)] hover:text-[var(--color-neutral-700)]"
                                onClick={(e) => {
                                  e.stopPropagation();
                                  setExpandedIndicators((prev) => {
                                    const next = new Set(prev);
                                    if (next.has(indicator.id))
                                      next.delete(indicator.id);
                                    else next.add(indicator.id);
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
                              className="flex-shrink-0 text-[var(--color-neutral-500)]"
                            />
                            <span className="flex-1 min-w-0 font-[var(--font-heading)] text-[var(--text-sm)] font-medium text-[var(--color-neutral-950)] leading-[1.15] truncate">
                              {indicator.title}
                            </span>
                          </div>
                          <div className="flex items-center gap-8 flex-[2_1_66.66%] justify-between">
                            <div className="flex-shrink-0 flex flex-col gap-[2px] text-left whitespace-nowrap">
                              <span className="font-[var(--font-heading)] text-[var(--text-xs)] font-bold text-[var(--color-neutral-950)] leading-[1.15]">
                                {indicator.periodLabel ?? ""}
                              </span>
                              <span className="font-[var(--font-body)] text-[var(--text-xs)] text-[var(--color-neutral-950)] leading-[1.15]">
                                {formatPeriodRange(
                                  indicator.periodStart,
                                  indicator.periodEnd,
                                )}
                              </span>
                            </div>
                            {/* eslint-disable-next-line jsx-a11y/click-events-have-key-events, jsx-a11y/no-static-element-interactions */}
                            <div
                              className="flex-[1_1_200px] min-w-[160px]"
                              onClick={(e) => e.stopPropagation()}
                            >
                              {(() => {
                                const val = getIndicatorValue(indicator);
                                return indicator.missionType === "reach" ? (
                                  <GoalProgressBar
                                    label={getMissionLabel(indicator)}
                                    value={val}
                                    target={numVal(indicator.targetValue)}
                                    expected={numVal(indicator.expectedValue)}
                                    formattedValue={`${val}%`}
                                    onChange={(v: number) =>
                                      handleIndicatorDrag(indicator, v)
                                    }
                                  />
                                ) : (
                                  <GoalGaugeBar
                                    label={getMissionLabel(indicator)}
                                    value={val}
                                    goalType={
                                      indicator.missionType as
                                        | "above"
                                        | "below"
                                        | "between"
                                    }
                                    low={numVal(indicator.lowThreshold)}
                                    high={numVal(indicator.highThreshold)}
                                    min={0}
                                    max={100}
                                    formattedValue={String(val)}
                                    onChange={(v: number) =>
                                      handleIndicatorDrag(indicator, v)
                                    }
                                  />
                                );
                              })()}
                            </div>
                            <div className="flex items-center gap-2 ml-auto flex-shrink-0">
                              <Avatar
                                initials={getOwnerInitials(indicator.owner)}
                                size="sm"
                              />
                              {/* eslint-disable-next-line jsx-a11y/click-events-have-key-events, jsx-a11y/no-static-element-interactions */}
                              <div
                                className="flex items-center"
                                onClick={(e) => e.stopPropagation()}
                                onPointerDown={(e) => e.stopPropagation()}
                              >
                                <Button
                                  ref={(el: HTMLButtonElement | null) => {
                                    if (rowMenuBtnRefs)
                                      rowMenuBtnRefs.current[indicator.id] = el;
                                  }}
                                  variant="tertiary"
                                  size="sm"
                                  leftIcon={DotsThree}
                                  aria-label="Mais ações"
                                  onClick={() => {
                                    setOpenRowMenu?.(
                                      openRowMenu === indicator.id
                                        ? null
                                        : indicator.id,
                                    );
                                    setOpenContributeFor?.(null);
                                  }}
                                />
                                <FilterDropdown
                                  open={
                                    openRowMenu === indicator.id &&
                                    openContributeFor !== indicator.id
                                  }
                                  onClose={() => setOpenRowMenu?.(null)}
                                  anchorRef={{
                                    current:
                                      rowMenuBtnRefs?.current[indicator.id] ??
                                      null,
                                  }}
                                  noOverlay
                                >
                                  <div className="flex flex-col p-1 max-h-80 overflow-y-auto">
                                    <button
                                      type="button"
                                      className="flex items-center gap-2 w-full px-2 py-2 border-none bg-transparent rounded-[var(--radius-2xs)] font-[var(--font-label)] font-medium text-[var(--text-xs)] text-[var(--color-neutral-950)] cursor-pointer whitespace-nowrap transition-colors duration-[120ms] text-left hover:bg-[var(--color-caramel-100)]"
                                      onClick={() => {
                                        setOpenContributeFor?.(indicator.id);
                                        setContributePickerSearch?.("");
                                      }}
                                    >
                                      <GitBranch size={14} />
                                      <span>Contribui para...</span>
                                    </button>
                                    {(indicator.contributesTo?.length ?? 0) >
                                      0 && (
                                      <>
                                        <div className="h-px bg-[var(--color-caramel-200)] my-1" />
                                        {indicator.contributesTo!.map((ct) => (
                                          <button
                                            key={ct.missionId}
                                            type="button"
                                            className="flex items-center gap-2 w-full px-2 py-2 border-none bg-transparent rounded-[var(--radius-2xs)] font-[var(--font-label)] font-medium text-[var(--text-xs)] text-[var(--color-error-600)] cursor-pointer whitespace-nowrap transition-colors duration-[120ms] text-left hover:bg-[var(--color-error-50)]"
                                            onClick={() =>
                                              onRemoveContribution?.(
                                                indicator.id,
                                                "indicator",
                                                ct.missionId,
                                                ct.missionTitle,
                                              )
                                            }
                                          >
                                            <X size={14} />
                                            <span>
                                              Desconectar de {ct.missionTitle}
                                            </span>
                                          </button>
                                        ))}
                                      </>
                                    )}
                                  </div>
                                </FilterDropdown>
                                <FilterDropdown
                                  open={openContributeFor === indicator.id}
                                  onClose={() => {
                                    setOpenContributeFor?.(null);
                                    setOpenRowMenu?.(null);
                                  }}
                                  anchorRef={{
                                    current:
                                      rowMenuBtnRefs?.current[indicator.id] ??
                                      null,
                                  }}
                                  noOverlay
                                >
                                  <div className="flex flex-col p-1 max-h-80 overflow-y-auto">
                                    <div className="flex items-center gap-2 px-2 py-2 border-b border-[var(--color-caramel-200)] mb-1">
                                      <MagnifyingGlass
                                        size={14}
                                        className="flex-shrink-0 text-[var(--color-neutral-400)]"
                                      />
                                      <input
                                        type="text"
                                        className="flex-1 min-w-0 border-none bg-transparent outline-none font-[var(--font-label)] font-medium text-[var(--text-xs)] text-[var(--color-neutral-950)] placeholder:text-[var(--color-neutral-400)]"
                                        placeholder="Buscar missão..."
                                        value={contributePickerSearch}
                                        onChange={(
                                          e: React.ChangeEvent<HTMLInputElement>,
                                        ) =>
                                          setContributePickerSearch?.(
                                            e.target.value,
                                          )
                                        }
                                      />
                                    </div>
                                    {allMissions
                                      .filter((m) => m.id !== mission.id)
                                      .filter(
                                        (m) =>
                                          !indicator.contributesTo?.some(
                                            (c) => c.missionId === m.id,
                                          ),
                                      )
                                      .filter((m) =>
                                        m.title
                                          .toLowerCase()
                                          .includes(
                                            contributePickerSearch.toLowerCase(),
                                          ),
                                      )
                                      .map((m) => (
                                        <button
                                          key={m.id}
                                          type="button"
                                          className="flex items-center gap-2 w-full px-2 py-2 border-none bg-transparent rounded-[var(--radius-2xs)] font-[var(--font-label)] font-medium text-[var(--text-xs)] text-[var(--color-neutral-950)] cursor-pointer whitespace-nowrap transition-colors duration-[120ms] text-left hover:bg-[var(--color-caramel-100)]"
                                          onClick={() =>
                                            onAddContribution?.(
                                              indicator,
                                              "indicator",
                                              mission.id,
                                              mission.title,
                                              m.id,
                                              m.title,
                                            )
                                          }
                                        >
                                          <Target size={14} />
                                          <span>{m.title}</span>
                                        </button>
                                      ))}
                                  </div>
                                </FilterDropdown>
                              </div>
                            </div>
                          </div>
                        </div>
                        {hasIndChildren && isIndExpanded && (
                          <div className="flex flex-col gap-1 pt-2 pl-8 ml-4 overflow-hidden">
                            {indicator.children?.map((sub) => {
                              const SubIcon = getIndicatorIcon(sub);
                              const subVal = getIndicatorValue(sub);
                              return (
                                <div
                                  key={sub.id}
                                  className={`relative flex items-center px-4 py-2 bg-[var(--color-caramel-50)] rounded-[var(--radius-2xs)] transition-colors duration-[120ms] ${onCheckin ? "cursor-pointer hover:bg-[var(--color-caramel-100)]" : ""}`}
                                  onClick={() => handleIndicatorClick(sub)}
                                  role={onCheckin ? "button" : undefined}
                                  tabIndex={onCheckin ? 0 : undefined}
                                >
                                  <div className="flex items-center gap-4 flex-[1_1_33.33%] min-w-0">
                                    <SubIcon
                                      size={20}
                                      className="flex-shrink-0 text-[var(--color-neutral-500)]"
                                    />
                                    <span className="flex-1 min-w-0 font-[var(--font-heading)] text-[var(--text-sm)] font-medium text-[var(--color-neutral-950)] leading-[1.15] truncate">
                                      {sub.title}
                                    </span>
                                  </div>
                                  <div className="flex items-center gap-8 flex-[2_1_66.66%] justify-between">
                                    {/* eslint-disable-next-line jsx-a11y/click-events-have-key-events, jsx-a11y/no-static-element-interactions */}
                                    <div
                                      className="flex-[1_1_200px] min-w-[160px]"
                                      onClick={(e) => e.stopPropagation()}
                                    >
                                      {sub.missionType === "reach" ? (
                                        <GoalProgressBar
                                          label={getMissionLabel(sub)}
                                          value={subVal}
                                          target={numVal(sub.targetValue)}
                                          expected={numVal(sub.expectedValue)}
                                          formattedValue={`${subVal}%`}
                                          onChange={(v: number) =>
                                            handleIndicatorDrag(sub, v)
                                          }
                                        />
                                      ) : (
                                        <GoalGaugeBar
                                          label={getMissionLabel(sub)}
                                          value={subVal}
                                          goalType={
                                            sub.missionType as
                                              | "above"
                                              | "below"
                                              | "between"
                                          }
                                          low={numVal(sub.lowThreshold)}
                                          high={numVal(sub.highThreshold)}
                                          min={0}
                                          max={100}
                                          formattedValue={String(subVal)}
                                          onChange={(v: number) =>
                                            handleIndicatorDrag(sub, v)
                                          }
                                        />
                                      )}
                                    </div>
                                    <div className="flex items-center gap-2 ml-auto flex-shrink-0">
                                      <Avatar
                                        initials={getOwnerInitials(sub.owner)}
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
                                        <Button
                                          ref={(
                                            el: HTMLButtonElement | null,
                                          ) => {
                                            if (rowMenuBtnRefs)
                                              rowMenuBtnRefs.current[sub.id] =
                                                el;
                                          }}
                                          variant="tertiary"
                                          size="sm"
                                          leftIcon={DotsThree}
                                          aria-label="Mais ações"
                                          onClick={() => {
                                            setOpenRowMenu?.(
                                              openRowMenu === sub.id
                                                ? null
                                                : sub.id,
                                            );
                                            setOpenContributeFor?.(null);
                                          }}
                                        />
                                        <FilterDropdown
                                          open={
                                            openRowMenu === sub.id &&
                                            openContributeFor !== sub.id
                                          }
                                          onClose={() => setOpenRowMenu?.(null)}
                                          anchorRef={{
                                            current:
                                              rowMenuBtnRefs?.current[sub.id] ??
                                              null,
                                          }}
                                          noOverlay
                                        >
                                          <div className="flex flex-col p-[var(--sp-3xs)] max-h-[320px] overflow-y-auto">
                                            <button
                                              type="button"
                                              className="flex items-center gap-2 w-full px-2 py-2 border-none bg-transparent rounded-[var(--radius-2xs)] font-[var(--font-label)] font-medium text-[var(--text-xs)] text-[var(--color-neutral-950)] cursor-pointer whitespace-nowrap transition-colors duration-[120ms] text-left hover:bg-[var(--color-caramel-100)]"
                                              onClick={() => {
                                                setOpenContributeFor?.(sub.id);
                                                setContributePickerSearch?.("");
                                              }}
                                            >
                                              <GitBranch size={14} />
                                              <span>Contribui para...</span>
                                            </button>
                                            {(sub.contributesTo?.length ?? 0) >
                                              0 && (
                                              <>
                                                <div className="h-px bg-[var(--color-caramel-200)] my-1" />
                                                {sub.contributesTo!.map(
                                                  (ct) => (
                                                    <button
                                                      key={ct.missionId}
                                                      type="button"
                                                      className="flex items-center gap-2 w-full px-2 py-2 border-none bg-transparent rounded-[var(--radius-2xs)] font-[var(--font-label)] font-medium text-[var(--text-xs)] text-[var(--color-error-600)] cursor-pointer whitespace-nowrap transition-colors duration-[120ms] text-left hover:bg-[var(--color-error-50)]"
                                                      onClick={() =>
                                                        onRemoveContribution?.(
                                                          sub.id,
                                                          "indicator",
                                                          ct.missionId,
                                                          ct.missionTitle,
                                                        )
                                                      }
                                                    >
                                                      <X size={14} />
                                                      <span>
                                                        Desconectar de{" "}
                                                        {ct.missionTitle}
                                                      </span>
                                                    </button>
                                                  ),
                                                )}
                                              </>
                                            )}
                                          </div>
                                        </FilterDropdown>
                                        <FilterDropdown
                                          open={openContributeFor === sub.id}
                                          onClose={() => {
                                            setOpenContributeFor?.(null);
                                            setOpenRowMenu?.(null);
                                          }}
                                          anchorRef={{
                                            current:
                                              rowMenuBtnRefs?.current[sub.id] ??
                                              null,
                                          }}
                                          noOverlay
                                        >
                                          <div className="flex flex-col p-[var(--sp-3xs)] max-h-[320px] overflow-y-auto">
                                            <div className="flex items-center gap-2 px-2 py-2 border-b border-[var(--color-caramel-200)] mb-1">
                                              <MagnifyingGlass
                                                size={14}
                                                className="flex-shrink-0 text-[var(--color-neutral-400)]"
                                              />
                                              <input
                                                type="text"
                                                className="flex-1 min-w-0 border-none bg-transparent outline-none font-[var(--font-label)] font-medium text-[var(--text-xs)] text-[var(--color-neutral-950)] placeholder:text-[var(--color-neutral-400)]"
                                                placeholder="Buscar missão..."
                                                value={contributePickerSearch}
                                                onChange={(
                                                  e: React.ChangeEvent<HTMLInputElement>,
                                                ) =>
                                                  setContributePickerSearch?.(
                                                    e.target.value,
                                                  )
                                                }
                                              />
                                            </div>
                                            {allMissions
                                              .filter(
                                                (m) => m.id !== mission.id,
                                              )
                                              .filter(
                                                (m) =>
                                                  !sub.contributesTo?.some(
                                                    (c) => c.missionId === m.id,
                                                  ),
                                              )
                                              .filter((m) =>
                                                m.title
                                                  .toLowerCase()
                                                  .includes(
                                                    contributePickerSearch.toLowerCase(),
                                                  ),
                                              )
                                              .map((m) => (
                                                <button
                                                  key={m.id}
                                                  type="button"
                                                  className="flex items-center gap-2 w-full px-2 py-2 border-none bg-transparent rounded-[var(--radius-2xs)] font-[var(--font-label)] font-medium text-[var(--text-xs)] text-[var(--color-neutral-950)] cursor-pointer whitespace-nowrap transition-colors duration-[120ms] text-left hover:bg-[var(--color-caramel-100)]"
                                                  onClick={() =>
                                                    onAddContribution?.(
                                                      sub,
                                                      "indicator",
                                                      mission.id,
                                                      mission.title,
                                                      m.id,
                                                      m.title,
                                                    )
                                                  }
                                                >
                                                  <Target size={14} />
                                                  <span>{m.title}</span>
                                                </button>
                                              ))}
                                          </div>
                                        </FilterDropdown>
                                      </div>
                                    </div>
                                  </div>
                                </div>
                              );
                            })}
                            {indicator.tasks?.map((task) => (
                              <div
                                key={task.id}
                                className="relative flex items-center px-4 py-2 bg-[var(--color-caramel-50)] rounded-[var(--radius-2xs)] cursor-pointer hover:bg-[var(--color-caramel-100)] transition-colors duration-[120ms]"
                                onClick={() =>
                                  onOpenTaskDrawer?.(
                                    task,
                                    `${mission.title} › ${indicator.title}`,
                                  )
                                }
                              >
                                <div className="flex items-center gap-4 flex-[1_1_33.33%] min-w-0">
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
                                    className={`flex-1 min-w-0 font-[var(--font-heading)] text-[var(--text-sm)] font-medium text-[var(--color-neutral-950)] leading-[1.15] truncate ${task.isDone ? "line-through text-[var(--color-neutral-400)]" : ""}`}
                                  >
                                    {task.title}
                                  </span>
                                </div>
                                <div className="flex items-center gap-8 flex-[2_1_66.66%] justify-between">
                                  <Badge
                                    color={task.isDone ? "success" : "neutral"}
                                  >
                                    {task.isDone ? "Concluída" : "Pendente"}
                                  </Badge>
                                  <div className="flex items-center gap-2 ml-auto flex-shrink-0">
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
                                      <Button
                                        ref={(el: HTMLButtonElement | null) => {
                                          if (rowMenuBtnRefs)
                                            rowMenuBtnRefs.current[task.id] =
                                              el;
                                        }}
                                        variant="tertiary"
                                        size="sm"
                                        leftIcon={DotsThree}
                                        aria-label="Mais ações"
                                        onClick={() => {
                                          setOpenRowMenu?.(
                                            openRowMenu === task.id
                                              ? null
                                              : task.id,
                                          );
                                          setOpenContributeFor?.(null);
                                        }}
                                      />
                                      <FilterDropdown
                                        open={
                                          openRowMenu === task.id &&
                                          openContributeFor !== task.id
                                        }
                                        onClose={() => setOpenRowMenu?.(null)}
                                        anchorRef={{
                                          current:
                                            rowMenuBtnRefs?.current[task.id] ??
                                            null,
                                        }}
                                        noOverlay
                                      >
                                        <div className="flex flex-col p-1 max-h-80 overflow-y-auto">
                                          <button
                                            type="button"
                                            className="flex items-center gap-2 w-full px-2 py-2 border-none bg-transparent rounded-[var(--radius-2xs)] font-[var(--font-label)] font-medium text-[var(--text-xs)] text-[var(--color-neutral-950)] cursor-pointer whitespace-nowrap transition-colors duration-[120ms] text-left hover:bg-[var(--color-caramel-100)]"
                                            onClick={() => {
                                              setOpenContributeFor?.(task.id);
                                              setContributePickerSearch?.("");
                                            }}
                                          >
                                            <GitBranch size={14} />
                                            <span>Contribui para...</span>
                                          </button>
                                          {(task.contributesTo?.length ?? 0) >
                                            0 && (
                                            <>
                                              <div className="h-px bg-[var(--color-caramel-200)] my-1" />
                                              {task.contributesTo!.map((ct) => (
                                                <button
                                                  key={ct.missionId}
                                                  type="button"
                                                  className="flex items-center gap-2 w-full px-2 py-2 border-none bg-transparent rounded-[var(--radius-2xs)] font-[var(--font-label)] font-medium text-[var(--text-xs)] text-[var(--color-error-600)] cursor-pointer whitespace-nowrap transition-colors duration-[120ms] text-left hover:bg-[var(--color-error-50)]"
                                                  onClick={() =>
                                                    onRemoveContribution?.(
                                                      task.id,
                                                      "task",
                                                      ct.missionId,
                                                      ct.missionTitle,
                                                    )
                                                  }
                                                >
                                                  <X size={14} />
                                                  <span>
                                                    Desconectar de{" "}
                                                    {ct.missionTitle}
                                                  </span>
                                                </button>
                                              ))}
                                            </>
                                          )}
                                        </div>
                                      </FilterDropdown>
                                      <FilterDropdown
                                        open={openContributeFor === task.id}
                                        onClose={() => {
                                          setOpenContributeFor?.(null);
                                          setOpenRowMenu?.(null);
                                        }}
                                        anchorRef={{
                                          current:
                                            rowMenuBtnRefs?.current[task.id] ??
                                            null,
                                        }}
                                        noOverlay
                                      >
                                        <div className="flex flex-col p-1 max-h-80 overflow-y-auto">
                                          <div className="flex items-center gap-2 px-2 py-2 border-b border-[var(--color-caramel-200)] mb-1">
                                            <MagnifyingGlass
                                              size={14}
                                              className="flex-shrink-0 text-[var(--color-neutral-400)]"
                                            />
                                            <input
                                              type="text"
                                              className="flex-1 min-w-0 border-none bg-transparent outline-none font-[var(--font-label)] font-medium text-[var(--text-xs)] text-[var(--color-neutral-950)] placeholder:text-[var(--color-neutral-400)]"
                                              placeholder="Buscar missão..."
                                              value={contributePickerSearch}
                                              onChange={(
                                                e: React.ChangeEvent<HTMLInputElement>,
                                              ) =>
                                                setContributePickerSearch?.(
                                                  e.target.value,
                                                )
                                              }
                                            />
                                          </div>
                                          {allMissions
                                            .filter((m) => m.id !== mission.id)
                                            .filter(
                                              (m) =>
                                                !task.contributesTo?.some(
                                                  (c) => c.missionId === m.id,
                                                ),
                                            )
                                            .filter((m) =>
                                              m.title
                                                .toLowerCase()
                                                .includes(
                                                  contributePickerSearch.toLowerCase(),
                                                ),
                                            )
                                            .map((m) => (
                                              <button
                                                key={m.id}
                                                type="button"
                                                className="flex items-center gap-2 w-full px-2 py-2 border-none bg-transparent rounded-[var(--radius-2xs)] font-[var(--font-label)] font-medium text-[var(--text-xs)] text-[var(--color-neutral-950)] cursor-pointer whitespace-nowrap transition-colors duration-[120ms] text-left hover:bg-[var(--color-caramel-100)]"
                                                onClick={() =>
                                                  onAddContribution?.(
                                                    task,
                                                    "task",
                                                    mission.id,
                                                    mission.title,
                                                    m.id,
                                                    m.title,
                                                  )
                                                }
                                              >
                                                <Target size={14} />
                                                <span>{m.title}</span>
                                              </button>
                                            ))}
                                        </div>
                                      </FilterDropdown>
                                    </div>
                                  </div>
                                </div>
                              </div>
                            ))}
                          </div>
                        )}
                      </div>
                    );
                  }

                  if (item.type === "task") {
                    const task = item.data as MissionTask;
                    return (
                      <div
                        key={task.id}
                        className="relative flex items-center px-4 py-3 bg-[var(--color-caramel-50)] rounded-[var(--radius-2xs)] min-h-[48px] cursor-pointer hover:bg-[var(--color-caramel-100)] transition-colors duration-[120ms]"
                        onClick={() => onOpenTaskDrawer?.(task, mission.title)}
                      >
                        <div className="flex items-center gap-4 flex-[1_1_33.33%] min-w-0">
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
                            className={`flex-1 min-w-0 font-[var(--font-heading)] text-[var(--text-sm)] font-medium text-[var(--color-neutral-950)] leading-[1.15] truncate ${task.isDone ? "line-through text-[var(--color-neutral-400)]" : ""}`}
                          >
                            {task.title}
                          </span>
                        </div>
                        <div className="flex items-center gap-8 flex-[2_1_66.66%] justify-between">
                          <Badge color={task.isDone ? "success" : "neutral"}>
                            {task.isDone ? "Concluída" : "Pendente"}
                          </Badge>
                          <div className="flex items-center gap-2 ml-auto flex-shrink-0">
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
                              <Button
                                ref={(el: HTMLButtonElement | null) => {
                                  if (rowMenuBtnRefs)
                                    rowMenuBtnRefs.current[task.id] = el;
                                }}
                                variant="tertiary"
                                size="sm"
                                leftIcon={DotsThree}
                                aria-label="Mais ações"
                                onClick={() => {
                                  setOpenRowMenu?.(
                                    openRowMenu === task.id ? null : task.id,
                                  );
                                  setOpenContributeFor?.(null);
                                }}
                              />
                              <FilterDropdown
                                open={
                                  openRowMenu === task.id &&
                                  openContributeFor !== task.id
                                }
                                onClose={() => setOpenRowMenu?.(null)}
                                anchorRef={{
                                  current:
                                    rowMenuBtnRefs?.current[task.id] ?? null,
                                }}
                                noOverlay
                              >
                                <div className="flex flex-col p-1 max-h-80 overflow-y-auto">
                                  <button
                                    type="button"
                                    className="flex items-center gap-2 w-full px-2 py-2 border-none bg-transparent rounded-[var(--radius-2xs)] font-[var(--font-label)] font-medium text-[var(--text-xs)] text-[var(--color-neutral-950)] cursor-pointer whitespace-nowrap transition-colors duration-[120ms] text-left hover:bg-[var(--color-caramel-100)]"
                                    onClick={() => {
                                      setOpenContributeFor?.(task.id);
                                      setContributePickerSearch?.("");
                                    }}
                                  >
                                    <GitBranch size={14} />
                                    <span>Contribui para...</span>
                                  </button>
                                  {(task.contributesTo?.length ?? 0) > 0 && (
                                    <>
                                      <div className="h-px bg-[var(--color-caramel-200)] my-1" />
                                      {task.contributesTo!.map((ct) => (
                                        <button
                                          key={ct.missionId}
                                          type="button"
                                          className="flex items-center gap-2 w-full px-2 py-2 border-none bg-transparent rounded-[var(--radius-2xs)] font-[var(--font-label)] font-medium text-[var(--text-xs)] text-[var(--color-error-600)] cursor-pointer whitespace-nowrap transition-colors duration-[120ms] text-left hover:bg-[var(--color-error-50)]"
                                          onClick={() =>
                                            onRemoveContribution?.(
                                              task.id,
                                              "task",
                                              ct.missionId,
                                              ct.missionTitle,
                                            )
                                          }
                                        >
                                          <X size={14} />
                                          <span>
                                            Desconectar de {ct.missionTitle}
                                          </span>
                                        </button>
                                      ))}
                                    </>
                                  )}
                                </div>
                              </FilterDropdown>
                              <FilterDropdown
                                open={openContributeFor === task.id}
                                onClose={() => {
                                  setOpenContributeFor?.(null);
                                  setOpenRowMenu?.(null);
                                }}
                                anchorRef={{
                                  current:
                                    rowMenuBtnRefs?.current[task.id] ?? null,
                                }}
                                noOverlay
                              >
                                <div className="flex flex-col p-1 max-h-80 overflow-y-auto">
                                  <div className="flex items-center gap-2 px-2 py-2 border-b border-[var(--color-caramel-200)] mb-1">
                                    <MagnifyingGlass
                                      size={14}
                                      className="flex-shrink-0 text-[var(--color-neutral-400)]"
                                    />
                                    <input
                                      type="text"
                                      className="flex-1 min-w-0 border-none bg-transparent outline-none font-[var(--font-label)] font-medium text-[var(--text-xs)] text-[var(--color-neutral-950)] placeholder:text-[var(--color-neutral-400)]"
                                      placeholder="Buscar missão..."
                                      value={contributePickerSearch}
                                      onChange={(
                                        e: React.ChangeEvent<HTMLInputElement>,
                                      ) =>
                                        setContributePickerSearch?.(
                                          e.target.value,
                                        )
                                      }
                                    />
                                  </div>
                                  {allMissions
                                    .filter((m) => m.id !== mission.id)
                                    .filter(
                                      (m) =>
                                        !task.contributesTo?.some(
                                          (c) => c.missionId === m.id,
                                        ),
                                    )
                                    .filter((m) =>
                                      m.title
                                        .toLowerCase()
                                        .includes(
                                          contributePickerSearch.toLowerCase(),
                                        ),
                                    )
                                    .map((m) => (
                                      <button
                                        key={m.id}
                                        type="button"
                                        className="flex items-center gap-2 w-full px-2 py-2 border-none bg-transparent rounded-[var(--radius-2xs)] font-[var(--font-label)] font-medium text-[var(--text-xs)] text-[var(--color-neutral-950)] cursor-pointer whitespace-nowrap transition-colors duration-[120ms] text-left hover:bg-[var(--color-caramel-100)]"
                                        onClick={() =>
                                          onAddContribution?.(
                                            task,
                                            "task",
                                            mission.id,
                                            mission.title,
                                            m.id,
                                            m.title,
                                          )
                                        }
                                      >
                                        <Target size={14} />
                                        <span>{m.title}</span>
                                      </button>
                                    ))}
                                </div>
                              </FilterDropdown>
                            </div>
                          </div>
                        </div>
                      </div>
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
                    />
                  );
                })}
                {extContribs.length > 0 && (
                  <>
                    <div className="flex items-center gap-2 px-4 py-1 font-[var(--font-label)] text-[var(--text-xs)] text-[var(--color-neutral-500)] uppercase tracking-[0.03em]">
                      <GitBranch size={14} />
                      <span>Contribuições externas</span>
                      <Badge color="neutral" size="sm">
                        {extContribs.length}
                      </Badge>
                    </div>
                    {extContribs.map((ec: ExternalContribution) => (
                      <div
                        key={ec.id}
                        className="flex flex-row items-center gap-3 px-3 py-3 pl-4 bg-[var(--color-neutral-0)] border border-dashed border-[var(--color-caramel-200)] rounded-[var(--radius-2xs)]"
                      >
                        <div className="flex-1 min-w-0 flex flex-col gap-1">
                          <div className="flex items-center gap-3">
                            {ec.type === "indicator" ? (
                              <Gauge size={16} />
                            ) : (
                              <ListChecks size={16} />
                            )}
                            <span className="flex-1 min-w-0 overflow-hidden text-ellipsis whitespace-nowrap font-[var(--font-body)] text-[var(--text-sm)] text-[var(--color-neutral-900)]">
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
                              <span className="font-[var(--font-label)] text-[var(--text-sm)] text-[var(--color-neutral-500)] flex-shrink-0">
                                {ec.progress}%
                              </span>
                            )}
                          </div>
                          <div className="flex items-center gap-1 pl-[calc(16px+12px)] font-[var(--font-body)] text-[var(--text-xs)] text-[var(--color-neutral-400)]">
                            <Target size={12} />
                            <span>de {ec.sourceMission.title}</span>
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
                      <div className="flex items-center gap-3 px-4 py-3 bg-[var(--color-caramel-50)] border-[1.5px] border-dashed border-[var(--color-caramel-300)] rounded-[var(--radius-2xs)] font-[var(--font-body)] text-[var(--text-sm)] text-[var(--color-neutral-500)]">
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
