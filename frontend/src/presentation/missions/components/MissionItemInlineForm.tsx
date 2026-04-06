import { useState, useRef } from "react";
import type { Dispatch, SetStateAction } from "react";
import {
  FilterDropdown,
  Button,
  Input,
  Select,
  Radio,
  Checkbox,
  DatePicker,
  Badge,
  Avatar,
} from "@mdonangelo/bud-ds";
import type { CalendarDate } from "@mdonangelo/bud-ds";
import {
  Plus,
  CaretRight,
  Ruler,
  Calendar,
  DotsThree,
  Tag,
  MagnifyingGlass,
  ChartBar,
  PlusSquare,
} from "@phosphor-icons/react";
import {
  MEASUREMENT_MODES,
  MANUAL_INDICATOR_TYPES,
  UNIT_OPTIONS,
  MORE_MISSION_OPTIONS,
} from "../consts";
import { getTemplateConfig } from "../utils/utils";
import type { MissionItemData } from "../types";

interface MissionItemInlineFormProps {
  editingItem: MissionItemData;
  setEditingItem: Dispatch<SetStateAction<MissionItemData | null>>;
  isEditingExisting: boolean;
  onSave: () => void;
  onCancel: () => void;
  selectedTemplate?: string;
  drawerEditing?: boolean;
  missionOwnerOptions: { id: string; label: string; initials?: string }[];
  missionTagOptions: { id: string; label: string }[];
  visibilityOptions: { id: string; label: string }[];
  presetPeriods: {
    id: string;
    label: string;
    start: CalendarDate;
    end: CalendarDate;
  }[];
  createTag: (opts: { name: string }) => { id: string; name: string };
}

export function MissionItemInlineForm({
  editingItem,
  setEditingItem,
  isEditingExisting,
  onSave,
  onCancel,
  selectedTemplate,
  drawerEditing,
  missionOwnerOptions,
  missionTagOptions,
  visibilityOptions,
  presetPeriods,
  createTag,
}: MissionItemInlineFormProps) {
  const [itemMeasureOpen, setItemMeasureOpen] = useState(false);
  const [itemManualOpen, setItemManualOpen] = useState(false);
  const [itemMoreOpen, setItemMoreOpen] = useState(false);
  const [itemMoreSubPanel, setItemMoreSubPanel] = useState<string | null>(null);
  const [itemSupportTeam, setItemSupportTeam] = useState<string[]>([]);
  const [itemSupportSearch, setItemSupportSearch] = useState("");
  const [itemTags, setItemTags] = useState<string[]>([]);
  const [itemTagsSearch, setItemTagsSearch] = useState("");
  const [itemNewTagName, setItemNewTagName] = useState("");
  const [itemCustomTags, setItemCustomTags] = useState<
    { id: string; label: string }[]
  >([]);
  const [itemVisibility, setItemVisibility] = useState("org");
  const [itemVisibilitySearch, setItemVisibilitySearch] = useState("");
  const [itemPeriodOpen, setItemPeriodOpen] = useState(false);
  const [itemPeriodCustom, setItemPeriodCustom] = useState(false);

  const itemMeasureBtnRef = useRef<HTMLButtonElement>(null);
  const itemPeriodBtnRef = useRef<HTMLButtonElement>(null);
  const itemPeriodCustomBtnRef = useRef<HTMLButtonElement>(null);
  const itemMoreBtnRef = useRef<HTMLButtonElement>(null);
  const itemManualBtnRef = useRef<HTMLButtonElement>(null);
  const itemMoreItemRefs = useRef<Record<string, HTMLButtonElement | null>>({});

  const tplCfg = getTemplateConfig(selectedTemplate);

  return (
    <div className="flex flex-col w-full bg-[var(--color-caramel-100)] border border-[var(--color-caramel-300)] rounded-[var(--radius-xs)]">
      <div className="flex items-center justify-between px-4 py-3 border-b border-[var(--color-caramel-300)]">
        <span className="font-[var(--font-label)] font-medium text-[var(--text-xs)] text-[var(--color-neutral-950)] leading-[1.15]">
          {isEditingExisting
            ? tplCfg.editItemFormTitle
            : tplCfg.addItemFormTitle}
        </span>
        <PlusSquare
          size={16}
          className="text-[var(--color-neutral-500)] flex-shrink-0"
        />
      </div>

      <div className="flex flex-col gap-3 px-4 py-3">
        <div className="flex flex-col gap-2">
          <input
            type="text"
            className="w-full border-none bg-transparent outline-none font-[var(--font-heading)] font-medium text-[var(--text-md)] text-[var(--color-neutral-950)] leading-[1.1] placeholder:text-[var(--color-neutral-500)]"
            placeholder={tplCfg.itemTitlePlaceholder}
            value={editingItem.name}
            onChange={(e: React.ChangeEvent<HTMLInputElement>) =>
              setEditingItem((prev) =>
                prev ? { ...prev, name: e.target.value } : prev,
              )
            }
          />
          <input
            type="text"
            className="w-full border-none bg-transparent outline-none font-[var(--font-heading)] font-medium text-[var(--text-xs)] text-[var(--color-neutral-950)] leading-[1.15] placeholder:text-[var(--color-neutral-500)]"
            placeholder={tplCfg.itemDescPlaceholder}
            value={editingItem.description}
            onChange={(e: React.ChangeEvent<HTMLInputElement>) =>
              setEditingItem((prev) =>
                prev ? { ...prev, description: e.target.value } : prev,
              )
            }
          />
        </div>

        <div className="flex gap-2 items-center">
          <Button
            ref={itemMeasureBtnRef}
            variant="secondary"
            size="sm"
            leftIcon={Ruler}
            onClick={() => setItemMeasureOpen((v) => !v)}
          >
            {editingItem.measurementMode
              ? MEASUREMENT_MODES.find(
                  (m) => m.id === editingItem.measurementMode,
                )?.label
              : "Modo de mensuração"}
          </Button>

          <Button
            ref={itemPeriodBtnRef}
            variant="secondary"
            size="sm"
            leftIcon={Calendar}
            onClick={() => {
              setItemPeriodOpen((v) => !v);
              setItemPeriodCustom(false);
            }}
          >
            {editingItem.period[0] && editingItem.period[1]
              ? `${String(editingItem.period[0].day).padStart(2, "0")}/${String(editingItem.period[0].month).padStart(2, "0")} - ${String(editingItem.period[1].day).padStart(2, "0")}/${String(editingItem.period[1].month).padStart(2, "0")}/${editingItem.period[1].year}`
              : "Período"}
          </Button>

          <Button
            ref={itemMoreBtnRef}
            variant="secondary"
            size="sm"
            leftIcon={DotsThree}
            aria-label="Mais opções"
            onClick={() => {
              setItemMoreSubPanel(null);
              setItemMoreOpen((v) => !v);
            }}
          />
        </div>

        {/* ——— Mission inputs based on measurement mode ——— */}
        {editingItem.measurementMode === "manual" && editingItem.manualType && (
          <div className="flex flex-col gap-2 pt-3 border-t border-[var(--color-caramel-200)]">
            <span className="font-[var(--font-label)] font-medium text-[var(--text-xs)] text-[var(--color-neutral-500)] leading-[1.15]">
              {
                MANUAL_INDICATOR_TYPES.find(
                  (t) => t.id === editingItem.manualType,
                )?.label
              }
            </span>

            {(editingItem.manualType === "reach" ||
              editingItem.manualType === "reduce") && (
              <div className="flex gap-2 items-end [&>*]:flex-1 [&>*]:min-w-0">
                <Input
                  label="Valor alvo"
                  placeholder="Ex: 1000"
                  value={editingItem.missionValue}
                  onChange={(e: React.ChangeEvent<HTMLInputElement>) =>
                    setEditingItem((prev) =>
                      prev ? { ...prev, missionValue: e.target.value } : prev,
                    )
                  }
                />
                <Select
                  label="Unidade"
                  placeholder="Selecionar"
                  options={UNIT_OPTIONS}
                  value={editingItem.missionUnit}
                  onChange={(v: string) =>
                    setEditingItem((prev) =>
                      prev ? { ...prev, missionUnit: v } : prev,
                    )
                  }
                />
              </div>
            )}

            {editingItem.manualType === "above" && (
              <div className="flex gap-2 items-end [&>*]:flex-1 [&>*]:min-w-0">
                <Input
                  label="Mínimo"
                  placeholder="Ex: 70"
                  value={editingItem.missionValueMin}
                  onChange={(e: React.ChangeEvent<HTMLInputElement>) =>
                    setEditingItem((prev) =>
                      prev
                        ? { ...prev, missionValueMin: e.target.value }
                        : prev,
                    )
                  }
                />
                <Select
                  label="Unidade"
                  placeholder="Selecionar"
                  options={UNIT_OPTIONS}
                  value={editingItem.missionUnit}
                  onChange={(v: string) =>
                    setEditingItem((prev) =>
                      prev ? { ...prev, missionUnit: v } : prev,
                    )
                  }
                />
              </div>
            )}

            {editingItem.manualType === "below" && (
              <div className="flex gap-2 items-end [&>*]:flex-1 [&>*]:min-w-0">
                <Input
                  label="Máximo"
                  placeholder="Ex: 5"
                  value={editingItem.missionValueMax}
                  onChange={(e: React.ChangeEvent<HTMLInputElement>) =>
                    setEditingItem((prev) =>
                      prev
                        ? { ...prev, missionValueMax: e.target.value }
                        : prev,
                    )
                  }
                />
                <Select
                  label="Unidade"
                  placeholder="Selecionar"
                  options={UNIT_OPTIONS}
                  value={editingItem.missionUnit}
                  onChange={(v: string) =>
                    setEditingItem((prev) =>
                      prev ? { ...prev, missionUnit: v } : prev,
                    )
                  }
                />
              </div>
            )}

            {editingItem.manualType === "between" && (
              <div className="flex gap-2 items-end [&>*]:flex-1 [&>*]:min-w-0">
                <Input
                  label="Mínimo"
                  placeholder="Ex: 50"
                  value={editingItem.missionValueMin}
                  onChange={(e: React.ChangeEvent<HTMLInputElement>) =>
                    setEditingItem((prev) =>
                      prev
                        ? { ...prev, missionValueMin: e.target.value }
                        : prev,
                    )
                  }
                />
                <Input
                  label="Máximo"
                  placeholder="Ex: 90"
                  value={editingItem.missionValueMax}
                  onChange={(e: React.ChangeEvent<HTMLInputElement>) =>
                    setEditingItem((prev) =>
                      prev
                        ? { ...prev, missionValueMax: e.target.value }
                        : prev,
                    )
                  }
                />
                <Select
                  label="Unidade"
                  placeholder="Selecionar"
                  options={UNIT_OPTIONS}
                  value={editingItem.missionUnit}
                  onChange={(v: string) =>
                    setEditingItem((prev) =>
                      prev ? { ...prev, missionUnit: v } : prev,
                    )
                  }
                />
              </div>
            )}

          </div>
        )}
      </div>

      {/* Modo de mensuração dropdown */}
      <FilterDropdown
        open={itemMeasureOpen}
        onClose={() => setItemMeasureOpen(false)}
        anchorRef={itemMeasureBtnRef}
      >
        <div className="flex flex-col p-1 max-h-80 overflow-y-auto">
          {MEASUREMENT_MODES.filter((mode) => {
            if (drawerEditing) return true;
            const cfg = getTemplateConfig(selectedTemplate);
            return !cfg.allowedModes || cfg.allowedModes.includes(mode.id);
          }).map((mode) => {
            const Icon = mode.icon;
            const isActive = editingItem?.measurementMode === mode.id;
            return (
              <button
                key={mode.id}
                ref={
                  mode.id === "manual"
                    ? (el) => {
                        itemManualBtnRef.current = el;
                      }
                    : undefined
                }
                type="button"
                className={`flex items-start gap-2 w-full px-2 py-2 border-none bg-transparent rounded-[var(--radius-2xs)] cursor-pointer transition-colors duration-[120ms] text-left hover:bg-[var(--color-caramel-100)] ${isActive ? "bg-[var(--color-caramel-50)]" : ""}`}
                onClick={() => {
                  if (mode.id === "manual") {
                    setItemManualOpen(true);
                  } else {
                    setEditingItem((prev) =>
                      prev
                        ? {
                            ...prev,
                            measurementMode: mode.id,
                            manualType: null,
                            missionValue: "",
                            missionValueMin: "",
                            missionValueMax: "",
                            missionUnit: "",
                            children: mode.id === "mission" ? [] : undefined,
                          }
                        : prev,
                    );
                    setItemMeasureOpen(false);
                  }
                }}
              >
                <Icon
                  size={16}
                  className="flex-shrink-0 text-[var(--color-neutral-500)] mt-[2px]"
                />
                <div className="flex flex-col gap-[2px] flex-1 min-w-0">
                  <span className="font-[var(--font-label)] font-medium text-[var(--text-xs)] text-[var(--color-neutral-950)] leading-[1.15]">
                    {mode.label}
                  </span>
                  <span className="font-[var(--font-body)] text-[10px] text-[var(--color-neutral-500)] leading-[1.3]">
                    {mode.description}
                  </span>
                </div>
                {mode.id === "manual" && (
                  <CaretRight
                    size={12}
                    className="ml-auto text-[var(--color-neutral-400)] flex-shrink-0"
                  />
                )}
              </button>
            );
          })}
        </div>
      </FilterDropdown>

      {/* Sub-panel: Indicador manual — tipo */}
      <FilterDropdown
        open={itemMeasureOpen && itemManualOpen}
        onClose={() => setItemManualOpen(false)}
        anchorRef={itemManualBtnRef}
        placement="right-start"
        noOverlay
      >
        <div className="flex flex-col p-1 max-h-80 overflow-y-auto">
          {MANUAL_INDICATOR_TYPES.map((t) => {
            const Icon = t.icon;
            const isActive = editingItem?.manualType === t.id;
            return (
              <button
                key={t.id}
                type="button"
                className={`flex items-center gap-2 w-full px-2 py-2 border-none bg-transparent rounded-[var(--radius-2xs)] font-[var(--font-label)] font-medium text-[var(--text-xs)] text-[var(--color-neutral-950)] cursor-pointer whitespace-nowrap transition-colors duration-[120ms] text-left hover:bg-[var(--color-caramel-100)] ${isActive ? "bg-[var(--color-caramel-50)]" : ""}`}
                onClick={() => {
                  setEditingItem((prev) =>
                    prev
                      ? {
                          ...prev,
                          measurementMode: "manual",
                          manualType: t.id,
                          missionValue: "",
                          missionValueMin: "",
                          missionValueMax: "",
                          missionUnit: "",
                        }
                      : prev,
                  );
                  setItemManualOpen(false);
                  setItemMeasureOpen(false);
                }}
              >
                <Icon size={14} />
                <span>{t.label}</span>
              </button>
            );
          })}
        </div>
      </FilterDropdown>


      {/* Período dropdown — presets */}
      <FilterDropdown
        open={itemPeriodOpen}
        onClose={() => {
          setItemPeriodOpen(false);
          setItemPeriodCustom(false);
        }}
        anchorRef={itemPeriodBtnRef}
      >
        <div className="flex flex-col p-1 max-h-80 overflow-y-auto">
          {presetPeriods.map((p) => {
            const isActive =
              editingItem?.period[0]?.year === p.start.year &&
              editingItem?.period[0]?.month === p.start.month &&
              editingItem?.period[0]?.day === p.start.day &&
              editingItem?.period[1]?.year === p.end.year &&
              editingItem?.period[1]?.month === p.end.month &&
              editingItem?.period[1]?.day === p.end.day;
            return (
              <button
                key={p.id}
                type="button"
                className={`flex items-center gap-2 w-full px-2 py-2 border-none bg-transparent rounded-[var(--radius-2xs)] font-[var(--font-label)] font-medium text-[var(--text-xs)] text-[var(--color-neutral-950)] cursor-pointer whitespace-nowrap transition-colors duration-[120ms] text-left hover:bg-[var(--color-caramel-100)] ${isActive ? "bg-[var(--color-caramel-50)]" : ""}`}
                onClick={() => {
                  setEditingItem((prev) =>
                    prev ? { ...prev, period: [p.start, p.end] } : prev,
                  );
                  setItemPeriodOpen(false);
                }}
              >
                <Radio checked={isActive} readOnly />
                <span>{p.label}</span>
              </button>
            );
          })}
        </div>
        <div className="border-t border-[var(--color-caramel-300)] p-1">
          <button
            ref={itemPeriodCustomBtnRef}
            type="button"
            className={`flex items-center gap-2 w-full px-2 py-2 border-none bg-transparent rounded-[var(--radius-2xs)] font-[var(--font-label)] font-medium text-[var(--text-xs)] text-[var(--color-neutral-950)] cursor-pointer whitespace-nowrap transition-colors duration-[120ms] text-left hover:bg-[var(--color-caramel-100)] ${itemPeriodCustom ? "bg-[var(--color-caramel-50)]" : ""}`}
            onClick={() => setItemPeriodCustom((v) => !v)}
          >
            <Plus size={14} />
            <span>Período personalizado</span>
            <CaretRight
              size={12}
              className="ml-auto text-[var(--color-neutral-400)] flex-shrink-0"
            />
          </button>
        </div>
      </FilterDropdown>

      {/* Período — sub-panel: calendário personalizado */}
      <FilterDropdown
        open={itemPeriodOpen && itemPeriodCustom}
        onClose={() => setItemPeriodCustom(false)}
        anchorRef={itemPeriodCustomBtnRef}
        placement="right-start"
        noOverlay
      >
        <div className="p-3">
          <DatePicker
            mode="range"
            value={editingItem?.period ?? [null, null]}
            onChange={(range: [CalendarDate | null, CalendarDate | null]) => {
              setEditingItem((prev) =>
                prev ? { ...prev, period: range } : prev,
              );
              if (range[0] && range[1]) {
                setItemPeriodOpen(false);
                setItemPeriodCustom(false);
              }
            }}
          />
        </div>
      </FilterDropdown>

      {/* Item "..." — main menu */}
      <FilterDropdown
        open={itemMoreOpen}
        onClose={() => {
          setItemMoreOpen(false);
          setItemMoreSubPanel(null);
        }}
        anchorRef={itemMoreBtnRef}
      >
        <div className="flex flex-col p-1 max-h-80 overflow-y-auto">
          {MORE_MISSION_OPTIONS.map((opt) => {
            const Icon = opt.icon;
            const isActive = itemMoreSubPanel === opt.id;
            const count =
              opt.id === "team-support"
                ? itemSupportTeam.length
                : opt.id === "organizers"
                  ? itemTags.length
                  : 0;
            return (
              <button
                key={opt.id}
                ref={(el) => {
                  itemMoreItemRefs.current[opt.id] = el;
                }}
                type="button"
                className={`flex items-center gap-2 w-full px-2 py-2 border-none bg-transparent rounded-[var(--radius-2xs)] font-[var(--font-label)] font-medium text-[var(--text-xs)] text-[var(--color-neutral-950)] cursor-pointer whitespace-nowrap transition-colors duration-[120ms] text-left hover:bg-[var(--color-caramel-100)] ${isActive ? "bg-[var(--color-caramel-50)]" : ""}`}
                onClick={() => setItemMoreSubPanel(isActive ? null : opt.id)}
              >
                <Icon size={14} />
                <span>{opt.label}</span>
                {count > 0 && (
                  <Badge color="neutral" size="sm">
                    {count}
                  </Badge>
                )}
                <CaretRight
                  size={12}
                  className="ml-auto text-[var(--color-neutral-400)] flex-shrink-0"
                />
              </button>
            );
          })}
        </div>
      </FilterDropdown>

      {/* Item "..." sub-panel: Time de apoio */}
      <FilterDropdown
        open={itemMoreOpen && itemMoreSubPanel === "team-support"}
        onClose={() => setItemMoreSubPanel(null)}
        anchorRef={{
          current: itemMoreItemRefs.current["team-support"] ?? null,
        }}
        placement="right-start"
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
              placeholder="Buscar pessoa..."
              value={itemSupportSearch}
              onChange={(e: React.ChangeEvent<HTMLInputElement>) =>
                setItemSupportSearch(e.target.value)
              }
            />
          </div>
          {missionOwnerOptions
            .filter((opt) =>
              opt.label.toLowerCase().includes(itemSupportSearch.toLowerCase()),
            )
            .map((opt) => {
              const checked = itemSupportTeam.includes(opt.id);
              return (
                <button
                  key={opt.id}
                  type="button"
                  className={`flex items-center gap-2 w-full px-2 py-2 border-none bg-transparent rounded-[var(--radius-2xs)] font-[var(--font-label)] font-medium text-[var(--text-xs)] text-[var(--color-neutral-950)] cursor-pointer whitespace-nowrap transition-colors duration-[120ms] text-left hover:bg-[var(--color-caramel-100)] ${checked ? "bg-[var(--color-caramel-50)]" : ""}`}
                  onClick={() =>
                    setItemSupportTeam((prev) =>
                      prev.includes(opt.id)
                        ? prev.filter((id) => id !== opt.id)
                        : [...prev, opt.id],
                    )
                  }
                >
                  <Checkbox checked={checked} readOnly />
                  {opt.initials && <Avatar initials={opt.initials} size="xs" />}
                  <span>{opt.label}</span>
                </button>
              );
            })}
        </div>
      </FilterDropdown>

      {/* Item "..." sub-panel: Tags */}
      <FilterDropdown
        open={itemMoreOpen && itemMoreSubPanel === "organizers"}
        onClose={() => setItemMoreSubPanel(null)}
        anchorRef={{
          current: itemMoreItemRefs.current["organizers"] ?? null,
        }}
        placement="right-start"
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
              placeholder="Buscar tag..."
              value={itemTagsSearch}
              onChange={(e: React.ChangeEvent<HTMLInputElement>) =>
                setItemTagsSearch(e.target.value)
              }
            />
          </div>
          {[...missionTagOptions, ...itemCustomTags]
            .filter((tag) =>
              tag.label.toLowerCase().includes(itemTagsSearch.toLowerCase()),
            )
            .map((tag) => {
              const checked = itemTags.includes(tag.id);
              return (
                <button
                  key={tag.id}
                  type="button"
                  className={`flex items-center gap-2 w-full px-2 py-2 border-none bg-transparent rounded-[var(--radius-2xs)] font-[var(--font-label)] font-medium text-[var(--text-xs)] text-[var(--color-neutral-950)] cursor-pointer whitespace-nowrap transition-colors duration-[120ms] text-left hover:bg-[var(--color-caramel-100)] ${checked ? "bg-[var(--color-caramel-50)]" : ""}`}
                  onClick={() =>
                    setItemTags((prev) =>
                      prev.includes(tag.id)
                        ? prev.filter((id) => id !== tag.id)
                        : [...prev, tag.id],
                    )
                  }
                >
                  <Checkbox checked={checked} readOnly />
                  <Tag size={14} />
                  <span>{tag.label}</span>
                </button>
              );
            })}
          <div className="flex items-center gap-1 px-2 py-1 border-t border-[var(--color-caramel-200)] mt-1">
            <input
              type="text"
              className="flex-1 min-w-0 border-none bg-transparent outline-none font-[var(--font-label)] font-medium text-[var(--text-xs)] text-[var(--color-neutral-950)] placeholder:text-[var(--color-neutral-400)]"
              placeholder="Criar nova tag..."
              value={itemNewTagName}
              onChange={(e: React.ChangeEvent<HTMLInputElement>) =>
                setItemNewTagName(e.target.value)
              }
              onKeyDown={(e: React.KeyboardEvent) => {
                if (e.key === "Enter" && itemNewTagName.trim()) {
                  const created = createTag({ name: itemNewTagName.trim() });
                  setItemCustomTags((prev) => [
                    ...prev,
                    { id: created.id, label: created.name },
                  ]);
                  setItemTags((prev) => [...prev, created.id]);
                  setItemNewTagName("");
                }
              }}
            />
            <Button
              variant="tertiary"
              size="sm"
              leftIcon={Plus}
              aria-label="Criar tag"
              disabled={!itemNewTagName.trim()}
              onClick={() => {
                if (itemNewTagName.trim()) {
                  const created = createTag({ name: itemNewTagName.trim() });
                  setItemCustomTags((prev) => [
                    ...prev,
                    { id: created.id, label: created.name },
                  ]);
                  setItemTags((prev) => [...prev, created.id]);
                  setItemNewTagName("");
                }
              }}
            />
          </div>
        </div>
      </FilterDropdown>

      {/* Item "..." sub-panel: Conectar com outra missão */}
      {/* Item "..." sub-panel: Quem pode ver */}
      <FilterDropdown
        open={itemMoreOpen && itemMoreSubPanel === "visibility"}
        onClose={() => setItemMoreSubPanel(null)}
        anchorRef={{
          current: itemMoreItemRefs.current["visibility"] ?? null,
        }}
        placement="right-start"
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
              placeholder="Buscar visibilidade..."
              value={itemVisibilitySearch}
              onChange={(e: React.ChangeEvent<HTMLInputElement>) =>
                setItemVisibilitySearch(e.target.value)
              }
            />
          </div>
          {visibilityOptions
            .filter((opt) =>
              opt.label
                .toLowerCase()
                .includes(itemVisibilitySearch.toLowerCase()),
            )
            .map((opt) => (
              <button
                key={opt.id}
                type="button"
                className={`flex items-center gap-2 w-full px-2 py-2 border-none bg-transparent rounded-[var(--radius-2xs)] font-[var(--font-label)] font-medium text-[var(--text-xs)] text-[var(--color-neutral-950)] cursor-pointer whitespace-nowrap transition-colors duration-[120ms] text-left hover:bg-[var(--color-caramel-100)] ${itemVisibility === opt.id ? "bg-[var(--color-caramel-50)]" : ""}`}
                onClick={() => setItemVisibility(opt.id)}
              >
                <Radio checked={itemVisibility === opt.id} readOnly />
                <span>{opt.label}</span>
              </button>
            ))}
        </div>
      </FilterDropdown>

      <div className="flex items-center justify-between px-4 py-3 border-t border-[var(--color-caramel-300)]">
        <Button variant="tertiary" size="sm" onClick={onCancel}>
          Cancelar
        </Button>
        <Button
          variant="primary"
          size="sm"
          disabled={!editingItem.name.trim()}
          onClick={onSave}
        >
          {isEditingExisting ? "Salvar" : "Adicionar"}
        </Button>
      </div>
    </div>
  );
}
