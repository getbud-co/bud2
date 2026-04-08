import {
  Modal,
  ModalHeader,
  ModalBody,
  ModalFooter,
  Button,
} from "@mdonangelo/bud-ds";
import { Trash } from "@phosphor-icons/react";
import type { Team } from "@/types";

interface DeleteTeamModalProps {
  team: Team | null;
  onClose: () => void;
  onConfirm: () => void;
}

export function DeleteTeamModal({
  team,
  onClose,
  onConfirm,
}: DeleteTeamModalProps) {
  return (
    <Modal open={!!team} onClose={onClose} size="sm">
      {team && (
        <>
          <ModalHeader title="Excluir time" onClose={onClose} />
          <ModalBody>
            <p className="font-[var(--font-body)] text-[var(--text-sm)] text-[var(--color-neutral-700)] m-0 leading-[1.5]">
              Tem certeza que deseja excluir o time{" "}
              <strong>{team.name}</strong>? Esta ação removerá o time de todos
              os colaboradores associados e não pode ser desfeita.
            </p>
          </ModalBody>
          <ModalFooter>
            <Button variant="secondary" size="md" onClick={onClose}>
              Cancelar
            </Button>
            <Button variant="danger" size="md" leftIcon={Trash} onClick={onConfirm}>
              Excluir
            </Button>
          </ModalFooter>
        </>
      )}
    </Modal>
  );
}
