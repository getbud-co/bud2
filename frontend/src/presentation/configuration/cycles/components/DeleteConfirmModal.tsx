"use client";

import {
  Modal,
  ModalHeader,
  ModalBody,
  ModalFooter,
  Button,
} from "@getbud-co/buds";
import { Trash } from "@phosphor-icons/react";
import type { Cycle } from "@/types";

interface DeleteConfirmModalProps {
  cycle: Cycle | null;
  onClose: () => void;
  onConfirm: () => void;
  isPending: boolean;
}

export function DeleteConfirmModal({
  cycle,
  onClose,
  onConfirm,
  isPending,
}: DeleteConfirmModalProps) {
  return (
    <Modal open={!!cycle} onClose={onClose} size="sm">
      <ModalHeader title="Excluir ciclo" onClose={onClose} />
      <ModalBody>
        {cycle && (
          <p className="[font-family:var(--font-body)] [font-size:var(--text-sm)] text-[var(--color-neutral-700)] m-0 leading-[1.5]">
            Tem certeza que deseja excluir o ciclo <strong>{cycle.name}</strong>
            ? Missões e indicadores vinculados a este período não serão
            afetados.
          </p>
        )}
      </ModalBody>
      <ModalFooter>
        <Button variant="tertiary" size="md" onClick={onClose}>
          Cancelar
        </Button>
        <Button
          variant="danger"
          size="md"
          leftIcon={Trash}
          disabled={isPending}
          onClick={onConfirm}
        >
          Excluir
        </Button>
      </ModalFooter>
    </Modal>
  );
}
