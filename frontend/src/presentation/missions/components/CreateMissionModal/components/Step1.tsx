import type { ReactNode, RefObject } from "react";
import {
  Button,
  CardDivider,
  FilterDropdown,
  Badge,
  DatePicker,
  Radio,
} from "@getbud-co/buds";
import type { CalendarDate } from "@getbud-co/buds";
import {
  UserCircle,
  Calendar,
  DotsThree,
  Plus,
  CaretRight,
  Eye,
  EyeSlash,
  Tag,
} from "@phosphor-icons/react";
import { PopoverSelect } from "@/components/PopoverSelect";
import type { PopoverSelectOption } from "@/components/PopoverSelect";
import { MORE_MISSION_OPTIONS, type TemplateConfig } from "../../../consts";

type OwnerOption = { id: string; label: string; initials?: string | null };

function toSelectOptions(opts: OwnerOption[]): PopoverSelectOption[] {
  return opts.map(({ id, label, initials }) => ({
    id,
    label,
    initials: initials ?? undefined,
  }));
}

const VISIBILITY_OPTIONS = [
  {
    id: "public",
    label: "Público",
    description: "Visível para toda a organização",
  },
  {
    id: "private",
    label: "Privado",
    description: "Visível apenas para o responsável e time de apoio",
  },
];

interface Step1Props {
  editingMissionId: string | null;
  tplCfg: TemplateConfig;
  newMissionName: string;
  setNewMissionName: (v: string) => void;
  newMissionDesc: string;
  setNewMissionDesc: (v: string) => void;
  ownerBtnRef: RefObject<HTMLButtonElement | null>;
  ownerPopoverOpen: boolean;
  onToggleOwnerPopover: () => void;
  selectedMissionOwners: string[];
  missionOwnerOptions: OwnerOption[];
  onOwnerChange: (val: string) => void;
  onCloseOwnerPopover: () => void;
  missionPeriodBtnRef: RefObject<HTMLButtonElement | null>;
  missionPeriod: [CalendarDate | null, CalendarDate | null];
  missionPeriodOpen: boolean;
  missionPeriodCustom: boolean;
  missionPeriodCustomBtnRef: RefObject<HTMLButtonElement | null>;
  presetPeriods: { id: string; label: string; start: CalendarDate; end: CalendarDate }[];
  onTogglePeriodOpen: () => void;
  onClosePeriod: () => void;
  onSetMissionPeriod: (p: [CalendarDate | null, CalendarDate | null]) => void;
  onTogglePeriodCustom: () => void;
  moreBtnRef: RefObject<HTMLButtonElement | null>;
  morePopoverOpen: boolean;
  moreSubPanel: string | null;
  moreItemRefs: RefObject<Record<string, HTMLButtonElement | null>>;
  onToggleMorePopover: () => void;
  onCloseMorePopover: () => void;
  onSetMoreSubPanel: (id: string | null) => void;
  selectedSupportTeam: string[];
  onSupportTeamChange: (v: string[]) => void;
  selectedTags: string[];
  onTagsChange: (v: string[]) => void;
  customTags: { id: string; label: string }[];
  onAddCustomTag: (t: { id: string; label: string }) => void;
  selectedVisibility: string;
  onVisibilityChange: (v: string) => void;
  createTag: (tag: { name: string }) => { id: string; name: string };
  missionTagOptions: { id: string; label: string }[];
  itemsSection: ReactNode;
}

export function Step1({
  editingMissionId,
  tplCfg,
  newMissionName,
  setNewMissionName,
  newMissionDesc,
  setNewMissionDesc,
  ownerBtnRef,
  ownerPopoverOpen,
  onToggleOwnerPopover,
  selectedMissionOwners,
  missionOwnerOptions,
  onOwnerChange,
  onCloseOwnerPopover,
  missionPeriodBtnRef,
  missionPeriod,
  missionPeriodOpen,
  missionPeriodCustom,
  missionPeriodCustomBtnRef,
  presetPeriods,
  onTogglePeriodOpen,
  onClosePeriod,
  onSetMissionPeriod,
  onTogglePeriodCustom,
  moreBtnRef,
  morePopoverOpen,
  moreSubPanel,
  moreItemRefs,
  onToggleMorePopover,
  onCloseMorePopover,
  onSetMoreSubPanel,
  selectedSupportTeam,
  onSupportTeamChange,
  selectedTags,
  onTagsChange,
  customTags,
  onAddCustomTag,
  selectedVisibility,
  onVisibilityChange,
  createTag,
  missionTagOptions,
  itemsSection,
}: Step1Props) {
  return (
    <div className="flex flex-col gap-[var(--sp-sm)]">
      <p className="m-0 [font-family:var(--font-heading)] font-semibold text-[var(--text-sm)] text-[var(--color-neutral-950)] leading-[1.25]">
        {editingMissionId ? "Editar missão" : tplCfg.stepTitle}
      </p>

      <div className="flex flex-col gap-[var(--sp-xs)]">
        <input
          type="text"
          className="w-full border-none bg-transparent outline-none [font-family:var(--font-heading)] font-medium text-[var(--text-2xl)] text-[var(--color-neutral-950)] leading-[1.1] tracking-[-0.5px] placeholder:text-[var(--color-neutral-500)]"
          placeholder={tplCfg.namePlaceholder}
          value={newMissionName}
          onChange={(e: React.ChangeEvent<HTMLInputElement>) =>
            setNewMissionName(e.target.value)
          }
        />
        <input
          type="text"
          className="w-full border-none bg-transparent outline-none [font-family:var(--font-heading)] font-medium text-[var(--text-md)] text-[var(--color-neutral-950)] leading-[1.1] placeholder:text-[var(--color-neutral-500)]"
          placeholder={tplCfg.descPlaceholder}
          value={newMissionDesc}
          onChange={(e: React.ChangeEvent<HTMLInputElement>) =>
            setNewMissionDesc(e.target.value)
          }
        />
      </div>

      <div className="flex gap-[var(--sp-2xs)] items-center">
        <Button
          ref={ownerBtnRef}
          variant="secondary"
          size="sm"
          leftIcon={UserCircle}
          onClick={onToggleOwnerPopover}
        >
          {selectedMissionOwners.length > 0
            ? (missionOwnerOptions.find(
                (o) => o.id === selectedMissionOwners[0],
              )?.label ?? "Responsável")
            : "Responsável"}
        </Button>

        <Button
          ref={missionPeriodBtnRef}
          variant="secondary"
          size="sm"
          leftIcon={Calendar}
          onClick={onTogglePeriodOpen}
        >
          {missionPeriod[0] && missionPeriod[1]
            ? `${String(missionPeriod[0].day).padStart(2, "0")}/${String(missionPeriod[0].month).padStart(2, "0")} - ${String(missionPeriod[1].day).padStart(2, "0")}/${String(missionPeriod[1].month).padStart(2, "0")}/${missionPeriod[1].year}`
            : "Período"}
        </Button>

        <FilterDropdown
          open={missionPeriodOpen}
          onClose={onClosePeriod}
          anchorRef={missionPeriodBtnRef}
        >
          <div className="flex flex-col p-[var(--sp-3xs)] max-h-[320px] overflow-y-auto">
            {presetPeriods.map((p) => {
              const isActive =
                missionPeriod[0]?.year === p.start.year &&
                missionPeriod[0]?.month === p.start.month &&
                missionPeriod[0]?.day === p.start.day &&
                missionPeriod[1]?.year === p.end.year &&
                missionPeriod[1]?.month === p.end.month &&
                missionPeriod[1]?.day === p.end.day;
              return (
                <button
                  key={p.id}
                  type="button"
                  className={`flex items-center gap-[var(--sp-2xs)] w-full p-[var(--sp-2xs)] border-none bg-transparent rounded-[var(--radius-2xs)] [font-family:var(--font-label)] font-medium text-[var(--text-xs)] text-[var(--color-neutral-950)] cursor-pointer whitespace-nowrap transition-colors duration-[120ms] ease-in-out text-left hover:bg-[var(--color-caramel-100)]${isActive ? " bg-[var(--color-caramel-50)]" : ""}`}
                  onClick={() => {
                    onSetMissionPeriod([p.start, p.end]);
                    onClosePeriod();
                  }}
                >
                  <Radio checked={isActive} readOnly />
                  <span>{p.label}</span>
                </button>
              );
            })}
          </div>
          <div className="border-t border-[var(--color-caramel-300)] p-[var(--sp-3xs)]">
            <button
              ref={missionPeriodCustomBtnRef}
              type="button"
              className={`flex items-center gap-[var(--sp-2xs)] w-full p-[var(--sp-2xs)] border-none bg-transparent rounded-[var(--radius-2xs)] [font-family:var(--font-label)] font-medium text-[var(--text-xs)] text-[var(--color-neutral-950)] cursor-pointer whitespace-nowrap transition-colors duration-[120ms] ease-in-out text-left hover:bg-[var(--color-caramel-100)]${missionPeriodCustom ? " bg-[var(--color-caramel-50)]" : ""}`}
              onClick={onTogglePeriodCustom}
            >
              <Plus size={14} />
              <span>Período personalizado</span>
              <CaretRight size={12} className="ml-auto text-[var(--color-neutral-400)] shrink-0" />
            </button>
          </div>
        </FilterDropdown>

        <FilterDropdown
          open={missionPeriodOpen && missionPeriodCustom}
          onClose={() => onSetMissionPeriod(missionPeriod)}
          anchorRef={missionPeriodCustomBtnRef}
          placement="right-start"
          noOverlay
        >
          <div className="p-[var(--sp-xs)]">
            <DatePicker
              mode="range"
              value={missionPeriod}
              onChange={(range: [CalendarDate | null, CalendarDate | null]) => {
                onSetMissionPeriod(range);
                if (range[0] && range[1]) {
                  onClosePeriod();
                }
              }}
            />
          </div>
        </FilterDropdown>

        <Button
          ref={moreBtnRef}
          variant="secondary"
          size="sm"
          leftIcon={DotsThree}
          aria-label="Mais opções"
          onClick={onToggleMorePopover}
        />
      </div>

      {/* Responsável dropdown */}
      <PopoverSelect
        mode="single"
        open={ownerPopoverOpen}
        onClose={onCloseOwnerPopover}
        anchorRef={ownerBtnRef}
        options={toSelectOptions(missionOwnerOptions)}
        value={selectedMissionOwners[0] ?? ""}
        onChange={(val) => {
          onOwnerChange(val);
          onCloseOwnerPopover();
        }}
        searchable
        searchPlaceholder="Buscar responsável..."
      />

      {/* More options — main menu */}
      <FilterDropdown
        open={morePopoverOpen}
        onClose={onCloseMorePopover}
        anchorRef={moreBtnRef}
      >
        <div className="flex flex-col p-[var(--sp-3xs)] max-h-[320px] overflow-y-auto">
          {MORE_MISSION_OPTIONS.map((opt) => {
            const isActive = moreSubPanel === opt.id;
            const count =
              opt.id === "team-support"
                ? selectedSupportTeam.length
                : opt.id === "organizers"
                  ? selectedTags.length
                  : 0;
            const displayLabel =
              opt.id === "organizers" && tplCfg.tagLabel
                ? tplCfg.tagLabel
                : opt.id === "visibility"
                  ? selectedVisibility === "private"
                    ? "Privado"
                    : "Público"
                  : opt.label;
            const Icon =
              opt.id === "visibility"
                ? selectedVisibility === "private"
                  ? EyeSlash
                  : Eye
                : opt.icon;
            return (
              <button
                key={opt.id}
                ref={(el) => {
                  moreItemRefs.current[opt.id] = el;
                }}
                type="button"
                className={`flex items-center gap-[var(--sp-2xs)] w-full p-[var(--sp-2xs)] border-none bg-transparent rounded-[var(--radius-2xs)] [font-family:var(--font-label)] font-medium text-[var(--text-xs)] text-[var(--color-neutral-950)] cursor-pointer whitespace-nowrap transition-[background-color] duration-[120ms] ease-in text-left hover:bg-[var(--color-caramel-100)]${isActive ? " bg-[var(--color-caramel-50)]" : ""}`}
                onClick={() => onSetMoreSubPanel(isActive ? null : opt.id)}
              >
                <Icon size={14} />
                <span>{displayLabel}</span>
                {count > 0 && (
                  <Badge color="neutral" size="sm">
                    {count}
                  </Badge>
                )}
                <CaretRight size={12} className="ml-auto text-[var(--color-neutral-400)] shrink-0" />
              </button>
            );
          })}
        </div>
      </FilterDropdown>

      {/* Sub-panel: Time de apoio */}
      <PopoverSelect
        mode="multiple"
        open={morePopoverOpen && moreSubPanel === "team-support"}
        onClose={() => onSetMoreSubPanel(null)}
        anchorRef={{
          current: moreItemRefs.current["team-support"] ?? null,
        }}
        placement="right-start"
        noOverlay
        options={toSelectOptions(missionOwnerOptions)}
        value={selectedSupportTeam}
        onChange={onSupportTeamChange}
        searchable
        searchPlaceholder="Buscar pessoa..."
      />

      {/* Sub-panel: Tags */}
      <PopoverSelect
        mode="multiple"
        open={morePopoverOpen && moreSubPanel === "organizers"}
        onClose={() => onSetMoreSubPanel(null)}
        anchorRef={{
          current: moreItemRefs.current["organizers"] ?? null,
        }}
        placement="right-start"
        noOverlay
        options={[...missionTagOptions, ...customTags]}
        value={selectedTags}
        onChange={onTagsChange}
        renderOptionPrefix={() => <Tag size={14} />}
        searchable
        searchPlaceholder="Buscar tag..."
        creatable
        createPlaceholder="Criar nova tag..."
        onCreateOption={(label) => {
          const created = createTag({ name: label });
          const newTag = { id: created.id, label: created.name };
          onAddCustomTag(newTag);
          return newTag;
        }}
      />

      {/* Sub-panel: Quem pode ver */}
      <FilterDropdown
        open={morePopoverOpen && moreSubPanel === "visibility"}
        onClose={() => onSetMoreSubPanel(null)}
        anchorRef={{
          current: moreItemRefs.current["visibility"] ?? null,
        }}
        placement="right-start"
        noOverlay
      >
        <div className="flex flex-col gap-[var(--sp-3xs)] p-[var(--sp-3xs)]">
          {VISIBILITY_OPTIONS.map((opt) => {
            const isSelected = selectedVisibility === opt.id;
            const Icon = opt.id === "public" ? Eye : EyeSlash;
            return (
              <button
                key={opt.id}
                type="button"
                className={`flex items-center gap-[var(--sp-2xs)] w-full px-[var(--sp-xs)] py-[var(--sp-2xs)] bg-transparent border-none rounded-[var(--radius-xs)] cursor-pointer text-left transition-[background-color] duration-[120ms] ease-in hover:bg-[var(--color-caramel-50)]${isSelected ? " bg-[var(--color-orange-50)]" : ""}`}
                onClick={() => {
                  onVisibilityChange(opt.id);
                  onSetMoreSubPanel(null);
                }}
              >
                <Icon size={16} className="shrink-0 text-[var(--color-neutral-400)]" />
                <div className="flex flex-col">
                  <span className="[font-family:var(--font-label)] font-medium text-[var(--text-sm)] text-[var(--color-neutral-950)]">
                    {opt.label}
                  </span>
                  <span className="[font-family:var(--font-body)] text-[10px] text-[var(--color-neutral-400)]">
                    {opt.description}
                  </span>
                </div>
              </button>
            );
          })}
        </div>
      </FilterDropdown>

      <CardDivider />

      <div className="flex flex-col gap-[var(--sp-xs)]">{itemsSection}</div>
    </div>
  );
}
