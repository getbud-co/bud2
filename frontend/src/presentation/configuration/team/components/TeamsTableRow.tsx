import {
  TableRow,
  TableCell,
  Badge,
  AvatarLabelGroup,
  AvatarGroup,
} from "@mdonangelo/bud-ds";
import type { AvatarGroupItem, PopoverItem } from "@mdonangelo/bud-ds";
import { UsersThree } from "@phosphor-icons/react";
import { RowActionsPopover } from "@/components/table/RowActionsPopover";
import type { Team, TeamMember } from "@/types";
import { STATUS_BADGE } from "../consts";
import type { PersonView } from "./TeamModal";

interface TeamsTableRowProps {
  team: Team;
  leader: PersonView | null;
  avatars: AvatarGroupItem[];
  rowActions: PopoverItem[];
  isActionsOpen: boolean;
  onActionsToggle: () => void;
  onActionsClose: () => void;
  onOpenMembers: (team: Team) => void;
}

export function TeamsTableRow({
  team,
  leader,
  avatars,
  rowActions,
  isActionsOpen,
  onActionsToggle,
  onActionsClose,
  onOpenMembers,
}: TeamsTableRowProps) {
  const sb = STATUS_BADGE[team.status]!;
  const members: TeamMember[] = team.members ?? [];

  return (
    <TableRow rowId={team.id}>
      <TableCell isCheckbox rowId={team.id} />

      <TableCell>
        <div className="flex flex-col items-start gap-[var(--sp-3xs)]">
          <Badge color={team.color} size="sm">
            {team.name}
          </Badge>
          {team.description && (
            <span className="font-[var(--font-body)] text-[var(--text-xs)] text-[var(--color-neutral-500)] leading-[1.3] max-w-[280px] overflow-hidden text-ellipsis whitespace-nowrap">
              {team.description}
            </span>
          )}
        </div>
      </TableCell>

      <TableCell>
        {leader ? (
          <AvatarLabelGroup
            size="md"
            initials={leader.initials}
            name={leader.fullName}
            supportingText={leader.jobTitle}
          />
        ) : (
          <span className="font-[var(--font-body)] text-[var(--text-sm)] text-[var(--color-neutral-400)]">
            Sem líder
          </span>
        )}
      </TableCell>

      <TableCell>
        <div className="flex items-center gap-[var(--sp-2xs)]">
          {members.length > 0 ? (
            <>
              <AvatarGroup
                size="xs"
                avatars={avatars}
                maxVisible={4}
                showAddButton
                onAddClick={() => onOpenMembers(team)}
              />
              <span className="font-[var(--font-label)] font-medium text-[var(--text-xs)] text-[var(--color-neutral-500)]">
                {members.length}
              </span>
            </>
          ) : (
            <button
              type="button"
              className="inline-flex items-center gap-[var(--sp-3xs)] px-[var(--sp-2xs)] py-[var(--sp-3xs)] border border-dashed border-[var(--color-caramel-300)] rounded-[var(--radius-xs)] bg-transparent font-[var(--font-label)] font-medium text-[var(--text-xs)] text-[var(--color-neutral-500)] cursor-pointer transition-[background-color,border-color,color] duration-100 hover:bg-[var(--color-orange-50)] hover:border-[var(--color-orange-300)] hover:text-[var(--color-orange-700)]"
              onClick={() => onOpenMembers(team)}
            >
              <UsersThree size={14} />
              <span>Adicionar</span>
            </button>
          )}
        </div>
      </TableCell>

      <TableCell>
        <Badge color={sb.color}>{sb.label}</Badge>
      </TableCell>

      <TableCell>
        <RowActionsPopover
          className="flex justify-end"
          items={rowActions}
          open={isActionsOpen}
          onToggle={onActionsToggle}
          onClose={onActionsClose}
          buttonAriaLabel={`Abrir ações do time ${team.name}`}
        />
      </TableCell>
    </TableRow>
  );
}
