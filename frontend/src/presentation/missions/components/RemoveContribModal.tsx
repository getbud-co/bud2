import {
  Modal,
  ModalHeader,
  ModalBody,
  ModalFooter,
  Button,
} from "@getbud-co/buds";
import { Trash } from "@phosphor-icons/react";

interface RemoveContribConfirm {
  itemId: string;
  itemType: "indicator" | "task";
  targetMissionId: string;
  targetMissionTitle: string;
}

interface RemoveContribModalProps {
  confirm: RemoveContribConfirm | null;
  onClose: () => void;
  onConfirm: (
    itemId: string,
    itemType: "indicator" | "task",
    targetMissionId: string,
  ) => void;
}

export function RemoveContribModal({
  confirm,
  onClose,
  onConfirm,
}: RemoveContribModalProps) {
  return (
    <Modal open={!!confirm} onClose={onClose} size="sm">
      <ModalHeader
        title="Remover contribuição"
        description="Esta ação não pode ser desfeita."
        onClose={onClose}
      />
      <ModalBody>
        <p className="m-0 font-[var(--font-body)] text-[var(--text-sm)] leading-[1.5] text-[var(--color-neutral-700)]">
          Tem certeza que deseja remover a contribuição para{" "}
          <strong>{confirm?.targetMissionTitle}</strong>?
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
            if (!confirm) return;
            onConfirm(
              confirm.itemId,
              confirm.itemType,
              confirm.targetMissionId,
            );
            onClose();
          }}
        >
          Remover
        </Button>
      </ModalFooter>
    </Modal>
  );
}
