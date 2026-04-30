import { Button, Modal, ModalBody, ModalFooter, ModalHeader } from "@getbud-co/buds";
import { Warning } from "@phosphor-icons/react";

interface DeleteViewModalProps {
  open: boolean;
  viewName: string | undefined;
  onClose: () => void;
  onConfirm: () => void;
}

export function DeleteViewModal({
  open,
  viewName,
  onClose,
  onConfirm,
}: DeleteViewModalProps) {
  return (
    <Modal open={open} onClose={onClose} size="sm">
      <ModalHeader
        title={
          <span className="flex items-center gap-[var(--sp-2xs)]">
            <Warning size={20} className="text-[var(--color-red-600)] shrink-0" />
            Excluir visualização
          </span>
        }
        description="Esta ação não pode ser desfeita."
        onClose={onClose}
      />
      <ModalBody>
        <p className="font-[var(--font-body)] font-normal text-[var(--text-sm)] leading-[1.6] text-[var(--color-neutral-700)] m-0">
          A visualização <strong>&quot;{viewName}&quot;</strong> será excluída
          permanentemente. Os filtros salvos serão perdidos.
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
