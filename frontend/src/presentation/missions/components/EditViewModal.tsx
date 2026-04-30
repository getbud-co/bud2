import type { Dispatch, SetStateAction } from "react";
import {
  Modal,
  ModalHeader,
  ModalBody,
  ModalFooter,
  Input,
  Button,
} from "@getbud-co/buds";
import {
  Users,
  CalendarBlank,
  FunnelSimple,
  User,
  ListBullets,
  Crosshair,
  GitBranch,
  UsersThree,
  ListChecks,
  Target,
} from "@phosphor-icons/react";
import type { SavedView } from "@/contexts/SavedViewsContext";
import { FILTER_OPTIONS } from "@/presentation/missions/consts";

const filterChipIcons: Record<string, typeof Users | undefined> = {
  team: Users,
  period: CalendarBlank,
  status: FunnelSimple,
  owner: User,
  itemType: ListBullets,
  indicatorType: Crosshair,
  contribution: GitBranch,
  supporter: UsersThree,
  taskState: ListChecks,
  missionStatus: Target,
};

interface EditViewModalProps {
  open: boolean;
  onClose: () => void;
  currentView: SavedView | undefined;
  viewName: string;
  setViewName: Dispatch<SetStateAction<string>>;
  onSave: () => void;
  activeFilters: string[];
  getFilterValueSummary: (filterId: string) => string;
}

export function EditViewModal({
  open,
  onClose,
  currentView,
  viewName,
  setViewName,
  onSave,
  activeFilters,
  getFilterValueSummary,
}: EditViewModalProps) {
  return (
    <Modal open={open} onClose={onClose} size="sm">
      <ModalHeader
        title={currentView ? "Atualizar visualização" : "Salvar visualização"}
        description={
          currentView
            ? "Atualize o nome ou os filtros desta visualização salva."
            : "Defina um nome para esta combinação de filtros. Você poderá aplicá-la rapidamente no futuro."
        }
        onClose={onClose}
      />
      <ModalBody>
        <div className="flex flex-col gap-6">
          <Input
            label="Nome da visualização"
            placeholder="Ex: Recrutamento setembro"
            value={viewName}
            onChange={(e: React.ChangeEvent<HTMLInputElement>) =>
              setViewName(e.target.value)
            }
            onKeyDown={(e: React.KeyboardEvent) => {
              if (e.key === "Enter") onSave();
            }}
          />
          <div className="flex flex-col gap-2">
            <span className="font-[var(--font-label)] font-medium text-[var(--text-sm)] text-[var(--color-neutral-950)] leading-[1.05]">
              Filtros incluídos
            </span>
            <ul className="list-none m-0 p-0 flex flex-col border border-[var(--color-caramel-200)] rounded-[var(--radius-xs)] overflow-hidden">
              {activeFilters.map((filterId) => {
                const Icon = filterChipIcons[filterId];
                const filterMeta = FILTER_OPTIONS.find(
                  (f) => f.id === filterId,
                );
                return (
                  <li
                    key={filterId}
                    className="flex items-center gap-2 px-3 py-3 font-[var(--font-label)] font-medium text-[var(--text-xs)] border-b border-[var(--color-caramel-200)] last:border-b-0"
                  >
                    {Icon && (
                      <Icon
                        size={14}
                        className="flex-shrink-0 text-[var(--color-neutral-400)]"
                      />
                    )}
                    <span className="text-[var(--color-neutral-600)] whitespace-nowrap">
                      {filterMeta?.label ?? filterId}
                    </span>
                    <span className="flex-1 min-w-0 text-right text-[var(--color-neutral-950)] overflow-hidden text-ellipsis whitespace-nowrap">
                      {getFilterValueSummary(filterId)}
                    </span>
                  </li>
                );
              })}
            </ul>
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
          onClick={onSave}
          disabled={!viewName.trim()}
        >
          {currentView ? "Atualizar" : "Salvar"}
        </Button>
      </ModalFooter>
    </Modal>
  );
}
