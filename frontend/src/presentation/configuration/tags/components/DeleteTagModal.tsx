import {
  Button,
  Modal,
  ModalBody,
  ModalFooter,
  ModalHeader,
} from "@getbud-co/buds";
import { Trash } from "@phosphor-icons/react";
import type { TagView } from "../types";

interface Props {
  tag: TagView | null;
  onClose: () => void;
  onConfirm: () => void;
}

export function DeleteTagModal({ tag, onClose, onConfirm }: Props) {
  return (
    <Modal open={!!tag} onClose={onClose} size="sm">
      <ModalHeader title="Excluir tag" onClose={onClose} />
      <ModalBody>
        {tag && (
          <p className="font-[var(--font-body)] text-[length:var(--text-sm)] text-[var(--color-neutral-700)] m-0 leading-relaxed">
            Tem certeza que deseja excluir a tag <strong>{tag.name}</strong>?
            {tag.linkedItems > 0 &&
              ` Ela está vinculada a ${tag.linkedItems} itens.`}
          </p>
        )}
      </ModalBody>
      <ModalFooter>
        <Button variant="tertiary" size="md" onClick={onClose}>
          Cancelar
        </Button>
        <Button variant="danger" size="md" leftIcon={Trash} onClick={onConfirm}>
          Excluir
        </Button>
      </ModalFooter>
    </Modal>
  );
}
