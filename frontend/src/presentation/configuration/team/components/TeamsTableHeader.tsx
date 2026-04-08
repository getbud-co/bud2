import type { ChangeEvent } from "react";
import { Badge, Button, Input, TableCardHeader } from "@mdonangelo/bud-ds";
import { MagnifyingGlass, Plus } from "@phosphor-icons/react";

interface TeamsTableHeaderProps {
  count: number;
  search: string;
  onSearch: (value: string) => void;
  onCreate: () => void;
}

export function TeamsTableHeader({
  count,
  search,
  onSearch,
  onCreate,
}: TeamsTableHeaderProps) {
  return (
    <TableCardHeader
      title="Times"
      badge={<Badge color="neutral">{count}</Badge>}
      actions={
        <div className="flex items-center gap-[var(--sp-2xs)]">
          <div className="flex-1 max-w-[400px]">
            <Input
              placeholder="Buscar times..."
              value={search}
              onChange={(e: ChangeEvent<HTMLInputElement>) =>
                onSearch(e.target.value)
              }
              leftIcon={MagnifyingGlass}
            />
          </div>
          <Button variant="primary" size="md" leftIcon={Plus} onClick={onCreate}>
            Novo time
          </Button>
        </div>
      }
    />
  );
}
