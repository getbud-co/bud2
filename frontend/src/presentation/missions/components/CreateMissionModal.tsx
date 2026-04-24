import { useState, useRef, useEffect } from "react";
import type { ReactNode } from "react";
import {
  FilterDropdown,
  Button,
  Card,
  CardBody,
  CardDivider,
  Modal,
  ModalHeader,
  ModalBody,
  ModalFooter,
  Radio,
  DatePicker,
  Breadcrumb,
  ChoiceBoxGroup,
  ChoiceBox,
  AiAssistant,
  AssistantButton,
  Badge,
  Alert,
} from "@getbud-co/buds";
import type { CalendarDate } from "@getbud-co/buds";
import {
  Plus,
  CaretRight,
  UserCircle,
  Calendar,
  DotsThree,
  Tag,
  FloppyDisk,
  ArrowRight,
} from "@phosphor-icons/react";
import { PopoverSelect, formatMultiLabel } from "@/components/PopoverSelect";
import { MissionItemsList } from "./MissionItemsList";
import { MissionItemInlineForm } from "./MissionItemInlineForm";
import type { Mission, Indicator, MissionMember } from "@/types";
import {
  ASSISTANT_MISSIONS,
  CREATE_STEPS,
  MISSION_TEMPLATES,
  MORE_MISSION_OPTIONS,
  MEASUREMENT_MODES,
  MANUAL_INDICATOR_TYPES,
  UNIT_OPTIONS,
} from "../consts";
import { getTemplateConfig } from "../utils/utils";
import type { MissionItemData } from "../types";
import { numVal, getMissionLabel } from "@/lib/tempStorage/missions";

interface TagOption {
  id: string;
  label: string;
}

interface OwnerOption {
  id: string;
  label: string;
  initials?: string;
}

interface PresetPeriod {
  id: string;
  label: string;
  start: CalendarDate;
  end: CalendarDate;
}

interface CreateMissionModalProps {
  open: boolean;
  onClose: () => void;
  editingMission?: Mission;
  onSave: (mission: Mission) => void;
  onDraft: (mission: Mission) => void;
  // Data needed by buildMissionFromForm:
  activeOrgId: string;
  missionsCount: number;
  resolveTagId: (id: string) => string;
  getTagById: (id: string) => {
    id: string;
    orgId: string;
    name: string;
    color: string;
    createdAt: string;
    updatedAt: string;
    deletedAt: string | null;
  } | null;
  missionOwnerOptions: OwnerOption[];
  missionTagOptions: TagOption[];
  visibilityOptions: TagOption[];
  presetPeriods: PresetPeriod[];
  createTag: (opts: { name: string }) => { id: string; name: string };
}

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

function countAllItems(items: MissionItemData[]): number {
  return items.reduce(
    (sum, item) => sum + 1 + countAllItems(item.children ?? []),
    0,
  );
}

function renderReviewItems(items: MissionItemData[], depth: number): ReactNode {
  return items.map((item) => {
    const missionSummary = getMissionSummary(item);
    const isSubMission = item.measurementMode === "mission";
    const modeLabel = MEASUREMENT_MODES.find(
      (m) => m.id === item.measurementMode,
    )?.label;
    const typeLabel = item.manualType
      ? MANUAL_INDICATOR_TYPES.find((t) => t.id === item.manualType)?.label
      : null;
    const badgeText = modeLabel
      ? `${modeLabel}${typeLabel ? ` · ${typeLabel}` : ""}`
      : null;

    return (
      <div
        key={item.id}
        className="flex flex-col gap-2"
        style={
          depth > 0
            ? { marginLeft: `calc(${depth} * var(--sp-lg))` }
            : undefined
        }
      >
        <div className="flex flex-col gap-1 px-4 py-3 bg-[var(--color-caramel-50)] rounded-[var(--radius-xs)]">
          <div className="flex items-center gap-2">
            <span className="font-[var(--font-label)] font-medium text-[var(--text-xs)] text-[var(--color-neutral-950)] leading-[1.15]">
              {item.name}
            </span>
            {badgeText && <Badge color="neutral">{badgeText}</Badge>}
          </div>
          {item.description && (
            <span className="font-[var(--font-body)] text-[var(--text-xs)] text-[var(--color-neutral-500)] leading-[1.3]">
              {item.description}
            </span>
          )}
          <div className="flex items-center gap-2 flex-wrap">
            {missionSummary && <Badge color="orange">{missionSummary}</Badge>}
            {item.period[0] && item.period[1] && (
              <span className="flex items-center gap-1 font-[var(--font-body)] text-[var(--text-xs)] text-[var(--color-neutral-500)] leading-[1.3]">
                <Calendar size={12} />
                {`${String(item.period[0].day).padStart(2, "0")}/${String(item.period[0].month).padStart(2, "0")}/${item.period[0].year} — ${String(item.period[1].day).padStart(2, "0")}/${String(item.period[1].month).padStart(2, "0")}/${item.period[1].year}`}
              </span>
            )}
          </div>
        </div>
        {isSubMission &&
          item.children &&
          item.children.length > 0 &&
          renderReviewItems(item.children, depth + 1)}
      </div>
    );
  });
}

export function CreateMissionModal({
  open,
  onClose,
  editingMission,
  onSave,
  onDraft,
  activeOrgId,
  missionsCount,
  resolveTagId,
  getTagById,
  missionOwnerOptions,
  missionTagOptions,
  visibilityOptions,
  presetPeriods,
  createTag,
}: CreateMissionModalProps) {
  const editingMissionId = editingMission?.id ?? null;

  // ——— Internal state ———
  const [createStep, setCreateStep] = useState(0);
  const [selectedTemplate, setSelectedTemplate] = useState<string | undefined>(
    undefined,
  );
  const [newMissionName, setNewMissionName] = useState("");
  const [newMissionDesc, setNewMissionDesc] = useState("");
  const [newMissionItems, setNewMissionItems] = useState<MissionItemData[]>([]);
  const [editingItem, setEditingItem] = useState<MissionItemData | null>(null);
  const [isEditingExisting, setIsEditingExisting] = useState(false);
  const [editingParentId, setEditingParentId] = useState<string | null>(null);
  const [expandedSubMissions, setExpandedSubMissions] = useState<Set<string>>(
    new Set(),
  );
  const [selectedMissionOwners, setSelectedMissionOwners] = useState<string[]>(
    [],
  );
  const [missionPeriod, setMissionPeriod] = useState<
    [CalendarDate | null, CalendarDate | null]
  >([null, null]);
  const [selectedSupportTeam, setSelectedSupportTeam] = useState<string[]>([]);
  const [selectedTags, setSelectedTags] = useState<string[]>([]);
  const [selectedVisibility, setSelectedVisibility] = useState("org");
  const [customTags, setCustomTags] = useState<TagOption[]>([]);

  // ——— CreateMissionModal-local state ———
  const [showCreateAssistant, setShowCreateAssistant] = useState(false);
  const [createAssistantMissions, setCreateAssistantMissions] = useState<
    string[]
  >([]);
  const [ownerPopoverOpen, setOwnerPopoverOpen] = useState(false);
  const [morePopoverOpen, setMorePopoverOpen] = useState(false);
  const [moreSubPanel, setMoreSubPanel] = useState<string | null>(null);
  const [missionPeriodOpen, setMissionPeriodOpen] = useState(false);
  const [missionPeriodCustom, setMissionPeriodCustom] = useState(false);

  const ownerBtnRef = useRef<HTMLButtonElement>(null);
  const missionPeriodBtnRef = useRef<HTMLButtonElement>(null);
  const missionPeriodCustomBtnRef = useRef<HTMLButtonElement>(null);
  const moreBtnRef = useRef<HTMLButtonElement>(null);
  const moreItemRefs = useRef<Record<string, HTMLButtonElement | null>>({});

  // ——— Helper: convert Mission → MissionItemData[] for editing ———
  function missionToItems(m: Mission): MissionItemData[] {
    const items: MissionItemData[] = (m.indicators ?? []).map((indicator) => {
      const missionValue =
        indicator.missionType === "reach" || indicator.missionType === "between"
          ? String(numVal(indicator.targetValue))
          : "";
      const missionValueMin =
        indicator.lowThreshold != null
          ? String(numVal(indicator.lowThreshold))
          : "";
      const missionValueMax =
        indicator.highThreshold != null
          ? String(numVal(indicator.highThreshold))
          : "";
      return {
        id: indicator.id,
        name: indicator.title,
        description: getMissionLabel(indicator),
        measurementMode: "manual",
        manualType: indicator.missionType,
        surveyId: null,
        period: [null, null] as [CalendarDate | null, CalendarDate | null],
        missionValue,
        missionValueMin,
        missionValueMax,
        missionUnit: "",
      };
    });

    (m.children ?? []).forEach((child) => {
      items.push({
        id: child.id,
        name: child.title,
        description: "",
        measurementMode: "mission",
        manualType: null,
        period: [null, null],
        missionValue: "",
        missionValueMin: "",
        missionValueMax: "",
        missionUnit: "",
        children: missionToItems(child),
      });
    });

    return items;
  }

  // ——— Initialize state from editingMission when modal opens ———
  useEffect(() => {
    if (!open) return;
    if (editingMission) {
      setCreateStep(1);
      setSelectedTemplate("scratch");
      setNewMissionName(editingMission.title);
      setNewMissionDesc(editingMission.description ?? "");
      setNewMissionItems(missionToItems(editingMission));
      setSelectedMissionOwners([]);
      setMissionPeriod([null, null]);
      setSelectedSupportTeam(
        (editingMission.members ?? [])
          .filter((m) => m.role === "supporter")
          .map((m) => m.userId),
      );
      setSelectedTags((editingMission.tags ?? []).map((tag) => tag.id));
      setSelectedVisibility("org");
      setExpandedSubMissions(
        new Set(
          missionToItems(editingMission)
            .filter((i) => i.measurementMode === "mission")
            .map((i) => i.id),
        ),
      );
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [open, editingMission]);

  // ——— Reset form ———
  function resetCreateForm() {
    setCreateStep(0);
    setSelectedTemplate(undefined);
    setNewMissionName("");
    setNewMissionDesc("");
    setNewMissionItems([]);
    setEditingItem(null);
    setIsEditingExisting(false);
    setEditingParentId(null);
    setExpandedSubMissions(new Set());
    setSelectedMissionOwners([]);
    setMissionPeriod([null, null]);
    setSelectedSupportTeam([]);
    setSelectedTags([]);
    setSelectedVisibility("org");
    setCustomTags([]);
  }

  // ——— Pure helpers ———
  function toIsoDate(date: CalendarDate | null): string | null {
    if (!date) return null;
    return `${date.year}-${String(date.month).padStart(2, "0")}-${String(date.day).padStart(2, "0")}`;
  }

  function unitFromValue(unit: string): Indicator["unit"] {
    if (unit === "%") return "percent";
    if (unit === "R$" || unit === "US$") return "currency";
    if (!unit || unit === "un") return "count";
    return "custom";
  }

  function ownerFromSelection() {
    const selected = missionOwnerOptions.find(
      (option) => option.id === selectedMissionOwners[0],
    );
    const fallback = missionOwnerOptions[0] ?? {
      id: "local-user",
      label: "Usuário local",
      initials: "UL",
    };
    const owner = selected ?? fallback;
    return {
      id: owner.id,
      fullName: owner.label,
      initials: owner.initials ?? null,
    };
  }

  function materializeMissionItems(
    rootMissionId: string,
    items: MissionItemData[],
    ownerId: string,
  ): { indicators: Indicator[]; children: Mission[] } {
    const indicators: Indicator[] = [];
    const children: Mission[] = [];
    const now = new Date().toISOString();

    for (const item of items) {
      if (item.measurementMode === "mission") {
        const childMissionId =
          item.id ||
          `mission-${Date.now()}-${Math.random().toString(36).slice(2, 8)}`;
        const childTree = materializeMissionItems(
          childMissionId,
          item.children ?? [],
          ownerId,
        );
        const childProgress =
          childTree.indicators.length > 0
            ? Math.round(
                childTree.indicators.reduce(
                  (acc, indicator) => acc + indicator.progress,
                  0,
                ) / childTree.indicators.length,
              )
            : 0;

        children.push({
          id: childMissionId,
          orgId: activeOrgId,
          cycleId: null,
          parentId: rootMissionId,
          depth: 1,
          path: [rootMissionId, childMissionId],
          title: item.name || "Submissão sem título",
          description: item.description || null,
          ownerId,
          teamId: null,
          status: "active",
          visibility: "public",
          progress: childProgress,
          kanbanStatus: "doing",
          sortOrder: children.length,
          dueDate: toIsoDate(item.period[1]),
          completedAt: null,
          createdAt: now,
          updatedAt: now,
          deletedAt: null,
          indicators: childTree.indicators,
          children: childTree.children,
        });
        continue;
      }

      const missionType =
        (item.manualType as Indicator["missionType"] | null) ??
        (item.measurementMode === "survey" ? "survey" : "reach");
      const targetValue =
        item.missionValue ||
        (missionType === "between" ? item.missionValueMax || null : null);
      const currentValue = "0";

      indicators.push({
        id:
          item.id ||
          `indicator-${Date.now()}-${Math.random().toString(36).slice(2, 8)}`,
        orgId: activeOrgId,
        missionId: rootMissionId,
        parentKrId: null,
        title: item.name || "Indicador sem título",
        description: item.description || null,
        ownerId,
        measurementMode:
          (item.measurementMode as Indicator["measurementMode"] | null) ??
          "manual",
        linkedMissionId: null,
        linkedSurveyId: null,
        externalSource: null,
        externalConfig: null,
        missionType,
        targetValue,
        currentValue,
        startValue: "0",
        lowThreshold: item.missionValueMin || null,
        highThreshold: item.missionValueMax || null,
        unit: unitFromValue(item.missionUnit),
        unitLabel: item.missionUnit || null,
        expectedValue: null,
        status: "attention",
        progress: 0,
        periodLabel: null,
        periodStart: toIsoDate(item.period[0]),
        periodEnd: toIsoDate(item.period[1]),
        sortOrder: indicators.length,
        createdAt: now,
        updatedAt: now,
        deletedAt: null,
      });
    }

    return { indicators, children };
  }

  function buildMissionFromForm(existing?: Mission): Mission {
    const now = new Date().toISOString();
    const missionId = existing?.id ?? `mission-${Date.now()}`;
    const owner = ownerFromSelection();
    const tree = materializeMissionItems(missionId, newMissionItems, owner.id);
    const selectedMissionTags = selectedTags.map((tagId) => {
      const resolvedTagId = resolveTagId(tagId);
      const canonical = getTagById(resolvedTagId);
      if (canonical) return canonical;

      return {
        id: resolvedTagId,
        orgId: existing?.orgId ?? activeOrgId,
        name:
          missionTagOptions.find((option) => option.id === tagId)?.label ??
          resolvedTagId,
        color: "neutral",
        createdAt: now,
        updatedAt: now,
        deletedAt: null,
      };
    });
    const progress =
      tree.indicators.length > 0
        ? Math.round(
            tree.indicators.reduce(
              (acc, indicator) => acc + indicator.progress,
              0,
            ) / tree.indicators.length,
          )
        : 0;

    return {
      id: missionId,
      orgId: existing?.orgId ?? activeOrgId,
      cycleId: existing?.cycleId ?? null,
      parentId: existing?.parentId ?? null,
      depth: existing?.depth ?? 0,
      path: existing?.path ?? [missionId],
      title: newMissionName || existing?.title || "Missão sem título",
      description: newMissionDesc || null,
      ownerId: owner.id,
      teamId: existing?.teamId ?? null,
      status: existing?.status ?? "active",
      visibility:
        selectedVisibility === "private"
          ? "private"
          : selectedVisibility === "team"
            ? "team_only"
            : "public",
      progress,
      kanbanStatus: existing?.kanbanStatus ?? "doing",
      sortOrder: existing?.sortOrder ?? missionsCount,
      dueDate: toIsoDate(missionPeriod[1]),
      completedAt: existing?.completedAt ?? null,
      createdAt: existing?.createdAt ?? now,
      updatedAt: now,
      deletedAt: existing?.deletedAt ?? null,
      owner,
      indicators: tree.indicators,
      children: tree.children,
      tasks: existing?.tasks ?? [],
      tags: selectedMissionTags,
      members: selectedSupportTeam.map((userId): MissionMember => {
        const opt = missionOwnerOptions.find((o) => o.id === userId);
        return {
          missionId,
          userId,
          role: "supporter",
          addedAt: now,
          addedBy: owner.id,
          user: {
            id: userId,
            fullName: opt?.label ?? userId,
            initials: opt?.initials ?? null,
            jobTitle: null,
            avatarUrl: null,
          },
        };
      }),
    };
  }

  // ——— Item tree helpers ———
  function addChildToParent(
    items: MissionItemData[],
    parentId: string,
    child: MissionItemData,
  ): MissionItemData[] {
    return items.map((item) => {
      if (item.id === parentId) {
        return { ...item, children: [...(item.children ?? []), child] };
      }
      if (item.children?.length) {
        return {
          ...item,
          children: addChildToParent(item.children, parentId, child),
        };
      }
      return item;
    });
  }

  function replaceItemInTree(
    items: MissionItemData[],
    itemId: string,
    newItem: MissionItemData,
  ): MissionItemData[] {
    return items.map((item) => {
      if (item.id === itemId) return newItem;
      if (item.children?.length) {
        return {
          ...item,
          children: replaceItemInTree(item.children, itemId, newItem),
        };
      }
      return item;
    });
  }

  function handleSaveItem() {
    if (!editingItem || !editingItem.name.trim()) return;
    if (isEditingExisting) {
      setNewMissionItems((prev) =>
        replaceItemInTree(prev, editingItem.id, editingItem),
      );
    } else if (editingParentId) {
      setNewMissionItems((prev) =>
        addChildToParent(prev, editingParentId, editingItem),
      );
      setExpandedSubMissions((prev) => new Set(prev).add(editingParentId));
    } else {
      setNewMissionItems((prev) => [...prev, editingItem]);
    }
    if (
      editingItem.measurementMode === "mission" ||
      editingItem.measurementMode === "manual"
    ) {
      setExpandedSubMissions((prev) => new Set(prev).add(editingItem.id));
    }
    setEditingItem(null);
    setIsEditingExisting(false);
    setEditingParentId(null);
  }

  // ——— Inline form renderer ———
  function renderInlineForm(): ReactNode {
    if (!editingItem) return null;
    return (
      <MissionItemInlineForm
        editingItem={editingItem}
        setEditingItem={setEditingItem}
        isEditingExisting={isEditingExisting}
        onSave={handleSaveItem}
        onCancel={() => {
          setEditingItem(null);
          setEditingParentId(null);
          setIsEditingExisting(false);
        }}
        selectedTemplate={selectedTemplate}
        drawerEditing={false}
        missionOwnerOptions={missionOwnerOptions}
        missionTagOptions={missionTagOptions}
        visibilityOptions={visibilityOptions}
        presetPeriods={presetPeriods}
        createTag={createTag}
      />
    );
  }

  return (
    <Modal
      open={open}
      onClose={onClose}
      size="lg"
      sidePanel={
        showCreateAssistant ? (
          <AiAssistant
            onClose={() => setShowCreateAssistant(false)}
            allowUpload
            missions={ASSISTANT_MISSIONS}
            selectedMissions={createAssistantMissions}
            onMissionsChange={setCreateAssistantMissions}
            onMessage={async () =>
              "Desculpe, ainda estou em desenvolvimento. Em breve poderei ajudá-lo!"
            }
          />
        ) : null
      }
    >
      <ModalHeader
        title={editingMissionId ? "Editar missão" : "Criar missão"}
        onClose={() => {
          onClose();
          resetCreateForm();
        }}
      >
        <AssistantButton
          active={showCreateAssistant}
          onClick={() => setShowCreateAssistant((v) => !v)}
        />
      </ModalHeader>

      <Breadcrumb
        items={CREATE_STEPS.map((step, i) => ({
          ...step,
          onClick: i < createStep ? () => setCreateStep(i) : undefined,
        }))}
        current={createStep}
      />

      <ModalBody>
        {createStep === 0 && (
          <div className="flex flex-col gap-[var(--sp-sm)]">
            <p className="m-0 font-[var(--font-heading)] font-semibold text-[var(--text-sm)] text-[var(--color-neutral-950)] leading-[1.25]">
              Escolha o seu template de missão
            </p>
            <ChoiceBoxGroup
              value={selectedTemplate}
              onChange={(v: string | undefined) => setSelectedTemplate(v)}
            >
              {MISSION_TEMPLATES.map((t) => (
                <ChoiceBox
                  key={t.value}
                  value={t.value}
                  title={t.title}
                  description={t.description}
                />
              ))}
            </ChoiceBoxGroup>
          </div>
        )}

        {createStep === 1 &&
          (() => {
            const tplCfg = getTemplateConfig(selectedTemplate);
            return (
              <div className="flex flex-col gap-[var(--sp-sm)]">
                <p className="m-0 font-[var(--font-heading)] font-semibold text-[var(--text-sm)] text-[var(--color-neutral-950)] leading-[1.25]">
                  {editingMissionId ? "Editar missão" : tplCfg.stepTitle}
                </p>

                <div className="flex flex-col gap-[var(--sp-xs)]">
                  <input
                    type="text"
                    className="w-full border-none bg-transparent outline-none font-[var(--font-heading)] font-medium text-[var(--text-2xl)] text-[var(--color-neutral-950)] leading-[1.1] tracking-[-0.5px] placeholder:text-[var(--color-neutral-500)]"
                    placeholder={tplCfg.namePlaceholder}
                    value={newMissionName}
                    onChange={(e: React.ChangeEvent<HTMLInputElement>) =>
                      setNewMissionName(e.target.value)
                    }
                  />
                  <input
                    type="text"
                    className="w-full border-none bg-transparent outline-none font-[var(--font-heading)] font-medium text-[var(--text-md)] text-[var(--color-neutral-950)] leading-[1.1] placeholder:text-[var(--color-neutral-500)]"
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
                    onClick={() => setOwnerPopoverOpen((v) => !v)}
                  >
                    {selectedMissionOwners.length > 0
                      ? formatMultiLabel(
                          selectedMissionOwners,
                          missionOwnerOptions,
                          "Responsável",
                        )
                      : "Responsável"}
                  </Button>

                  <Button
                    ref={missionPeriodBtnRef}
                    variant="secondary"
                    size="sm"
                    leftIcon={Calendar}
                    onClick={() => {
                      setMissionPeriodOpen((v) => !v);
                      setMissionPeriodCustom(false);
                    }}
                  >
                    {missionPeriod[0] && missionPeriod[1]
                      ? `${String(missionPeriod[0].day).padStart(2, "0")}/${String(missionPeriod[0].month).padStart(2, "0")} - ${String(missionPeriod[1].day).padStart(2, "0")}/${String(missionPeriod[1].month).padStart(2, "0")}/${missionPeriod[1].year}`
                      : "Período"}
                  </Button>
                  <FilterDropdown
                    open={missionPeriodOpen}
                    onClose={() => {
                      setMissionPeriodOpen(false);
                      setMissionPeriodCustom(false);
                    }}
                    anchorRef={missionPeriodBtnRef}
                  >
                    <div className="flex flex-col p-1 max-h-80 overflow-y-auto">
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
                            className={`flex items-center gap-2 w-full px-2 py-2 border-none bg-transparent rounded-[var(--radius-2xs)] font-[var(--font-label)] font-medium text-[var(--text-xs)] text-[var(--color-neutral-950)] cursor-pointer whitespace-nowrap transition-colors duration-[120ms] text-left hover:bg-[var(--color-caramel-100)] ${isActive ? "bg-[var(--color-caramel-50)]" : ""}`}
                            onClick={() => {
                              setMissionPeriod([p.start, p.end]);
                              setMissionPeriodOpen(false);
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
                        ref={missionPeriodCustomBtnRef}
                        type="button"
                        className={`flex items-center gap-2 w-full px-2 py-2 border-none bg-transparent rounded-[var(--radius-2xs)] font-[var(--font-label)] font-medium text-[var(--text-xs)] text-[var(--color-neutral-950)] cursor-pointer whitespace-nowrap transition-colors duration-[120ms] text-left hover:bg-[var(--color-caramel-100)] ${missionPeriodCustom ? "bg-[var(--color-caramel-50)]" : ""}`}
                        onClick={() => setMissionPeriodCustom((v) => !v)}
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
                  <FilterDropdown
                    open={missionPeriodOpen && missionPeriodCustom}
                    onClose={() => setMissionPeriodCustom(false)}
                    anchorRef={missionPeriodCustomBtnRef}
                    placement="right-start"
                    noOverlay
                  >
                    <div className="p-3">
                      <DatePicker
                        mode="range"
                        value={missionPeriod}
                        onChange={(
                          range: [CalendarDate | null, CalendarDate | null],
                        ) => {
                          setMissionPeriod(range);
                          if (range[0] && range[1]) {
                            setMissionPeriodOpen(false);
                            setMissionPeriodCustom(false);
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
                    onClick={() => {
                      setMoreSubPanel(null);
                      setMorePopoverOpen((v) => !v);
                    }}
                  />
                </div>

                {/* Responsável dropdown */}
                <PopoverSelect
                  mode="multiple"
                  open={ownerPopoverOpen}
                  onClose={() => setOwnerPopoverOpen(false)}
                  anchorRef={ownerBtnRef}
                  options={missionOwnerOptions}
                  value={selectedMissionOwners}
                  onChange={setSelectedMissionOwners}
                  searchable
                  searchPlaceholder="Buscar responsável..."
                />

                {/* More options — main menu */}
                <FilterDropdown
                  open={morePopoverOpen}
                  onClose={() => {
                    setMorePopoverOpen(false);
                    setMoreSubPanel(null);
                  }}
                  anchorRef={moreBtnRef}
                >
                  <div className="flex flex-col p-1 max-h-80 overflow-y-auto">
                    {MORE_MISSION_OPTIONS.map((opt) => {
                      const Icon = opt.icon;
                      const isActive = moreSubPanel === opt.id;
                      const count =
                        opt.id === "team-support"
                          ? selectedSupportTeam.length
                          : opt.id === "organizers"
                            ? selectedTags.length
                            : 0;
                      return (
                        <button
                          key={opt.id}
                          ref={(el) => {
                            moreItemRefs.current[opt.id] = el;
                          }}
                          type="button"
                          className={`flex items-center gap-2 w-full px-2 py-2 border-none bg-transparent rounded-[var(--radius-2xs)] font-[var(--font-label)] font-medium text-[var(--text-xs)] text-[var(--color-neutral-950)] cursor-pointer whitespace-nowrap transition-colors duration-[120ms] text-left hover:bg-[var(--color-caramel-100)] ${isActive ? "bg-[var(--color-caramel-50)]" : ""}`}
                          onClick={() =>
                            setMoreSubPanel(isActive ? null : opt.id)
                          }
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

                {/* Sub-panel: Time de apoio */}
                <PopoverSelect
                  mode="multiple"
                  open={morePopoverOpen && moreSubPanel === "team-support"}
                  onClose={() => setMoreSubPanel(null)}
                  anchorRef={{
                    current: moreItemRefs.current["team-support"] ?? null,
                  }}
                  placement="right-start"
                  noOverlay
                  options={missionOwnerOptions}
                  value={selectedSupportTeam}
                  onChange={setSelectedSupportTeam}
                  searchable
                  searchPlaceholder="Buscar pessoa..."
                />

                {/* Sub-panel: Tags */}
                <PopoverSelect
                  mode="multiple"
                  open={morePopoverOpen && moreSubPanel === "organizers"}
                  onClose={() => setMoreSubPanel(null)}
                  anchorRef={{
                    current: moreItemRefs.current["organizers"] ?? null,
                  }}
                  placement="right-start"
                  noOverlay
                  options={[...missionTagOptions, ...customTags]}
                  value={selectedTags}
                  onChange={setSelectedTags}
                  renderOptionPrefix={() => <Tag size={14} />}
                  searchable
                  searchPlaceholder="Buscar tag..."
                  creatable
                  createPlaceholder="Criar nova tag..."
                  onCreateOption={(label) => {
                    const created = createTag({ name: label });
                    const newTag = { id: created.id, label: created.name };
                    setCustomTags((prev) => [...prev, newTag]);
                    return newTag;
                  }}
                />

                {/* Sub-panel: Quem pode ver */}
                <PopoverSelect
                  mode="single"
                  open={morePopoverOpen && moreSubPanel === "visibility"}
                  onClose={() => setMoreSubPanel(null)}
                  anchorRef={{
                    current: moreItemRefs.current["visibility"] ?? null,
                  }}
                  placement="right-start"
                  noOverlay
                  options={visibilityOptions}
                  value={selectedVisibility}
                  onChange={setSelectedVisibility}
                  searchable
                  searchPlaceholder="Buscar visibilidade..."
                />

                <CardDivider />

                <div className="flex flex-col gap-[var(--sp-xs)]">
                  <MissionItemsList
                    items={newMissionItems}
                    parentId={null}
                    isEditingExisting={isEditingExisting}
                    editingItem={editingItem}
                    editingParentId={editingParentId}
                    expandedSubMissions={expandedSubMissions}
                    setExpandedSubMissions={setExpandedSubMissions}
                    setNewMissionItems={setNewMissionItems}
                    setEditingItem={setEditingItem}
                    setIsEditingExisting={setIsEditingExisting}
                    setEditingParentId={setEditingParentId}
                    selectedTemplate={selectedTemplate}
                    renderInlineForm={renderInlineForm}
                  />
                </div>
              </div>
            );
          })()}
        {createStep === 2 && (
          <div className="flex flex-col gap-[var(--sp-sm)]">
            <p className="m-0 font-[var(--font-heading)] font-semibold text-[var(--text-sm)] text-[var(--color-neutral-950)] leading-[1.25]">
              Revisão da missão
            </p>

            {/* Mission header info */}
            <Card padding="sm">
              <CardBody>
                <div className="flex flex-col gap-[var(--sp-xs)]">
                  <div className="flex items-baseline gap-[var(--sp-sm)]">
                    <span className="flex-shrink-0 w-[120px] font-[var(--font-label)] font-medium text-[var(--text-xs)] text-[var(--color-neutral-500)] leading-[1.4]">
                      Template
                    </span>
                    <span className="font-[var(--font-body)] text-[var(--text-xs)] text-[var(--color-neutral-950)] leading-[1.4]">
                      {MISSION_TEMPLATES.find(
                        (t) => t.value === selectedTemplate,
                      )?.title ?? "—"}
                    </span>
                  </div>
                  <div className="flex items-baseline gap-[var(--sp-sm)]">
                    <span className="flex-shrink-0 w-[120px] font-[var(--font-label)] font-medium text-[var(--text-xs)] text-[var(--color-neutral-500)] leading-[1.4]">
                      Nome
                    </span>
                    <span className="font-[var(--font-body)] text-[var(--text-xs)] text-[var(--color-neutral-950)] leading-[1.4]">
                      {newMissionName || "—"}
                    </span>
                  </div>
                  {newMissionDesc && (
                    <div className="flex items-baseline gap-[var(--sp-sm)]">
                      <span className="flex-shrink-0 w-[120px] font-[var(--font-label)] font-medium text-[var(--text-xs)] text-[var(--color-neutral-500)] leading-[1.4]">
                        Descrição
                      </span>
                      <span className="font-[var(--font-body)] text-[var(--text-xs)] text-[var(--color-neutral-950)] leading-[1.4]">
                        {newMissionDesc}
                      </span>
                    </div>
                  )}
                  <div className="flex items-baseline gap-[var(--sp-sm)]">
                    <span className="flex-shrink-0 w-[120px] font-[var(--font-label)] font-medium text-[var(--text-xs)] text-[var(--color-neutral-500)] leading-[1.4]">
                      Responsável
                    </span>
                    <span className="font-[var(--font-body)] text-[var(--text-xs)] text-[var(--color-neutral-950)] leading-[1.4]">
                      {selectedMissionOwners.length > 0
                        ? selectedMissionOwners
                            .map(
                              (id) =>
                                missionOwnerOptions.find((o) => o.id === id)
                                  ?.label ?? id,
                            )
                            .join(", ")
                        : "Nenhum definido"}
                    </span>
                  </div>
                  <div className="flex items-baseline gap-[var(--sp-sm)]">
                    <span className="flex-shrink-0 w-[120px] font-[var(--font-label)] font-medium text-[var(--text-xs)] text-[var(--color-neutral-500)] leading-[1.4]">
                      Período
                    </span>
                    <span className="font-[var(--font-body)] text-[var(--text-xs)] text-[var(--color-neutral-950)] leading-[1.4]">
                      {missionPeriod[0] && missionPeriod[1]
                        ? `${String(missionPeriod[0].day).padStart(2, "0")}/${String(missionPeriod[0].month).padStart(2, "0")}/${missionPeriod[0].year} — ${String(missionPeriod[1].day).padStart(2, "0")}/${String(missionPeriod[1].month).padStart(2, "0")}/${missionPeriod[1].year}`
                        : "Não definido"}
                    </span>
                  </div>
                  {selectedSupportTeam.length > 0 && (
                    <div className="flex items-baseline gap-[var(--sp-sm)]">
                      <span className="flex-shrink-0 w-[120px] font-[var(--font-label)] font-medium text-[var(--text-xs)] text-[var(--color-neutral-500)] leading-[1.4]">
                        Time de apoio
                      </span>
                      <span className="font-[var(--font-body)] text-[var(--text-xs)] text-[var(--color-neutral-950)] leading-[1.4]">
                        {selectedSupportTeam
                          .map(
                            (id) =>
                              missionOwnerOptions.find((o) => o.id === id)
                                ?.label ?? id,
                          )
                          .join(", ")}
                      </span>
                    </div>
                  )}
                  {selectedTags.length > 0 && (
                    <div className="flex items-baseline gap-[var(--sp-sm)]">
                      <span className="flex-shrink-0 w-[120px] font-[var(--font-label)] font-medium text-[var(--text-xs)] text-[var(--color-neutral-500)] leading-[1.4]">
                        Tags
                      </span>
                      <div className="flex flex-wrap gap-[var(--sp-3xs)]">
                        {selectedTags.map((id) => {
                          const label =
                            missionTagOptions.find((t) => t.id === id)?.label ??
                            customTags.find((t) => t.id === id)?.label ??
                            id;
                          return (
                            <Badge key={id} color="neutral">
                              {label}
                            </Badge>
                          );
                        })}
                      </div>
                    </div>
                  )}
                  <div className="flex items-baseline gap-[var(--sp-sm)]">
                    <span className="flex-shrink-0 w-[120px] font-[var(--font-label)] font-medium text-[var(--text-xs)] text-[var(--color-neutral-500)] leading-[1.4]">
                      Visibilidade
                    </span>
                    <span className="font-[var(--font-body)] text-[var(--text-xs)] text-[var(--color-neutral-950)] leading-[1.4]">
                      {visibilityOptions.find(
                        (o) => o.id === selectedVisibility,
                      )?.label ?? selectedVisibility}
                    </span>
                  </div>
                </div>
              </CardBody>
            </Card>

            {/* Mission items tree */}
            {newMissionItems.length > 0 && (
              <Card padding="sm">
                <CardBody>
                  <span className="font-[var(--font-heading)] font-semibold text-[var(--text-sm)] text-[var(--color-neutral-950)] leading-[1.15]">
                    Itens da missão ({countAllItems(newMissionItems)})
                  </span>
                </CardBody>
                <CardDivider />
                <CardBody>
                  <div className="flex flex-col gap-[var(--sp-2xs)]">
                    {renderReviewItems(newMissionItems, 0)}
                  </div>
                </CardBody>
              </Card>
            )}

            {newMissionItems.length === 0 && (
              <Alert variant="warning" title="Nenhum item adicionado">
                Nenhum item adicionado à missão.
              </Alert>
            )}
          </div>
        )}
      </ModalBody>

      <ModalFooter align="between">
        <Button
          variant="tertiary"
          size="md"
          onClick={() => {
            if (createStep > (editingMissionId ? 1 : 0)) {
              setCreateStep((s) => s - 1);
            } else {
              onClose();
              resetCreateForm();
            }
          }}
        >
          {createStep > (editingMissionId ? 1 : 0) ? "Voltar" : "Cancelar"}
        </Button>
        <div className="flex gap-[var(--sp-2xs)] items-center">
          {!editingMissionId && (
            <Button
              variant="secondary"
              size="md"
              leftIcon={FloppyDisk}
              onClick={() => {
                onDraft(buildMissionFromForm());
                resetCreateForm();
                onClose();
              }}
            >
              Salvar rascunho
            </Button>
          )}
          <Button
            variant="primary"
            size="md"
            rightIcon={createStep === 2 ? undefined : ArrowRight}
            disabled={createStep === 0 ? !selectedTemplate : false}
            onClick={() => {
              if (createStep < 2) {
                setCreateStep((s) => s + 1);
              } else if (editingMission) {
                onSave(buildMissionFromForm(editingMission));
                resetCreateForm();
                onClose();
              } else {
                onSave(buildMissionFromForm());
                resetCreateForm();
                onClose();
              }
            }}
          >
            {createStep === 2
              ? editingMissionId
                ? "Salvar alterações"
                : "Criar missão"
              : "Próximo"}
          </Button>
        </div>
      </ModalFooter>
    </Modal>
  );
}
