import { Button, Modal, ModalBody, ModalFooter, ModalHeader } from "@getbud-co/buds";
import { Trash } from "@phosphor-icons/react";

interface RemoveContribTarget {
  itemId: string;
  itemType: "indicator" | "task";
  targetMissionId: string;
  targetMissionTitle: string;
}

interface RemoveContribModalProps {
  target: RemoveContribTarget | null;
  onClose: () => void;
  onConfirm: (itemId: string, itemType: "indicator" | "task", targetMissionId: string) => void;
}

export type { RemoveContribTarget };

export function RemoveContribModal({
  target,
  onClose,
  onConfirm,
}: RemoveContribModalProps) {
  return (
    <Modal open={!!target} onClose={onClose} size="sm">
      <ModalHeader
        title="Remover contribuição"
        description="Esta ação não pode ser desfeita."
        onClose={onClose}
      />
      <ModalBody>
        <p className="font-[var(--font-body)] text-[var(--text-sm)] text-[var(--color-neutral-700)] m-0 leading-[1.5]">
          Tem certeza que deseja remover a contribuição para{" "}
          <strong>{target?.targetMissionTitle}</strong>?
        </p>
      </ModalBody>
      <ModalFooter>
        <Button variant="secondary" size="md" onClick={onClose}>
          Cancelar
        </Button>
        <Button
          variant="danger"
          size="md"
          leftIcon={Trash}
          onClick={() => {
            if (!target) return;
            onConfirm(target.itemId, target.itemType, target.targetMissionId);
            onClose();
          }}
        >
          Remover
        </Button>
      </ModalFooter>
    </Modal>
  );
}
