import { useState, useRef, useEffect, type ReactNode } from "react";
import {
  Modal,
  ModalHeader,
  ModalBody,
  ModalFooter,
  Breadcrumb,
  Button,
  FilterDropdown,
  Badge,
  Input,
  Select,
  DatePicker,
  Radio,
  Checkbox,
  AiAssistant as AiAssistantBase,
  AssistantButton,
  toast,
} from "@getbud-co/buds";

// AiAssistant typed with extra props used by this modal
// eslint-disable-next-line @typescript-eslint/no-explicit-any
const AiAssistant = AiAssistantBase as any;
import type { CalendarDate } from "@getbud-co/buds";
import {
  ArrowRight,
  FloppyDisk,
  UserCircle,
  Calendar,
  Ruler,
  DotsThree,
  PlusSquare,
  Tag,
  Eye,
  EyeSlash,
  CaretRight,
  CaretUp,
  CaretDown,
  Plus,
  MagnifyingGlass,
  ChartBar,
  X,
  PencilSimple,
} from "@phosphor-icons/react";
import { PopoverSelect } from "@/components/PopoverSelect";
import type { PopoverSelectOption } from "@/components/PopoverSelect";
import type { Mission, Indicator, MissionTask, MissionMember } from "@/types";
import { numVal, getMissionLabel } from "@/lib/tempStorage/missions";
import {
  CREATE_STEPS,
  MISSION_TEMPLATES,
  MORE_MISSION_OPTIONS,
  EXAMPLE_LIBRARY,
  ASSISTANT_MISSIONS,
  MEASUREMENT_MODES,
  MANUAL_INDICATOR_TYPES,
  UNIT_OPTIONS,
} from "../../consts";
import {
  getTemplateConfig,
  generateItemId,
  splitFullName,
  parseKeyResultGoal,
} from "../../utils";
import type { MissionItemData } from "./types";
import { Step0 } from "./components/Step0";
import { Step1 } from "./components/Step1";
import { Step2 } from "./components/Step2";

/* ——— Visibility options (module-level constant) ——— */
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

/* ——— Props ——— */
export interface CreateMissionModalProps {
  open: boolean;
  editingMission: Mission | null;
  onClose: () => void;
  onSubmit: (mission: Mission) => void;
  onDraft: (mission: Mission) => void;
  missionOwnerOptions: {
    id: string;
    label: string;
    initials?: string | null;
  }[];
  missionTagOptions: { id: string; label: string }[];
  presetPeriods: {
    id: string;
    label: string;
    start: CalendarDate;
    end: CalendarDate;
  }[];
  currentUserOption: {
    id: string;
    label: string;
    initials?: string | null;
  } | null;
  activeOrgId: string | null;
  teamOptions: { id: string; label: string; color?: string }[];
  missionsCount: number;
  createTag: (tag: { name: string }) => { id: string; name: string };
  resolveTagId: (id: string) => string;
  getTagById: (id: string) => unknown;
}

function toSelectOptions(
  opts: { id: string; label: string; initials?: string | null }[],
): PopoverSelectOption[] {
  return opts.map(({ id, label, initials }) => ({
    id,
    label,
    initials: initials ?? undefined,
  }));
}

export function CreateMissionModal({
  open,
  editingMission,
  onClose,
  onSubmit,
  onDraft,
  missionOwnerOptions,
  missionTagOptions,
  presetPeriods,
  currentUserOption,
  activeOrgId,
  teamOptions,
  missionsCount,
  createTag,
  resolveTagId,
  getTagById,
}: CreateMissionModalProps) {
  /* ——— Step / template ——— */
  const [createStep, setCreateStep] = useState(0);
  const [selectedTemplate, setSelectedTemplate] = useState<string | undefined>(
    undefined,
  );
  const [showCreateAssistant, setShowCreateAssistant] = useState(false);
  const [createAssistantMissions, setCreateAssistantMissions] = useState<
    string[]
  >([]);

  /* ——— Mission fields ——— */
  const [newMissionName, setNewMissionName] = useState("");
  const [newMissionDesc, setNewMissionDesc] = useState("");
  const [newMissionItems, setNewMissionItems] = useState<MissionItemData[]>([]);

  /* ——— Item form state ——— */
  const [editingItem, setEditingItem] = useState<MissionItemData | null>(null);
  const [isEditingExisting, setIsEditingExisting] = useState(false);
  const [editingParentId, setEditingParentId] = useState<string | null>(null);
  const [editingParentMode, setEditingParentMode] = useState<string | null>(
    null,
  );
  const [expandedSubMissions, setExpandedSubMissions] = useState<Set<string>>(
    new Set(),
  );

  /* ——— Item dropdowns ——— */
  const [itemMeasureOpen, setItemMeasureOpen] = useState(false);
  const [itemManualOpen, setItemManualOpen] = useState(false);
  const [itemSurveyOpen, setItemSurveyOpen] = useState(false);
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
  const [itemPeriodOpen, setItemPeriodOpen] = useState(false);
  const [itemPeriodCustom, setItemPeriodCustom] = useState(false);
  const [itemOwnerPopoverOpen, setItemOwnerPopoverOpen] = useState(false);

  /* ——— Item refs ——— */
  const itemMeasureBtnRef = useRef<HTMLButtonElement>(null);
  const itemOwnerBtnRef = useRef<HTMLButtonElement>(null);
  const itemPeriodBtnRef = useRef<HTMLButtonElement>(null);
  const itemPeriodCustomBtnRef = useRef<HTMLButtonElement>(null);
  const itemMoreBtnRef = useRef<HTMLButtonElement>(null);
  const itemManualBtnRef = useRef<HTMLButtonElement>(null);
  const itemSurveyBtnRef = useRef<HTMLButtonElement>(null);
  const itemMoreItemRefs = useRef<Record<string, HTMLButtonElement | null>>({});

  /* ——— Mission-level popovers ——— */
  const [ownerPopoverOpen, setOwnerPopoverOpen] = useState(false);
  const [morePopoverOpen, setMorePopoverOpen] = useState(false);
  const [moreSubPanel, setMoreSubPanel] = useState<string | null>(null);
  const [selectedMissionOwners, setSelectedMissionOwners] = useState<string[]>(
    [],
  );
  const [missionPeriod, setMissionPeriod] = useState<
    [CalendarDate | null, CalendarDate | null]
  >([null, null]);
  const [selectedMissionTeam, setSelectedMissionTeam] = useState<string | null>(
    null,
  );
  const [selectedSupportTeam, setSelectedSupportTeam] = useState<string[]>([]);
  const [selectedTags, setSelectedTags] = useState<string[]>([]);
  const [customTags, setCustomTags] = useState<{ id: string; label: string }[]>(
    [],
  );
  const [selectedVisibility, setSelectedVisibility] = useState("public");
  const [missionPeriodOpen, setMissionPeriodOpen] = useState(false);
  const [missionPeriodCustom, setMissionPeriodCustom] = useState(false);

  /* ——— Mission-level refs ——— */
  const ownerBtnRef = useRef<HTMLButtonElement>(null);
  const missionPeriodBtnRef = useRef<HTMLButtonElement>(null);
  const missionPeriodCustomBtnRef = useRef<HTMLButtonElement>(null);
  const moreBtnRef = useRef<HTMLButtonElement>(null);
  const moreItemRefs = useRef<Record<string, HTMLButtonElement | null>>({});

  /* ——— Populate form when editing an existing mission ——— */
  useEffect(() => {
    if (!open) return;
    if (editingMission) {
      setCreateStep(1);
      setSelectedTemplate("scratch");
      setNewMissionName(editingMission.title);
      setNewMissionDesc(editingMission.description ?? "");
      setNewMissionItems(missionToItems(editingMission));
      setSelectedMissionOwners([]);
      setSelectedMissionTeam(editingMission.teamId ?? null);
      setMissionPeriod([null, null]);
      setSelectedSupportTeam(
        (editingMission.members ?? [])
          .filter((m) => m.role === "supporter")
          .map((m) => m.userId),
      );
      setSelectedTags((editingMission.tags ?? []).map((tag) => tag.id));
      setSelectedVisibility("public");
      setExpandedSubMissions(
        new Set(
          missionToItems(editingMission)
            .filter((i) => i.measurementMode === "mission")
            .map((i) => i.id),
        ),
      );
    }
  }, [open, editingMission?.id]); // eslint-disable-line react-hooks/exhaustive-deps

  /* ——— Helpers ——— */
  function resetForm() {
    setCreateStep(0);
    setSelectedTemplate(undefined);
    setNewMissionName("");
    setNewMissionDesc("");
    setNewMissionItems([]);
    setEditingItem(null);
    setIsEditingExisting(false);
    setEditingParentId(null);
    setEditingParentMode(null);
    setSelectedMissionOwners([]);
    setSelectedMissionTeam(null);
    setMissionPeriod([null, null]);
    setSelectedSupportTeam([]);
    setSelectedTags([]);
    setSelectedVisibility("public");
    setShowCreateAssistant(false);
    setCreateAssistantMissions([]);
  }

  function toIsoDate(date: CalendarDate | null): string | null {
    if (!date) return null;
    return `${date.year}-${String(date.month).padStart(2, "0")}-${String(date.day).padStart(2, "0")}`;
  }

  function ownerFromSelection() {
    const selected = missionOwnerOptions.find(
      (option) => option.id === selectedMissionOwners[0],
    );
    const fallback = currentUserOption ??
      missionOwnerOptions[0] ?? {
        id: "local-user",
        label: "Usuário local",
        initials: "UL",
      };
    const owner = selected ?? fallback;
    const name = splitFullName(owner.label);
    return {
      id: owner.id,
      firstName: name.firstName,
      lastName: name.lastName,
      initials: owner.initials,
    };
  }

  function unitFromValue(unit: string): Indicator["unit"] {
    if (unit === "%") return "percent";
    if (unit === "R$" || unit === "US$") return "currency";
    if (!unit || unit === "un") return "count";
    return "custom";
  }

  function materializeMissionItems(
    rootMissionId: string,
    items: MissionItemData[],
    ownerId: string,
    // eslint-disable-next-line @typescript-eslint/no-explicit-any
  ): { keyResults: any[]; children: any[] } {
    // eslint-disable-next-line @typescript-eslint/no-explicit-any
    const keyResults: any[] = [];
    // eslint-disable-next-line @typescript-eslint/no-explicit-any
    const children: any[] = [];
    const now = new Date().toISOString();

    for (const item of items) {
      if (item.measurementMode === "mission") {
        const childMissionId =
          item.id ||
          `mission-${Date.now()}-${Math.random().toString(36).slice(2, 8)}`;
        const itemOwnerId = item.ownerId ?? ownerId;
        const itemTeamId = item.teamId ?? selectedMissionTeam;
        const childTree = materializeMissionItems(
          childMissionId,
          item.children ?? [],
          itemOwnerId,
        );
        const childProgress =
          childTree.keyResults.length > 0
            ? Math.round(
                childTree.keyResults.reduce((acc, kr) => acc + kr.progress, 0) /
                  childTree.keyResults.length,
              )
            : 0;

        children.push({
          id: childMissionId,
          orgId: activeOrgId ?? "",
          cycleId: null,
          parentId: rootMissionId,
          path: [rootMissionId, childMissionId],
          title: item.name || "Submissão sem título",
          description: item.description || null,
          ownerId: itemOwnerId,
          teamId: itemTeamId,
          status: "active",
          visibility: "public",
          progress: childProgress,
          kanbanStatus: "doing",
          sortOrder: children.length,
          dueDate: toIsoDate(item.period[1]) ?? toIsoDate(missionPeriod[1]),
          completedAt: null,
          createdAt: now,
          updatedAt: now,
          deletedAt: null,
          keyResults: childTree.keyResults,
          children: childTree.children,
        });
        continue;
      }

      const goalType =
        (item.manualType as Indicator["goalType"] | null) ??
        (item.measurementMode === "survey" ? "survey" : "reach");
      const targetValue =
        item.goalValue ||
        (goalType === "between" ? item.goalValueMax || null : null);
      const currentValue = "0";

      const krId =
        item.id || `kr-${Date.now()}-${Math.random().toString(36).slice(2, 8)}`;
      const krTasks = (item.children ?? [])
        .filter((child) => child.measurementMode === "task")
        .map((child, idx) => ({
          id:
            child.id ||
            `t-${Date.now()}-${Math.random().toString(36).slice(2, 8)}`,
          missionId: null,
          indicatorId: krId,
          title: child.name || "Tarefa sem título",
          description: child.description || null,
          ownerId: child.ownerId ?? item.ownerId ?? ownerId,
          dueDate:
            toIsoDate(child.period[1]) ??
            toIsoDate(item.period[1]) ??
            toIsoDate(missionPeriod[1]),
          isDone: false,
          sortOrder: idx,
          completedAt: null,
          createdAt: now,
          updatedAt: now,
        })) as MissionTask[];

      keyResults.push({
        id: krId,
        orgId: activeOrgId ?? "",
        missionId: rootMissionId,
        parentKrId: null,
        title: item.name || "Indicador sem título",
        description: item.description || null,
        ownerId: item.ownerId ?? ownerId,
        teamId: item.teamId ?? selectedMissionTeam,
        measurementMode:
          (item.measurementMode as Indicator["measurementMode"] | null) ??
          "manual",
        linkedMissionId: null,
        linkedSurveyId:
          item.measurementMode === "survey" ? item.surveyId : null,
        externalSource: null,
        externalConfig: null,
        goalType,
        targetValue,
        currentValue,
        startValue: "0",
        lowThreshold: item.goalValueMin || null,
        highThreshold: item.goalValueMax || null,
        unit: unitFromValue(item.goalUnit),
        unitLabel: item.goalUnit || null,
        expectedValue: null,
        status: "attention",
        progress: 0,
        periodLabel: null,
        periodStart: toIsoDate(item.period[0]) ?? toIsoDate(missionPeriod[0]),
        periodEnd: toIsoDate(item.period[1]) ?? toIsoDate(missionPeriod[1]),
        sortOrder: keyResults.length,
        createdAt: now,
        updatedAt: now,
        deletedAt: null,
        ...(krTasks.length > 0 ? { tasks: krTasks } : {}),
      });
    }

    return { keyResults, children };
  }

  function buildMissionFromForm(existing?: Mission): Mission {
    const now = new Date().toISOString();
    const missionId = existing?.id ?? `mission-${Date.now()}`;
    const ownerSel = ownerFromSelection();
    const tree = materializeMissionItems(
      missionId,
      newMissionItems,
      ownerSel.id,
    );
    const selectedMissionTags = selectedTags.map((tagId) => {
      const resolvedTagId = resolveTagId(tagId);
      const canonical = getTagById(resolvedTagId);
      if (canonical) return canonical as NonNullable<Mission["tags"]>[number];

      return {
        id: resolvedTagId,
        orgId: existing?.orgId ?? activeOrgId ?? "",
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
      tree.keyResults.length > 0
        ? Math.round(
            tree.keyResults.reduce(
              (acc: number, kr: { progress: number }) => acc + kr.progress,
              0,
            ) / tree.keyResults.length,
          )
        : 0;

    const members: MissionMember[] = selectedSupportTeam.map((userId) => {
      const opt = missionOwnerOptions.find((o) => o.id === userId);
      const nameParts = opt
        ? splitFullName(opt.label)
        : { firstName: userId, lastName: "" };
      const fullName = [nameParts.firstName, nameParts.lastName]
        .filter(Boolean)
        .join(" ");
      return {
        missionId,
        userId,
        role: "supporter" as const,
        addedAt: now,
        addedBy: ownerSel.id,
        user: {
          id: userId,
          fullName,
          initials: opt?.initials ?? null,
          jobTitle: null,
          avatarUrl: null,
        },
      };
    });

    return {
      id: missionId,
      orgId: existing?.orgId ?? activeOrgId ?? "",
      cycleId: existing?.cycleId ?? null,
      parentId: existing?.parentId ?? null,
      path: existing?.path ?? [missionId],
      title: newMissionName || existing?.title || "Missão sem título",
      description: newMissionDesc || null,
      ownerId: ownerSel.id,
      teamId: selectedMissionTeam,
      status: existing?.status ?? "active",
      visibility: selectedVisibility === "private" ? "private" : "public",
      progress,
      kanbanStatus: existing?.kanbanStatus ?? "doing",
      sortOrder: existing?.sortOrder ?? missionsCount,
      dueDate: toIsoDate(missionPeriod[1]),
      completedAt: existing?.completedAt ?? null,
      createdAt: existing?.createdAt ?? now,
      updatedAt: now,
      deletedAt: existing?.deletedAt ?? null,
      owner: {
        id: ownerSel.id,
        fullName: [ownerSel.firstName, ownerSel.lastName]
          .filter(Boolean)
          .join(" "),
        initials: ownerSel.initials ?? null,
      },
      team: selectedMissionTeam
        ? {
            id: selectedMissionTeam,
            name:
              teamOptions.find((t) => t.id === selectedMissionTeam)?.label ??
              "",
            color: "neutral" as const,
          }
        : undefined,
      indicators: tree.keyResults as Indicator[],
      children: tree.children as Mission[],
      tasks: existing?.tasks ?? [],
      tags: selectedMissionTags as Mission["tags"],
      members,
    };
  }

  function missionToItems(m: Mission): MissionItemData[] {
    const items: MissionItemData[] = (m.indicators ?? []).map((kr) => {
      const goalValue =
        kr.goalType === "reach" || kr.goalType === "between"
          ? String(numVal(kr.targetValue))
          : "";
      const goalValueMin =
        kr.lowThreshold != null ? String(numVal(kr.lowThreshold)) : "";
      const goalValueMax =
        kr.highThreshold != null ? String(numVal(kr.highThreshold)) : "";
      return {
        id: kr.id,
        name: kr.title,
        description: getMissionLabel(kr),
        measurementMode: "manual",
        manualType: kr.goalType,
        surveyId: null,
        period: [null, null] as [CalendarDate | null, CalendarDate | null],
        goalValue,
        goalValueMin,
        goalValueMax,
        goalUnit: "",
        ownerId: kr.ownerId,
        // eslint-disable-next-line @typescript-eslint/no-explicit-any
        teamId: (kr as any).teamId ?? null,
      };
    });

    (m.children ?? []).forEach((child) => {
      items.push({
        id: child.id,
        name: child.title,
        description: "",
        measurementMode: "mission",
        manualType: null,
        surveyId: null,
        period: [null, null],
        goalValue: "",
        goalValueMin: "",
        goalValueMax: "",
        goalUnit: "",
        ownerId: child.ownerId,
        teamId: child.teamId ?? null,
        children: missionToItems(child),
      });
    });

    return items;
  }

  function getGoalSummary(item: MissionItemData): string {
    if (item.measurementMode !== "manual" || !item.manualType) return "";
    const unit =
      UNIT_OPTIONS.find((u) => u.value === item.goalUnit)?.label ??
      item.goalUnit;
    if (item.manualType === "between")
      return item.goalValueMin && item.goalValueMax
        ? `${item.goalValueMin} – ${item.goalValueMax} ${unit}`
        : "";
    if (item.manualType === "above")
      return item.goalValueMin ? `≥ ${item.goalValueMin} ${unit}` : "";
    if (item.manualType === "below")
      return item.goalValueMax ? `≤ ${item.goalValueMax} ${unit}` : "";
    if (item.goalValue) return `${item.goalValue} ${unit}`;
    return "";
  }

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
      editingItem.measurementMode === "manual" ||
      editingItem.measurementMode === "external"
    ) {
      setExpandedSubMissions((prev) => new Set(prev).add(editingItem.id));
    }
    setEditingItem(null);
    setIsEditingExisting(false);
    setEditingParentId(null);
    setEditingParentMode(null);
  }

  /* ——— Inline item form ——— */
  function renderInlineForm(): ReactNode {
    if (!editingItem) return null;
    const tplCfg = getTemplateConfig(selectedTemplate);
    return (
      <div className="flex flex-col w-full bg-[var(--color-caramel-100)] border border-[var(--color-caramel-300)] rounded-[var(--radius-xs)]">
        <div className="flex items-center justify-between px-[var(--sp-sm)] py-[var(--sp-xs)] border-b border-[var(--color-caramel-300)]">
          <span className="[font-family:var(--font-label)] font-medium text-[var(--text-xs)] text-[var(--color-neutral-950)] leading-[1.15]">
            {isEditingExisting
              ? tplCfg.editItemFormTitle
              : tplCfg.addItemFormTitle}
          </span>
          <PlusSquare
            size={16}
            className="text-[var(--color-neutral-500)] shrink-0"
          />
        </div>

        <div className="flex flex-col gap-[var(--sp-xs)] px-[var(--sp-sm)] py-[var(--sp-xs)]">
          <div className="flex flex-col gap-[var(--sp-2xs)]">
            <input
              type="text"
              className="w-full border-none bg-transparent outline-none [font-family:var(--font-heading)] font-medium text-[var(--text-md)] text-[var(--color-neutral-950)] leading-[1.1] placeholder:text-[var(--color-neutral-500)]"
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
              className="w-full border-none bg-transparent outline-none [font-family:var(--font-heading)] font-medium text-[var(--text-xs)] text-[var(--color-neutral-950)] leading-[1.15] placeholder:text-[var(--color-neutral-500)]"
              placeholder={tplCfg.itemDescPlaceholder}
              value={editingItem.description}
              onChange={(e: React.ChangeEvent<HTMLInputElement>) =>
                setEditingItem((prev) =>
                  prev ? { ...prev, description: e.target.value } : prev,
                )
              }
            />
          </div>

          <div className="flex gap-[var(--sp-2xs)] items-center">
            {/* Owner */}
            <Button
              ref={itemOwnerBtnRef}
              variant="secondary"
              size="sm"
              leftIcon={UserCircle}
              onClick={() => setItemOwnerPopoverOpen((v) => !v)}
            >
              {(() => {
                const effectiveId =
                  editingItem.ownerId ?? selectedMissionOwners[0] ?? null;
                const label = effectiveId
                  ? missionOwnerOptions.find((o) => o.id === effectiveId)?.label
                  : null;
                return label ?? "Responsável";
              })()}
            </Button>

            {/* Period */}
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
              {(() => {
                const p0 = editingItem.period[0] ?? missionPeriod[0];
                const p1 = editingItem.period[1] ?? missionPeriod[1];
                if (p0 && p1)
                  return `${String(p0.day).padStart(2, "0")}/${String(p0.month).padStart(2, "0")} – ${String(p1.day).padStart(2, "0")}/${String(p1.month).padStart(2, "0")}/${p1.year}`;
                return "Período";
              })()}
            </Button>

            {/* Measurement mode */}
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

            {/* More */}
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

          {/* Item owner popover */}
          <PopoverSelect
            mode="single"
            open={itemOwnerPopoverOpen}
            onClose={() => setItemOwnerPopoverOpen(false)}
            anchorRef={itemOwnerBtnRef}
            options={toSelectOptions(missionOwnerOptions)}
            value={editingItem.ownerId ?? selectedMissionOwners[0] ?? ""}
            onChange={(val) => {
              setEditingItem((prev) =>
                prev ? { ...prev, ownerId: val || null } : prev,
              );
              setItemOwnerPopoverOpen(false);
            }}
            searchable
            searchPlaceholder="Buscar responsável..."
          />

          {/* Goal inputs */}
          {editingItem.measurementMode === "manual" &&
            editingItem.manualType && (
              <div className="flex flex-col gap-[var(--sp-2xs)] pt-[var(--sp-xs)] border-t border-[var(--color-caramel-200)]">
                <span className="[font-family:var(--font-label)] font-medium text-[var(--text-xs)] text-[var(--color-neutral-500)] leading-[1.15]">
                  {
                    MANUAL_INDICATOR_TYPES.find(
                      (t) => t.id === editingItem.manualType,
                    )?.label
                  }
                </span>

                {(editingItem.manualType === "reach" ||
                  editingItem.manualType === "reduce") && (
                  <div className="flex gap-[var(--sp-2xs)] items-end [&>*]:flex-1 [&>*]:min-w-0">
                    <Input
                      label="Valor alvo"
                      placeholder="Ex: 1000"
                      value={editingItem.goalValue}
                      onChange={(e: React.ChangeEvent<HTMLInputElement>) =>
                        setEditingItem((prev) =>
                          prev ? { ...prev, goalValue: e.target.value } : prev,
                        )
                      }
                    />
                    <Select
                      label="Unidade"
                      placeholder="Selecionar"
                      options={UNIT_OPTIONS}
                      value={editingItem.goalUnit}
                      onChange={(v: string) =>
                        setEditingItem((prev) =>
                          prev ? { ...prev, goalUnit: v } : prev,
                        )
                      }
                    />
                  </div>
                )}

                {editingItem.manualType === "above" && (
                  <div className="flex gap-[var(--sp-2xs)] items-end [&>*]:flex-1 [&>*]:min-w-0">
                    <Input
                      label="Mínimo"
                      placeholder="Ex: 70"
                      value={editingItem.goalValueMin}
                      onChange={(e: React.ChangeEvent<HTMLInputElement>) =>
                        setEditingItem((prev) =>
                          prev
                            ? { ...prev, goalValueMin: e.target.value }
                            : prev,
                        )
                      }
                    />
                    <Select
                      label="Unidade"
                      placeholder="Selecionar"
                      options={UNIT_OPTIONS}
                      value={editingItem.goalUnit}
                      onChange={(v: string) =>
                        setEditingItem((prev) =>
                          prev ? { ...prev, goalUnit: v } : prev,
                        )
                      }
                    />
                  </div>
                )}

                {editingItem.manualType === "below" && (
                  <div className="flex gap-[var(--sp-2xs)] items-end [&>*]:flex-1 [&>*]:min-w-0">
                    <Input
                      label="Máximo"
                      placeholder="Ex: 5"
                      value={editingItem.goalValueMax}
                      onChange={(e: React.ChangeEvent<HTMLInputElement>) =>
                        setEditingItem((prev) =>
                          prev
                            ? { ...prev, goalValueMax: e.target.value }
                            : prev,
                        )
                      }
                    />
                    <Select
                      label="Unidade"
                      placeholder="Selecionar"
                      options={UNIT_OPTIONS}
                      value={editingItem.goalUnit}
                      onChange={(v: string) =>
                        setEditingItem((prev) =>
                          prev ? { ...prev, goalUnit: v } : prev,
                        )
                      }
                    />
                  </div>
                )}

                {editingItem.manualType === "between" && (
                  <div className="flex gap-[var(--sp-2xs)] items-end [&>*]:flex-1 [&>*]:min-w-0">
                    <Input
                      label="Mínimo"
                      placeholder="Ex: 50"
                      value={editingItem.goalValueMin}
                      onChange={(e: React.ChangeEvent<HTMLInputElement>) =>
                        setEditingItem((prev) =>
                          prev
                            ? { ...prev, goalValueMin: e.target.value }
                            : prev,
                        )
                      }
                    />
                    <Input
                      label="Máximo"
                      placeholder="Ex: 90"
                      value={editingItem.goalValueMax}
                      onChange={(e: React.ChangeEvent<HTMLInputElement>) =>
                        setEditingItem((prev) =>
                          prev
                            ? { ...prev, goalValueMax: e.target.value }
                            : prev,
                        )
                      }
                    />
                    <Select
                      label="Unidade"
                      placeholder="Selecionar"
                      options={UNIT_OPTIONS}
                      value={editingItem.goalUnit}
                      onChange={(v: string) =>
                        setEditingItem((prev) =>
                          prev ? { ...prev, goalUnit: v } : prev,
                        )
                      }
                    />
                  </div>
                )}
              </div>
            )}
        </div>

        {/* Measurement mode dropdown */}
        <FilterDropdown
          open={itemMeasureOpen}
          onClose={() => setItemMeasureOpen(false)}
          anchorRef={itemMeasureBtnRef}
        >
          <div className="flex flex-col p-[var(--sp-3xs)] max-h-[320px] overflow-y-auto">
            {MEASUREMENT_MODES.filter((mode) => {
              // Inside a KR (non-mission parent), only tasks are allowed
              if (editingParentMode && editingParentMode !== "mission") {
                return mode.id === "task";
              }
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
                  className={`flex items-start gap-[var(--sp-2xs)] w-full p-[var(--sp-2xs)] border-none bg-transparent rounded-[var(--radius-2xs)] cursor-pointer transition-[background-color] duration-[120ms] ease-in text-left hover:bg-[var(--color-caramel-100)]${isActive ? " bg-[var(--color-caramel-50)]" : ""}`}
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
                              surveyId: null,
                              goalValue: "",
                              goalValueMin: "",
                              goalValueMax: "",
                              goalUnit: "",
                              ownerId: null,
                              teamId: null,
                              children:
                                mode.id === "mission" ||
                                mode.id === "manual" ||
                                mode.id === "external"
                                  ? []
                                  : undefined,
                            }
                          : prev,
                      );
                      setItemMeasureOpen(false);
                    }
                  }}
                >
                  <Icon
                    size={16}
                    className="shrink-0 text-[var(--color-neutral-500)] mt-[2px]"
                  />
                  <div className="flex flex-col gap-[2px] flex-1 min-w-0">
                    <span className="[font-family:var(--font-label)] font-medium text-[var(--text-xs)] text-[var(--color-neutral-950)] leading-[1.15]">
                      {mode.label}
                    </span>
                    <span className="[font-family:var(--font-body)] text-[10px] text-[var(--color-neutral-500)] leading-[1.3]">
                      {mode.description}
                    </span>
                  </div>
                  {mode.id === "manual" && (
                    <CaretRight
                      size={12}
                      className="ml-auto text-[var(--color-neutral-400)] shrink-0"
                    />
                  )}
                </button>
              );
            })}
          </div>
        </FilterDropdown>

        {/* Sub-panel: manual indicator type */}
        <FilterDropdown
          open={itemMeasureOpen && itemManualOpen}
          onClose={() => setItemManualOpen(false)}
          anchorRef={itemManualBtnRef}
          placement="right-start"
          noOverlay
        >
          <div className="flex flex-col p-[var(--sp-3xs)] max-h-[320px] overflow-y-auto">
            {MANUAL_INDICATOR_TYPES.map((t) => {
              const Icon = t.icon;
              const isActive = editingItem?.manualType === t.id;
              return (
                <button
                  key={t.id}
                  ref={
                    t.id === "survey"
                      ? (el) => {
                          itemSurveyBtnRef.current = el;
                        }
                      : undefined
                  }
                  type="button"
                  className={`flex items-center gap-[var(--sp-2xs)] w-full p-[var(--sp-2xs)] border-none bg-transparent rounded-[var(--radius-2xs)] [font-family:var(--font-label)] font-medium text-[var(--text-xs)] text-[var(--color-neutral-950)] cursor-pointer whitespace-nowrap transition-[background-color] duration-[120ms] ease-in text-left hover:bg-[var(--color-caramel-100)]${isActive ? " bg-[var(--color-caramel-50)]" : ""}`}
                  onClick={() => {
                    if (t.id === "survey") {
                      setItemSurveyOpen(true);
                    } else {
                      setEditingItem((prev) =>
                        prev
                          ? {
                              ...prev,
                              measurementMode: "manual",
                              manualType: t.id,
                              surveyId: null,
                              goalValue: "",
                              goalValueMin: "",
                              goalValueMax: "",
                              goalUnit: "",
                            }
                          : prev,
                      );
                      setItemManualOpen(false);
                      setItemMeasureOpen(false);
                    }
                  }}
                >
                  <Icon size={14} />
                  <span>{t.label}</span>
                  {t.id === "survey" && (
                    <CaretRight
                      size={12}
                      className="ml-auto text-[var(--color-neutral-400)] shrink-0"
                    />
                  )}
                </button>
              );
            })}
          </div>
        </FilterDropdown>

        {/* Period dropdown */}
        <FilterDropdown
          open={itemPeriodOpen}
          onClose={() => {
            setItemPeriodOpen(false);
            setItemPeriodCustom(false);
          }}
          anchorRef={itemPeriodBtnRef}
        >
          <div className="flex flex-col p-[var(--sp-3xs)] max-h-[320px] overflow-y-auto">
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
                  className={`flex items-center gap-[var(--sp-2xs)] w-full p-[var(--sp-2xs)] border-none bg-transparent rounded-[var(--radius-2xs)] [font-family:var(--font-label)] font-medium text-[var(--text-xs)] text-[var(--color-neutral-950)] cursor-pointer whitespace-nowrap transition-colors duration-[120ms] ease-in-out text-left hover:bg-[var(--color-caramel-100)]${isActive ? " bg-[var(--color-caramel-50)]" : ""}`}
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
          <div className="border-t border-[var(--color-caramel-300)] p-[var(--sp-3xs)]">
            <button
              ref={itemPeriodCustomBtnRef}
              type="button"
              className={`flex items-center gap-[var(--sp-2xs)] w-full p-[var(--sp-2xs)] border-none bg-transparent rounded-[var(--radius-2xs)] [font-family:var(--font-label)] font-medium text-[var(--text-xs)] text-[var(--color-neutral-950)] cursor-pointer whitespace-nowrap transition-colors duration-[120ms] ease-in-out text-left hover:bg-[var(--color-caramel-100)]${itemPeriodCustom ? " bg-[var(--color-caramel-50)]" : ""}`}
              onClick={() => setItemPeriodCustom((v) => !v)}
            >
              <Plus size={14} />
              <span>Período personalizado</span>
              <CaretRight
                size={12}
                className="ml-auto text-[var(--color-neutral-400)] shrink-0"
              />
            </button>
          </div>
        </FilterDropdown>

        {/* Period custom calendar sub-panel */}
        <FilterDropdown
          open={itemPeriodOpen && itemPeriodCustom}
          onClose={() => setItemPeriodCustom(false)}
          anchorRef={itemPeriodCustomBtnRef}
          placement="right-start"
          noOverlay
        >
          <div className="p-[var(--sp-xs)]">
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

        {/* Item "..." main menu */}
        <FilterDropdown
          open={itemMoreOpen}
          onClose={() => {
            setItemMoreOpen(false);
            setItemMoreSubPanel(null);
          }}
          anchorRef={itemMoreBtnRef}
        >
          <div className="flex flex-col p-[var(--sp-3xs)] max-h-[320px] overflow-y-auto">
            {MORE_MISSION_OPTIONS.map((opt) => {
              const isActive = itemMoreSubPanel === opt.id;
              const count =
                opt.id === "team-support"
                  ? itemSupportTeam.length
                  : opt.id === "organizers"
                    ? itemTags.length
                    : 0;
              const displayLabel =
                opt.id === "visibility"
                  ? itemVisibility === "private"
                    ? "Privado"
                    : "Público"
                  : opt.label;
              const Icon =
                opt.id === "visibility"
                  ? itemVisibility === "private"
                    ? EyeSlash
                    : Eye
                  : opt.icon;
              return (
                <button
                  key={opt.id}
                  ref={(el) => {
                    itemMoreItemRefs.current[opt.id] = el;
                  }}
                  type="button"
                  className={`flex items-center gap-[var(--sp-2xs)] w-full p-[var(--sp-2xs)] border-none bg-transparent rounded-[var(--radius-2xs)] [font-family:var(--font-label)] font-medium text-[var(--text-xs)] text-[var(--color-neutral-950)] cursor-pointer whitespace-nowrap transition-[background-color] duration-[120ms] ease-in text-left hover:bg-[var(--color-caramel-100)]${isActive ? " bg-[var(--color-caramel-50)]" : ""}`}
                  onClick={() => setItemMoreSubPanel(isActive ? null : opt.id)}
                >
                  <Icon size={14} />
                  <span>{displayLabel}</span>
                  {count > 0 && (
                    <Badge color="neutral" size="sm">
                      {count}
                    </Badge>
                  )}
                  <CaretRight
                    size={12}
                    className="ml-auto text-[var(--color-neutral-400)] shrink-0"
                  />
                </button>
              );
            })}
          </div>
        </FilterDropdown>

        {/* Sub-panel: Team support */}
        <FilterDropdown
          open={itemMoreOpen && itemMoreSubPanel === "team-support"}
          onClose={() => setItemMoreSubPanel(null)}
          anchorRef={{
            current: itemMoreItemRefs.current["team-support"] ?? null,
          }}
          placement="right-start"
          noOverlay
        >
          <div className="flex flex-col p-[var(--sp-3xs)] max-h-[320px] overflow-y-auto">
            <div className="flex items-center gap-[var(--sp-2xs)] p-[var(--sp-2xs)] border-b border-[var(--color-caramel-200)] mb-[var(--sp-3xs)]">
              <MagnifyingGlass
                size={14}
                className="shrink-0 text-[var(--color-neutral-400)]"
              />
              <input
                type="text"
                className="flex-1 min-w-0 border-none bg-transparent outline-none [font-family:var(--font-label)] font-medium text-[var(--text-xs)] text-[var(--color-neutral-950)] placeholder:text-[var(--color-neutral-400)]"
                placeholder="Buscar pessoa..."
                value={itemSupportSearch}
                onChange={(e: React.ChangeEvent<HTMLInputElement>) =>
                  setItemSupportSearch(e.target.value)
                }
              />
            </div>
            {missionOwnerOptions
              .filter((opt) =>
                opt.label
                  .toLowerCase()
                  .includes(itemSupportSearch.toLowerCase()),
              )
              .map((opt) => {
                const checked = itemSupportTeam.includes(opt.id);
                return (
                  <button
                    key={opt.id}
                    type="button"
                    className={`flex items-center gap-[var(--sp-2xs)] w-full p-[var(--sp-2xs)] border-none bg-transparent rounded-[var(--radius-2xs)] [font-family:var(--font-label)] font-medium text-[var(--text-xs)] text-[var(--color-neutral-950)] cursor-pointer whitespace-nowrap transition-colors duration-[120ms] ease-in-out text-left hover:bg-[var(--color-caramel-100)]${checked ? " bg-[var(--color-caramel-50)]" : ""}`}
                    onClick={() =>
                      setItemSupportTeam((prev) =>
                        prev.includes(opt.id)
                          ? prev.filter((id) => id !== opt.id)
                          : [...prev, opt.id],
                      )
                    }
                  >
                    <Checkbox checked={checked} readOnly />
                    <span>{opt.label}</span>
                  </button>
                );
              })}
          </div>
        </FilterDropdown>

        {/* Sub-panel: Tags */}
        <FilterDropdown
          open={itemMoreOpen && itemMoreSubPanel === "organizers"}
          onClose={() => setItemMoreSubPanel(null)}
          anchorRef={{
            current: itemMoreItemRefs.current["organizers"] ?? null,
          }}
          placement="right-start"
          noOverlay
        >
          <div className="flex flex-col p-[var(--sp-3xs)] max-h-[320px] overflow-y-auto">
            <div className="flex items-center gap-[var(--sp-2xs)] p-[var(--sp-2xs)] border-b border-[var(--color-caramel-200)] mb-[var(--sp-3xs)]">
              <MagnifyingGlass
                size={14}
                className="shrink-0 text-[var(--color-neutral-400)]"
              />
              <input
                type="text"
                className="flex-1 min-w-0 border-none bg-transparent outline-none [font-family:var(--font-label)] font-medium text-[var(--text-xs)] text-[var(--color-neutral-950)] placeholder:text-[var(--color-neutral-400)]"
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
                    className={`flex items-center gap-[var(--sp-2xs)] w-full p-[var(--sp-2xs)] border-none bg-transparent rounded-[var(--radius-2xs)] [font-family:var(--font-label)] font-medium text-[var(--text-xs)] text-[var(--color-neutral-950)] cursor-pointer whitespace-nowrap transition-colors duration-[120ms] ease-in-out text-left hover:bg-[var(--color-caramel-100)]${checked ? " bg-[var(--color-caramel-50)]" : ""}`}
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
            <div className="flex items-center gap-[var(--sp-3xs)] px-[var(--sp-2xs)] py-[var(--sp-3xs)] border-t border-[var(--color-caramel-200)] mt-[var(--sp-3xs)]">
              <input
                type="text"
                className="flex-1 min-w-0 border-none bg-transparent outline-none [font-family:var(--font-label)] font-medium text-[var(--text-xs)] text-[var(--color-neutral-950)] placeholder:text-[var(--color-neutral-400)]"
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

        {/* Sub-panel: Visibility */}
        <FilterDropdown
          open={itemMoreOpen && itemMoreSubPanel === "visibility"}
          onClose={() => setItemMoreSubPanel(null)}
          anchorRef={{
            current: itemMoreItemRefs.current["visibility"] ?? null,
          }}
          placement="right-start"
          noOverlay
        >
          <div className="flex flex-col gap-[var(--sp-3xs)] p-[var(--sp-3xs)]">
            {VISIBILITY_OPTIONS.map((opt) => {
              const isSelected = itemVisibility === opt.id;
              const Icon = opt.id === "public" ? Eye : EyeSlash;
              return (
                <button
                  key={opt.id}
                  type="button"
                  className={`flex items-center gap-[var(--sp-2xs)] w-full px-[var(--sp-xs)] py-[var(--sp-2xs)] bg-transparent border-none rounded-[var(--radius-xs)] cursor-pointer text-left transition-[background-color] duration-[120ms] ease-in hover:bg-[var(--color-caramel-50)]${isSelected ? " bg-[var(--color-orange-50)]" : ""}`}
                  onClick={() => {
                    setItemVisibility(opt.id);
                    setItemMoreSubPanel(null);
                  }}
                >
                  <Icon
                    size={16}
                    className="shrink-0 text-[var(--color-neutral-400)]"
                  />
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

        <div className="flex items-center justify-between px-[var(--sp-sm)] py-[var(--sp-xs)] border-t border-[var(--color-caramel-300)]">
          <Button
            variant="tertiary"
            size="sm"
            onClick={() => {
              setEditingItem(null);
              setEditingParentId(null);
              setEditingParentMode(null);
              setIsEditingExisting(false);
            }}
          >
            Cancelar
          </Button>
          <Button
            variant="primary"
            size="sm"
            disabled={!editingItem.name.trim()}
            onClick={handleSaveItem}
          >
            {isEditingExisting ? "Salvar" : "Adicionar"}
          </Button>
        </div>
      </div>
    );
  }

  /* ——— Render mission items list ——— */
  function renderMissionItems(
    items: MissionItemData[],
    parentId: string | null,
    parentMeasurementMode?: string | null,
  ): ReactNode {
    const isNested = parentId !== null;
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
                className={
                  isNested ? undefined : "flex flex-col gap-[var(--sp-2xs)]"
                }
              >
                {isNested ? (
                  <div className="relative">{renderInlineForm()}</div>
                ) : (
                  renderInlineForm()
                )}
              </div>
            );
          }

          const goalSummary = getGoalSummary(item);
          const canHaveChildren =
            item.measurementMode !== "task" &&
            (item.measurementMode === "mission" ||
              item.measurementMode === "manual" ||
              item.measurementMode === "external");
          const isExpanded = expandedSubMissions.has(item.id);
          const childCount = item.children?.length ?? 0;

          const handleEdit = () => {
            setEditingItem({ ...item });
            setIsEditingExisting(true);
            setEditingParentId(parentId);
            setEditingParentMode(parentMeasurementMode ?? null);
          };

          const handleRemove = () => {
            if (parentId) {
              setNewMissionItems((prev) => removeChildFromTree(prev, item.id));
            } else {
              setNewMissionItems((prev) =>
                prev.filter((i) => i.id !== item.id),
              );
            }
          };

          const rowClass = isNested
            ? "relative flex items-center gap-[var(--sp-2xs)] px-[var(--sp-sm)] py-[var(--sp-xs)] bg-[var(--color-caramel-50)] rounded-[var(--radius-2xs)] min-h-[48px]"
            : "flex items-center gap-[var(--sp-2xs)] px-[var(--sp-sm)] py-[var(--sp-xs)] bg-[var(--color-caramel-100)] border border-[var(--color-caramel-300)] rounded-[var(--radius-xs)] transition-[border-color] duration-[120ms] ease-in hover:border-[var(--color-caramel-500)]";

          return (
            <div
              key={item.id}
              className={
                isNested ? "relative" : "flex flex-col gap-[var(--sp-2xs)]"
              }
            >
              <div className={rowClass}>
                {canHaveChildren && (
                  <button
                    type="button"
                    className="flex items-center justify-center shrink-0 w-[24px] h-[24px] border-none bg-transparent rounded-[var(--radius-2xs)] text-[var(--color-neutral-500)] cursor-pointer transition-[background-color] duration-[120ms] ease-in hover:bg-[var(--color-caramel-200)]"
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
                    {isExpanded ? (
                      <CaretUp size={14} />
                    ) : (
                      <CaretDown size={14} />
                    )}
                  </button>
                )}
                {/* eslint-disable-next-line jsx-a11y/click-events-have-key-events, jsx-a11y/no-static-element-interactions */}
                <div
                  className="flex flex-col gap-[2px] flex-1 min-w-0 cursor-pointer"
                  onClick={handleEdit}
                >
                  <div className="flex items-center gap-[var(--sp-2xs)] min-w-0">
                    <span className="[font-family:var(--font-label)] font-medium text-[var(--text-xs)] text-[var(--color-neutral-950)] overflow-hidden text-ellipsis whitespace-nowrap">
                      {item.name}
                    </span>
                    {item.measurementMode && (
                      <span className="shrink-0 [font-family:var(--font-label)] font-medium text-[10px] text-[var(--color-neutral-500)] bg-[var(--color-caramel-200)] px-[var(--sp-3xs)] py-[2px] rounded-[var(--radius-2xs)] whitespace-nowrap">
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
                      <span className="shrink-0 [font-family:var(--font-body)] text-[10px] text-[var(--color-neutral-500)] whitespace-nowrap">
                        {childCount} {childCount === 1 ? "item" : "itens"}
                      </span>
                    )}
                  </div>
                  <div className="flex items-center gap-[var(--sp-2xs)] flex-wrap">
                    {(() => {
                      const ownerLabel = item.ownerId
                        ? missionOwnerOptions.find((o) => o.id === item.ownerId)
                            ?.label
                        : selectedMissionOwners.length > 0
                          ? missionOwnerOptions.find(
                              (o) => o.id === selectedMissionOwners[0],
                            )?.label
                          : null;
                      const ownerInherited =
                        !item.ownerId && selectedMissionOwners.length > 0;

                      const hasPeriod = item.period[0] && item.period[1];
                      const periodLabel = hasPeriod
                        ? `${String(item.period[0]!.day).padStart(2, "0")}/${String(item.period[0]!.month).padStart(2, "0")} – ${String(item.period[1]!.day).padStart(2, "0")}/${String(item.period[1]!.month).padStart(2, "0")}`
                        : missionPeriod[0] && missionPeriod[1]
                          ? `${String(missionPeriod[0].day).padStart(2, "0")}/${String(missionPeriod[0].month).padStart(2, "0")} – ${String(missionPeriod[1].day).padStart(2, "0")}/${String(missionPeriod[1].month).padStart(2, "0")}`
                          : null;
                      const periodInherited =
                        !hasPeriod && !!(missionPeriod[0] && missionPeriod[1]);

                      return (
                        <>
                          {ownerLabel && (
                            <Badge
                              color={ownerInherited ? "neutral" : "orange"}
                              size="sm"
                              leftIcon={UserCircle}
                            >
                              {ownerLabel}
                            </Badge>
                          )}
                          {periodLabel && (
                            <Badge
                              color={periodInherited ? "neutral" : "orange"}
                              size="sm"
                              leftIcon={Calendar}
                            >
                              {periodLabel}
                            </Badge>
                          )}
                        </>
                      );
                    })()}
                    {goalSummary && (
                      <span className="[font-family:var(--font-body)] text-[10px] text-[var(--color-neutral-500)] leading-[1.3] overflow-hidden text-ellipsis whitespace-nowrap">
                        {goalSummary}
                      </span>
                    )}
                  </div>
                </div>
                <div className="flex shrink-0 gap-[2px]">
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
                <div className="flex flex-col gap-[var(--sp-2xs)] pt-[var(--sp-2xs)] pl-[40px] overflow-hidden">
                  {renderMissionItems(
                    item.children ?? [],
                    item.id,
                    item.measurementMode,
                  )}
                </div>
              )}
            </div>
          );
        })}

        {/* Add new item form / button */}
        {(() => {
          if (parentMeasurementMode === "task") return null;
          const tplCfg = getTemplateConfig(selectedTemplate);
          const isInsideKR =
            parentMeasurementMode && parentMeasurementMode !== "mission";
          const addLabel = isInsideKR
            ? "Adicionar tarefa"
            : isNested
              ? "Adicionar item"
              : tplCfg.addItemLabel;

          if (
            editingItem &&
            !isEditingExisting &&
            editingParentId === parentId
          ) {
            return isNested ? (
              <div className="relative">{renderInlineForm()}</div>
            ) : (
              renderInlineForm()
            );
          }

          if (!editingItem || editingParentId !== parentId) {
            const handleAdd = () => {
              setIsEditingExisting(false);
              setEditingParentId(parentId);
              setEditingParentMode(parentMeasurementMode ?? null);
              setEditingItem({
                id: `item-${Date.now()}`,
                name: "",
                description: "",
                measurementMode: isInsideKR ? "task" : null,
                manualType: null,
                surveyId: null,
                period: [null, null],
                goalValue: "",
                goalValueMin: "",
                goalValueMax: "",
                goalUnit: "",
                ownerId: null,
                teamId: null,
              });
            };
            return isNested ? (
              <div className="relative">
                <button
                  type="button"
                  className="flex items-center justify-between w-full px-[var(--sp-sm)] py-[var(--sp-xs)] bg-[var(--color-caramel-100)] border border-[var(--color-caramel-300)] rounded-[var(--radius-xs)] cursor-pointer [font-family:var(--font-label)] font-medium text-[var(--text-xs)] text-[var(--color-neutral-950)] transition-[background-color,border-color] duration-[120ms] ease-in hover:bg-[var(--color-caramel-50)] hover:border-[var(--color-caramel-500)]"
                  onClick={handleAdd}
                >
                  <span>{addLabel}</span>
                  <PlusSquare size={16} />
                </button>
              </div>
            ) : (
              <button
                type="button"
                className="flex items-center justify-between w-full px-[var(--sp-sm)] py-[var(--sp-xs)] bg-[var(--color-caramel-100)] border border-[var(--color-caramel-300)] rounded-[var(--radius-xs)] cursor-pointer [font-family:var(--font-label)] font-medium text-[var(--text-xs)] text-[var(--color-neutral-950)] transition-[background-color,border-color] duration-[120ms] ease-in hover:bg-[var(--color-caramel-50)] hover:border-[var(--color-caramel-500)]"
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

  /* ——— Render ——— */
  const tplCfg = getTemplateConfig(selectedTemplate);

  return (
    <Modal
      open={open}
      onClose={() => {
        resetForm();
        onClose();
      }}
      size="lg"
      sidePanel={
        showCreateAssistant ? (
          <AiAssistant
            title="Exemplos e inspiração"
            heading={
              selectedTemplate
                ? `Exemplos de ${MISSION_TEMPLATES.find((t) => t.value === selectedTemplate)?.title ?? "missão"}`
                : "Selecione um template para ver exemplos"
            }
            onClose={() => setShowCreateAssistant(false)}
            allowUpload
            missions={ASSISTANT_MISSIONS}
            selectedMissions={createAssistantMissions}
            onMissionsChange={setCreateAssistantMissions}
            suggestions={(() => {
              const lib = selectedTemplate
                ? EXAMPLE_LIBRARY[selectedTemplate]
                : null;
              if (!lib) return ["Ver exemplos por departamento"];
              return lib
                .flatMap((cat) => cat.examples.map((ex) => ex.objective))
                .slice(0, 6);
            })()}
            onMessage={async (msg: string) => {
              const lib = selectedTemplate
                ? EXAMPLE_LIBRARY[selectedTemplate]
                : null;
              if (!lib)
                return "Selecione um template primeiro para ver exemplos.";
              for (const cat of lib) {
                for (const ex of cat.examples) {
                  if (msg.includes(ex.objective)) {
                    return `**${ex.objective}**\n\n${ex.keyResults.map((kr: string, i: number) => `${i + 1}. ${kr}`).join("\n")}\n\nClique em "Usar como base" para preencher o formulário com este exemplo.`;
                  }
                }
              }
              return "Desculpe, ainda estou em desenvolvimento. Em breve poderei ajudá-lo a criar OKRs com IA!";
            }}
            onUseAsBase={(content: string) => {
              const lib = selectedTemplate
                ? EXAMPLE_LIBRARY[selectedTemplate]
                : null;
              if (!lib) return;
              for (const cat of lib) {
                for (const ex of cat.examples) {
                  if (content.includes(ex.objective)) {
                    setNewMissionName(ex.objective);
                    setNewMissionItems(
                      ex.keyResults.map((kr: string) => {
                        const parsed = parseKeyResultGoal(kr);
                        return {
                          id: generateItemId(),
                          name: kr,
                          description: "",
                          measurementMode: "manual",
                          manualType: parsed.manualType,
                          surveyId: null,
                          period: [null, null] as [
                            CalendarDate | null,
                            CalendarDate | null,
                          ],
                          goalValue: parsed.goalValue,
                          goalValueMin:
                            parsed.manualType === "above"
                              ? parsed.goalValue
                              : "",
                          goalValueMax:
                            parsed.manualType === "below"
                              ? parsed.goalValue
                              : "",
                          goalUnit: parsed.goalUnit,
                          ownerId: null,
                          teamId: null,
                        };
                      }),
                    );
                    if (createStep === 0) setCreateStep(1);
                    toast.success(
                      "Exemplo aplicado ao formulário — edite como quiser",
                    );
                    return;
                  }
                }
              }
            }}
          />
        ) : null
      }
    >
      <ModalHeader
        title={editingMission ? "Editar missão" : "Criar missão"}
        onClose={() => {
          resetForm();
          onClose();
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
          <Step0
            selectedTemplate={selectedTemplate}
            onChange={setSelectedTemplate}
          />
        )}

        {createStep === 1 && (
          <Step1
            editingMissionId={editingMission?.id ?? null}
            tplCfg={tplCfg}
            newMissionName={newMissionName}
            setNewMissionName={setNewMissionName}
            newMissionDesc={newMissionDesc}
            setNewMissionDesc={setNewMissionDesc}
            ownerBtnRef={ownerBtnRef}
            ownerPopoverOpen={ownerPopoverOpen}
            onToggleOwnerPopover={() => setOwnerPopoverOpen((v) => !v)}
            selectedMissionOwners={selectedMissionOwners}
            missionOwnerOptions={missionOwnerOptions}
            onOwnerChange={(val) => setSelectedMissionOwners(val ? [val] : [])}
            onCloseOwnerPopover={() => setOwnerPopoverOpen(false)}
            missionPeriodBtnRef={missionPeriodBtnRef}
            missionPeriod={missionPeriod}
            missionPeriodOpen={missionPeriodOpen}
            missionPeriodCustom={missionPeriodCustom}
            missionPeriodCustomBtnRef={missionPeriodCustomBtnRef}
            presetPeriods={presetPeriods}
            onTogglePeriodOpen={() => {
              setMissionPeriodOpen((v) => !v);
              setMissionPeriodCustom(false);
            }}
            onClosePeriod={() => {
              setMissionPeriodOpen(false);
              setMissionPeriodCustom(false);
            }}
            onSetMissionPeriod={setMissionPeriod}
            onTogglePeriodCustom={() => setMissionPeriodCustom((v) => !v)}
            moreBtnRef={moreBtnRef}
            morePopoverOpen={morePopoverOpen}
            moreSubPanel={moreSubPanel}
            moreItemRefs={moreItemRefs}
            onToggleMorePopover={() => {
              setMoreSubPanel(null);
              setMorePopoverOpen((v) => !v);
            }}
            onCloseMorePopover={() => {
              setMorePopoverOpen(false);
              setMoreSubPanel(null);
            }}
            onSetMoreSubPanel={setMoreSubPanel}
            selectedSupportTeam={selectedSupportTeam}
            onSupportTeamChange={setSelectedSupportTeam}
            selectedTags={selectedTags}
            onTagsChange={setSelectedTags}
            customTags={customTags}
            onAddCustomTag={(t) => setCustomTags((prev) => [...prev, t])}
            selectedVisibility={selectedVisibility}
            onVisibilityChange={setSelectedVisibility}
            createTag={createTag}
            missionTagOptions={missionTagOptions}
            itemsSection={renderMissionItems(newMissionItems, null)}
          />
        )}

        {createStep === 2 && (
          <Step2
            selectedTemplate={selectedTemplate}
            newMissionName={newMissionName}
            newMissionDesc={newMissionDesc}
            selectedMissionOwners={selectedMissionOwners}
            missionOwnerOptions={missionOwnerOptions}
            missionPeriod={missionPeriod}
            selectedSupportTeam={selectedSupportTeam}
            selectedTags={selectedTags}
            missionTagOptions={missionTagOptions}
            customTags={customTags}
            selectedVisibility={selectedVisibility}
            newMissionItems={newMissionItems}
          />
        )}
      </ModalBody>

      <ModalFooter align="between">
        <Button
          variant="tertiary"
          size="md"
          onClick={() => {
            if (createStep > (editingMission ? 1 : 0)) {
              setCreateStep((s) => s - 1);
            } else {
              resetForm();
              onClose();
            }
          }}
        >
          {createStep > (editingMission ? 1 : 0) ? "Voltar" : "Cancelar"}
        </Button>
        <div className="flex gap-[var(--sp-2xs)] items-center">
          {!editingMission && (
            <Button
              variant="secondary"
              size="md"
              leftIcon={FloppyDisk}
              onClick={() => {
                const draftMission = {
                  ...buildMissionFromForm(),
                  id: `draft-${Date.now()}`,
                  status: "draft" as const,
                };
                onDraft(draftMission);
                toast.success("Rascunho salvo com sucesso!");
                resetForm();
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
                const updated = {
                  ...buildMissionFromForm(editingMission),
                  status: "active" as const,
                };
                onSubmit(updated);
                toast.success("Missão atualizada com sucesso!");
                resetForm();
                onClose();
              } else {
                const newMission = {
                  ...buildMissionFromForm(),
                  status: "active" as const,
                };
                onSubmit(newMission);
                toast.success("Missão criada com sucesso!");
                resetForm();
                onClose();
              }
            }}
          >
            {createStep === 2
              ? editingMission
                ? "Salvar alterações"
                : "Criar missão"
              : "Próximo"}
          </Button>
        </div>
      </ModalFooter>
    </Modal>
  );
}
