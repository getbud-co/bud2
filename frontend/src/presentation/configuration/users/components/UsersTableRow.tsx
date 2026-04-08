"use client";

import { memo, useRef } from "react";
import {
  TableRow,
  TableCell,
  Badge,
  AvatarLabelGroup,
  FilterDropdown,
  RowActionsPopover,
} from "@mdonangelo/bud-ds";
import { CaretDown } from "@phosphor-icons/react";
import type { PopoverItem } from "@mdonangelo/bud-ds";
import type { PeopleUserView } from "@/contexts/PeopleDataContext";
import { STATUS_BADGE } from "../consts";

interface RoleOption {
  value: string;
  label: string;
  description: string;
}

interface UsersTableRowProps {
  user: PeopleUserView;
  roleLabelBySlug: Map<string, string>;
  roleSelectionOptions: RoleOption[];
  resolveRoleSlug: (role: string) => string;
  isRolePopoverOpen: boolean;
  isActionsPopoverOpen: boolean;
  rowActions: PopoverItem[];
  onRolePopoverToggle: () => void;
  onRolePopoverClose: () => void;
  onRoleChange: (newRole: string) => void;
  onActionsToggle: () => void;
  onActionsClose: () => void;
}

export const UsersTableRow = memo(function UsersTableRow({
  user,
  roleLabelBySlug,
  roleSelectionOptions,
  resolveRoleSlug,
  isRolePopoverOpen,
  isActionsPopoverOpen,
  rowActions,
  onRolePopoverToggle,
  onRolePopoverClose,
  onRoleChange,
  onActionsToggle,
  onActionsClose,
}: UsersTableRowProps) {
  const roleButtonRef = useRef<HTMLButtonElement | null>(null);
  const sb = STATUS_BADGE[user.status]!;

  return (
    <TableRow rowId={user.id}>
      <TableCell isCheckbox rowId={user.id} />
      <TableCell>
        <AvatarLabelGroup
          size="md"
          initials={user.initials ?? undefined}
          name={user.fullName}
          supportingText={user.jobTitle ?? undefined}
        />
      </TableCell>
      <TableCell>
        <div className="flex flex-wrap gap-[var(--sp-3xs)]">
          {user.teams.map((team) => (
            <Badge key={team} color="neutral">
              {team}
            </Badge>
          ))}
        </div>
      </TableCell>
      <TableCell>
        <div className="flex flex-col gap-[var(--sp-3xs)] relative">
          <button
            ref={roleButtonRef}
            className="flex items-center gap-[var(--sp-2xs)] px-[var(--sp-sm)] py-[var(--sp-2xs)] border border-[var(--color-caramel-300)] rounded-[var(--radius-sm)] bg-[var(--color-neutral-0)] cursor-pointer min-h-[36px] text-left hover:border-[var(--color-caramel-500)] focus-visible:outline-2 focus-visible:outline-[var(--color-orange-500)] focus-visible:outline-offset-2"
            onClick={onRolePopoverToggle}
            type="button"
          >
            <span className="flex-1 min-w-0 font-[var(--font-label)] font-medium text-[var(--text-sm)] text-[var(--color-neutral-900)] leading-[1.15]">
              {roleLabelBySlug.get(resolveRoleSlug(user.roleType)) ??
                user.roleType}
            </span>
            <CaretDown
              size={14}
              className="shrink-0 text-[var(--color-neutral-400)] transition-transform duration-150"
            />
          </button>
          <FilterDropdown
            open={isRolePopoverOpen}
            onClose={onRolePopoverClose}
            anchorRef={roleButtonRef}
          >
            <div className="flex flex-col p-[var(--sp-3xs)] max-h-[360px] overflow-y-auto">
              {roleSelectionOptions.map((opt) => (
                <button
                  key={opt.value}
                  type="button"
                  className={`flex items-start gap-[var(--sp-2xs)] px-[var(--sp-sm)] py-[var(--sp-2xs)] border-0 bg-transparent cursor-pointer text-left w-full rounded-[var(--radius-xs)] transition-colors duration-[120ms] hover:bg-[var(--color-caramel-100)] ${resolveRoleSlug(user.roleType) === opt.value ? "bg-[var(--color-caramel-50)]" : ""}`}
                  onClick={() => onRoleChange(opt.value)}
                >
                  <div className="flex flex-col gap-[2px] flex-1 min-w-0">
                    <span className="font-[var(--font-label)] font-medium text-[var(--text-sm)] text-[var(--color-neutral-900)] leading-[1.15]">
                      {opt.label}
                    </span>
                    <span className="font-[var(--font-body)] text-[var(--text-xs)] text-[var(--color-neutral-500)] leading-[1.35]">
                      {opt.description}
                    </span>
                  </div>
                </button>
              ))}
            </div>
          </FilterDropdown>
        </div>
      </TableCell>
      <TableCell>
        <Badge color={sb.color}>{sb.label}</Badge>
      </TableCell>
      <TableCell>
        <RowActionsPopover
          className="flex justify-end"
          items={rowActions}
          open={isActionsPopoverOpen}
          onToggle={onActionsToggle}
          onClose={onActionsClose}
          buttonAriaLabel={`Abrir ações de ${user.fullName}`}
        />
      </TableCell>
    </TableRow>
  );
});
