"use client";

import { useState, useMemo } from "react";
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
} from "@mdonangelo/bud-ds";
import type { AvatarGroupItem } from "@mdonangelo/bud-ds";
import {
  Archive,
  Trash,
  PencilSimple,
  UsersThree,
  ArrowCounterClockwise,
} from "@phosphor-icons/react";
import type { Team, TeamMember, TeamColor } from "@/types";
import { useOrganization } from "@/contexts/OrganizationContext";
import { useOrgPeople } from "@/hooks/useOrgPeople";
import { useTeams } from "./hooks/useTeams";
import {
  useCreateTeam,
  useUpdateTeam,
  useDeleteTeam,
  useBulkArchiveTeams,
  useBulkDeleteTeams,
  extractLeaderId,
} from "./hooks/useTeamMutations";
import { useDataTable } from "@/hooks/useDataTable";
import { TeamsTableHeader } from "./components/TeamsTableHeader";
import { TeamsFilterBar } from "./components/TeamsFilterBar";
import { TeamsTableRow } from "./components/TeamsTableRow";
import { DeleteTeamModal } from "./components/DeleteTeamModal";
import { TeamModal } from "./components/TeamModal";
import type { PersonView } from "./components/TeamModal";
import { PageHeader } from "@/presentation/layout/page-header";
import { ConfigLoadingState } from "@/components/ConfigLoadingState";
import { ConfigErrorState } from "@/components/ConfigErrorState";

/* ——— View helper ——— */

function personFromMember(m: TeamMember): PersonView | null {
  if (!m.user) return null;
  return {
    id: m.user.id,
    fullName: m.user.fullName,
    jobTitle: m.user.jobTitle ?? "",
    initials: m.user.initials ?? "",
    teamIds: [],
  };
}

/* ——— Component ——— */

export function TeamsModule() {
  const { activeOrgId } = useOrganization();
  const { data: orgPeople = [] } = useOrgPeople(activeOrgId);
  const { data: teams = [], isLoading, isError } = useTeams(activeOrgId);
  const createTeam = useCreateTeam(activeOrgId);
  const updateTeam = useUpdateTeam(activeOrgId);
  const deleteTeamMutation = useDeleteTeam(activeOrgId);
  const bulkArchive = useBulkArchiveTeams(activeOrgId);
  const bulkDelete = useBulkDeleteTeams(activeOrgId);

  const [search, setSearch] = useState("");
  const [filterStatus, setFilterStatus] = useState("all");
  const [actionsPopoverTeam, setActionsPopoverTeam] = useState<string | null>(
    null,
  );
  const [teamModalState, setTeamModalState] = useState<{
    team: Team | null;
    tab: "details" | "members";
  } | null>(null);
  const [deleteTeam, setDeleteTeam] = useState<Team | null>(null);

  type SortKey = "name" | "members" | "status";
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

  /* ——— Derived ——— */

  const filtered = useMemo(
    () =>
      teams
        .filter((t) => {
          if (
            search &&
            !t.name.toLowerCase().includes(search.toLowerCase()) &&
            !(t.description ?? "").toLowerCase().includes(search.toLowerCase())
          )
            return false;
          if (filterStatus !== "all" && t.status !== filterStatus) return false;
          return true;
        })
        .sort((a, b) => {
          if (!sortKey) return 0;
          const dir = sortDir === "asc" ? 1 : -1;
          switch (sortKey) {
            case "name":
              return dir * a.name.localeCompare(b.name);
            case "members":
              return (
                dir * ((a.members ?? []).length - (b.members ?? []).length)
              );
            case "status":
              return dir * a.status.localeCompare(b.status);
            default:
              return 0;
          }
        }),
    [teams, search, filterStatus, sortKey, sortDir],
  );

  const rowIds = useMemo(() => filtered.map((t) => t.id), [filtered]);

  const teamIdByName = useMemo(
    () => new Map(teams.map((t) => [t.name, t.id])),
    [teams],
  );

  const peoplePool = useMemo(
    () =>
      orgPeople.map((person) => ({
        id: person.id,
        fullName: person.fullName,
        jobTitle: person.jobTitle ?? "",
        initials:
          person.initials ??
          person.fullName
            .trim()
            .split(" ")
            .filter(Boolean)
            .map((p) => p[0] ?? "")
            .slice(0, 2)
            .join("")
            .toUpperCase(),
        teamIds: person.teams
          .map((t) => teamIdByName.get(t.name))
          .filter((id): id is string => !!id),
      })),
    [orgPeople, teamIdByName],
  );

  /* ——— Handlers ——— */

  function openCreate() {
    setTeamModalState({ team: null, tab: "details" });
  }

  function openEdit(team: Team) {
    setTeamModalState({ team, tab: "details" });
    setActionsPopoverTeam(null);
  }

  function openMembers(team: Team) {
    setTeamModalState({ team, tab: "members" });
    setActionsPopoverTeam(null);
  }

  function handleTeamModalSave(data: {
    name: string;
    description: string;
    color: TeamColor;
    members: TeamMember[];
    parentTeamId: string | null;
  }) {
    const leaderId = extractLeaderId(data.members);
    const editingTeam = teamModalState?.team;

    if (editingTeam) {
      updateTeam.mutate(
        {
          id: editingTeam.id,
          name: data.name,
          description: data.description || null,
          color: data.color,
          leaderId,
        },
        {
          onSuccess: () => toast.success(`Time "${data.name}" atualizado`),
          onError: () => toast.error(`Erro ao atualizar "${data.name}"`),
        },
      );
    } else {
      if (!activeOrgId || !leaderId) return;
      createTeam.mutate(
        {
          name: data.name,
          description: data.description || null,
          color: data.color,
          organizationId: activeOrgId,
          leaderId,
        },
        {
          onSuccess: () => toast.success(`Time "${data.name}" criado`),
          onError: () => toast.error(`Erro ao criar "${data.name}"`),
        },
      );
    }
    setTeamModalState(null);
  }

  function handleToggleStatus(team: Team) {
    const newStatus =
      team.status === "active" ? ("archived" as const) : ("active" as const);
    setActionsPopoverTeam(null);
    updateTeam.mutate(
      { id: team.id, status: newStatus },
      {
        onSuccess: () =>
          toast.success(
            newStatus === "active"
              ? `"${team.name}" ativado`
              : `"${team.name}" arquivado`,
          ),
        onError: () => toast.error(`Erro ao alterar status de "${team.name}"`),
      },
    );
  }

  function handleDelete() {
    if (!deleteTeam) return;
    const name = deleteTeam.name;
    deleteTeamMutation.mutate(deleteTeam.id, {
      onSuccess: () => toast.success(`Time "${name}" excluído`),
      onError: () => toast.error(`Erro ao excluir "${name}"`),
    });
    setDeleteTeam(null);
  }

  function handleBulkArchive() {
    const ids = [...selectedRows];
    bulkArchive.mutate(ids, {
      onSuccess: () => toast.success(`${ids.length} time(s) arquivado(s)`),
      onError: () => toast.error("Erro ao arquivar times"),
    });
    clearSelection();
  }

  function handleBulkDelete() {
    const ids = [...selectedRows];
    bulkDelete.mutate(ids, {
      onSuccess: () => toast.success(`${ids.length} time(s) excluído(s)`),
      onError: () => toast.error("Erro ao excluir times"),
    });
    clearSelection();
  }

  function getRowActions(team: Team) {
    return [
      {
        id: "edit",
        label: "Editar time",
        icon: PencilSimple,
        onClick: () => openEdit(team),
      },
      {
        id: "members",
        label: "Gerenciar membros",
        icon: UsersThree,
        onClick: () => openMembers(team),
      },
      team.status === "active"
        ? {
            id: "archive",
            label: "Arquivar time",
            icon: Archive,
            onClick: () => handleToggleStatus(team),
          }
        : {
            id: "activate",
            label: "Ativar time",
            icon: ArrowCounterClockwise,
            onClick: () => handleToggleStatus(team),
          },
      {
        id: "delete",
        label: "Excluir time",
        icon: Trash,
        danger: true,
        onClick: () => {
          setDeleteTeam(team);
          setActionsPopoverTeam(null);
        },
      },
    ];
  }

  /* ——— Render ——— */

  if (isLoading) return <ConfigLoadingState />;
  if (isError)
    return (
      <ConfigErrorState message="Não foi possível carregar os times. Verifique sua conexão e tente novamente." />
    );

  return (
    <div className="flex flex-col gap-[var(--sp-2xs)] w-full">
      <PageHeader title="Times" />
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
          <TeamsTableHeader
            count={filtered.length}
            search={search}
            onSearch={setSearch}
            onCreate={openCreate}
          />

          <TeamsFilterBar
            filterStatus={filterStatus}
            onFilterStatusChange={setFilterStatus}
          />

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
                <TableHeaderCell>Líder</TableHeaderCell>
                <TableHeaderCell
                  sortable
                  sortDirection={getSortDirection("members")}
                  onSort={() => handleSort("members")}
                >
                  Membros
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
              {filtered.map((team) => {
                const members = team.members ?? [];
                const leader = team.leaderId
                  ? personFromMember(
                      members.find((m) => m.userId === team.leaderId) ?? {
                        userId: "",
                        teamId: "",
                        roleInTeam: "member",
                        joinedAt: "",
                        user: undefined,
                      },
                    )
                  : null;
                const avatars: AvatarGroupItem[] = members
                  .slice(0, 5)
                  .map((m) => ({ initials: m.user?.initials ?? "" }));

                return (
                  <TeamsTableRow
                    key={team.id}
                    team={team}
                    leader={leader}
                    avatars={avatars}
                    rowActions={getRowActions(team)}
                    isActionsOpen={actionsPopoverTeam === team.id}
                    onActionsToggle={() =>
                      setActionsPopoverTeam(
                        actionsPopoverTeam === team.id ? null : team.id,
                      )
                    }
                    onActionsClose={() => setActionsPopoverTeam(null)}
                    onOpenMembers={openMembers}
                  />
                );
              })}
            </TableBody>
          </TableContent>

          <TableBulkActions count={selectedRows.size} onClear={clearSelection}>
            <Button
              variant="secondary"
              size="md"
              leftIcon={Archive}
              onClick={handleBulkArchive}
            >
              Arquivar
            </Button>
            <Button
              variant="secondary"
              size="md"
              leftIcon={Trash}
              onClick={handleBulkDelete}
            >
              Excluir
            </Button>
          </TableBulkActions>
        </Table>

        <TeamModal
          open={!!teamModalState}
          team={teamModalState?.team ?? null}
          initialTab={teamModalState?.tab ?? "details"}
          peoplePool={peoplePool}
          allTeams={teams.map((t) => ({ id: t.id, name: t.name }))}
          onClose={() => setTeamModalState(null)}
          onSave={handleTeamModalSave}
        />

        <DeleteTeamModal
          team={deleteTeam}
          onClose={() => setDeleteTeam(null)}
          onConfirm={handleDelete}
        />
      </div>
    </div>
  );
}
