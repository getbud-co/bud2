import {
  Card,
  CardBody,
  Chart,
  Badge,
  MissionProgressBar,
  AvatarGroup,
  Button,
} from "@mdonangelo/bud-ds";
import { PencilSimple, Trash, ArrowsOutSimple } from "@phosphor-icons/react";
import type { Mission } from "@/types";
import type { OwnerOption } from "@/contexts/PeopleDataContext";
import { getOwnerInitials } from "@/lib/tempStorage/missions";

interface ViewCardsModeProps {
  missions: Mission[];
  ownerFilterOptions: Array<
    OwnerOption | { id: string; label: string; initials: string }
  >;
  onExpand: (mission: Mission) => void;
  onEdit: (mission: Mission) => void;
  onDelete: (mission: Mission) => void;
}

export function ViewCardsMode({
  missions,
  ownerFilterOptions,
  onExpand,
  onEdit,
  onDelete,
}: ViewCardsModeProps) {
  return (
    <CardBody>
      <div className="grid grid-cols-[repeat(auto-fill,minmax(320px,1fr))] gap-4">
        {missions.map((mission) => {
          const mIndicators = mission.indicators ?? [];
          const totalIndicators =
            mIndicators.length +
            (mission.children ?? []).reduce(
              (acc, c) => acc + (c.indicators ?? []).length,
              0,
            );
          return (
            <Card
              key={mission.id}
              padding="sm"
              className={`flex flex-col transition-colors duration-[120ms] hover:border-[var(--color-caramel-300)] cursor-pointer hover:bg-[var(--color-caramel-100)] ${mission.status === "draft" ? "border-dashed border-[var(--color-caramel-300)] opacity-85" : ""}`}
              onClick={() => onExpand(mission)}
            >
              <CardBody>
                <div className="flex items-start gap-4">
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
                {mIndicators.length > 0 && (
                  <div className="mt-3">
                    <MissionProgressBar
                      label=""
                      value={mission.progress}
                      target={100}
                      formattedValue={`${mission.progress}%`}
                    />
                  </div>
                )}
                <div className="flex items-center justify-between mt-3">
                  <AvatarGroup
                    size="xs"
                    avatars={[
                      ...new Set(
                        mIndicators.map((indicator) =>
                          getOwnerInitials(indicator.owner),
                        ),
                      ),
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
        })}
      </div>
    </CardBody>
  );
}
