"use client";

import { Badge, Button, Input, TableCardHeader } from "@mdonangelo/bud-ds";
import { MagnifyingGlass, Plus } from "@phosphor-icons/react";

interface CyclesTableHeaderProps {
  isLoading: boolean;
  count: number;
  search: string;
  onSearch: (value: string) => void;
  onCreateClick: () => void;
}

export function CyclesTableHeader({
  isLoading,
  count,
  search,
  onSearch,
  onCreateClick,
}: CyclesTableHeaderProps) {
  return (
    <TableCardHeader
      title="Ciclos"
      badge={<Badge color="neutral">{isLoading ? "…" : count}</Badge>}
      actions={
        <div className="flex items-center gap-[var(--sp-2xs)]">
          <div className="max-w-[400px] flex-1">
            <Input
              placeholder="Buscar ciclo..."
              value={search}
              onChange={(e: React.ChangeEvent<HTMLInputElement>) =>
                onSearch(e.target.value)
              }
              leftIcon={MagnifyingGlass}
            />
          </div>
          <Button
            variant="primary"
            size="md"
            leftIcon={Plus}
            onClick={onCreateClick}
          >
            Novo ciclo
          </Button>
        </div>
      }
    />
  );
}
