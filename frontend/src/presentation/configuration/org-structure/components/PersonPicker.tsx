import { OrgPersonView } from "@/contexts/PeopleDataContext";
import { Avatar, Input } from "@getbud-co/buds";
import { MagnifyingGlass, X } from "@phosphor-icons/react";
import { ChangeEvent, useEffect, useMemo, useRef, useState } from "react";

export function PersonPicker({
  people,
  excludeIds,
  onSelect,
  onCancel,
  placeholder = "Buscar colaborador...",
}: {
  people: OrgPersonView[];
  excludeIds: Set<string>;
  onSelect: (id: string) => void;
  onCancel: () => void;
  placeholder?: string;
}) {
  const [query, setQuery] = useState("");
  const inputRef = useRef<HTMLInputElement>(null);

  useEffect(() => {
    inputRef.current?.focus();
  }, []);

  const filtered = useMemo(() => {
    const q = query.toLowerCase();
    return people
      .filter((p) => !excludeIds.has(p.id))
      .filter(
        (p) =>
          !q ||
          p.fullName.toLowerCase().includes(q) ||
          (p.jobTitle ?? "").toLowerCase().includes(q),
      )
      .slice(0, 8);
  }, [people, excludeIds, query]);

  return (
    <div className="flex flex-col border border-[var(--color-caramel-200)] rounded-[var(--radius-sm)] bg-[var(--color-neutral-0)] overflow-hidden">
      <div className="flex items-center gap-[var(--sp-2xs)] p-[var(--sp-2xs)]">
        <div className="flex-1">
          <Input
            ref={inputRef}
            placeholder={placeholder}
            value={query}
            onChange={(e: ChangeEvent<HTMLInputElement>) =>
              setQuery(e.target.value)
            }
            leftIcon={MagnifyingGlass}
          />
        </div>
        <button
          type="button"
          className="flex items-center justify-center w-7 h-7 border-0 bg-transparent text-[var(--color-neutral-400)] cursor-pointer rounded-[var(--radius-xs)] shrink-0 transition-colors duration-100 hover:bg-[var(--color-caramel-50)]"
          onClick={onCancel}
        >
          <X size={16} />
        </button>
      </div>
      <div className="max-h-[240px] overflow-y-auto border-t border-[var(--color-caramel-200)]">
        {filtered.length === 0 && (
          <div className="p-[var(--sp-sm)] text-center font-[var(--font-body)] text-[var(--text-sm)] text-[var(--color-neutral-500)]">
            Nenhum resultado
          </div>
        )}
        {filtered.map((p) => (
          <button
            key={p.id}
            type="button"
            className="flex items-center gap-[var(--sp-sm)] px-[var(--sp-sm)] py-[var(--sp-2xs)] border-0 bg-transparent cursor-pointer text-left w-full transition-colors duration-100 hover:bg-[var(--color-caramel-50)]"
            onClick={() => onSelect(p.id)}
          >
            <Avatar initials={p.initials ?? undefined} size="sm" />
            <div className="flex flex-col gap-[1px] min-w-0">
              <span className="font-[var(--font-label)] font-medium text-[var(--text-sm)] text-[var(--color-neutral-900)]">
                {p.fullName}
              </span>
              <span className="font-[var(--font-body)] text-[var(--text-xs)] text-[var(--color-neutral-500)]">
                {p.jobTitle} · {p.teams.join(", ")}
              </span>
            </div>
          </button>
        ))}
      </div>
    </div>
  );
}
