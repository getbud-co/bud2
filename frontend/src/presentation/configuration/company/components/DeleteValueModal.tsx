"use client";

import {
  Modal,
  ModalHeader,
  ModalBody,
  ModalFooter,
  Button,
} from "@mdonangelo/bud-ds";
import { Trash } from "@phosphor-icons/react";

interface DeleteValueModalProps {
  valueId: string | null;
  onClose: () => void;
}

export function DeleteValueModal({ valueId, onClose }: DeleteValueModalProps) {
  function handleConfirm() {
    // TODO: DELETE /api/organizations/:orgId/values/:valueId
    onClose();
  }

  return (
    <Modal open={!!valueId} onClose={onClose} size="sm">
      <ModalHeader title="Excluir valor" onClose={onClose} />
      <ModalBody>
        <p className="font-[var(--font-body)] text-[var(--text-sm)] text-[var(--color-neutral-700)] m-0 leading-[1.5]">
          Tem certeza que deseja excluir este valor? Esta ação não pode ser
          desfeita.
        </p>
      </ModalBody>
      <ModalFooter>
        <Button variant="tertiary" size="md" onClick={onClose}>
          Cancelar
        </Button>
        <Button
          variant="danger"
          size="md"
          leftIcon={Trash}
          onClick={handleConfirm}
        >
          Excluir
        </Button>
      </ModalFooter>
    </Modal>
  );
}
