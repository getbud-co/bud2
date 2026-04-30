import { Badge, Alert, Card, CardBody, CardDivider } from "@getbud-co/buds";
import type { CalendarDate } from "@getbud-co/buds";
import { Calendar } from "@phosphor-icons/react";
import { MISSION_TEMPLATES, MEASUREMENT_MODES, MANUAL_INDICATOR_TYPES, UNIT_OPTIONS } from "../../../consts";
import type { MissionItemData } from "../types";

interface Step2Props {
  selectedTemplate: string | undefined;
  newMissionName: string;
  newMissionDesc: string;
  selectedMissionOwners: string[];
  missionOwnerOptions: { id: string; label: string; initials?: string | null }[];
  missionPeriod: [CalendarDate | null, CalendarDate | null];
  selectedSupportTeam: string[];
  selectedTags: string[];
  missionTagOptions: { id: string; label: string }[];
  customTags: { id: string; label: string }[];
  selectedVisibility: string;
  newMissionItems: MissionItemData[];
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

function getGoalSummary(item: MissionItemData): string {
  if (item.measurementMode !== "manual" || !item.manualType) return "";
  const unit =
    UNIT_OPTIONS.find((u) => u.value === item.goalUnit)?.label ?? item.goalUnit;
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

function countAllItems(items: MissionItemData[]): number {
  return items.reduce(
    (sum, item) => sum + 1 + countAllItems(item.children ?? []),
    0,
  );
}

function renderReviewItems(items: MissionItemData[], depth: number): React.ReactNode {
  return items.map((item) => {
    const goalSummary = getGoalSummary(item);
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
        className="flex flex-col gap-[var(--sp-2xs)]"
        style={
          depth > 0
            ? { marginLeft: `calc(${depth} * var(--sp-lg))` }
            : undefined
        }
      >
        <div className="flex flex-col gap-[var(--sp-3xs)] px-[var(--sp-sm)] py-[var(--sp-xs)] bg-[var(--color-caramel-50)] rounded-[var(--radius-xs)]">
          <div className="flex items-center gap-[var(--sp-2xs)]">
            <span className="[font-family:var(--font-label)] font-medium text-[var(--text-xs)] text-[var(--color-neutral-950)] leading-[1.15]">
              {item.name}
            </span>
            {badgeText && <Badge color="neutral">{badgeText}</Badge>}
          </div>
          {item.description && (
            <span className="[font-family:var(--font-body)] text-[var(--text-xs)] text-[var(--color-neutral-500)] leading-[1.3]">
              {item.description}
            </span>
          )}
          <div className="flex items-center gap-[var(--sp-2xs)] flex-wrap">
            {goalSummary && <Badge color="orange">{goalSummary}</Badge>}
            {item.period[0] && item.period[1] && (
              <span className="flex items-center gap-[var(--sp-3xs)] [font-family:var(--font-body)] text-[var(--text-xs)] text-[var(--color-neutral-500)] leading-[1.3]">
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

export function Step2({
  selectedTemplate,
  newMissionName,
  newMissionDesc,
  selectedMissionOwners,
  missionOwnerOptions,
  missionPeriod,
  selectedSupportTeam,
  selectedTags,
  missionTagOptions,
  customTags,
  selectedVisibility,
  newMissionItems,
}: Step2Props) {
  return (
    <div className="flex flex-col gap-[var(--sp-sm)]">
      <p className="m-0 [font-family:var(--font-heading)] font-semibold text-[var(--text-sm)] text-[var(--color-neutral-950)] leading-[1.25]">
        Revisão da missão
      </p>

      {/* Mission header info */}
      <Card padding="sm">
        <CardBody>
          <div className="flex flex-col gap-[var(--sp-xs)]">
            <div className="flex items-baseline gap-[var(--sp-sm)]">
              <span className="shrink-0 w-[120px] [font-family:var(--font-label)] font-medium text-[var(--text-xs)] text-[var(--color-neutral-500)] leading-[1.4]">
                Template
              </span>
              <span className="[font-family:var(--font-body)] text-[var(--text-xs)] text-[var(--color-neutral-950)] leading-[1.4]">
                {MISSION_TEMPLATES.find(
                  (t) => t.value === selectedTemplate,
                )?.title ?? "—"}
              </span>
            </div>
            <div className="flex items-baseline gap-[var(--sp-sm)]">
              <span className="shrink-0 w-[120px] [font-family:var(--font-label)] font-medium text-[var(--text-xs)] text-[var(--color-neutral-500)] leading-[1.4]">
                Nome
              </span>
              <span className="[font-family:var(--font-body)] text-[var(--text-xs)] text-[var(--color-neutral-950)] leading-[1.4]">
                {newMissionName || "—"}
              </span>
            </div>
            {newMissionDesc && (
              <div className="flex items-baseline gap-[var(--sp-sm)]">
                <span className="shrink-0 w-[120px] [font-family:var(--font-label)] font-medium text-[var(--text-xs)] text-[var(--color-neutral-500)] leading-[1.4]">
                  Descrição
                </span>
                <span className="[font-family:var(--font-body)] text-[var(--text-xs)] text-[var(--color-neutral-950)] leading-[1.4]">
                  {newMissionDesc}
                </span>
              </div>
            )}
            <div className="flex items-baseline gap-[var(--sp-sm)]">
              <span className="shrink-0 w-[120px] [font-family:var(--font-label)] font-medium text-[var(--text-xs)] text-[var(--color-neutral-500)] leading-[1.4]">
                Responsável
              </span>
              <span className="[font-family:var(--font-body)] text-[var(--text-xs)] text-[var(--color-neutral-950)] leading-[1.4]">
                {selectedMissionOwners.length > 0
                  ? selectedMissionOwners
                      .map(
                        (id) =>
                          missionOwnerOptions.find((o) => o.id === id)?.label ??
                          id,
                      )
                      .join(", ")
                  : "Nenhum definido"}
              </span>
            </div>
            <div className="flex items-baseline gap-[var(--sp-sm)]">
              <span className="shrink-0 w-[120px] [font-family:var(--font-label)] font-medium text-[var(--text-xs)] text-[var(--color-neutral-500)] leading-[1.4]">
                Período
              </span>
              <span className="[font-family:var(--font-body)] text-[var(--text-xs)] text-[var(--color-neutral-950)] leading-[1.4]">
                {missionPeriod[0] && missionPeriod[1]
                  ? `${String(missionPeriod[0].day).padStart(2, "0")}/${String(missionPeriod[0].month).padStart(2, "0")}/${missionPeriod[0].year} — ${String(missionPeriod[1].day).padStart(2, "0")}/${String(missionPeriod[1].month).padStart(2, "0")}/${missionPeriod[1].year}`
                  : "Não definido"}
              </span>
            </div>
            {selectedSupportTeam.length > 0 && (
              <div className="flex items-baseline gap-[var(--sp-sm)]">
                <span className="shrink-0 w-[120px] [font-family:var(--font-label)] font-medium text-[var(--text-xs)] text-[var(--color-neutral-500)] leading-[1.4]">
                  Time de apoio
                </span>
                <span className="[font-family:var(--font-body)] text-[var(--text-xs)] text-[var(--color-neutral-950)] leading-[1.4]">
                  {selectedSupportTeam
                    .map(
                      (id) =>
                        missionOwnerOptions.find((o) => o.id === id)?.label ??
                        id,
                    )
                    .join(", ")}
                </span>
              </div>
            )}
            {selectedTags.length > 0 && (
              <div className="flex items-baseline gap-[var(--sp-sm)]">
                <span className="shrink-0 w-[120px] [font-family:var(--font-label)] font-medium text-[var(--text-xs)] text-[var(--color-neutral-500)] leading-[1.4]">
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
              <span className="shrink-0 w-[120px] [font-family:var(--font-label)] font-medium text-[var(--text-xs)] text-[var(--color-neutral-500)] leading-[1.4]">
                Visibilidade
              </span>
              <span className="[font-family:var(--font-body)] text-[var(--text-xs)] text-[var(--color-neutral-950)] leading-[1.4]">
                {VISIBILITY_OPTIONS.find(
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
            <span className="[font-family:var(--font-heading)] font-semibold text-[var(--text-sm)] text-[var(--color-neutral-950)] leading-[1.15]">
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
  );
}
