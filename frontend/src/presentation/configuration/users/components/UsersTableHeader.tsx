"use client";

import type { ChangeEvent } from "react";
import { Badge, Button, Input, TableCardHeader } from "@mdonangelo/bud-ds";
import { MagnifyingGlass, Plus, UploadSimple } from "@phosphor-icons/react";

interface UsersTableHeaderProps {
  count: number;
  search: string;
  onSearch: (value: string) => void;
  onImport: () => void;
  onInvite: () => void;
}

export function UsersTableHeader({
  count,
  search,
  onSearch,
  onImport,
  onInvite,
}: UsersTableHeaderProps) {
  return (
    <TableCardHeader
      title="Usuários"
      badge={<Badge color="neutral">{count}</Badge>}
      actions={
        <div className="flex items-center gap-[var(--sp-2xs)]">
          <div className="max-w-[400px] flex-1">
            <Input
              placeholder="Buscar por nome ou e-mail..."
              value={search}
              onChange={(e: ChangeEvent<HTMLInputElement>) =>
                onSearch(e.target.value)
              }
              leftIcon={MagnifyingGlass}
            />
          </div>
          <Button
            variant="secondary"
            size="md"
            leftIcon={UploadSimple}
            onClick={onImport}
          >
            Importar usuários
          </Button>
          <Button
            variant="primary"
            size="md"
            leftIcon={Plus}
            onClick={onInvite}
          >
            Convidar usuário
          </Button>
        </div>
      }
    />
  );
}
