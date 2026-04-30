import {
  Button,
  CalendarDate,
  formatMultiLabel,
  Input,
  Modal,
  ModalBody,
  ModalFooter,
  ModalHeader,
} from "@getbud-co/buds";
import type React from "react";
import { filterChipIcons } from "../utils";
import { FilterValues } from "./Filters";
import {
  CONTRIBUTION_OPTIONS,
  INDICATOR_TYPE_OPTIONS,
  ITEM_TYPE_OPTIONS,
  STATUS_OPTIONS,
} from "../consts";

interface SaveViewModalProps {
  open: boolean;
  isUpdate: boolean;
  viewName: string;
  onViewNameChange: (name: string) => void;
  activeFilters: string[];
  filterOptions: { id: string; label: string }[];
  filters: FilterValues;
  onClose: () => void;
  onConfirm: () => void;
}

export function SaveViewModal({
  open,
  isUpdate,
  viewName,
  onViewNameChange,
  activeFilters,
  filterOptions,
  filters,
  onClose,
  onConfirm,
}: SaveViewModalProps) {
  const {
    selectedTeams,
    selectedStatus,
    selectedPeriod,
    selectedOwners,
    selectedItemTypes,
    selectedIndicatorTypes,
    selectedContributions,
    selectedTaskState,
    selectedMissionStatuses,
    selectedSupporters,
  } = filters;

  function formatPeriodLabel(): string {
    const [start, end] = selectedPeriod;
    if (!start && !end) return "Selecionar período";
    const fmt = (d: CalendarDate) =>
      `${String(d.day).padStart(2, "0")}/${String(d.month).padStart(2, "0")}`;
    if (start && end) {
      return `${fmt(start)} - ${fmt(end)}/${end.year}`;
    }
    if (start) return fmt(start);
    return "";
  }

  function getFilterValueSummary(filterId: string): string {
    switch (filterId) {
      case "team": {
        const selected = selectedTeams.filter((n) => n !== "all");
        return selected.length === 0 ? "Todos os times" : selected.join(", ");
      }
      case "period":
        return formatPeriodLabel();
      case "status":
        return (
          STATUS_OPTIONS.find((s) => s.id === selectedStatus)?.label ?? "Todos"
        );
      case "owner": {
        const selected = selectedOwners.filter((n) => n !== "all");
        return selected.length === 0 ? "Todos" : selected.join(", ");
      }
      case "itemType":
        return formatMultiLabel(
          selectedItemTypes,
          ITEM_TYPE_OPTIONS,
          "Todos os itens",
        );
      case "indicatorType":
        return formatMultiLabel(
          selectedIndicatorTypes,
          INDICATOR_TYPE_OPTIONS,
          "Todos os tipos",
        );
      case "contribution":
        return formatMultiLabel(
          selectedContributions,
          CONTRIBUTION_OPTIONS,
          "Todas",
        );
      case "supporter":
        return formatMultiLabel(
          selectedSupporters,
          ownerFilterOptions,
          "Todos",
        );
      case "taskState":
        return (
          TASK_STATE_OPTIONS.find((s) => s.id === selectedTaskState)?.label ??
          "Todas"
        );
      case "missionStatus":
        return formatMultiLabel(
          selectedMissionStatuses,
          MISSION_STATUS_OPTIONS,
          "Todos",
        );
      default:
        return "";
    }
  }

  return (
    <Modal open={open} onClose={onClose} size="sm">
      <ModalHeader
        title={isUpdate ? "Atualizar visualização" : "Salvar visualização"}
        description={
          isUpdate
            ? "Atualize o nome ou os filtros desta visualização salva."
            : "Defina um nome para esta combinação de filtros. Você poderá aplicá-la rapidamente no futuro."
        }
        onClose={onClose}
      />
      <ModalBody>
        <div className="flex flex-col gap-[var(--sp-lg)]">
          <Input
            label="Nome da visualização"
            placeholder="Ex: Recrutamento setembro"
            value={viewName}
            onChange={(e: React.ChangeEvent<HTMLInputElement>) =>
              onViewNameChange(e.target.value)
            }
            onKeyDown={(e: React.KeyboardEvent) => {
              if (e.key === "Enter") onConfirm();
            }}
          />
          <div className="flex flex-col gap-[var(--sp-2xs)]">
            <span className="font-[var(--font-label)] font-medium text-[var(--text-sm)] text-[var(--color-neutral-950)] leading-[1.05]">
              Filtros incluídos
            </span>
            <ul className="list-none m-0 p-0 flex flex-col border border-[var(--color-caramel-200)] rounded-[var(--radius-xs)] overflow-hidden">
              {activeFilters.map((filterId) => {
                const Icon = filterChipIcons[filterId];
                const filterMeta = filterOptions.find((f) => f.id === filterId);
                return (
                  <li
                    key={filterId}
                    className="flex items-center gap-[var(--sp-2xs)] p-[var(--sp-xs)] font-[var(--font-label)] font-medium text-[var(--text-xs)] border-b border-[var(--color-caramel-200)] last:border-b-0"
                  >
                    {Icon && (
                      <Icon
                        size={14}
                        className="shrink-0 text-[var(--color-neutral-400)]"
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
          onClick={onConfirm}
          disabled={!viewName.trim()}
        >
          {isUpdate ? "Atualizar" : "Salvar"}
        </Button>
      </ModalFooter>
    </Modal>
  );
}
