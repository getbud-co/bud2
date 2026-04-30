import { useRef, useState } from "react";
import { Modal, ModalBody, ModalHeader } from "@getbud-co/buds";
import type { Mission } from "@/types";
import MissionItem from "./MissionItem";

// ── Helpers ───────────────────────────────────────────────────────────────────

export function collectMissionIds(mission: Mission): string[] {
  const ids = [mission.id];
  for (const child of mission.children ?? []) {
    ids.push(...collectMissionIds(child));
  }
  return ids;
}

// ── Internal ─────────────────────────────────────────────────────────────────

function ModalMissionContent({
  mission,
  onExpand,
  onEdit,
  onDelete,
  onToggleTask,
  onToggleSubtask,
  allMissions = [],
}: {
  mission: Mission;
  onExpand: (mission: Mission) => void;
  onEdit: (mission: Mission) => void;
  onDelete?: (mission: Mission) => void;
  onToggleTask?: (taskId: string) => void;
  onToggleSubtask?: (taskId: string, subtaskId: string) => void;
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
      onToggleTask={onToggleTask}
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
      onToggleSubtask={onToggleSubtask}
    />
  );
}

// ── Component ─────────────────────────────────────────────────────────────────

interface ExpandedMissionModalProps {
  mission: Mission | null;
  onClose: () => void;
  onExpand: (mission: Mission) => void;
  onEdit: (mission: Mission) => void;
  onDelete: (mission: Mission) => void;
  onToggleTask: (taskId: string) => void;
  onToggleSubtask: (taskId: string, subtaskId: string) => void;
  allMissions: { id: string; title: string }[];
}

export function ExpandedMissionModal({
  mission,
  onClose,
  onExpand,
  onEdit,
  onDelete,
  onToggleTask,
  onToggleSubtask,
  allMissions,
}: ExpandedMissionModalProps) {
  return (
    <Modal
      key="expanded-mission-modal"
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
              onToggleTask={onToggleTask}
              onToggleSubtask={onToggleSubtask}
              allMissions={allMissions}
            />
          </ModalBody>
        </>
      )}
    </Modal>
  );
}
