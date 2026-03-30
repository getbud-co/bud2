import {
  Modal,
  ModalHeader,
  ModalBody,
  ModalFooter,
  Button,
} from "@mdonangelo/bud-ds";
import { Warning } from "@phosphor-icons/react";
import type { SavedView } from "@/contexts/SavedViewsContext";

interface DeleteViewModalProps {
  open: boolean;
  onClose: () => void;
  currentView: SavedView | undefined;
  onConfirm: () => void;
}

export function DeleteViewModal({
  open,
  onClose,
  currentView,
  onConfirm,
}: DeleteViewModalProps) {
  return (
    <Modal open={open} onClose={onClose} size="sm">
      <ModalHeader
        title={
          <span className="flex items-center gap-[var(--sp-2xs)]">
            <Warning
              size={20}
              className="text-[var(--color-red-600)] flex-shrink-0"
            />
            Excluir visualização
          </span>
        }
        description="Esta ação não pode ser desfeita."
        onClose={onClose}
      />
      <ModalBody>
        <p className="m-0 font-[var(--font-body)] font-normal text-[var(--text-sm)] leading-[1.6] text-[var(--color-neutral-700)]">
          A visualização <strong>&quot;{currentView?.name}&quot;</strong> será
          excluída permanentemente. Os filtros salvos serão perdidos.
        </p>
      </ModalBody>
      <ModalFooter>
        <Button variant="tertiary" size="md" onClick={onClose}>
          Cancelar
        </Button>
        <Button variant="danger" size="md" onClick={onConfirm}>
          Excluir visualização
        </Button>
      </ModalFooter>
    </Modal>
  );
}
