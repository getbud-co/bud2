import { Modal, ModalHeader, ModalBody } from "@getbud-co/buds";
import type { Mission, CheckinPayload, MissionTask } from "@/types";
import type { MissionItemProps } from "./MissionItem";
import { ModalMissionContent } from "./MissionItemContent";

interface ExpandedMissionModalProps {
  overlayKey: number;
  mission: Mission | null;
  onClose: () => void;
  onExpand: (mission: Mission) => void;
  onEdit: (mission: Mission) => void;
  onDelete: (mission: Mission) => void;
  onCheckin: (payload: CheckinPayload) => void;
  onToggleTask: (taskId: string) => void;
  onOpenTaskDrawer: (task: MissionTask, parentLabel: string) => void;
  onAddContribution: MissionItemProps["onAddContribution"];
  onRemoveContribution: MissionItemProps["onRemoveContribution"];
  allMissions: { id: string; title: string }[];
}

export function ExpandedMissionModal({
  overlayKey,
  mission,
  onClose,
  onExpand,
  onEdit,
  onDelete,
  onCheckin,
  onToggleTask,
  onOpenTaskDrawer,
  onAddContribution,
  onRemoveContribution,
  allMissions,
}: ExpandedMissionModalProps) {
  return (
    <Modal
      key={`expanded-mission-${overlayKey}`}
      open={mission !== null}
      onClose={onClose}
      size="lg"
    >
      {mission && (
        <>
          <ModalHeader title={mission.title} onClose={onClose} />
          <ModalBody>
            <ModalMissionContent
              mission={mission}
              onExpand={onExpand}
              onEdit={onEdit}
              onDelete={onDelete}
              onCheckin={onCheckin}
              onToggleTask={onToggleTask}
              onOpenTaskDrawer={onOpenTaskDrawer}
              onAddContribution={onAddContribution}
              onRemoveContribution={onRemoveContribution}
              allMissions={allMissions}
            />
          </ModalBody>
        </>
      )}
    </Modal>
  );
}
