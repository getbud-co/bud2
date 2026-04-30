import { Fragment } from "react";
import {
  AvatarGroup,
  Badge,
  Button,
  Card,
  CardBody,
  Chart,
} from "@getbud-co/buds";
import { ArrowsOutSimple, PencilSimple, Trash } from "@phosphor-icons/react";
import type { Mission } from "@/types";
import { getOwnerInitials } from "@/lib/tempStorage/missions";

// ── Types ─────────────────────────────────────────────────────────────────────

interface OwnerOption {
  id: string;
  label: string;
  initials?: string;
}

interface MissionGroup {
  teamName: string;
  teamColor: string;
  missions: Mission[];
}

interface ViewModeCardsProps {
  displayedMissions: Mission[];
  groupedMissions: MissionGroup[] | null;
  ownerFilterOptions: OwnerOption[];
  onExpand: (mission: Mission) => void;
  onEdit: (mission: Mission) => void;
  onDelete: (mission: Mission) => void;
}

// ── Internal components ───────────────────────────────────────────────────────

function TeamSectionHeader({
  teamName,
  teamColor,
  missions: groupMissions,
}: {
  teamName: string;
  teamColor: string;
  missions: Mission[];
}) {
  const avgProgress =
    groupMissions.length > 0
      ? Math.round(
          groupMissions.reduce((a, m) => a + m.progress, 0) /
            groupMissions.length,
        )
      : 0;
  return (
    <div className="flex items-center gap-[var(--sp-xs)] pt-[var(--sp-sm)] pb-[var(--sp-2xs)]">
      <span
        className="w-[10px] h-[10px] rounded-full shrink-0"
        style={{ backgroundColor: `var(--color-${teamColor}-500)` }}
      />
      <span className="font-[var(--font-heading)] text-[var(--text-sm)] font-semibold text-[var(--color-neutral-900)] whitespace-nowrap">
        {teamName}
      </span>
      <span className="font-[var(--font-label)] text-[var(--text-xs)] text-[var(--color-neutral-500)] whitespace-nowrap">
        {groupMissions.length}{" "}
        {groupMissions.length === 1 ? "missão" : "missões"} · {avgProgress}%
      </span>
      <div className="flex-1 h-px bg-[var(--color-caramel-200)]" />
    </div>
  );
}

// ── Component ─────────────────────────────────────────────────────────────────

export function ViewModeCards({
  displayedMissions,
  groupedMissions,
  ownerFilterOptions,
  onExpand,
  onEdit,
  onDelete,
}: ViewModeCardsProps) {
  function renderCard(mission: Mission) {
    const mKRs = mission.indicators ?? [];
    const totalIndicators =
      mKRs.length +
      (mission.children ?? []).reduce(
        (acc, c) => acc + (c.indicators ?? []).length,
        0,
      );

    return (
      <Card
        key={mission.id}
        padding="sm"
        className={`flex flex-col transition-[border-color] duration-[120ms] ease-in-out cursor-pointer hover:bg-[var(--color-caramel-100)]${
          mission.status === "draft"
            ? " border-dashed border-[var(--color-caramel-300)] opacity-[0.85] hover:border-[var(--color-caramel-500)]"
            : " hover:border-[var(--color-caramel-300)]"
        }`}
        onClick={() => onExpand(mission)}
      >
        <CardBody>
          <div className="flex items-start gap-[var(--sp-sm)]">
            <Chart value={mission.progress} size={48} />
            <div className="flex-1 min-w-0 flex flex-col gap-[2px]">
              <span className="font-[var(--font-heading)] text-[var(--text-sm)] font-bold text-[var(--color-neutral-950)] leading-[1.25] line-clamp-2">
                {mission.title}
              </span>
              <span className="font-[var(--font-body)] text-[var(--text-xs)] text-[var(--color-neutral-500)] leading-[1.3]">
                {totalIndicators}{" "}
                {totalIndicators === 1 ? "indicador" : "indicadores"}
                {(mission.children?.length ?? 0) > 0 &&
                  ` · ${mission.children!.length} sub-${mission.children!.length === 1 ? "missão" : "missões"}`}
              </span>
            </div>
            {mission.status === "draft" && (
              <Badge color="caramel">Rascunho</Badge>
            )}
          </div>
          <div className="flex items-center justify-between mt-[var(--sp-xs)]">
            <AvatarGroup
              size="xs"
              avatars={[
                ...new Set(mKRs.map((kr) => getOwnerInitials(kr.owner))),
              ].map((initials) => ({
                initials,
                alt:
                  ownerFilterOptions.find((o) => o.initials === initials)
                    ?.label ?? initials,
              }))}
              maxVisible={4}
            />
            {/* eslint-disable-next-line jsx-a11y/click-events-have-key-events, jsx-a11y/no-static-element-interactions */}
            <div
              className="flex items-center gap-[2px]"
              onClick={(e) => e.stopPropagation()}
            >
              <Button
                variant="tertiary"
                size="sm"
                leftIcon={PencilSimple}
                aria-label="Editar missão"
                onClick={() => onEdit(mission)}
              />
              <Button
                variant="tertiary"
                size="sm"
                leftIcon={Trash}
                aria-label="Excluir missão"
                onClick={() => onDelete(mission)}
              />
              <Button
                variant="tertiary"
                size="sm"
                leftIcon={ArrowsOutSimple}
                aria-label="Expandir missão"
                onClick={() => onExpand(mission)}
              />
            </div>
          </div>
        </CardBody>
      </Card>
    );
  }

  return (
    <div className="grid grid-cols-[repeat(auto-fill,minmax(320px,1fr))] gap-[var(--sp-sm)] max-[480px]:grid-cols-1">
      {groupedMissions
        ? groupedMissions.map((group) => (
            <Fragment key={group.teamName}>
              <div className="col-span-full">
                <TeamSectionHeader
                  teamName={group.teamName}
                  teamColor={group.teamColor}
                  missions={group.missions}
                />
              </div>
              {group.missions.map((mission) => renderCard(mission))}
            </Fragment>
          ))
        : displayedMissions.map((mission) => renderCard(mission))}
    </div>
  );
}
