import {
  Alert,
  Button,
  Modal,
  ModalBody,
  ModalFooter,
  ModalHeader,
} from "@getbud-co/buds";
import { Trash, Warning } from "@phosphor-icons/react";
import type { Mission } from "@/types";

interface DeleteMissionModalProps {
  mission: Mission | null;
  onClose: () => void;
  onConfirm: () => void;
}

export function DeleteMissionModal({
  mission,
  onClose,
  onConfirm,
}: DeleteMissionModalProps) {
  const krs = mission?.indicators ?? [];
  const tasks = mission?.tasks ?? [];
  const children = mission?.children ?? [];
  const totalItems = krs.length + tasks.length + children.length;

  return (
    <Modal open={!!mission} onClose={onClose} size="sm">
      <ModalHeader
        title={
          <span className="flex items-center gap-[var(--sp-2xs)]">
            <Warning
              size={20}
              className="text-[var(--color-red-600)] shrink-0"
            />
            Excluir missão
          </span>
        }
        description="Esta ação não pode ser desfeita."
        onClose={onClose}
      />
      <ModalBody>
        {mission && (
          <div className="flex flex-col gap-[var(--sp-sm)] font-[var(--font-body)] font-normal text-[var(--text-sm)] leading-[1.6] text-[var(--color-neutral-700)]">
            <p>
              Tem certeza que deseja excluir a missão{" "}
              <strong>{mission.title}</strong>?
            </p>
            {totalItems > 0 && (
              <Alert variant="warning" title="Todos os itens serão removidos">
                {krs.length > 0 && (
                  <span>
                    {krs.length}{" "}
                    {krs.length === 1 ? "indicador" : "indicadores"}
                  </span>
                )}
                {krs.length > 0 &&
                  (tasks.length > 0 || children.length > 0) && <span>, </span>}
                {tasks.length > 0 && (
                  <span>
                    {tasks.length} {tasks.length === 1 ? "tarefa" : "tarefas"}
                  </span>
                )}
                {tasks.length > 0 && children.length > 0 && <span>, </span>}
                {children.length > 0 && (
                  <span>
                    {children.length}{" "}
                    {children.length === 1 ? "sub-missão" : "sub-missões"}
                  </span>
                )}{" "}
                serão excluídos permanentemente.
              </Alert>
            )}
          </div>
        )}
      </ModalBody>
      <ModalFooter>
        <Button variant="secondary" size="md" onClick={onClose}>
          Cancelar
        </Button>
        <Button variant="danger" size="md" leftIcon={Trash} onClick={onConfirm}>
          Excluir missão
        </Button>
      </ModalFooter>
    </Modal>
  );
}
