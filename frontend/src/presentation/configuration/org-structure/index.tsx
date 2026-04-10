"use client";

import {
  useState,
  useRef,
  useMemo,
  useCallback,
  type ChangeEvent,
  type MouseEvent,
  type TouchEvent,
  type WheelEvent,
} from "react";
import {
  Card,
  CardBody,
  Button,
  Input,
  Badge,
  Avatar,
  toast,
  DrawerHeader,
  DrawerBody,
  DragToCloseDrawer,
} from "@mdonangelo/bud-ds";
import {
  MagnifyingGlass,
  Plus,
  Minus,
  CaretDown,
  CaretRight,
  UsersThree,
  Warning,
  X,
  PencilSimple,
  UserCirclePlus,
  ListBullets,
  TreeStructure,
  ArrowsOutSimple,
  ArrowsInSimple,
} from "@phosphor-icons/react";
import { useOrganization } from "@/contexts/OrganizationContext";
import { useOrgPeople } from "@/hooks/useOrgPeople";
import { useTeams } from "@/presentation/configuration/team/hooks/useTeams";
import { usePatchEmployee } from "./hooks/useEmployeeMutations";
import { useUpdateTeam } from "@/presentation/configuration/team/hooks/useTeamMutations";
import { buildTeamTree, flattenTeamTree, type TeamTreeNode } from "./utils";
import { PersonPicker } from "./components/PersonPicker";

/* ——— Main component ——— */

export function OrgStructureModule() {
  const { activeOrgId } = useOrganization();
  const { data: people = [] } = useOrgPeople(activeOrgId);
  const { data: rawTeams = [] } = useTeams(activeOrgId);
  const patchEmployee = usePatchEmployee(activeOrgId);
  const updateTeam = useUpdateTeam(activeOrgId);

  /* filter to active teams */
  const teams = useMemo(
    () => rawTeams.filter((t) => t.status === "active"),
    [rawTeams],
  );

  const [search, setSearch] = useState("");
  const [expanded, setExpanded] = useState<Set<string>>(new Set());
  const [zoom, setZoom] = useState(100);
  const [highlightId, setHighlightId] = useState<string | null>(null);
  const [viewMode, setViewMode] = useState<"list" | "chart">("chart");

  /* pan state for chart view */
  const [panOffset, setPanOffset] = useState({ x: 0, y: 0 });
  const [isDragging, setIsDragging] = useState(false);
  const isPanning = useRef(false);
  const didPan = useRef(false);
  const panStart = useRef({ x: 0, y: 0 });
  const panOffsetStart = useRef({ x: 0, y: 0 });
  const chartContainerRef = useRef<HTMLDivElement>(null);

  /* detail panel */
  const [selectedId, setSelectedId] = useState<string | null>(null);
  const [editingLeader, setEditingLeader] = useState(false);
  const [addingMember, setAddingMember] = useState(false);

  /* view mode dropdown */
  const [viewModeOpen, setViewModeOpen] = useState(false);
  const viewModeBtnRef = useRef<HTMLButtonElement>(null);

  function closePanel() {
    setSelectedId(null);
    setEditingLeader(false);
    setAddingMember(false);
  }

  /* derived */
  const teamTree = useMemo(() => buildTeamTree(teams, people), [teams, people]);
  const allFlatTeams = useMemo(() => flattenTeamTree(teamTree), [teamTree]);

  const selectedTeamNode = useMemo(
    () =>
      selectedId
        ? (allFlatTeams.find((n) => n.team.id === selectedId) ?? null)
        : null,
    [allFlatTeams, selectedId],
  );
  const selectedTeam = selectedTeamNode?.team ?? null;
  const selectedLeader = selectedTeamNode?.leader ?? null;
  const selectedMembers = useMemo(
    () => selectedTeamNode?.members ?? [],
    [selectedTeamNode],
  );

  /* search results — teams + people */
  const searchResults = useMemo(() => {
    if (!search.trim()) return [];
    const q = search.toLowerCase();
    const matchedTeams = teams
      .filter((t) => t.name.toLowerCase().includes(q))
      .slice(0, 5)
      .map((t) => ({ kind: "team" as const, id: t.id, label: t.name }));
    const matchedPeople = people
      .filter(
        (p) =>
          p.fullName.toLowerCase().includes(q) ||
          (p.jobTitle ?? "").toLowerCase().includes(q),
      )
      .slice(0, 5)
      .map((p) => ({
        kind: "person" as const,
        id: p.id,
        label: p.fullName,
        teamId: p.teams[0]?.id ?? null,
      }));
    return [...matchedTeams, ...matchedPeople].slice(0, 8);
  }, [search, teams, people]);

  const teamsWithoutLeader = useMemo(
    () => teams.filter((t) => !t.leaderId),
    [teams],
  );

  /* expand / collapse */
  function toggleExpand(id: string) {
    setExpanded((prev) => {
      const next = new Set(prev);
      next.has(id) ? next.delete(id) : next.add(id);
      return next;
    });
  }

  function expandAll() {
    setExpanded(
      new Set(
        teams
          .filter((t) => teams.some((child) => child.parentTeamId === t.id))
          .map((t) => t.id),
      ),
    );
  }

  function collapseAll() {
    setExpanded(new Set());
  }

  /* zoom */
  function zoomIn() {
    setZoom((z) => Math.min(z + 10, 150));
  }
  function zoomOut() {
    setZoom((z) => Math.max(z - 10, 50));
  }
  function zoomReset() {
    setZoom(100);
    setPanOffset({ x: 0, y: 0 });
  }

  /* pan — mouse */
  function handlePanStart(e: MouseEvent) {
    if (e.button !== 0) return;
    const target = e.target as HTMLElement;
    if (target.closest("button, a, input")) return;
    isPanning.current = true;
    didPan.current = false;
    setIsDragging(true);
    panStart.current = { x: e.clientX, y: e.clientY };
    panOffsetStart.current = { ...panOffset };
    e.preventDefault();
  }

  function handlePanMove(e: MouseEvent) {
    if (!isPanning.current) return;
    const dx = e.clientX - panStart.current.x;
    const dy = e.clientY - panStart.current.y;
    if (Math.abs(dx) > 3 || Math.abs(dy) > 3) didPan.current = true;
    setPanOffset({
      x: panOffsetStart.current.x + dx,
      y: panOffsetStart.current.y + dy,
    });
  }

  function handlePanEnd() {
    isPanning.current = false;
    setIsDragging(false);
  }

  /* pan — touch */
  const lastTouchDist = useRef<number | null>(null);

  function handleTouchStart(e: TouchEvent) {
    const target = e.target as HTMLElement;
    if (target.closest("button, a, input")) return;
    if (e.touches.length === 1) {
      const touch = e.touches.item(0);
      if (!touch) return;
      isPanning.current = true;
      didPan.current = false;
      setIsDragging(true);
      panStart.current = { x: touch.clientX, y: touch.clientY };
      panOffsetStart.current = { ...panOffset };
    } else if (e.touches.length === 2) {
      const t0 = e.touches.item(0);
      const t1 = e.touches.item(1);
      if (!t0 || !t1) return;
      lastTouchDist.current = Math.hypot(
        t0.clientX - t1.clientX,
        t0.clientY - t1.clientY,
      );
    }
  }

  function handleTouchMove(e: TouchEvent) {
    if (e.touches.length === 1 && isPanning.current) {
      const touch = e.touches.item(0);
      if (!touch) return;
      const dx = touch.clientX - panStart.current.x;
      const dy = touch.clientY - panStart.current.y;
      if (Math.abs(dx) > 3 || Math.abs(dy) > 3) didPan.current = true;
      setPanOffset({
        x: panOffsetStart.current.x + dx,
        y: panOffsetStart.current.y + dy,
      });
      e.preventDefault();
    } else if (e.touches.length === 2 && lastTouchDist.current !== null) {
      const t0 = e.touches.item(0);
      const t1 = e.touches.item(1);
      if (!t0 || !t1) return;
      const dist = Math.hypot(t0.clientX - t1.clientX, t0.clientY - t1.clientY);
      const delta = dist - lastTouchDist.current;
      if (Math.abs(delta) > 2) {
        setZoom((z) => Math.min(Math.max(z + (delta > 0 ? 3 : -3), 50), 150));
        lastTouchDist.current = dist;
      }
      e.preventDefault();
    }
  }

  function handleTouchEnd() {
    isPanning.current = false;
    setIsDragging(false);
    lastTouchDist.current = null;
  }

  function handleWheel(e: WheelEvent) {
    if (e.ctrlKey || e.metaKey) {
      e.preventDefault();
      setZoom((z) => Math.min(Math.max(z + (e.deltaY > 0 ? -5 : 5), 50), 150));
    }
  }

  /* select */
  function selectTeam(id: string) {
    if (didPan.current) return;
    setSelectedId(id);
    setEditingLeader(false);
    setAddingMember(false);

    /* expand ancestors */
    const node = allFlatTeams.find((n) => n.team.id === id);
    if (node) {
      const ancestorIds: string[] = [];
      let parentId = node.team.parentTeamId;
      while (parentId) {
        ancestorIds.push(parentId);
        const parent = allFlatTeams.find((n) => n.team.id === parentId);
        parentId = parent?.team.parentTeamId ?? null;
      }
      setExpanded((prev) => {
        const next = new Set(prev);
        ancestorIds.forEach((aid) => next.add(aid));
        return next;
      });
    }

    setHighlightId(id);
    setTimeout(() => setHighlightId(null), 2000);
  }

  function handleSearchSelect(result: (typeof searchResults)[number]) {
    if (result.kind === "team") {
      selectTeam(result.id);
    } else {
      const teamId = result.teamId;
      if (teamId) selectTeam(teamId);
    }
    setSearch("");
  }

  /* ——— Edit actions ——— */

  function handleChangeLeader(newLeaderId: string) {
    if (!selectedTeam) return;
    const newLeader = people.find((p) => p.id === newLeaderId);
    updateTeam.mutate(
      { id: selectedTeam.id, leaderId: newLeaderId },
      {
        onSuccess: () => {
          setEditingLeader(false);
          toast.success(
            `Líder de ${selectedTeam.name} alterado para ${newLeader?.fullName ?? "?"}`,
          );
        },
        onError: () => toast.error("Erro ao alterar líder"),
      },
    );
  }

  function handleRemoveLeader() {
    if (!selectedTeam) return;
    updateTeam.mutate(
      { id: selectedTeam.id, leaderId: null },
      {
        onSuccess: () =>
          toast.success(`Líder removido de ${selectedTeam.name}`),
        onError: () => toast.error("Erro ao remover líder"),
      },
    );
  }

  function handleAddMember(personId: string) {
    const person = people.find((p) => p.id === personId);
    if (!person || !selectedTeam) return;
    patchEmployee.mutate(
      {
        id: personId,
        teams: [
          ...person.teams,
          { id: selectedTeam.id, name: selectedTeam.name },
        ],
      },
      {
        onSuccess: () => {
          setAddingMember(false);
          toast.success(
            `${person.fullName.split(" ")[0]} adicionado a ${selectedTeam.name}`,
          );
        },
        onError: () => toast.error("Erro ao adicionar membro"),
      },
    );
  }

  function handleRemoveMember(personId: string) {
    const person = people.find((p) => p.id === personId);
    if (!person || !selectedTeam) return;
    patchEmployee.mutate(
      {
        id: personId,
        teams: person.teams.filter((t) => t.id !== selectedTeam.id),
      },
      {
        onSuccess: () =>
          toast.success(
            `${person.fullName.split(" ")[0]} removido de ${selectedTeam.name}`,
          ),
        onError: () => toast.error("Erro ao remover membro"),
      },
    );
  }

  /* picker exclude sets */
  const memberExcludeIds = useMemo(() => {
    const ids = new Set<string>(selectedMembers.map((m) => m.id));
    if (selectedLeader) ids.add(selectedLeader.id);
    return ids;
  }, [selectedMembers, selectedLeader]);

  /* ——— Render list node ——— */
  const renderTeamNode = useCallback(
    (node: TeamTreeNode, depth: number) => {
      const { team, leader, members, subTeams } = node;
      const hasChildren = subTeams.length > 0;
      const isExpanded = expanded.has(team.id);
      const isHighlighted = highlightId === team.id;
      const isSelected = selectedId === team.id;

      return (
        <div key={team.id} className="flex flex-col">
          <div
            className={[
              "flex items-center gap-[var(--sp-2xs)] px-[var(--sp-sm)] py-[var(--sp-2xs)] rounded-[var(--radius-sm)] cursor-pointer transition-colors duration-[120ms]",
              isHighlighted ? "org-node-highlighted" : "",
              isSelected
                ? "bg-[var(--color-orange-50)] hover:bg-[var(--color-orange-50)]"
                : depth === 0
                  ? "bg-[var(--color-caramel-50)] hover:bg-[var(--color-caramel-100)]"
                  : "hover:bg-[var(--color-caramel-50)]",
            ]
              .filter(Boolean)
              .join(" ")}
            onClick={() => selectTeam(team.id)}
          >
            <div className="flex items-center gap-[var(--sp-sm)] flex-1 min-w-0">
              {hasChildren ? (
                <button
                  type="button"
                  className="flex items-center justify-center w-5 h-5 text-[var(--color-neutral-500)] shrink-0 border-0 bg-transparent cursor-pointer rounded-[var(--radius-2xs)] transition-colors duration-100 hover:bg-[var(--color-caramel-100)]"
                  onClick={(e) => {
                    e.stopPropagation();
                    toggleExpand(team.id);
                  }}
                >
                  {isExpanded ? (
                    <CaretDown size={14} />
                  ) : (
                    <CaretRight size={14} />
                  )}
                </button>
              ) : (
                <span className="w-5 shrink-0" />
              )}

              <Badge color={team.color} size="sm">
                {team.name}
              </Badge>

              {leader && (
                <div className="flex items-center gap-[var(--sp-2xs)] min-w-0 flex-1">
                  <Avatar initials={leader.initials ?? undefined} size="sm" />
                  <div className="flex flex-col gap-[1px] min-w-0">
                    <span className="font-[var(--font-label)] font-medium text-[var(--text-sm)] text-[var(--color-neutral-900)] whitespace-nowrap overflow-hidden text-ellipsis">
                      {leader.fullName}
                    </span>
                    <span className="font-[var(--font-body)] text-[var(--text-xs)] text-[var(--color-neutral-500)] whitespace-nowrap overflow-hidden text-ellipsis">
                      {leader.jobTitle}
                    </span>
                  </div>
                </div>
              )}

              <div className="flex items-center gap-[var(--sp-3xs)] shrink-0 ml-auto">
                {members.length > 0 && (
                  <span
                    className="flex items-center gap-[var(--sp-3xs)] font-[var(--font-label)] font-medium text-[var(--text-xs)] text-[var(--color-neutral-500)] px-[var(--sp-2xs)] py-[2px] bg-[var(--color-caramel-100)] rounded-[var(--radius-2xs)] max-sm:hidden"
                    title={`${members.length} membros`}
                  >
                    <UsersThree size={14} />
                    {members.length}
                  </span>
                )}
                {!leader && (
                  <Badge color="error" size="sm" leftIcon={Warning}>
                    Sem líder
                  </Badge>
                )}
              </div>
            </div>
          </div>

          {hasChildren && isExpanded && (
            <div className="flex relative pl-[var(--sp-lg)]">
              <div className="absolute left-[26px] top-0 bottom-[var(--sp-sm)] w-px bg-[var(--color-caramel-300)]" />
              <div className="flex flex-col gap-[var(--sp-3xs)] flex-1 min-w-0">
                {subTeams.map((child) => renderTeamNode(child, depth + 1))}
              </div>
            </div>
          )}
        </div>
      );
    },
    [expanded, highlightId, selectedId],
  );

  /* ——— Render chart node ——— */
  const renderChartTeamNode = useCallback(
    (node: TeamTreeNode) => {
      const { team, leader, members, subTeams } = node;
      const hasChildren = subTeams.length > 0;
      const isExpanded = expanded.has(team.id);
      const isHighlighted = highlightId === team.id;
      const isSelected = selectedId === team.id;

      const visibleChildren =
        hasChildren && isExpanded
          ? subTeams.map((child) => renderChartTeamNode(child)).filter(Boolean)
          : [];

      return (
        <div
          key={team.id}
          className="org-chart-node-group flex flex-col items-center"
        >
          <div
            className={[
              "flex flex-col items-center gap-[var(--sp-3xs)] p-[var(--sp-sm)] px-[var(--sp-md)] border border-[var(--color-caramel-200)] rounded-[var(--radius-sm)] bg-[var(--color-neutral-0)] cursor-pointer min-w-[160px] max-w-[200px] text-center transition-[border-color,box-shadow] duration-[120ms] relative hover:border-[var(--color-caramel-300)] hover:shadow-sm",
              isSelected
                ? "border-[var(--color-orange-400)] bg-[var(--color-orange-50)] shadow-[0_0_0_2px_var(--color-orange-200)]"
                : "",
              isHighlighted ? "org-chart-card-highlighted" : "",
            ]
              .filter(Boolean)
              .join(" ")}
            onClick={() => selectTeam(team.id)}
          >
            <Badge color={team.color} size="sm">
              {team.name}
            </Badge>

            {leader ? (
              <>
                <Avatar initials={leader.initials ?? undefined} size="md" />
                <span className="font-[var(--font-label)] font-medium text-[var(--text-sm)] text-[var(--color-neutral-900)] leading-[1.15]">
                  {leader.fullName}
                </span>
                <span className="font-[var(--font-body)] text-[var(--text-xs)] text-[var(--color-neutral-500)] leading-[1.3]">
                  {leader.jobTitle}
                </span>
              </>
            ) : (
              <span className="font-[var(--font-body)] text-[var(--text-xs)] text-[var(--color-red-500)]">
                Sem líder
              </span>
            )}

            {members.length > 0 && (
              <span className="flex items-center gap-[var(--sp-3xs)] font-[var(--font-label)] font-medium text-[var(--text-xs)] text-[var(--color-neutral-500)]">
                <UsersThree size={12} />
                {members.length} membros
              </span>
            )}

            {hasChildren && (
              <button
                type="button"
                className="absolute -bottom-3 left-1/2 -translate-x-1/2 flex items-center justify-center w-6 h-6 rounded-full border border-[var(--color-caramel-200)] bg-[var(--color-neutral-0)] text-[var(--color-neutral-500)] cursor-pointer font-[var(--font-label)] font-medium text-[var(--text-xs)] z-[2] transition-[background-color,border-color] duration-100 hover:bg-[var(--color-caramel-50)] hover:border-[var(--color-caramel-300)]"
                onClick={(e) => {
                  e.stopPropagation();
                  toggleExpand(team.id);
                }}
              >
                {isExpanded ? (
                  <Minus size={12} />
                ) : (
                  <span>{subTeams.length}</span>
                )}
              </button>
            )}
          </div>

          {visibleChildren.length > 0 && (
            <div className="org-chart-children-wrap flex flex-col items-center">
              <div
                className={`org-chart-children flex gap-[var(--sp-md)] relative pt-[var(--sp-md)] ${visibleChildren.length === 1 ? "org-chart-children-single" : ""}`}
              >
                {visibleChildren}
              </div>
            </div>
          )}
        </div>
      );
    },
    [expanded, highlightId, selectedId],
  );

  /* stats */
  const totalActive = people.filter((p) => p.status === "active").length;
  const totalTeams = teams.length;

  return (
    <>
      {/* Toolbar */}
      <Card>
        <CardBody>
          <div className="flex items-center justify-between gap-[var(--sp-md)] flex-wrap max-md:flex-col max-md:items-stretch">
            <div className="flex items-center gap-[var(--sp-sm)] flex-1 min-w-0 max-md:flex-col max-md:items-stretch">
              {/* Search */}
              <div className="relative max-w-[320px] flex-1 max-md:max-w-none">
                <Input
                  placeholder="Buscar time ou colaborador..."
                  value={search}
                  onChange={(e: ChangeEvent<HTMLInputElement>) =>
                    setSearch(e.target.value)
                  }
                  leftIcon={MagnifyingGlass}
                />
                {searchResults.length > 0 && search.trim() && (
                  <div className="absolute top-full left-0 right-0 mt-[var(--sp-3xs)] bg-[var(--color-neutral-0)] border border-[var(--color-caramel-200)] rounded-[var(--radius-sm)] shadow-lg max-h-[320px] overflow-y-auto z-50">
                    {searchResults.map((r) => (
                      <button
                        key={`${r.kind}-${r.id}`}
                        type="button"
                        className="flex items-center gap-[var(--sp-sm)] px-[var(--sp-sm)] py-[var(--sp-2xs)] border-0 bg-transparent cursor-pointer text-left w-full transition-colors duration-100 hover:bg-[var(--color-caramel-50)]"
                        onClick={() => handleSearchSelect(r)}
                      >
                        {r.kind === "team" ? (
                          <UsersThree
                            size={16}
                            className="text-[var(--color-neutral-400)] shrink-0"
                          />
                        ) : (
                          <Avatar size="sm" />
                        )}
                        <span className="font-[var(--font-label)] font-medium text-[var(--text-sm)] text-[var(--color-neutral-900)]">
                          {r.label}
                        </span>
                      </button>
                    ))}
                  </div>
                )}
              </div>

              {/* Stats */}
              <div className="flex items-center gap-[var(--sp-3xs)] max-md:flex-wrap">
                <Badge color="neutral">{totalActive} ativos</Badge>
                <Badge color="neutral">{totalTeams} times</Badge>
                {teamsWithoutLeader.length > 0 && (
                  <Badge color="error" leftIcon={Warning}>
                    {teamsWithoutLeader.length} sem líder
                  </Badge>
                )}
              </div>
            </div>

            {/* Controls */}
            <div className="flex items-center gap-[var(--sp-2xs)] max-md:flex-wrap max-md:justify-start max-sm:gap-[var(--sp-3xs)]">
              {/* View mode */}
              <Button
                ref={viewModeBtnRef}
                variant="secondary"
                size="md"
                leftIcon={viewMode === "chart" ? TreeStructure : ListBullets}
                onClick={() => setViewModeOpen((v) => !v)}
              >
                {viewMode === "chart" ? "Vendo em árvore" : "Vendo em lista"}
              </Button>
              {viewModeOpen && (
                <div className="absolute z-50 mt-1 flex flex-col p-[var(--sp-3xs)] bg-[var(--color-neutral-0)] border border-[var(--color-caramel-200)] rounded-[var(--radius-sm)] shadow-lg">
                  {(
                    [
                      { id: "chart", label: "Árvore", icon: TreeStructure },
                      { id: "list", label: "Lista", icon: ListBullets },
                    ] as const
                  ).map((opt) => (
                    <button
                      key={opt.id}
                      type="button"
                      className={[
                        "flex items-center gap-[var(--sp-2xs)] w-full p-[var(--sp-2xs)] border-0 bg-transparent rounded-[var(--radius-2xs)] font-[var(--font-label)] font-medium text-[var(--text-xs)] text-[var(--color-neutral-950)] cursor-pointer whitespace-nowrap transition-colors duration-[120ms] text-left hover:bg-[var(--color-caramel-100)]",
                        viewMode === opt.id
                          ? "bg-[var(--color-caramel-50)]"
                          : "",
                      ]
                        .filter(Boolean)
                        .join(" ")}
                      onClick={() => {
                        setViewMode(opt.id);
                        setViewModeOpen(false);
                      }}
                    >
                      <opt.icon size={14} />
                      <span>{opt.label}</span>
                    </button>
                  ))}
                </div>
              )}

              <Button
                variant="secondary"
                size="md"
                leftIcon={ArrowsOutSimple}
                onClick={expandAll}
              >
                Expandir tudo
              </Button>
              <Button
                variant="secondary"
                size="md"
                leftIcon={ArrowsInSimple}
                onClick={collapseAll}
              >
                Recolher tudo
              </Button>

              {/* Zoom */}
              <div className="flex items-center gap-[var(--sp-3xs)] border border-[var(--color-caramel-200)] rounded-[var(--radius-sm)] p-[var(--sp-3xs)] bg-[var(--color-neutral-0)] max-sm:p-[2px]">
                <Button
                  variant="secondary"
                  size="md"
                  leftIcon={Minus}
                  onClick={zoomOut}
                />
                <button
                  type="button"
                  className="font-[var(--font-label)] font-medium text-[var(--text-xs)] text-[var(--color-neutral-600)] min-w-[40px] text-center cursor-pointer border-0 bg-transparent px-[var(--sp-2xs)] py-[var(--sp-3xs)] rounded-[var(--radius-xs)] transition-colors duration-100 hover:bg-[var(--color-caramel-50)]"
                  onClick={zoomReset}
                >
                  {zoom}%
                </button>
                <Button
                  variant="secondary"
                  size="md"
                  leftIcon={Plus}
                  onClick={zoomIn}
                />
              </div>
            </div>
          </div>
        </CardBody>
      </Card>

      {/* Tree + Detail panel */}
      <div className="flex gap-[var(--sp-2xs)] min-h-0">
        <Card padding="none" className="flex-1 min-w-0 overflow-hidden">
          {viewMode === "list" ? (
            <div
              className="p-[var(--sp-lg)] overflow-auto min-h-[400px] max-sm:p-[var(--sp-sm)]"
              style={{
                transform: `scale(${zoom / 100})`,
                transformOrigin: "top left",
              }}
            >
              <div className="flex flex-col gap-[var(--sp-3xs)]">
                {teamTree.map((root) => renderTeamNode(root, 0))}
              </div>
            </div>
          ) : (
            <div
              ref={chartContainerRef}
              className={`p-[var(--sp-xl)] overflow-hidden min-h-[400px] select-none max-sm:p-[var(--sp-sm)] ${isDragging ? "cursor-grabbing" : "cursor-grab"}`}
              onMouseDown={handlePanStart}
              onMouseMove={handlePanMove}
              onMouseUp={handlePanEnd}
              onMouseLeave={handlePanEnd}
              onTouchStart={handleTouchStart}
              onTouchMove={handleTouchMove}
              onTouchEnd={handleTouchEnd}
              onWheel={handleWheel}
            >
              <div
                className="flex flex-col items-center"
                style={{
                  transform: `translate(${panOffset.x}px, ${panOffset.y}px) scale(${zoom / 100})`,
                  transformOrigin: "top center",
                }}
              >
                {teamTree.map((root) => renderChartTeamNode(root))}
              </div>
            </div>
          )}
        </Card>
      </div>

      {/* Detail drawer */}
      <DragToCloseDrawer open={!!selectedTeam} onClose={closePanel} size="sm">
        {selectedTeam && (
          <>
            <DrawerHeader
              title={selectedTeam.name}
              onClose={closePanel}
              afterTitle={
                <div className="flex items-center gap-[var(--sp-sm)] p-[var(--sp-md)] border-b border-[var(--color-caramel-200)] max-sm:p-[var(--sp-sm)]">
                  <div className="flex flex-col gap-[var(--sp-3xs)] min-w-0">
                    <Badge color={selectedTeam.color}>
                      {selectedTeam.name}
                    </Badge>
                    {selectedTeam.description && (
                      <span className="font-[var(--font-body)] text-[var(--text-sm)] text-[var(--color-neutral-500)]">
                        {selectedTeam.description}
                      </span>
                    )}
                  </div>
                </div>
              }
            />

            <DrawerBody>
              {/* Líder */}
              <div className="flex flex-col gap-[var(--sp-sm)] p-[var(--sp-md)] border-b border-[var(--color-caramel-200)] last:border-b-0 max-sm:p-[var(--sp-sm)]">
                <div className="flex items-center justify-between">
                  <span className="font-[var(--font-label)] font-medium text-[var(--text-xs)] text-[var(--color-neutral-600)] uppercase tracking-[0.03em]">
                    Líder
                  </span>
                  {!editingLeader && selectedLeader && (
                    <button
                      type="button"
                      className="flex items-center gap-[var(--sp-3xs)] px-[var(--sp-2xs)] py-[var(--sp-3xs)] border-0 bg-transparent font-[var(--font-label)] font-medium text-[var(--text-xs)] text-[var(--color-orange-600)] cursor-pointer rounded-[var(--radius-xs)] transition-colors duration-100 hover:bg-[var(--color-orange-50)]"
                      onClick={() => setEditingLeader(true)}
                    >
                      <PencilSimple size={14} /> Alterar
                    </button>
                  )}
                </div>

                {editingLeader ? (
                  <PersonPicker
                    people={people}
                    excludeIds={new Set()}
                    onSelect={handleChangeLeader}
                    onCancel={() => setEditingLeader(false)}
                    placeholder="Buscar novo líder..."
                  />
                ) : selectedLeader ? (
                  <div className="flex items-center gap-[var(--sp-sm)] px-[var(--sp-sm)] py-[var(--sp-2xs)]">
                    <Avatar
                      initials={selectedLeader.initials ?? undefined}
                      size="sm"
                    />
                    <div className="flex flex-col gap-[1px] flex-1 min-w-0">
                      <span className="font-[var(--font-label)] font-medium text-[var(--text-sm)] text-[var(--color-neutral-900)]">
                        {selectedLeader.fullName}
                      </span>
                      <span className="font-[var(--font-body)] text-[var(--text-xs)] text-[var(--color-neutral-500)]">
                        {selectedLeader.jobTitle}
                      </span>
                    </div>
                  </div>
                ) : (
                  <div className="flex flex-col items-center gap-[var(--sp-sm)] p-[var(--sp-sm)] text-center">
                    <span className="font-[var(--font-body)] text-[var(--text-sm)] text-[var(--color-neutral-500)]">
                      Sem líder atribuído
                    </span>
                    <Button
                      variant="secondary"
                      size="md"
                      leftIcon={UserCirclePlus}
                      onClick={() => setEditingLeader(true)}
                    >
                      Atribuir líder
                    </Button>
                  </div>
                )}

                {selectedLeader && !editingLeader && (
                  <button
                    type="button"
                    className="border-0 bg-transparent font-[var(--font-body)] text-[var(--text-xs)] text-[var(--color-neutral-500)] cursor-pointer text-left py-[var(--sp-3xs)] underline underline-offset-2 transition-colors duration-100 hover:text-[var(--color-red-600)]"
                    onClick={handleRemoveLeader}
                  >
                    Remover líder
                  </button>
                )}
              </div>

              {/* Membros */}
              <div className="flex flex-col gap-[var(--sp-sm)] p-[var(--sp-md)] border-b border-[var(--color-caramel-200)] last:border-b-0 max-sm:p-[var(--sp-sm)]">
                <div className="flex items-center justify-between">
                  <div className="flex items-center gap-[var(--sp-2xs)] font-[var(--font-label)] font-medium text-[var(--text-xs)] text-[var(--color-neutral-600)] uppercase tracking-[0.03em]">
                    Membros
                    {selectedMembers.length > 0 && (
                      <Badge color="neutral" size="sm">
                        {selectedMembers.length}
                      </Badge>
                    )}
                  </div>
                  {!addingMember && (
                    <button
                      type="button"
                      className="flex items-center gap-[var(--sp-3xs)] px-[var(--sp-2xs)] py-[var(--sp-3xs)] border-0 bg-transparent font-[var(--font-label)] font-medium text-[var(--text-xs)] text-[var(--color-orange-600)] cursor-pointer rounded-[var(--radius-xs)] transition-colors duration-100 hover:bg-[var(--color-orange-50)]"
                      onClick={() => setAddingMember(true)}
                    >
                      <Plus size={14} /> Adicionar
                    </button>
                  )}
                </div>

                {addingMember && (
                  <PersonPicker
                    people={people}
                    excludeIds={memberExcludeIds}
                    onSelect={handleAddMember}
                    onCancel={() => setAddingMember(false)}
                    placeholder="Buscar membro..."
                  />
                )}

                {selectedMembers.length > 0 ? (
                  <div className="flex flex-col">
                    {selectedMembers.map((member) => (
                      <div
                        key={member.id}
                        className="flex items-center gap-[var(--sp-3xs)] group"
                      >
                        <div className="flex-1 flex items-center gap-[var(--sp-sm)] px-[var(--sp-sm)] py-[var(--sp-2xs)]">
                          <Avatar
                            initials={member.initials ?? undefined}
                            size="sm"
                          />
                          <div className="flex flex-col gap-[1px] flex-1 min-w-0">
                            <span className="font-[var(--font-label)] font-medium text-[var(--text-sm)] text-[var(--color-neutral-900)]">
                              {member.fullName}
                            </span>
                            <span className="font-[var(--font-body)] text-[var(--text-xs)] text-[var(--color-neutral-500)]">
                              {member.jobTitle}
                            </span>
                          </div>
                        </div>
                        <button
                          type="button"
                          className="flex items-center justify-center w-6 h-6 border-0 bg-transparent text-[var(--color-neutral-400)] cursor-pointer rounded-[var(--radius-xs)] shrink-0 opacity-0 transition-[opacity,background-color,color] duration-100 group-hover:opacity-100 hover:bg-[var(--color-red-50)] hover:text-[var(--color-red-600)]"
                          onClick={() => handleRemoveMember(member.id)}
                          title="Remover membro"
                        >
                          <X size={14} />
                        </button>
                      </div>
                    ))}
                  </div>
                ) : (
                  !addingMember && (
                    <div className="flex flex-col items-center gap-[var(--sp-sm)] p-[var(--sp-sm)] text-center">
                      <span className="font-[var(--font-body)] text-[var(--text-sm)] text-[var(--color-neutral-500)]">
                        Nenhum membro
                      </span>
                      <Button
                        variant="secondary"
                        size="md"
                        leftIcon={UserCirclePlus}
                        onClick={() => setAddingMember(true)}
                      >
                        Adicionar membro
                      </Button>
                    </div>
                  )
                )}
              </div>

              {/* Subtimes */}
              {selectedTeamNode && selectedTeamNode.subTeams.length > 0 && (
                <div className="flex flex-col gap-[var(--sp-sm)] p-[var(--sp-md)] border-b border-[var(--color-caramel-200)] last:border-b-0 max-sm:p-[var(--sp-sm)]">
                  <div className="flex items-center gap-[var(--sp-2xs)] font-[var(--font-label)] font-medium text-[var(--text-xs)] text-[var(--color-neutral-600)] uppercase tracking-[0.03em]">
                    Subtimes
                    <Badge color="neutral" size="sm">
                      {selectedTeamNode.subTeams.length}
                    </Badge>
                  </div>
                  <div className="flex flex-col">
                    {selectedTeamNode.subTeams.map((sub) => (
                      <button
                        key={sub.team.id}
                        type="button"
                        className="flex items-center gap-[var(--sp-sm)] px-[var(--sp-sm)] py-[var(--sp-2xs)] border-0 bg-transparent cursor-pointer text-left w-full rounded-[var(--radius-sm)] transition-colors duration-100 hover:bg-[var(--color-caramel-50)]"
                        onClick={() => selectTeam(sub.team.id)}
                      >
                        <Badge color={sub.team.color} size="sm">
                          {sub.team.name}
                        </Badge>
                        {sub.leader && (
                          <span className="font-[var(--font-body)] text-[var(--text-xs)] text-[var(--color-neutral-500)] truncate">
                            {sub.leader.fullName}
                          </span>
                        )}
                        {sub.members.length > 0 && (
                          <span className="flex items-center gap-[var(--sp-3xs)] font-[var(--font-label)] font-medium text-[var(--text-xs)] text-[var(--color-neutral-500)] ml-auto shrink-0">
                            <UsersThree size={12} />
                            {sub.members.length}
                          </span>
                        )}
                      </button>
                    ))}
                  </div>
                </div>
              )}
            </DrawerBody>
          </>
        )}
      </DragToCloseDrawer>
    </>
  );
}
