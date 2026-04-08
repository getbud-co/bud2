"use client";

import { useState, useEffect } from "react";
import {
  Modal,
  ModalHeader,
  ModalBody,
  ModalFooter,
  Button,
  Input,
  Textarea,
} from "@mdonangelo/bud-ds";

interface ValueFormModalProps {
  open: boolean;
  editingId: string | null;
  onClose: () => void;
}

export function ValueFormModal({
  open,
  editingId,
  onClose,
}: ValueFormModalProps) {
  const [name, setName] = useState("");
  const [description, setDescription] = useState("");

  useEffect(() => {
    if (!open) return;
    if (!editingId) {
      setName("");
      setDescription("");
      return;
    }
    // TODO: fetch GET /api/organizations/:orgId/values/:editingId
  }, [open, editingId]);

  function handleSave() {
    // TODO: POST /api/organizations/:orgId/values ou PATCH /:editingId
    onClose();
  }

  return (
    <Modal open={open} onClose={onClose} size="md">
      <ModalHeader
        title={editingId ? "Editar valor" : "Novo valor da empresa"}
        onClose={onClose}
      />
      <ModalBody>
        <div className="flex flex-col gap-[var(--sp-md)]">
          <Input
            label="Nome do valor"
            value={name}
            onChange={(e: React.ChangeEvent<HTMLInputElement>) =>
              setName(e.target.value)
            }
            placeholder="Ex: Inovação, Colaboração, Transparência..."
          />
          <Textarea
            label="Descrição"
            value={description}
            onChange={(e: React.ChangeEvent<HTMLTextAreaElement>) =>
              setDescription(e.target.value)
            }
            placeholder="Descreva o que este valor representa para a empresa e como ele se manifesta no dia a dia..."
            rows={4}
          />
        </div>
      </ModalBody>
      <ModalFooter>
        <Button variant="tertiary" size="md" onClick={onClose}>
          Cancelar
        </Button>
        <Button
          variant="primary"
          size="md"
          disabled={!name.trim()}
          onClick={handleSave}
        >
          {editingId ? "Salvar" : "Criar valor"}
        </Button>
      </ModalFooter>
    </Modal>
  );
}
