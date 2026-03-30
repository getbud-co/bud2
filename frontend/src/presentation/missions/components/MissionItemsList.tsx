import type { Dispatch, ReactNode, SetStateAction } from "react";
import { Button } from "@mdonangelo/bud-ds";
import {
  CaretUp,
  CaretDown,
  PlusSquare,
  PencilSimple,
  X,
} from "@phosphor-icons/react";
import {
  MEASUREMENT_MODES,
  MANUAL_INDICATOR_TYPES,
  UNIT_OPTIONS,
} from "../consts";
import { getTemplateConfig } from "../utils/utils";
import type { MissionItemData } from "../types";

function getMissionSummary(item: MissionItemData): string {
  if (item.measurementMode !== "manual" || !item.manualType) return "";
  const unit =
    UNIT_OPTIONS.find((u) => u.value === item.missionUnit)?.label ??
    item.missionUnit;
  if (item.manualType === "between")
    return item.missionValueMin && item.missionValueMax
      ? `${item.missionValueMin} – ${item.missionValueMax} ${unit}`
      : "";
  if (item.manualType === "above")
    return item.missionValueMin ? `≥ ${item.missionValueMin} ${unit}` : "";
  if (item.manualType === "below")
    return item.missionValueMax ? `≤ ${item.missionValueMax} ${unit}` : "";
  if (item.missionValue) return `${item.missionValue} ${unit}`;
  return "";
}

function removeChildFromTree(
  items: MissionItemData[],
  childId: string,
): MissionItemData[] {
  return items
    .filter((item) => item.id !== childId)
    .map((item) =>
      item.children?.length
        ? { ...item, children: removeChildFromTree(item.children, childId) }
        : item,
    );
}

interface MissionItemsListProps {
  items: MissionItemData[];
  parentId: string | null;
  isEditingExisting: boolean;
  editingItem: MissionItemData | null;
  editingParentId: string | null;
  expandedSubMissions: Set<string>;
  setExpandedSubMissions: Dispatch<SetStateAction<Set<string>>>;
  setNewMissionItems: Dispatch<SetStateAction<MissionItemData[]>>;
  setEditingItem: Dispatch<SetStateAction<MissionItemData | null>>;
  setIsEditingExisting: Dispatch<SetStateAction<boolean>>;
  setEditingParentId: Dispatch<SetStateAction<string | null>>;
  selectedTemplate: string | undefined;
  renderInlineForm: () => ReactNode;
}

export function MissionItemsList({
  items,
  parentId,
  isEditingExisting,
  editingItem,
  editingParentId,
  expandedSubMissions,
  setExpandedSubMissions,
  setNewMissionItems,
  setEditingItem,
  setIsEditingExisting,
  setEditingParentId,
  selectedTemplate,
  renderInlineForm,
}: MissionItemsListProps) {
  const isNested = parentId !== null;

  const sharedChildProps = {
    isEditingExisting,
    editingItem,
    editingParentId,
    expandedSubMissions,
    setExpandedSubMissions,
    setNewMissionItems,
    setEditingItem,
    setIsEditingExisting,
    setEditingParentId,
    selectedTemplate,
    renderInlineForm,
  };

  return (
    <>
      {items.map((item) => {
        const isBeingEdited =
          isEditingExisting &&
          editingItem?.id === item.id &&
          editingParentId === parentId;

        if (isBeingEdited) {
          return (
            <div
              key={item.id}
              className={isNested ? undefined : "flex flex-col gap-2"}
            >
              {isNested ? (
                <div className="relative pl-0">{renderInlineForm()}</div>
              ) : (
                renderInlineForm()
              )}
            </div>
          );
        }

        const missionSummary = getMissionSummary(item);
        const canHaveChildren =
          item.measurementMode === "mission" ||
          item.measurementMode === "manual";
        const isExpanded = expandedSubMissions.has(item.id);
        const childCount = item.children?.length ?? 0;

        const handleEdit = () => {
          setEditingItem({ ...item });
          setIsEditingExisting(true);
          setEditingParentId(parentId);
        };

        const handleRemove = () => {
          if (parentId) {
            setNewMissionItems((prev) => removeChildFromTree(prev, item.id));
          } else {
            setNewMissionItems((prev) => prev.filter((i) => i.id !== item.id));
          }
        };

        const rowClass = isNested
          ? "relative flex items-center gap-2 px-4 py-3 bg-[var(--color-caramel-50)] rounded-[var(--radius-2xs)] min-h-[48px] hover:bg-[var(--color-caramel-100)] transition-colors duration-[120ms]"
          : "flex items-center gap-2 px-4 py-3 bg-[var(--color-caramel-100)] border border-[var(--color-caramel-300)] rounded-[var(--radius-xs)] transition-colors duration-[120ms] hover:border-[var(--color-caramel-500)]";

        return (
          <div
            key={item.id}
            className={isNested ? undefined : "flex flex-col gap-2"}
          >
            <div className={rowClass}>
              {canHaveChildren && (
                <button
                  type="button"
                  className="flex items-center justify-center flex-shrink-0 w-6 h-6 border-none bg-transparent rounded-[var(--radius-2xs)] text-[var(--color-neutral-500)] cursor-pointer transition-colors duration-[120ms] hover:bg-[var(--color-caramel-200)]"
                  onClick={() =>
                    setExpandedSubMissions((prev) => {
                      const next = new Set(prev);
                      if (next.has(item.id)) next.delete(item.id);
                      else next.add(item.id);
                      return next;
                    })
                  }
                  aria-label={
                    isExpanded ? "Recolher sub-itens" : "Expandir sub-itens"
                  }
                >
                  {isExpanded ? <CaretUp size={14} /> : <CaretDown size={14} />}
                </button>
              )}
              {/* eslint-disable-next-line jsx-a11y/click-events-have-key-events, jsx-a11y/no-static-element-interactions */}
              <div
                className="flex flex-col gap-[2px] flex-1 min-w-0 cursor-pointer"
                onClick={handleEdit}
              >
                <div className="flex items-center gap-2 min-w-0">
                  <span className="font-[var(--font-label)] font-medium text-[var(--text-xs)] text-[var(--color-neutral-950)] overflow-hidden text-ellipsis whitespace-nowrap">
                    {item.name}
                  </span>
                  {item.measurementMode && (
                    <span className="flex-shrink-0 font-[var(--font-label)] font-medium text-[10px] text-[var(--color-neutral-500)] bg-[var(--color-caramel-200)] px-1 py-[2px] rounded-[var(--radius-2xs)] whitespace-nowrap">
                      {
                        MEASUREMENT_MODES.find(
                          (m) => m.id === item.measurementMode,
                        )?.label
                      }
                      {item.manualType &&
                        ` · ${MANUAL_INDICATOR_TYPES.find((t) => t.id === item.manualType)?.label}`}
                    </span>
                  )}
                  {canHaveChildren && childCount > 0 && (
                    <span className="flex-shrink-0 font-[var(--font-body)] text-[10px] text-[var(--color-neutral-500)] whitespace-nowrap">
                      {childCount} {childCount === 1 ? "item" : "itens"}
                    </span>
                  )}
                </div>
                {missionSummary && (
                  <span className="font-[var(--font-body)] text-[10px] text-[var(--color-neutral-500)] leading-[1.3] overflow-hidden text-ellipsis whitespace-nowrap">
                    {missionSummary}
                  </span>
                )}
              </div>
              <div className="flex flex-shrink-0 gap-[2px]">
                <Button
                  variant="tertiary"
                  size="sm"
                  leftIcon={PencilSimple}
                  aria-label="Editar item"
                  onClick={handleEdit}
                />
                <Button
                  variant="tertiary"
                  size="sm"
                  leftIcon={X}
                  aria-label="Remover item"
                  onClick={handleRemove}
                />
              </div>
            </div>

            {canHaveChildren && isExpanded && (
              <div className="flex flex-col gap-2 pl-10 overflow-hidden">
                <MissionItemsList
                  items={item.children ?? []}
                  parentId={item.id}
                  {...sharedChildProps}
                />
              </div>
            )}
          </div>
        );
      })}

      {(() => {
        const tplCfg = getTemplateConfig(selectedTemplate);
        const addLabel = isNested ? "Adicionar item" : tplCfg.addItemLabel;
        if (editingItem && !isEditingExisting && editingParentId === parentId) {
          return isNested ? (
            <div className="relative pl-0">{renderInlineForm()}</div>
          ) : (
            renderInlineForm()
          );
        }
        if (!editingItem || editingParentId !== parentId) {
          const handleAdd = () => {
            setIsEditingExisting(false);
            setEditingParentId(parentId);
            setEditingItem({
              id: `item-${Date.now()}`,
              name: "",
              description: "",
              measurementMode: null,
              manualType: null,
              surveyId: null,
              period: [null, null],
              missionValue: "",
              missionValueMin: "",
              missionValueMax: "",
              missionUnit: "",
            });
          };
          return isNested ? (
            <div className="relative pl-0">
              <button
                type="button"
                className="flex items-center justify-between w-full px-4 py-3 bg-[var(--color-caramel-100)] border border-[var(--color-caramel-300)] rounded-[var(--radius-xs)] cursor-pointer font-[var(--font-label)] font-medium text-[var(--text-xs)] text-[var(--color-neutral-950)] transition-colors duration-[120ms] hover:bg-[var(--color-caramel-50)] hover:border-[var(--color-caramel-500)]"
                onClick={handleAdd}
              >
                <span>{addLabel}</span>
                <PlusSquare size={16} />
              </button>
            </div>
          ) : (
            <button
              type="button"
              className="flex items-center justify-between w-full px-4 py-3 bg-[var(--color-caramel-100)] border border-[var(--color-caramel-300)] rounded-[var(--radius-xs)] cursor-pointer font-[var(--font-label)] font-medium text-[var(--text-xs)] text-[var(--color-neutral-950)] transition-colors duration-[120ms] hover:bg-[var(--color-caramel-50)] hover:border-[var(--color-caramel-500)]"
              onClick={handleAdd}
            >
              <span>{addLabel}</span>
              <PlusSquare size={16} />
            </button>
          );
        }
        return null;
      })()}
    </>
  );
}
