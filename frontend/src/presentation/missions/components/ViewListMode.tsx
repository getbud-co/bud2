import type { MutableRefObject } from "react";
import { CardBody } from "@mdonangelo/bud-ds";
import type { Mission, Indicator, MissionTask, CheckinPayload } from "@/types";
import { MissionItem } from "./MissionItem";

interface ViewListModeProps {
  missions: Mission[];
  expandedMissions: Set<string>;
  onToggle: (id: string) => void;
  onExpand: (mission: Mission) => void;
  onEdit: (mission: Mission) => void;
  onDelete: (mission: Mission) => void;
  onCheckin: (payload: CheckinPayload) => void;
  onToggleTask: (taskId: string) => void;
  onOpenTaskDrawer: (task: MissionTask, parentLabel: string) => void;
  openRowMenu: string | null;
  setOpenRowMenu: (id: string | null) => void;
  openContributeFor: string | null;
  setOpenContributeFor: (id: string | null) => void;
  contributePickerSearch: string;
  setContributePickerSearch: (s: string) => void;
  rowMenuBtnRefs: MutableRefObject<Record<string, HTMLButtonElement | null>>;
  allMissions: { id: string; title: string }[];
  onAddContribution: (
    item: Indicator | MissionTask,
    itemType: "indicator" | "task",
    sourceMissionId: string,
    sourceMissionTitle: string,
    targetMissionId: string,
    targetMissionTitle: string,
  ) => void;
  onRemoveContribution: (
    itemId: string,
    itemType: "indicator" | "task",
    targetMissionId: string,
    targetMissionTitle: string,
  ) => void;
}

export function ViewListMode({
  missions,
  expandedMissions,
  onToggle,
  onExpand,
  onEdit,
  onDelete,
  onCheckin,
  onToggleTask,
  onOpenTaskDrawer,
  openRowMenu,
  setOpenRowMenu,
  openContributeFor,
  setOpenContributeFor,
  contributePickerSearch,
  setContributePickerSearch,
  rowMenuBtnRefs,
  allMissions,
  onAddContribution,
  onRemoveContribution,
}: ViewListModeProps) {
  return (
    <CardBody>
      <div className="flex flex-col gap-3">
        {missions.map((mission) => (
          <MissionItem
            key={mission.id}
            mission={mission}
            isOpen={expandedMissions.has(mission.id)}
            onToggle={onToggle}
            onExpand={onExpand}
            onEdit={onEdit}
            onDelete={onDelete}
            onCheckin={onCheckin}
            onToggleTask={onToggleTask}
            onOpenTaskDrawer={onOpenTaskDrawer}
            expandedMissions={expandedMissions}
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
        ))}
      </div>
    </CardBody>
  );
}
