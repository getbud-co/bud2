import { useEffect, useState } from "react";
import {
  Badge,
  Button,
  Input,
  Modal,
  ModalBody,
  ModalFooter,
  ModalHeader,
} from "@getbud-co/buds";
import { COLOR_OPTIONS, type TagView } from "../types";

interface Props {
  open: boolean;
  editingTag: TagView | null;
  onClose: () => void;
  onSave: (name: string, color: string) => void;
}

export function TagFormModal({ open, editingTag, onClose, onSave }: Props) {
  const [formName, setFormName] = useState("");
  const [formColor, setFormColor] = useState("neutral");

  useEffect(() => {
    if (open) {
      setFormName(editingTag?.name ?? "");
      setFormColor(editingTag?.color ?? "neutral");
    }
  }, [open, editingTag]);

  return (
    <Modal open={open} onClose={onClose} size="sm">
      <ModalHeader
        title={editingTag ? "Editar tag" : "Nova tag"}
        onClose={onClose}
      />
      <ModalBody>
        <div className="flex flex-col gap-[var(--sp-md)]">
          <Input
            label="Nome"
            value={formName}
            onChange={(e) => setFormName(e.target.value)}
            placeholder="Nome da tag"
          />

          <div>
            <label className="block font-[var(--font-label)] text-[length:var(--text-sm)] text-[var(--color-neutral-700)] mb-[var(--sp-2xs)]">
              Cor
            </label>
            <div className="flex flex-wrap gap-[var(--sp-2xs)]">
              {COLOR_OPTIONS.map((c) => (
                <button
                  key={c.value}
                  type="button"
                  className={`border-2 rounded-[var(--radius-sm)] bg-transparent p-[var(--sp-3xs)] cursor-pointer transition-colors duration-150 hover:border-[var(--color-caramel-300)] ${formColor === c.value ? "border-[var(--color-orange-500)]" : "border-transparent"}`}
                  onClick={() => setFormColor(c.value)}
                  title={c.label}
                >
                  <Badge color={c.value as "neutral"}>{c.label}</Badge>
                </button>
              ))}
            </div>
          </div>
        </div>
      </ModalBody>
      <ModalFooter>
        <Button variant="tertiary" size="md" onClick={onClose}>
          Cancelar
        </Button>
        <Button
          variant="primary"
          size="md"
          disabled={!formName.trim()}
          onClick={() => onSave(formName, formColor)}
        >
          {editingTag ? "Salvar" : "Criar tag"}
        </Button>
      </ModalFooter>
    </Modal>
  );
}
