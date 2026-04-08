"use client";

import {
  Modal,
  ModalHeader,
  ModalBody,
  ModalFooter,
  Button,
} from "@mdonangelo/bud-ds";
import type { PeopleUserView } from "@/contexts/PeopleDataContext";

interface ToggleStatusModalProps {
  user: PeopleUserView | null;
  onClose: () => void;
  onConfirm: () => void;
}

export function ToggleStatusModal({
  user,
  onClose,
  onConfirm,
}: ToggleStatusModalProps) {
  return (
    <Modal open={!!user} onClose={onClose} size="sm">
      <ModalHeader
        title={
          user?.status === "active" ? "Desativar usuário" : "Ativar usuário"
        }
        onClose={onClose}
      />
      <ModalBody>
        {user && (
          <p className="font-[var(--font-body)] text-[var(--text-sm)] text-[var(--color-neutral-700)] m-0 leading-[1.5]">
            {user.status === "active" ? (
              <>
                Tem certeza que deseja desativar{" "}
                <strong>{user.fullName}</strong>? O usuário perderá acesso à
                plataforma.
              </>
            ) : (
              <>
                Tem certeza que deseja ativar <strong>{user.fullName}</strong>?
                O usuário voltará a ter acesso à plataforma.
              </>
            )}
          </p>
        )}
      </ModalBody>
      <ModalFooter>
        <Button variant="tertiary" size="md" onClick={onClose}>
          Cancelar
        </Button>
        <Button
          variant={user?.status === "active" ? "danger" : "primary"}
          size="md"
          onClick={onConfirm}
        >
          {user?.status === "active" ? "Desativar" : "Ativar"}
        </Button>
      </ModalFooter>
    </Modal>
  );
}
