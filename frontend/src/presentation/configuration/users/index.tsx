"use client";

import { useEffect, useMemo, useRef, useState } from "react";
import { useQueryClient } from "@tanstack/react-query";
import {
  Table,
  TableContent,
  TableHead,
  TableBody,
  TableRow,
  TableHeaderCell,
  TableBulkActions,
  Button,
  toast,
  useDataTable,
  useFilterChips,
} from "@mdonangelo/bud-ds";
import type { PopoverItem } from "@mdonangelo/bud-ds";
import {
  Key,
  UserCircle,
  UserMinus,
  UserCheck,
  Trash,
} from "@phosphor-icons/react";
import type { PeopleUserView } from "@/contexts/PeopleDataContext";
import { useOrganization } from "@/contexts/OrganizationContext";
import { useTeams } from "@/presentation/configuration/team/hooks/useTeams";
import { DEFAULT_ROLE_SLUG, STATUS_FILTER } from "./consts";
import { useEmployees, EMPLOYEES_QUERY_KEY } from "./hooks/useEmployees";
import { UsersLoadingState } from "./components/UsersLoadingState";
import { UsersErrorState } from "./components/UsersErrorState";
import { UsersTableHeader } from "./components/UsersTableHeader";
import { UsersTableRow } from "./components/UsersTableRow";
import { UsersFilterBar } from "./components/UsersFilterBar";
import {
  InviteUserModal,
  type InviteFormData,
} from "./components/InviteUserModal";
import { ImportUsersModal } from "./components/ImportUsersModal";
import { ToggleStatusModal } from "./components/ToggleStatusModal";
import { PageHeader } from "@/presentation/layout/page-header";

type UserView = PeopleUserView;

const FILTER_OPTIONS = [
  { id: "status", label: "Status" },
  { id: "role", label: "Tipo de usuário" },
];

// Values match EmployeeRole enum: Contributor=0, TeamLeader=1, HRManager=2, OrgAdmin=3
const roleOptions = [
  {
    value: "OrgAdmin",
    label: "Admin da Organização",
    description: "Acesso total à organização",
  },
  {
    value: "HRManager",
    label: "Gerente de RH",
    description: "Gestão de pessoas e times",
  },
  {
    value: "TeamLeader",
    label: "Líder de Time",
    description: "Gestão do próprio time",
  },
  {
    value: "Contributor",
    label: "Colaborador",
    description: "Acesso às próprias missões",
  },
];

function resolveRoleSlug(role: string): string {
  return role;
}

export function UsersModule() {
  const { activeOrgId } = useOrganization();
  const { data: teams = [] } = useTeams(activeOrgId ?? null);
  const queryClient = useQueryClient();

  const {
    data: users = [],
    isLoading,
    isError,
  } = useEmployees(activeOrgId ?? "");

  function invalidateUsers() {
    queryClient.invalidateQueries({
      queryKey: [EMPLOYEES_QUERY_KEY, activeOrgId],
    });
  }

  const [search, setSearch] = useState("");
  const [inviteOpen, setInviteOpen] = useState(false);
  const [importOpen, setImportOpen] = useState(false);
  const [deactivateUser, setDeactivateUser] = useState<UserView | null>(null);
  const [rolePopoverUser, setRolePopoverUser] = useState<string | null>(null);
  const [actionsPopoverUser, setActionsPopoverUser] = useState<string | null>(
    null,
  );

  type SortKey = "name" | "teams" | "role" | "status";
  const {
    selectedRows,
    clearSelection,
    sortKey,
    sortDir,
    handleSort,
    getSortDirection,
    handleSelectRow,
    handleSelectAll,
  } = useDataTable<SortKey>();

  const [filterStatus, setFilterStatus] = useState("all");
  const [filterRole, setFilterRole] = useState("all");
  const statusChipRef = useRef<HTMLDivElement>(null);
  const roleChipRef = useRef<HTMLDivElement>(null);
  const chipRefs = { status: statusChipRef, role: roleChipRef };

  const {
    activeFilters,
    openFilter,
    setOpenFilter,
    addFilterAndOpen,
    removeFilter,
    clearAllFilters,
    toggleFilterDropdown,
    getAvailableFilters,
    ignoreChipRefs,
  } = useFilterChips({
    chipRefs,
    onResetFilter: (id: string) => {
      if (id === "status") setFilterStatus("all");
      if (id === "role") setFilterRole("all");
    },
  });

  const inviteTeamOptions = useMemo(
    () => teams.map((t) => ({ value: t.id, label: t.name })),
    [teams],
  );

  const roleSelectionOptions = roleOptions.map((r) => ({
    value: r.value,
    label: r.label,
    description: r.description,
  }));

  const roleFilterOptions = [
    { id: "all", label: "Todos os tipos" },
    ...roleSelectionOptions.map((r) => ({ id: r.value, label: r.label })),
  ];

  const defaultInviteRole = roleOptions[0]?.value ?? DEFAULT_ROLE_SLUG;

  const roleLabelBySlug = useMemo(
    () => new Map(roleSelectionOptions.map((r) => [r.value, r.label])),
    [roleSelectionOptions],
  );

  const filtered = useMemo(
    () =>
      users
        .filter((u) => {
          const fullName = u.fullName.toLowerCase();
          if (
            search &&
            !fullName.includes(search.toLowerCase()) &&
            !u.email.toLowerCase().includes(search.toLowerCase())
          )
            return false;
          if (
            activeFilters.includes("status") &&
            filterStatus !== "all" &&
            u.status !== filterStatus
          )
            return false;
          const roleSlug = resolveRoleSlug(u.role);
          if (
            activeFilters.includes("role") &&
            filterRole !== "all" &&
            roleSlug !== filterRole
          )
            return false;
          return true;
        })
        .sort((a, b) => {
          if (!sortKey) return 0;
          const dir = sortDir === "asc" ? 1 : -1;
          switch (sortKey) {
            case "name":
              return dir * a.fullName.localeCompare(b.fullName);
            case "teams":
              return dir * a.teams.join(", ").localeCompare(b.teams.join(", "));
            case "role":
              return (
                dir *
                resolveRoleSlug(a.role).localeCompare(resolveRoleSlug(b.role))
              );
            case "status":
              return dir * a.status.localeCompare(b.status);
            default:
              return 0;
          }
        }),
    [users, search, activeFilters, filterStatus, filterRole, sortKey, sortDir],
  );

  const rowIds = useMemo(() => filtered.map((u) => u.id), [filtered]);

  const allSelectedInactive = useMemo(
    () =>
      selectedRows.size > 0 &&
      [...selectedRows].every(
        (id) => users.find((u) => u.id === id)?.status === "inactive",
      ),
    [selectedRows, users],
  );

  function getFilterLabel(id: string): string {
    if (id === "status")
      return (
        STATUS_FILTER.find((s) => s.id === filterStatus)?.label ?? "Status"
      );
    if (id === "role")
      return (
        roleFilterOptions.find((r) => r.id === filterRole)?.label ?? "Tipo"
      );
    return id;
  }

  function getRowActions(user: UserView): PopoverItem[] {
    const items: PopoverItem[] = [
      {
        id: "profile",
        label: "Ver perfil",
        icon: UserCircle,
        onClick: () => toast.success("Abrindo perfil de " + user.fullName),
      },
      {
        id: "reset-password",
        label: "Redefinir senha",
        icon: Key,
        onClick: () =>
          toast.success("E-mail de redefinição enviado para " + user.email),
      },
    ];
    if (user.status === "active") {
      items.push({
        id: "deactivate",
        label: "Desativar conta",
        icon: UserMinus,
        danger: true,
        onClick: () => setDeactivateUser(user),
      });
    } else {
      items.push({
        id: "activate",
        label: "Ativar conta",
        icon: UserCheck,
        onClick: () => setDeactivateUser(user),
      });
    }
    return items;
  }

  async function handleInvite(data: InviteFormData) {
    const teamId = data.team || undefined;
    const res = await fetch("/api/employees", {
      method: "POST",
      headers: {
        "Content-Type": "application/json",
        "X-Tenant-Id": activeOrgId ?? "",
      },
      body: JSON.stringify({
        fullName: data.fullName,
        email: data.email,
        role: data.role,
        ...(teamId ? { teamId } : {}),
      }),
    });

    if (!res.ok) {
      toast.error("Erro ao convidar usuário");
      return;
    }

    invalidateUsers();
    setInviteOpen(false);
    toast.success("Convite enviado com sucesso");
  }

  function handleImport(file: File) {
    // TODO: call POST /api/employees/import
    toast.success(
      `Arquivo "${file.name}" enviado. Os usuários serão importados em breve.`,
    );
    invalidateUsers();
    setImportOpen(false);
  }

  async function handleToggleStatus() {
    if (!deactivateUser) return;
    const newStatus =
      deactivateUser.status === "active"
        ? ("inactive" as const)
        : ("active" as const);

    const res = await fetch(`/api/employees/${deactivateUser.id}`, {
      method: "PATCH",
      headers: {
        "Content-Type": "application/json",
        "X-Tenant-Id": activeOrgId ?? "",
      },
      body: JSON.stringify({
        status: newStatus === "active" ? "Active" : "Inactive",
      }),
    });

    if (!res.ok) {
      toast.error("Erro ao alterar status do usuário");
      return;
    }

    invalidateUsers();
    setDeactivateUser(null);
    toast.success(
      newStatus === "active" ? "Usuário ativado" : "Usuário desativado",
    );
  }

  async function handleBulkToggleStatus() {
    const newStatus = allSelectedInactive ? "Active" : "Inactive";
    const ids = [...selectedRows];

    const results = await Promise.allSettled(
      ids.map((id) =>
        fetch(`/api/employees/${id}`, {
          method: "PATCH",
          headers: {
            "Content-Type": "application/json",
            "X-Tenant-Id": activeOrgId ?? "",
          },
          body: JSON.stringify({ status: newStatus }),
        }),
      ),
    );

    const failed = results.filter(
      (r) =>
        r.status === "rejected" || (r.status === "fulfilled" && !r.value.ok),
    ).length;

    invalidateUsers();
    clearSelection();

    if (failed > 0) {
      toast.error(`${failed} usuário(s) não puderam ser atualizados`);
    } else {
      toast.success(
        allSelectedInactive
          ? `${ids.length} usuário(s) ativado(s)`
          : `${ids.length} usuário(s) desativado(s)`,
      );
    }
  }

  async function handleBulkDelete() {
    const ids = [...selectedRows];

    const results = await Promise.allSettled(
      ids.map((id) =>
        fetch(`/api/employees/${id}`, {
          method: "DELETE",
          headers: { "X-Tenant-Id": activeOrgId ?? "" },
        }),
      ),
    );

    const failed = results.filter(
      (r) =>
        r.status === "rejected" || (r.status === "fulfilled" && !r.value.ok),
    ).length;

    invalidateUsers();
    clearSelection();

    if (failed > 0) {
      toast.error(`${failed} usuário(s) não puderam ser removidos`);
    } else {
      toast.success(`${ids.length} usuário(s) removido(s)`);
    }
  }

  async function handleRoleChange(userId: string, newRole: string) {
    const res = await fetch(`/api/employees/${userId}`, {
      method: "PATCH",
      headers: {
        "Content-Type": "application/json",
        "X-Tenant-Id": activeOrgId ?? "",
      },
      body: JSON.stringify({ role: newRole }),
    });

    if (!res.ok) {
      toast.error("Erro ao atualizar tipo de usuário");
      return;
    }

    invalidateUsers();
    setRolePopoverUser(null);
    toast.success("Tipo de usuário atualizado");
  }

  return (
    <div className="flex flex-col gap-[var(--sp-2xs)] w-full">
      <PageHeader title="Usuários" />
      <div className="flex flex-col gap-[var(--sp-2xs)] min-w-0">
        <Table
          variant="divider"
          elevated={false}
          selectable
          selectedRows={selectedRows}
          rowIds={rowIds}
          onSelectRow={handleSelectRow}
          onSelectAll={(checked: boolean) => handleSelectAll(checked, rowIds)}
        >
          <UsersTableHeader
            count={filtered.length}
            search={search}
            onSearch={setSearch}
            onImport={() => setImportOpen(true)}
            onInvite={() => setInviteOpen(true)}
          />

          <UsersFilterBar
            availableFilters={getAvailableFilters(FILTER_OPTIONS)}
            activeFilters={activeFilters}
            openFilter={openFilter}
            filterStatus={filterStatus}
            filterRole={filterRole}
            roleFilterOptions={roleFilterOptions}
            statusChipRef={statusChipRef}
            roleChipRef={roleChipRef}
            ignoreChipRefs={ignoreChipRefs}
            getFilterLabel={getFilterLabel}
            onAddFilter={addFilterAndOpen}
            onClearAll={activeFilters.length > 0 ? clearAllFilters : undefined}
            onToggleFilter={toggleFilterDropdown}
            onRemoveFilter={removeFilter}
            onSetOpenFilter={setOpenFilter}
            onStatusChange={setFilterStatus}
            onRoleChange={setFilterRole}
          />

          {isLoading ? (
            <UsersLoadingState />
          ) : isError ? (
            <UsersErrorState />
          ) : (
            <TableContent>
              <TableHead>
                <TableRow>
                  <TableHeaderCell isCheckbox />
                  <TableHeaderCell
                    sortable
                    sortDirection={getSortDirection("name")}
                    onSort={() => handleSort("name")}
                  >
                    Nome
                  </TableHeaderCell>
                  <TableHeaderCell
                    sortable
                    sortDirection={getSortDirection("teams")}
                    onSort={() => handleSort("teams")}
                  >
                    Times
                  </TableHeaderCell>
                  <TableHeaderCell
                    sortable
                    sortDirection={getSortDirection("role")}
                    onSort={() => handleSort("role")}
                  >
                    Tipo
                  </TableHeaderCell>
                  <TableHeaderCell
                    sortable
                    sortDirection={getSortDirection("status")}
                    onSort={() => handleSort("status")}
                  >
                    Status
                  </TableHeaderCell>
                  <TableHeaderCell />
                </TableRow>
              </TableHead>
              <TableBody>
                {filtered.map((u) => (
                  <UsersTableRow
                    key={u.id}
                    user={u}
                    roleLabelBySlug={roleLabelBySlug}
                    roleSelectionOptions={roleSelectionOptions}
                    resolveRoleSlug={resolveRoleSlug}
                    isRolePopoverOpen={rolePopoverUser === u.id}
                    isActionsPopoverOpen={actionsPopoverUser === u.id}
                    rowActions={getRowActions(u)}
                    onRolePopoverToggle={() =>
                      setRolePopoverUser(rolePopoverUser === u.id ? null : u.id)
                    }
                    onRolePopoverClose={() => setRolePopoverUser(null)}
                    onRoleChange={(newRole) => handleRoleChange(u.id, newRole)}
                    onActionsToggle={() =>
                      setActionsPopoverUser(
                        actionsPopoverUser === u.id ? null : u.id,
                      )
                    }
                    onActionsClose={() => setActionsPopoverUser(null)}
                  />
                ))}
              </TableBody>
            </TableContent>
          )}

          <TableBulkActions count={selectedRows.size} onClear={clearSelection}>
            <Button
              variant="secondary"
              size="md"
              leftIcon={allSelectedInactive ? UserCheck : UserMinus}
              onClick={handleBulkToggleStatus}
            >
              {allSelectedInactive ? "Ativar" : "Desativar"}
            </Button>
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

        <InviteUserModal
          open={inviteOpen}
          teamOptions={inviteTeamOptions}
          roleOptions={roleSelectionOptions}
          defaultRole={defaultInviteRole}
          roleLabelBySlug={roleLabelBySlug}
          onClose={() => setInviteOpen(false)}
          onSubmit={handleInvite}
        />

        <ImportUsersModal
          open={importOpen}
          teamOptions={inviteTeamOptions}
          roleOptions={roleSelectionOptions}
          onClose={() => setImportOpen(false)}
          onSubmit={handleImport}
        />

        <ToggleStatusModal
          user={deactivateUser}
          onClose={() => setDeactivateUser(null)}
          onConfirm={handleToggleStatus}
        />
      </div>
    </div>
  );
}
