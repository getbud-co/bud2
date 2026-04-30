import { CheckinPayload, Mission, MissionTask } from "@/types";
import { useRef, useState } from "react";
import { MissionItem, MissionItemProps } from "./MissionItem";
import { collectMissionIds } from "../utils/utils";

export function ModalMissionContent({
  mission,
  onExpand,
  onEdit,
  onDelete,
  onCheckin,
  onToggleTask,
  onOpenTaskDrawer,
  onAddContribution,
  onRemoveContribution,
  allMissions = [],
}: {
  mission: Mission;
  onExpand: (mission: Mission) => void;
  onEdit: (mission: Mission) => void;
  onDelete?: (mission: Mission) => void;
  onCheckin?: (payload: CheckinPayload) => void;
  onToggleTask?: (taskId: string) => void;
  onOpenTaskDrawer?: (task: MissionTask, parentLabel: string) => void;
  onAddContribution?: MissionItemProps["onAddContribution"];
  onRemoveContribution?: MissionItemProps["onRemoveContribution"];
  allMissions?: { id: string; title: string }[];
}) {
  const [modalExpanded, setModalExpanded] = useState<Set<string>>(
    () => new Set(collectMissionIds(mission)),
  );
  const [openRowMenu, setOpenRowMenu] = useState<string | null>(null);
  const [openContributeFor, setOpenContributeFor] = useState<string | null>(
    null,
  );
  const [contributePickerSearch, setContributePickerSearch] = useState("");
  const rowMenuBtnRefs = useRef<Record<string, HTMLButtonElement | null>>({});

  function toggleModal(id: string) {
    setModalExpanded((prev) => {
      const next = new Set(prev);
      if (next.has(id)) next.delete(id);
      else next.add(id);
      return next;
    });
  }

  return (
    <MissionItem
      mission={mission}
      isOpen={modalExpanded.has(mission.id)}
      onToggle={toggleModal}
      onExpand={onExpand}
      onEdit={onEdit}
      onDelete={onDelete}
      onCheckin={onCheckin}
      onToggleTask={onToggleTask}
      onOpenTaskDrawer={onOpenTaskDrawer}
      expandedMissions={modalExpanded}
      hideExpand
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
}
