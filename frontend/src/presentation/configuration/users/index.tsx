"use client";

import { useMemo, useRef, useState } from "react";
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
import {
  usePeopleData,
  type PeopleUserView,
} from "@/contexts/PeopleDataContext";
import { useConfigData } from "@/contexts/ConfigDataContext";
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

export function UsersModule() {
  const { teamNameOptions } = usePeopleData();
  const { activeOrgId, roleOptions, resolveRoleSlug } = useConfigData();
  const queryClient = useQueryClient();

  const { data: users = [], isLoading, isError } = useEmployees(activeOrgId);

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
    () => teamNameOptions.map((t) => ({ value: t, label: t })),
    [teamNameOptions],
  );

  const roleSelectionOptions = useMemo(
    () =>
      roleOptions.map((r) => ({
        value: r.value,
        label: r.label,
        description: r.description || "Sem descrição",
      })),
    [roleOptions],
  );

  const roleFilterOptions = useMemo(
    () => [
      { id: "all", label: "Todos os tipos" },
      ...roleSelectionOptions.map((r) => ({ id: r.value, label: r.label })),
    ],
    [roleSelectionOptions],
  );

  const defaultInviteRole = useMemo(
    () =>
      roleOptions.find((r) => r.isDefault)?.value ??
      roleOptions[0]?.value ??
      DEFAULT_ROLE_SLUG,
    [roleOptions],
  );

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
          const roleSlug = resolveRoleSlug(u.roleType);
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
                resolveRoleSlug(a.roleType).localeCompare(
                  resolveRoleSlug(b.roleType),
                )
              );
            case "status":
              return dir * a.status.localeCompare(b.status);
            default:
              return 0;
          }
        }),
    [
      users,
      search,
      activeFilters,
      filterStatus,
      filterRole,
      sortKey,
      sortDir,
      resolveRoleSlug,
    ],
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

  function handleInvite(data: InviteFormData) {
    void data;
    // TODO: call POST /api/employees and invalidate
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

  function handleToggleStatus() {
    if (!deactivateUser) return;
    const newStatus =
      deactivateUser.status === "active"
        ? ("inactive" as const)
        : ("active" as const);
    // TODO: call PATCH /api/employees/:id
    invalidateUsers();
    setDeactivateUser(null);
    toast.success(
      newStatus === "active" ? "Usuário ativado" : "Usuário desativado",
    );
  }

  function handleBulkToggleStatus() {
    const newStatus = allSelectedInactive
      ? ("active" as const)
      : ("inactive" as const);
    // TODO: call PATCH /api/employees bulk
    invalidateUsers();
    toast.success(
      allSelectedInactive
        ? `${selectedRows.size} usuário(s) ativado(s)`
        : `${selectedRows.size} usuário(s) desativado(s)`,
    );
    clearSelection();
  }

  function handleBulkDelete() {
    // TODO: call DELETE /api/employees bulk
    invalidateUsers();
    toast.success(`${selectedRows.size} usuário(s) removido(s)`);
    clearSelection();
  }

  function handleRoleChange(userId: string, newRole: string) {
    // TODO: call PATCH /api/employees/:id
    void userId;
    void newRole;
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
