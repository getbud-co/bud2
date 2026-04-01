"use client";

import { useState, useMemo } from "react";
import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";

import {
  Table,
  TableContent,
  TableHead,
  TableBody,
  TableRow,
  TableHeaderCell,
  TableBulkActions,
  Button,
  Alert,
  toast,
} from "@mdonangelo/bud-ds";
import { Trash } from "@phosphor-icons/react";
import type { Cycle, CycleType, CycleStatus } from "@/types";
import { useOrganization } from "@/contexts/OrganizationContext";
import { useDataTable } from "@/hooks/useDataTable";
import { CyclesLoadingState } from "./components/CyclesLoadingState";
import { CyclesErrorState } from "./components/CyclesErrorState";
import { CyclesTableHeader } from "./components/CyclesTableHeader";
import { CycleTableRow } from "./components/CycleTableRow";
import { CycleFormModal } from "./components/CycleFormModal";
import type { CycleFormData } from "./components/CycleFormModal";
import { DeleteConfirmModal } from "./components/DeleteConfirmModal";
import { PageHeader } from "../layout/page-header";

const CYCLES_QUERY_KEY = "cycles";

function tenantHeader(orgId: string): Record<string, string> {
  return { "X-Tenant-Id": orgId };
}

async function fetchCycles(orgId: string): Promise<Cycle[]> {
  const res = await fetch("/api/organizations/cycles", {
    headers: tenantHeader(orgId),
  });
  if (!res.ok) throw new Error(`HTTP ${res.status}`);
  return res.json();
}

async function createCycleApi(
  orgId: string,
  data: {
    name: string;
    type: CycleType;
    startDate: string;
    endDate: string;
    status: CycleStatus;
  },
): Promise<Cycle> {
  const res = await fetch("/api/organizations/cycles", {
    method: "POST",
    headers: { "Content-Type": "application/json", ...tenantHeader(orgId) },
    body: JSON.stringify(data),
  });
  if (!res.ok) throw new Error(`HTTP ${res.status}`);
  return res.json();
}

async function updateCycleApi(
  orgId: string,
  id: string,
  data: Partial<
    Pick<Cycle, "name" | "type" | "startDate" | "endDate" | "status">
  >,
): Promise<Cycle> {
  const res = await fetch(`/api/organizations/cycles/${id}`, {
    method: "PATCH",
    headers: { "Content-Type": "application/json", ...tenantHeader(orgId) },
    body: JSON.stringify(data),
  });
  if (!res.ok) throw new Error(`HTTP ${res.status}`);
  return res.json();
}

async function deleteCycleApi(orgId: string, id: string): Promise<void> {
  const res = await fetch(`/api/organizations/cycles/${id}`, {
    method: "DELETE",
    headers: tenantHeader(orgId),
  });
  if (!res.ok) throw new Error(`HTTP ${res.status}`);
}

export function CyclesComponent() {
  const { activeOrgId } = useOrganization();
  const queryClient = useQueryClient();

  const {
    data: cycles = [],
    isLoading,
    isError,
  } = useQuery<Cycle[]>({
    queryKey: [CYCLES_QUERY_KEY, activeOrgId],
    queryFn: () => fetchCycles(activeOrgId!),
    enabled: !!activeOrgId,
  });

  const createMutation = useMutation({
    mutationFn: (data: Parameters<typeof createCycleApi>[1]) =>
      createCycleApi(activeOrgId!, data),
    onSuccess: () => {
      queryClient.invalidateQueries({
        queryKey: [CYCLES_QUERY_KEY, activeOrgId],
      });
      toast.success("Ciclo criado");
      setModalOpen(false);
    },
    onError: () => toast.error("Erro ao criar ciclo"),
  });

  const updateMutation = useMutation({
    mutationFn: ({
      id,
      data,
    }: {
      id: string;
      data: Parameters<typeof updateCycleApi>[2];
    }) => updateCycleApi(activeOrgId!, id, data),
    onSuccess: () => {
      queryClient.invalidateQueries({
        queryKey: [CYCLES_QUERY_KEY, activeOrgId],
      });
    },
    onError: () => toast.error("Erro ao atualizar ciclo"),
  });

  const deleteMutation = useMutation({
    mutationFn: (id: string) => deleteCycleApi(activeOrgId!, id),
    onSuccess: () => {
      queryClient.invalidateQueries({
        queryKey: [CYCLES_QUERY_KEY, activeOrgId],
      });
    },
    onError: () => toast.error("Erro ao excluir ciclo"),
  });

  const [search, setSearch] = useState("");
  const [modalOpen, setModalOpen] = useState(false);
  const [editingCycle, setEditingCycle] = useState<Cycle | null>(null);
  const [deletingCycle, setDeletingCycle] = useState<Cycle | null>(null);
  const [actionsPopoverCycle, setActionsPopoverCycle] = useState<string | null>(
    null,
  );

  const { selectedRows, clearSelection, handleSelectRow, handleSelectAll } =
    useDataTable<"name">();

  const filtered = useMemo(
    () =>
      cycles.filter(
        (c) => !search || c.name.toLowerCase().includes(search.toLowerCase()),
      ),
    [cycles, search],
  );

  const rowIds = useMemo(() => filtered.map((c) => c.id), [filtered]);

  function handleBulkDelete() {
    const ids = Array.from(selectedRows);
    Promise.all(ids.map((id) => deleteMutation.mutateAsync(id)))
      .then(() => {
        toast.success(`${ids.length} ciclo(s) excluído(s)`);
        clearSelection();
      })
      .catch(() => toast.error("Erro ao excluir ciclos"));
  }

  function handleSave(data: CycleFormData) {
    if (editingCycle) {
      updateMutation.mutate(
        { id: editingCycle.id, data },
        {
          onSuccess: () => {
            toast.success("Ciclo atualizado");
            setModalOpen(false);
          },
        },
      );
    } else {
      createMutation.mutate(data);
    }
  }

  function handleDelete() {
    if (!deletingCycle) return;
    deleteMutation.mutate(deletingCycle.id, {
      onSuccess: () => {
        setDeletingCycle(null);
        toast.success("Ciclo excluído");
      },
    });
  }

  function handleToggleStatus(cycle: Cycle) {
    const newStatus: CycleStatus =
      cycle.status === "active" ? "ended" : "active";
    updateMutation.mutate(
      { id: cycle.id, data: { status: newStatus } },
      {
        onSuccess: () =>
          toast.success(
            newStatus === "active" ? "Ciclo ativado" : "Ciclo encerrado",
          ),
      },
    );
  }

  return (
    <div className="flex flex-col gap-[var(--sp-2xs)] w-full">
      <PageHeader title="Ciclos e períodos" />

      <div className="flex flex-col gap-[var(--sp-2xs)] min-w-0">
        <Alert variant="info" title="Defina os períodos da sua organização">
          Os ciclos criados aqui ficam disponíveis como períodos pré-definidos
          nas funcionalidades de missões e pesquisas da plataforma.
        </Alert>

        <Table
          variant="divider"
          elevated={false}
          selectable
          selectedRows={selectedRows}
          rowIds={rowIds}
          onSelectRow={handleSelectRow}
          onSelectAll={(checked: boolean) => handleSelectAll(checked, rowIds)}
        >
          <CyclesTableHeader
            isLoading={isLoading}
            count={filtered.length}
            search={search}
            onSearch={setSearch}
            onCreateClick={() => {
              setEditingCycle(null);
              setModalOpen(true);
            }}
          />
          {isLoading ? (
            <CyclesLoadingState />
          ) : isError ? (
            <CyclesErrorState />
          ) : (
            <TableContent>
              <TableHead>
                <TableRow>
                  <TableHeaderCell isCheckbox />
                  <TableHeaderCell>Nome</TableHeaderCell>
                  <TableHeaderCell>Tipo</TableHeaderCell>
                  <TableHeaderCell>Início</TableHeaderCell>
                  <TableHeaderCell>Fim</TableHeaderCell>
                  <TableHeaderCell>Status</TableHeaderCell>
                  <TableHeaderCell />
                </TableRow>
              </TableHead>
              <TableBody>
                {filtered.map((c) => (
                  <CycleTableRow
                    key={c.id}
                    cycle={c}
                    isPopoverOpen={actionsPopoverCycle === c.id}
                    onEdit={(cycle) => {
                      setEditingCycle(cycle);
                      setModalOpen(true);
                    }}
                    onDelete={setDeletingCycle}
                    onToggleStatus={handleToggleStatus}
                    onPopoverToggle={(id) =>
                      setActionsPopoverCycle(
                        actionsPopoverCycle === id ? null : id,
                      )
                    }
                    onPopoverClose={() => setActionsPopoverCycle(null)}
                  />
                ))}
              </TableBody>
            </TableContent>
          )}
          <TableBulkActions count={selectedRows.size} onClear={clearSelection}>
            <Button
              variant="danger"
              size="md"
              leftIcon={Trash}
              onClick={handleBulkDelete}
            >
              Excluir
            </Button>
          </TableBulkActions>
        </Table>

        <CycleFormModal
          open={modalOpen}
          editingCycle={editingCycle}
          onClose={() => setModalOpen(false)}
          onSave={handleSave}
          isPending={createMutation.isPending || updateMutation.isPending}
        />

        <DeleteConfirmModal
          cycle={deletingCycle}
          onClose={() => setDeletingCycle(null)}
          onConfirm={handleDelete}
          isPending={deleteMutation.isPending}
        />
      </div>
    </div>
  );
}
