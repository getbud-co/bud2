import {
  useEffect,
  useMemo,
  useRef,
  useState,
  type ChangeEvent,
  type RefObject,
} from "react";
import { useQuery } from "@tanstack/react-query";
import {
  Table,
  TableCardHeader,
  TableContent,
  TableHead,
  TableBody,
  TableRow,
  TableHeaderCell,
  TableCell,
  TableBulkActions,
  Button,
  Input,
  Badge,
  AvatarLabelGroup,
  Modal,
  ModalHeader,
  ModalBody,
  ModalFooter,
  FilterBar,
  FilterChip,
  FilterDropdown,
  Radio,
  toast,
  Select,
  DatePicker,
  RowActionsPopover,
  useDataTable,
  useFilterChips,
} from "@mdonangelo/bud-ds";
import type { PopoverItem, CalendarDate } from "@mdonangelo/bud-ds";
import {
  MagnifyingGlass,
  Plus,
  Envelope,
  UserCircle,
  Key,
  UserMinus,
  UserCheck,
  CaretDown,
  UploadSimple,
  DownloadSimple,
  FileText,
  Trash,
} from "@phosphor-icons/react";
import {
  usePeopleData,
  type PeopleUserView,
} from "@/contexts/PeopleDataContext";
import { useConfigData } from "@/contexts/ConfigDataContext";
import {
  DEFAULT_ROLE_SLUG,
  GENDER_OPTIONS,
  LANGUAGE_OPTIONS,
  STATUS_BADGE,
  STATUS_FILTER,
} from "./consts";
import { Gender } from "@/types";
import { UsersLoadingState } from "./UsersLoadingState";
import { UsersErrorState } from "./UsersErrorState";

/** Extends the DB User with UI-only fields used in this module. */
type UserView = PeopleUserView;

const EMPLOYEES_QUERY_KEY = "employees";

function tenantHeader(orgId: string): Record<string, string> {
  return { "X-Tenant-Id": orgId };
}

async function fetchEmployees(orgId: string): Promise<UserView[]> {
  const res = await fetch("/api/employees?pageSize=100", {
    headers: tenantHeader(orgId),
  });
  if (!res.ok) throw new Error(`HTTP ${res.status}`);
  return res.json();
}

export function UsersModule() {
  const { teamNameOptions } = usePeopleData();
  const { activeOrgId, roleOptions, resolveRoleSlug } = useConfigData();

  const {
    data: apiUsers = [],
    isLoading,
    isError,
  } = useQuery<UserView[]>({
    queryKey: [EMPLOYEES_QUERY_KEY, activeOrgId],
    queryFn: () => fetchEmployees(activeOrgId),
    enabled: !!activeOrgId,
  });

  const [users, setUsers] = useState<UserView[]>([]);

  useEffect(() => {
    setUsers(apiUsers);
  }, [apiUsers]);

  const [search, setSearch] = useState("");
  const [inviteOpen, setInviteOpen] = useState(false);
  const [importOpen, setImportOpen] = useState(false);
  const [importFile, setImportFile] = useState<File | null>(null);
  const fileInputRef = useRef<HTMLInputElement>(null);
  const [deactivateUser, setDeactivateUser] = useState<UserView | null>(null);

  /* invite form */
  const [inviteFirstName, setInviteFirstName] = useState("");
  const [inviteLastName, setInviteLastName] = useState("");
  const [inviteNickname, setInviteNickname] = useState("");
  const [inviteEmail, setInviteEmail] = useState("");
  const [inviteJobTitle, setInviteJobTitle] = useState("");
  const [inviteTeam, setInviteTeam] = useState("");
  const [inviteRole, setInviteRole] = useState("");
  const [inviteRoleOpen, setInviteRoleOpen] = useState(false);
  const inviteRoleBtnRef = useRef<HTMLButtonElement>(null);
  const [inviteBirthDate, setInviteBirthDate] = useState<CalendarDate | null>(
    null,
  );
  const [inviteLanguage, setInviteLanguage] = useState("pt-br");
  const [inviteGender, setInviteGender] = useState("");

  const inviteTeamOptions = useMemo(
    () =>
      teamNameOptions.map((teamName) => ({ value: teamName, label: teamName })),
    [teamNameOptions],
  );

  const roleSelectionOptions = useMemo(
    () =>
      roleOptions.map((role) => ({
        value: role.value,
        label: role.label,
        description: role.description || "Sem descrição",
      })),
    [roleOptions],
  );

  const roleFilterOptions = useMemo(
    () => [
      { id: "all", label: "Todos os tipos" },
      ...roleSelectionOptions.map((role) => ({
        id: role.value,
        label: role.label,
      })),
    ],
    [roleSelectionOptions],
  );

  const defaultInviteRole = useMemo(
    () =>
      roleOptions.find((role) => role.isDefault)?.value ??
      roleOptions[0]?.value ??
      DEFAULT_ROLE_SLUG,
    [roleOptions],
  );

  const roleLabelBySlug = useMemo(
    () => new Map(roleSelectionOptions.map((role) => [role.value, role.label])),
    [roleSelectionOptions],
  );

  /* inline role popover on table */
  const [rolePopoverUser, setRolePopoverUser] = useState<string | null>(null);
  const rolePopoverRefs = useRef<Record<string, HTMLButtonElement | null>>({});

  /* actions popover */
  const [actionsPopoverUser, setActionsPopoverUser] = useState<string | null>(
    null,
  );

  /* sorting */
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

  /* filters */
  const [filterStatus, setFilterStatus] = useState("all");
  const [filterRole, setFilterRole] = useState("all");
  const statusChipRef = useRef<HTMLDivElement>(null);
  const roleChipRef = useRef<HTMLDivElement>(null);

  const chipRefs: Record<string, RefObject<HTMLDivElement | null>> = {
    status: statusChipRef,
    role: roleChipRef,
  };

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
    onResetFilter: (id) => {
      if (id === "status") setFilterStatus("all");
      if (id === "role") setFilterRole("all");
    },
  });

  const FILTER_OPTIONS = [
    { id: "status", label: "Status" },
    { id: "role", label: "Tipo de usuário" },
  ];

  const filtered = useMemo(
    () =>
      users
        .filter((u) => {
          const fullName = `${u.firstName} ${u.lastName}`.toLowerCase();
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
              return (
                dir *
                `${a.firstName} ${a.lastName}`.localeCompare(
                  `${b.firstName} ${b.lastName}`,
                )
              );
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

  function handleBulkToggleStatus() {
    const newStatus = allSelectedInactive
      ? ("active" as const)
      : ("inactive" as const);
    setUsers((prev) =>
      prev.map((u) =>
        selectedRows.has(u.id) ? { ...u, status: newStatus } : u,
      ),
    );
    toast.success(
      allSelectedInactive
        ? `${selectedRows.size} usuário(s) ativado(s)`
        : `${selectedRows.size} usuário(s) desativado(s)`,
    );
    clearSelection();
  }

  function handleBulkDelete() {
    setUsers((prev) => prev.filter((u) => !selectedRows.has(u.id)));
    toast.success(`${selectedRows.size} usuário(s) removido(s)`);
    clearSelection();
  }

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

  function handleInvite() {
    const now = new Date().toISOString();
    const newUser: UserView = {
      id: String(Date.now()),
      orgId: activeOrgId,
      email: inviteEmail,
      firstName: inviteFirstName,
      lastName: inviteLastName,
      nickname: inviteNickname || null,
      jobTitle: inviteJobTitle || null,
      managerId: null,
      avatarUrl: null,
      initials:
        `${inviteFirstName[0] ?? ""}${inviteLastName[0] ?? ""}`.toUpperCase(),
      birthDate: inviteBirthDate
        ? `${inviteBirthDate.year}-${String(inviteBirthDate.month).padStart(2, "0")}-${String(inviteBirthDate.day).padStart(2, "0")}`
        : null,
      gender: (inviteGender as Gender) || null,
      language: inviteLanguage,
      phone: null,
      status: "invited",
      invitedAt: now,
      activatedAt: null,
      lastLoginAt: null,
      authProvider: "email",
      authProviderId: null,
      createdAt: now,
      updatedAt: now,
      deletedAt: null,
      roleId: createRoleIdForOrg(activeOrgId, resolveRoleSlug(inviteRole)),
      roleType: resolveRoleSlug(inviteRole),
      teams: inviteTeam ? [inviteTeam] : [],
    };

    setUsers((prev) => [...prev, newUser]);
    setInviteOpen(false);
    setInviteFirstName("");
    setInviteLastName("");
    setInviteNickname("");
    setInviteEmail("");
    setInviteJobTitle("");
    setInviteTeam("");
    setInviteRole(defaultInviteRole);
    setInviteBirthDate(null);
    setInviteLanguage("pt-br");
    setInviteGender("");
    toast.success("Convite enviado com sucesso");
  }

  function handleToggleStatus() {
    if (!deactivateUser) return;
    const newStatus =
      deactivateUser.status === "active"
        ? ("inactive" as const)
        : ("active" as const);
    setUsers((prev) =>
      prev.map((u) =>
        u.id === deactivateUser.id ? { ...u, status: newStatus } : u,
      ),
    );
    setDeactivateUser(null);
    toast.success(
      newStatus === "active" ? "Usuário ativado" : "Usuário desativado",
    );
  }

  function handleDownloadTemplate() {
    const headers = [
      "Nome",
      "Sobrenome",
      "Apelido",
      "E-mail",
      "Cargo",
      "Time",
      "Tipo de usuário",
      "Data de nascimento",
      "Idioma",
      "Gênero",
    ];
    const exampleRow = [
      "Maria",
      "Soares",
      "Mari",
      "maria@empresa.com",
      "Product Manager",
      "Produto; Liderança",
      DEFAULT_ROLE_SLUG,
      "15/03/1990",
      "pt-br",
      "Feminino",
    ];
    const hintsRow = [
      "",
      "",
      "",
      "",
      "",
      `Múltiplos times separados por ; — Valores: ${inviteTeamOptions.map((t) => t.value).join(" | ")}`,
      `Valores: ${roleSelectionOptions.map((r) => r.value).join(" | ")}`,
      "Formato: DD/MM/AAAA",
      `Valores: ${LANGUAGE_OPTIONS.map((l) => l.value).join(" | ")}`,
      `Valores: ${GENDER_OPTIONS.map((g) => g.label).join(" | ")}`,
    ];

    const escape = (v: string) =>
      v.includes(",") || v.includes('"') ? `"${v.replace(/"/g, '""')}"` : v;
    const csv = [headers, exampleRow, hintsRow]
      .map((row) => row.map(escape).join(","))
      .join("\n");
    const bom = "\uFEFF";
    const blob = new Blob([bom + csv], { type: "text/csv;charset=utf-8;" });
    const url = URL.createObjectURL(blob);
    const a = document.createElement("a");
    a.href = url;
    a.download = "template-usuarios-bud.csv";
    a.click();
    URL.revokeObjectURL(url);
  }

  function handleImport() {
    if (!importFile) return;
    toast.success(
      `Arquivo "${importFile.name}" enviado. Os usuários serão importados em breve.`,
    );
    setImportOpen(false);
    setImportFile(null);
  }

  function handleRoleChange(userId: string, newRole: string) {
    const normalizedRole = resolveRoleSlug(newRole);
    setUsers((prev) =>
      prev.map((u) =>
        u.id === userId ? { ...u, roleType: normalizedRole } : u,
      ),
    );
    setRolePopoverUser(null);
    toast.success("Tipo de usuário atualizado");
  }

  useEffect(() => {
    if (!inviteRole && defaultInviteRole) {
      setInviteRole(defaultInviteRole);
    }
  }, [inviteRole, defaultInviteRole]);

  function getRowActions(user: UserView): PopoverItem[] {
    const items: PopoverItem[] = [
      {
        id: "profile",
        label: "Ver perfil",
        icon: UserCircle,
        onClick: () => toast.success("Abrindo perfil de " + user.firstName),
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

  return (
    <>
      <Table
        variant="divider"
        elevated={false}
        selectable
        selectedRows={selectedRows}
        rowIds={rowIds}
        onSelectRow={handleSelectRow}
        onSelectAll={(checked: boolean) => handleSelectAll(checked, rowIds)}
      >
        <TableCardHeader
          title="Usuários"
          badge={<Badge color="neutral">{filtered.length}</Badge>}
          actions={
            <div className="flex items-center gap-[var(--sp-2xs)]">
              <div className="max-w-[400px] flex-1">
                <Input
                  placeholder="Buscar por nome ou e-mail..."
                  value={search}
                  onChange={(e: ChangeEvent<HTMLInputElement>) =>
                    setSearch(e.target.value)
                  }
                  leftIcon={MagnifyingGlass}
                />
              </div>
              <Button
                variant="secondary"
                size="md"
                leftIcon={UploadSimple}
                onClick={() => setImportOpen(true)}
              >
                Importar usuários
              </Button>
              <Button
                variant="primary"
                size="md"
                leftIcon={Plus}
                onClick={() => setInviteOpen(true)}
              >
                Convidar usuário
              </Button>
            </div>
          }
        />
        <div className="flex flex-col gap-[var(--sp-sm)] px-[var(--sp-lg)] py-[var(--sp-sm)]">
          <FilterBar
            filters={getAvailableFilters(FILTER_OPTIONS)}
            onAddFilter={(id: string) => {
              addFilterAndOpen(id);
            }}
            onClearAll={activeFilters.length > 0 ? clearAllFilters : undefined}
          >
            {activeFilters.map((filterId) => (
              <div
                key={filterId}
                ref={chipRefs[filterId]}
                className="inline-flex"
              >
                <FilterChip
                  label={getFilterLabel(filterId)}
                  active={openFilter === filterId}
                  onClick={() => toggleFilterDropdown(filterId)}
                  onRemove={() => removeFilter(filterId)}
                />
              </div>
            ))}
          </FilterBar>
        </div>

        <FilterDropdown
          open={openFilter === "status"}
          onClose={() => setOpenFilter(null)}
          anchorRef={statusChipRef}
          ignoreRefs={ignoreChipRefs}
        >
          <div className="flex flex-col max-h-[320px] overflow-y-auto">
            {STATUS_FILTER.map((opt) => (
              <button
                key={opt.id}
                type="button"
                className={`flex items-center gap-[var(--sp-2xs)] px-[var(--sp-sm)] py-[var(--sp-2xs)] border-0 bg-transparent font-[var(--font-body)] text-[var(--text-sm)] text-[var(--color-neutral-700)] cursor-pointer text-left w-full transition-[background] duration-100 hover:bg-[var(--color-caramel-50)] ${filterStatus === opt.id ? "bg-[var(--color-orange-50)] text-[var(--color-orange-700)]" : ""}`}
                onClick={() => {
                  setFilterStatus(opt.id);
                  setOpenFilter(null);
                }}
              >
                <Radio checked={filterStatus === opt.id} readOnly />
                <span>{opt.label}</span>
              </button>
            ))}
          </div>
        </FilterDropdown>

        <FilterDropdown
          open={openFilter === "role"}
          onClose={() => setOpenFilter(null)}
          anchorRef={roleChipRef}
          ignoreRefs={ignoreChipRefs}
        >
          <div className="flex flex-col max-h-[320px] overflow-y-auto">
            {roleFilterOptions.map((opt) => (
              <button
                key={opt.id}
                type="button"
                className={`flex items-center gap-[var(--sp-2xs)] px-[var(--sp-sm)] py-[var(--sp-2xs)] border-0 bg-transparent font-[var(--font-body)] text-[var(--text-sm)] text-[var(--color-neutral-700)] cursor-pointer text-left w-full transition-[background] duration-100 hover:bg-[var(--color-caramel-50)] ${filterRole === opt.id ? "bg-[var(--color-orange-50)] text-[var(--color-orange-700)]" : ""}`}
                onClick={() => {
                  setFilterRole(opt.id);
                  setOpenFilter(null);
                }}
              >
                <Radio checked={filterRole === opt.id} readOnly />
                <span>{opt.label}</span>
              </button>
            ))}
          </div>
        </FilterDropdown>

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
              {filtered.map((u) => {
                const sb = STATUS_BADGE[u.status]!;
                return (
                  <TableRow key={u.id} rowId={u.id}>
                    <TableCell isCheckbox rowId={u.id} />
                    <TableCell>
                      <AvatarLabelGroup
                        size="md"
                        initials={u.initials ?? undefined}
                        name={`${u.firstName} ${u.lastName}`}
                        supportingText={u.jobTitle ?? undefined}
                      />
                    </TableCell>
                    <TableCell>
                      <div className="flex flex-wrap gap-[var(--sp-3xs)]">
                        {u.teams.map((team) => (
                          <Badge key={team} color="neutral">
                            {team}
                          </Badge>
                        ))}
                      </div>
                    </TableCell>
                    <TableCell>
                      <div className="flex flex-col gap-[var(--sp-3xs)] relative">
                        <button
                          ref={(el) => {
                            rolePopoverRefs.current[u.id] = el;
                          }}
                          className="flex items-center gap-[var(--sp-2xs)] px-[var(--sp-sm)] py-[var(--sp-2xs)] border border-[var(--color-caramel-300)] rounded-[var(--radius-sm)] bg-[var(--color-neutral-0)] cursor-pointer min-h-[36px] text-left hover:border-[var(--color-caramel-500)] focus-visible:outline-2 focus-visible:outline-[var(--color-orange-500)] focus-visible:outline-offset-2"
                          onClick={() =>
                            setRolePopoverUser(
                              rolePopoverUser === u.id ? null : u.id,
                            )
                          }
                          type="button"
                        >
                          <span className="flex-1 min-w-0 font-[var(--font-label)] font-medium text-[var(--text-sm)] text-[var(--color-neutral-900)] leading-[1.15]">
                            {roleLabelBySlug.get(resolveRoleSlug(u.roleType)) ??
                              u.roleType}
                          </span>
                          <CaretDown
                            size={14}
                            className="shrink-0 text-[var(--color-neutral-400)] transition-transform duration-150"
                          />
                        </button>
                        <FilterDropdown
                          open={rolePopoverUser === u.id}
                          onClose={() => setRolePopoverUser(null)}
                          anchorRef={{
                            current: rolePopoverRefs.current[u.id] ?? null,
                          }}
                        >
                          <div className="flex flex-col p-[var(--sp-3xs)] max-h-[360px] overflow-y-auto">
                            {roleSelectionOptions.map((opt) => (
                              <button
                                key={opt.value}
                                type="button"
                                className={`flex items-start gap-[var(--sp-2xs)] px-[var(--sp-sm)] py-[var(--sp-2xs)] border-0 bg-transparent cursor-pointer text-left w-full rounded-[var(--radius-xs)] transition-colors duration-[120ms] hover:bg-[var(--color-caramel-100)] ${resolveRoleSlug(u.roleType) === opt.value ? "bg-[var(--color-caramel-50)]" : ""}`}
                                onClick={() =>
                                  handleRoleChange(u.id, opt.value)
                                }
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
                        items={getRowActions(u)}
                        open={actionsPopoverUser === u.id}
                        onToggle={() =>
                          setActionsPopoverUser(
                            actionsPopoverUser === u.id ? null : u.id,
                          )
                        }
                        onClose={() => setActionsPopoverUser(null)}
                        buttonAriaLabel={`Abrir ações de ${u.firstName} ${u.lastName}`}
                      />
                    </TableCell>
                  </TableRow>
                );
              })}
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

      {/* Invite modal */}
      <Modal open={inviteOpen} onClose={() => setInviteOpen(false)} size="md">
        <ModalHeader
          title="Convidar usuário"
          onClose={() => setInviteOpen(false)}
        />
        <ModalBody>
          <div className="flex flex-col gap-[var(--sp-md)]">
            <div className="grid grid-cols-2 gap-[var(--sp-md)]">
              <Input
                label="Nome"
                value={inviteFirstName}
                onChange={(e: ChangeEvent<HTMLInputElement>) =>
                  setInviteFirstName(e.target.value)
                }
                placeholder="Nome"
              />
              <Input
                label="Sobrenome"
                value={inviteLastName}
                onChange={(e: ChangeEvent<HTMLInputElement>) =>
                  setInviteLastName(e.target.value)
                }
                placeholder="Sobrenome"
              />
            </div>
            <div className="grid grid-cols-2 gap-[var(--sp-md)]">
              <Input
                label="Apelido"
                value={inviteNickname}
                onChange={(e: ChangeEvent<HTMLInputElement>) =>
                  setInviteNickname(e.target.value)
                }
                placeholder="Como prefere ser chamado"
              />
              <Input
                label="E-mail"
                value={inviteEmail}
                onChange={(e: ChangeEvent<HTMLInputElement>) =>
                  setInviteEmail(e.target.value)
                }
                placeholder="email@empresa.com"
                leftIcon={Envelope}
              />
            </div>
            <div className="grid grid-cols-2 gap-[var(--sp-md)]">
              <Input
                label="Cargo"
                value={inviteJobTitle}
                onChange={(e: ChangeEvent<HTMLInputElement>) =>
                  setInviteJobTitle(e.target.value)
                }
                placeholder="Ex: Product Manager"
              />
              <Select
                label="Time"
                value={inviteTeam}
                onChange={setInviteTeam}
                options={inviteTeamOptions}
              />
            </div>
            <div className="grid grid-cols-2 gap-[var(--sp-md)]">
              <div className="flex flex-col gap-[var(--sp-3xs)]">
                <span className="font-[var(--font-label)] font-medium text-[var(--text-xs)] text-[var(--color-neutral-700)] leading-[1.15]">
                  Data de nascimento
                </span>
                <DatePicker
                  mode="single"
                  value={inviteBirthDate}
                  onChange={setInviteBirthDate}
                />
              </div>
              <Select
                label="Gênero"
                value={inviteGender}
                onChange={setInviteGender}
                options={GENDER_OPTIONS}
              />
            </div>
            <Select
              label="Idioma"
              value={inviteLanguage}
              onChange={setInviteLanguage}
              options={LANGUAGE_OPTIONS}
            />
            <div className="flex flex-col gap-[var(--sp-3xs)] relative">
              <span className="font-[var(--font-label)] font-medium text-[var(--text-xs)] text-[var(--color-neutral-700)] leading-[1.15]">
                Tipo de usuário
              </span>
              <button
                ref={inviteRoleBtnRef}
                className="flex items-center gap-[var(--sp-2xs)] px-[var(--sp-sm)] py-[var(--sp-2xs)] border border-[var(--color-caramel-300)] rounded-[var(--radius-sm)] bg-[var(--color-neutral-0)] cursor-pointer min-h-[36px] text-left hover:border-[var(--color-caramel-500)] focus-visible:outline-2 focus-visible:outline-[var(--color-orange-500)] focus-visible:outline-offset-2"
                onClick={() => setInviteRoleOpen((v) => !v)}
                type="button"
              >
                <span className="flex-1 min-w-0 font-[var(--font-label)] font-medium text-[var(--text-sm)] text-[var(--color-neutral-900)] leading-[1.15]">
                  {roleLabelBySlug.get(inviteRole) ?? "Selecione"}
                </span>
                <CaretDown
                  size={14}
                  className="shrink-0 text-[var(--color-neutral-400)] transition-transform duration-150"
                />
              </button>
              <FilterDropdown
                open={inviteRoleOpen}
                onClose={() => setInviteRoleOpen(false)}
                anchorRef={inviteRoleBtnRef}
              >
                <div className="flex flex-col p-[var(--sp-3xs)] max-h-[360px] overflow-y-auto">
                  {roleSelectionOptions.map((opt) => (
                    <button
                      key={opt.value}
                      type="button"
                      className={`flex items-start gap-[var(--sp-2xs)] px-[var(--sp-sm)] py-[var(--sp-2xs)] border-0 bg-transparent cursor-pointer text-left w-full rounded-[var(--radius-xs)] transition-colors duration-[120ms] hover:bg-[var(--color-caramel-100)] ${inviteRole === opt.value ? "bg-[var(--color-caramel-50)]" : ""}`}
                      onClick={() => {
                        setInviteRole(opt.value);
                        setInviteRoleOpen(false);
                      }}
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
          </div>
        </ModalBody>
        <ModalFooter>
          <Button
            variant="tertiary"
            size="md"
            onClick={() => setInviteOpen(false)}
          >
            Cancelar
          </Button>
          <Button
            variant="primary"
            size="md"
            disabled={
              !inviteFirstName.trim() ||
              !inviteLastName.trim() ||
              !inviteEmail.trim()
            }
            onClick={handleInvite}
          >
            Enviar convite
          </Button>
        </ModalFooter>
      </Modal>

      {/* Import modal */}
      <Modal
        open={importOpen}
        onClose={() => {
          setImportOpen(false);
          setImportFile(null);
        }}
        size="sm"
      >
        <ModalHeader
          title="Importar usuários"
          onClose={() => {
            setImportOpen(false);
            setImportFile(null);
          }}
        />
        <ModalBody>
          <div className="flex flex-col gap-[var(--sp-md)]">
            <p className="font-[var(--font-body)] text-[var(--text-sm)] text-[var(--color-neutral-600)] m-0 leading-[1.5]">
              Faça o upload de uma planilha com os dados dos usuários para
              cadastrá-los em massa na plataforma.
            </p>
            <button
              type="button"
              className="flex items-center gap-[var(--sp-sm)] p-[var(--sp-sm)] border border-[var(--color-caramel-300)] rounded-[var(--radius-sm)] bg-[var(--color-neutral-0)] cursor-pointer text-left w-full transition-colors duration-[120ms] text-[var(--color-green-600)] hover:bg-[var(--color-caramel-50)]"
              onClick={handleDownloadTemplate}
            >
              <FileText size={20} />
              <div className="flex flex-col gap-[2px] flex-1 min-w-0">
                <span className="font-[var(--font-label)] font-medium text-[var(--text-sm)] text-[var(--color-neutral-900)]">
                  Baixar template
                </span>
                <span className="font-[var(--font-body)] text-[var(--text-xs)] text-[var(--color-neutral-500)]">
                  Template CSV com os campos e valores aceitos
                </span>
              </div>
              <DownloadSimple size={16} />
            </button>
            <div
              className="flex flex-col items-center justify-center gap-[var(--sp-2xs)] p-[var(--sp-xl)] border-2 border-dashed border-[var(--color-caramel-300)] rounded-[var(--radius-sm)] bg-[var(--color-caramel-50)] cursor-pointer transition-[border-color,background-color] duration-[120ms] text-[var(--color-neutral-400)] hover:border-[var(--color-orange-300)] hover:bg-[var(--color-orange-50)]"
              onClick={() => fileInputRef.current?.click()}
            >
              <input
                ref={fileInputRef}
                type="file"
                accept=".xls,.xlsx,.csv"
                className="hidden"
                onChange={(e) => setImportFile(e.target.files?.[0] ?? null)}
              />
              <UploadSimple size={24} />
              {importFile ? (
                <span className="font-[var(--font-label)] font-medium text-[var(--text-sm)] text-[var(--color-orange-600)]">
                  {importFile.name}
                </span>
              ) : (
                <>
                  <span className="font-[var(--font-label)] font-medium text-[var(--text-sm)] text-[var(--color-neutral-700)]">
                    Arraste ou clique para selecionar
                  </span>
                  <span className="font-[var(--font-body)] text-[var(--text-xs)] text-[var(--color-neutral-500)]">
                    .xls, .xlsx ou .csv
                  </span>
                </>
              )}
            </div>
          </div>
        </ModalBody>
        <ModalFooter>
          <Button
            variant="tertiary"
            size="md"
            onClick={() => {
              setImportOpen(false);
              setImportFile(null);
            }}
          >
            Cancelar
          </Button>
          <Button
            variant="primary"
            size="md"
            disabled={!importFile}
            onClick={handleImport}
          >
            Importar
          </Button>
        </ModalFooter>
      </Modal>

      {/* Activate/Deactivate confirmation */}
      <Modal
        open={!!deactivateUser}
        onClose={() => setDeactivateUser(null)}
        size="sm"
      >
        <ModalHeader
          title={
            deactivateUser?.status === "active"
              ? "Desativar usuário"
              : "Ativar usuário"
          }
          onClose={() => setDeactivateUser(null)}
        />
        <ModalBody>
          {deactivateUser && (
            <p className="font-[var(--font-body)] text-[var(--text-sm)] text-[var(--color-neutral-700)] m-0 leading-[1.5]">
              {deactivateUser.status === "active" ? (
                <>
                  Tem certeza que deseja desativar{" "}
                  <strong>
                    {deactivateUser.firstName} {deactivateUser.lastName}
                  </strong>
                  ? O usuário perderá acesso à plataforma.
                </>
              ) : (
                <>
                  Tem certeza que deseja ativar{" "}
                  <strong>
                    {deactivateUser.firstName} {deactivateUser.lastName}
                  </strong>
                  ? O usuário voltará a ter acesso à plataforma.
                </>
              )}
            </p>
          )}
        </ModalBody>
        <ModalFooter>
          <Button
            variant="tertiary"
            size="md"
            onClick={() => setDeactivateUser(null)}
          >
            Cancelar
          </Button>
          <Button
            variant={deactivateUser?.status === "active" ? "danger" : "primary"}
            size="md"
            onClick={handleToggleStatus}
          >
            {deactivateUser?.status === "active" ? "Desativar" : "Ativar"}
          </Button>
        </ModalFooter>
      </Modal>
    </>
  );
}
