"use client";

import { memo } from "react";
import { TableRow, TableCell, Badge } from "@mdonangelo/bud-ds";
import type { PopoverItem } from "@mdonangelo/bud-ds";
import { PencilSimple, Trash, Play, Stop } from "@phosphor-icons/react";
import type { Cycle } from "@/types";
import { RowActionsPopover } from "@/components/table/RowActionsPopover";
import { formatDateDisplay } from "../utils";
import { STATUS_BADGE, TYPE_OPTIONS } from "../consts";

interface CycleTableRowProps {
  cycle: Cycle;
  isPopoverOpen: boolean;
  onEdit: (cycle: Cycle) => void;
  onDelete: (cycle: Cycle) => void;
  onToggleStatus: (cycle: Cycle) => void;
  onPopoverToggle: (id: string) => void;
  onPopoverClose: () => void;
}

export const CycleTableRow = memo(function CycleTableRow({
  cycle,
  isPopoverOpen,
  onEdit,
  onDelete,
  onToggleStatus,
  onPopoverToggle,
  onPopoverClose,
}: CycleTableRowProps) {
  const sb = STATUS_BADGE[cycle.status] ?? {
    label: cycle.status,
    color: "neutral" as const,
  };

  const items: PopoverItem[] = [
    {
      id: "edit",
      label: "Editar",
      icon: PencilSimple,
      onClick: () => onEdit(cycle),
    },
    cycle.status === "active"
      ? {
          id: "end",
          label: "Encerrar",
          icon: Stop,
          onClick: () => onToggleStatus(cycle),
        }
      : {
          id: "activate",
          label: "Ativar",
          icon: Play,
          onClick: () => onToggleStatus(cycle),
        },
    {
      id: "delete",
      label: "Excluir",
      icon: Trash,
      danger: true,
      onClick: () => onDelete(cycle),
    },
  ];

  return (
    <TableRow rowId={cycle.id}>
      <TableCell isCheckbox rowId={cycle.id} />
      <TableCell>{cycle.name}</TableCell>
      <TableCell>
        {TYPE_OPTIONS.find((t) => t.value === cycle.type)?.label ?? cycle.type}
      </TableCell>
      <TableCell>{formatDateDisplay(cycle.startDate)}</TableCell>
      <TableCell>{formatDateDisplay(cycle.endDate)}</TableCell>
      <TableCell>
        <Badge color={sb.color}>{sb.label}</Badge>
      </TableCell>
      <TableCell>
        <RowActionsPopover
          className="flex justify-end"
          items={items}
          open={isPopoverOpen}
          onToggle={() => onPopoverToggle(cycle.id)}
          onClose={onPopoverClose}
          buttonAriaLabel={`Abrir ações do ciclo ${cycle.name}`}
        />
      </TableCell>
    </TableRow>
  );
});
