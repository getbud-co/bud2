"use client";

import { useState } from "react";
import { Input, Button } from "@getbud-co/buds";
import { RowActionsPopover } from "@getbud-co/buds";
import { ValueFormModal } from "./ValueFormModal";
import { DeleteValueModal } from "./DeleteValueModal";
import {
  MagnifyingGlass,
  Plus,
  Diamond,
  PencilSimple,
  Trash,
} from "@phosphor-icons/react";

// TODO: substituir por fetch real quando o endpoint de valores estiver disponível
interface CompanyValue {
  id: string;
  name: string;
  description: string;
  createdAt: string;
}

export function CompanyValuesTab() {
  const [modalOpen, setModalOpen] = useState(false);
  const [editingId, setEditingId] = useState<string | null>(null);
  const [deleteId, setDeleteId] = useState<string | null>(null);

  function onAdd() {
    setEditingId(null);
    setModalOpen(true);
  }

  function onEdit(id: string) {
    setEditingId(id);
    setModalOpen(true);
  }

  function onDelete(id: string) {
    setDeleteId(id);
  }
  const [search, setSearch] = useState("");
  const [actionsPopover, setActionsPopover] = useState<string | null>(null);

  // TODO: trocar por useQuery quando endpoint estiver pronto
  const values: CompanyValue[] = [];

  const filtered = search
    ? values.filter(
        (v) =>
          v.name.toLowerCase().includes(search.toLowerCase()) ||
          v.description.toLowerCase().includes(search.toLowerCase()),
      )
    : values;

  return (
    <div className="flex flex-col gap-[var(--sp-md)] p-[var(--sp-lg)]">
      <div className="flex items-center justify-between flex-wrap gap-[var(--sp-sm)] max-md:flex-col max-md:items-stretch">
        <div className="font-[var(--font-heading)] text-[var(--text-md)] font-semibold text-[var(--color-neutral-900)]">
          Valores ({filtered.length})
        </div>
        <div className="flex items-center gap-[var(--sp-2xs)] max-md:flex-col max-md:items-stretch">
          <div className="max-w-[400px] flex-1 max-md:max-w-none">
            <Input
              placeholder="Buscar valor..."
              value={search}
              onChange={(e) => setSearch(e.target.value)}
              leftIcon={MagnifyingGlass}
            />
          </div>
          <Button variant="primary" size="md" leftIcon={Plus} onClick={onAdd}>
            Novo valor
          </Button>
        </div>
      </div>

      {filtered.length > 0 ? (
        <div className="flex flex-col">
          {filtered.map((v) => (
            <div
              key={v.id}
              className="flex items-center justify-between gap-[var(--sp-sm)] py-[var(--sp-sm)] border-b border-[var(--color-caramel-200)] last:border-b-0"
            >
              <div className="flex items-center gap-[var(--sp-sm)] flex-1 min-w-0">
                <div className="flex items-center justify-center w-9 h-9 rounded-[var(--radius-sm)] bg-[var(--color-orange-50)] text-[var(--color-orange-500)] shrink-0">
                  <Diamond size={20} />
                </div>
                <div className="flex flex-col gap-[2px] min-w-0">
                  <div className="flex items-center gap-[var(--sp-2xs)]">
                    <span className="font-[var(--font-label)] font-medium text-[var(--text-sm)] text-[var(--color-neutral-900)]">
                      {v.name}
                    </span>
                  </div>
                  <span className="font-[var(--font-body)] text-[var(--text-xs)] text-[var(--color-neutral-500)] leading-[1.4] line-clamp-2">
                    {v.description}
                  </span>
                </div>
              </div>
              <div className="flex items-center gap-[var(--sp-2xs)] shrink-0">
                <RowActionsPopover
                  items={[
                    {
                      id: "edit",
                      label: "Editar",
                      icon: PencilSimple,
                      onClick: () => onEdit(v.id),
                    },
                    {
                      id: "delete",
                      label: "Excluir",
                      icon: Trash,
                      danger: true,
                      onClick: () => onDelete(v.id),
                    },
                  ]}
                  open={actionsPopover === v.id}
                  onToggle={() =>
                    setActionsPopover(actionsPopover === v.id ? null : v.id)
                  }
                  onClose={() => setActionsPopover(null)}
                  buttonAriaLabel={`Abrir ações do valor ${v.name}`}
                />
              </div>
            </div>
          ))}
        </div>
      ) : (
        <div className="flex flex-col items-center gap-[var(--sp-2xs)] py-[var(--sp-2xl)] text-[var(--color-neutral-400)]">
          <Diamond size={32} />
          <p className="font-[var(--font-label)] font-medium text-[var(--text-sm)] text-[var(--color-neutral-600)] m-0">
            Nenhum valor encontrado
          </p>
          <p className="font-[var(--font-body)] text-[var(--text-xs)] text-[var(--color-neutral-500)] m-0 text-center max-w-[320px]">
            {search
              ? "Nenhum valor corresponde à busca."
              : "Adicione valores culturais para usar como fatores de avaliação em pesquisas."}
          </p>
        </div>
      )}

      <ValueFormModal
        open={modalOpen}
        editingId={editingId}
        onClose={() => setModalOpen(false)}
      />

      <DeleteValueModal valueId={deleteId} onClose={() => setDeleteId(null)} />
    </div>
  );
}
