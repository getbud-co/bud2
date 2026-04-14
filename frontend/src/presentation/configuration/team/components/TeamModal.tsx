"use client";

import { useState, useMemo, useEffect, useRef, useCallback } from "react";
import {
  Modal,
  ModalHeader,
  ModalBody,
  ModalFooter,
  Button,
  Input,
  Textarea,
  Badge,
  AvatarLabelGroup,
  Checkbox,
  FilterBar,
  FilterChip,
  FilterDropdown,
  Radio,
  TabBar,
  toast,
} from "@getbud-co/buds";
import {
  MagnifyingGlass,
  UsersThree,
  X,
  CaretDown,
} from "@phosphor-icons/react";
import type { Team, TeamMember, TeamColor } from "@/types";
import { ConfigErrorState } from "@/components/ConfigErrorState";
import {
  COLOR_OPTIONS,
  ROLE_OPTIONS,
  ROLE_BADGE_COLOR,
  ROLE_LABEL,
  SEARCH_FILTERS,
  MEMBERS_FILTERS,
  MEMBERSHIP_OPTIONS,
  filterDropdownBodyCls,
  filterActionItemCls,
} from "../consts";

export interface PersonView {
  id: string;
  fullName: string;
  jobTitle: string;
  initials: string;
  teamIds: string[];
}

export interface TeamModalProps {
  open: boolean;
  team: Team | null;
  initialTab?: "details" | "members";
  peoplePool: PersonView[];
  allTeams: { id: string; name: string }[];
  onClose: () => void;
  onSave: (data: {
    name: string;
    description: string;
    color: TeamColor;
    members: TeamMember[];
    parentTeamId: string | null;
  }) => void;
}

function memberFromPerson(
  person: PersonView,
  teamId: string,
  role: TeamMember["roleInTeam"] = "member",
): TeamMember {
  return {
    teamId,
    userId: person.id,
    roleInTeam: role,
    joinedAt: new Date().toISOString(),
    user: {
      id: person.id,
      fullName: person.fullName,
      initials: person.initials,
      jobTitle: person.jobTitle,
      avatarUrl: null,
    },
  };
}

/* ——— Component ——— */

export function TeamModal({
  open,
  team,
  initialTab = "details",
  peoplePool,
  allTeams,
  onClose,
  onSave,
}: TeamModalProps) {
  const isCreating = team === null;

  const [activeTab, setActiveTab] = useState<"details" | "members">(initialTab);

  const [formName, setFormName] = useState("");
  const [formDesc, setFormDesc] = useState("");
  const [formColor, setFormColor] = useState<TeamColor>("neutral");
  const [formParentTeamId, setFormParentTeamId] = useState<string | null>(null);

  const [pendingMembers, setPendingMembers] = useState<TeamMember[]>([]);
  const [originalMembers, setOriginalMembers] = useState<TeamMember[]>([]);

  const [search, setSearch] = useState("");
  const searchInputRef = useRef<HTMLInputElement>(null);

  const [searchActiveFilters, setSearchActiveFilters] = useState<string[]>([]);
  const [searchOpenFilter, setSearchOpenFilter] = useState<string | null>(null);
  const [filterMembership, setFilterMembership] = useState("all");
  const [filterRole, setFilterRole] = useState("all");
  const [filterCargos, setFilterCargos] = useState<string[]>([]);
  const [filterTeamIds, setFilterTeamIds] = useState<string[]>([]);

  const membershipChipRef = useRef<HTMLDivElement>(null);
  const roleChipRef = useRef<HTMLDivElement>(null);
  const cargoChipRef = useRef<HTMLDivElement>(null);
  const timeChipRef = useRef<HTMLDivElement>(null);

  const [membersActiveFilters, setMembersActiveFilters] = useState<string[]>(
    [],
  );
  const [membersOpenFilter, setMembersOpenFilter] = useState<string | null>(
    null,
  );
  const [filterMembersRole, setFilterMembersRole] = useState("all");
  const membersRoleChipRef = useRef<HTMLDivElement>(null);

  const [openRoleFor, setOpenRoleFor] = useState<string | null>(null);
  const roleButtonRefs = useRef<Record<string, HTMLButtonElement | null>>({});

  const [mobileMembersTab, setMobileMembersTab] = useState<"add" | "members">(
    "add",
  );

  const wasOpenRef = useRef(false);

  useEffect(() => {
    const justOpened = open && !wasOpenRef.current;
    wasOpenRef.current = open;
    if (!justOpened) return;

    setActiveTab(initialTab);
    setFormName(team?.name ?? "");
    setFormDesc(team?.description ?? "");
    setFormColor(team?.color ?? "neutral");
    setFormParentTeamId(team?.parentTeamId ?? null);
    const initial = team?.members ?? [];
    setPendingMembers(initial);
    setOriginalMembers(initial);
    setSearch("");
    setSearchActiveFilters([]);
    setSearchOpenFilter(null);
    setFilterMembership("all");
    setFilterRole("all");
    setFilterCargos([]);
    setFilterTeamIds([]);
    setMembersActiveFilters([]);
    setMembersOpenFilter(null);
    setFilterMembersRole("all");
    setOpenRoleFor(null);
    setMobileMembersTab("add");
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [open]);

  /* ——— Auto-focus search when switching to members tab ——— */
  useEffect(() => {
    if (open && activeTab === "members") {
      requestAnimationFrame(() => searchInputRef.current?.focus());
    }
  }, [open, activeTab]);

  /* ——— Intercept Escape when role dropdown is open ——— */
  useEffect(() => {
    if (!openRoleFor) return;
    function onKeyDown(e: KeyboardEvent) {
      if (e.key === "Escape") {
        e.stopPropagation();
        setOpenRoleFor(null);
      }
    }
    document.addEventListener("keydown", onKeyDown, true);
    return () => document.removeEventListener("keydown", onKeyDown, true);
  }, [openRoleFor]);

  /* ——— Derived ——— */

  const pendingIds = useMemo(
    () => new Set(pendingMembers.map((m) => m.userId)),
    [pendingMembers],
  );

  const pendingRoleMap = useMemo(
    () => new Map(pendingMembers.map((m) => [m.userId, m.roleInTeam])),
    [pendingMembers],
  );

  const addedIds = useMemo(() => {
    const origIds = new Set(originalMembers.map((m) => m.userId));
    return new Set(
      pendingMembers.filter((m) => !origIds.has(m.userId)).map((m) => m.userId),
    );
  }, [pendingMembers, originalMembers]);

  const hasDetailsChanges = useMemo(() => {
    if (isCreating)
      return (
        formName.trim() !== "" ||
        formDesc.trim() !== "" ||
        formColor !== "neutral"
      );
    return (
      formName !== (team?.name ?? "") ||
      formDesc !== (team?.description ?? "") ||
      formColor !== (team?.color ?? "neutral") ||
      formParentTeamId !== (team?.parentTeamId ?? null)
    );
  }, [isCreating, formName, formDesc, formColor, formParentTeamId, team]);

  const hasMembersChanges = useMemo(() => {
    if (pendingMembers.length !== originalMembers.length) return true;
    const origMap = new Map(
      originalMembers.map((m) => [m.userId, m.roleInTeam]),
    );
    return pendingMembers.some(
      (m) => !origMap.has(m.userId) || origMap.get(m.userId) !== m.roleInTeam,
    );
  }, [pendingMembers, originalMembers]);

  const hasChanges = hasDetailsChanges || hasMembersChanges;

  const cargoOptions = useMemo(() => {
    const unique = [...new Set(peoplePool.map((p) => p.jobTitle))].sort(
      (a, b) => a.localeCompare(b),
    );
    return unique;
  }, [peoplePool]);

  const teamOptions = useMemo(
    () => allTeams.filter((t) => t.id !== team?.id),
    [allTeams, team],
  );

  const searchResults = useMemo(() => {
    const q = search.toLowerCase().trim();

    let list = [...peoplePool].sort((a, b) =>
      a.fullName.localeCompare(b.fullName),
    );

    if (q) {
      list = list.filter(
        (p) =>
          p.fullName.toLowerCase().includes(q) ||
          p.jobTitle.toLowerCase().includes(q),
      );
    }

    if (filterMembership === "in")
      list = list.filter((p) => pendingIds.has(p.id));
    if (filterMembership === "out")
      list = list.filter((p) => !pendingIds.has(p.id));

    if (filterRole !== "all") {
      list = list.filter(
        (p) => pendingIds.has(p.id) && pendingRoleMap.get(p.id) === filterRole,
      );
    }

    if (filterCargos.length > 0) {
      list = list.filter((p) => filterCargos.includes(p.jobTitle));
    }

    if (filterTeamIds.length > 0) {
      list = list.filter((p) =>
        filterTeamIds.some((tid) => p.teamIds.includes(tid)),
      );
    }

    return list;
  }, [
    search,
    peoplePool,
    filterMembership,
    filterRole,
    filterCargos,
    filterTeamIds,
    pendingIds,
    pendingRoleMap,
  ]);

  const filteredMembers = useMemo(() => {
    if (filterMembersRole === "all") return pendingMembers;
    return pendingMembers.filter((m) => m.roleInTeam === filterMembersRole);
  }, [pendingMembers, filterMembersRole]);

  /* ——— Actions ——— */

  const handleTogglePerson = useCallback(
    (person: PersonView) => {
      if (pendingIds.has(person.id)) {
        setPendingMembers((prev) => {
          const target = prev.find((m) => m.userId === person.id);
          if (target?.roleInTeam === "leader") {
            const leaderCount = prev.filter(
              (m) => m.roleInTeam === "leader",
            ).length;
            if (leaderCount <= 1) {
              toast.error(
                "O time precisa ter pelo menos um líder. Atribua outro líder antes de remover.",
              );
              return prev;
            }
          }
          return prev.filter((m) => m.userId !== person.id);
        });
      } else {
        const teamId = team?.id ?? "__new__";
        setPendingMembers((prev) => [
          ...prev,
          memberFromPerson(person, teamId),
        ]);
      }
    },
    [pendingIds, team],
  );

  const handleChangeRole = useCallback(
    (userId: string, role: TeamMember["roleInTeam"]) => {
      setPendingMembers((prev) => {
        const current = prev.find((m) => m.userId === userId);
        if (current?.roleInTeam === "leader" && role !== "leader") {
          const leaderCount = prev.filter(
            (m) => m.roleInTeam === "leader",
          ).length;
          if (leaderCount <= 1) {
            toast.error("O time precisa ter pelo menos um líder");
            return prev;
          }
        }
        return prev.map((m) => {
          if (m.userId === userId) return { ...m, roleInTeam: role };
          if (role === "leader" && m.roleInTeam === "leader")
            return { ...m, roleInTeam: "member" };
          return m;
        });
      });
      setOpenRoleFor(null);
    },
    [],
  );

  const handleRemoveMember = useCallback((userId: string) => {
    setPendingMembers((prev) => {
      const target = prev.find((m) => m.userId === userId);
      if (target?.roleInTeam === "leader") {
        const leaderCount = prev.filter(
          (m) => m.roleInTeam === "leader",
        ).length;
        if (leaderCount <= 1) {
          toast.error(
            "O time precisa ter pelo menos um líder. Atribua outro líder antes de remover.",
          );
          return prev;
        }
      }
      return prev.filter((m) => m.userId !== userId);
    });
  }, []);

  /* ——— Filter helpers ——— */

  function handleToggleCargoFilter(cargo: string) {
    setFilterCargos((prev) =>
      prev.includes(cargo) ? prev.filter((c) => c !== cargo) : [...prev, cargo],
    );
  }

  function handleToggleTeamFilter(teamId: string) {
    setFilterTeamIds((prev) =>
      prev.includes(teamId)
        ? prev.filter((id) => id !== teamId)
        : [...prev, teamId],
    );
  }

  function getSearchFilterLabel(id: string): string {
    if (id === "membership") {
      return (
        MEMBERSHIP_OPTIONS.find((o) => o.id === filterMembership)?.label ??
        "Situação"
      );
    }
    if (id === "role") {
      return filterRole === "all"
        ? "Papel"
        : (ROLE_LABEL[filterRole as TeamMember["roleInTeam"]] ?? "Papel");
    }
    if (id === "cargo") {
      if (filterCargos.length === 0) return "Cargo";
      if (filterCargos.length === 1) return filterCargos[0] ?? "Cargo";
      return `${filterCargos.length} cargos`;
    }
    if (id === "time") {
      if (filterTeamIds.length === 0) return "Time";
      if (filterTeamIds.length === 1) {
        return (
          teamOptions.find((t) => t.id === filterTeamIds[0])?.name ?? "Time"
        );
      }
      return `${filterTeamIds.length} times`;
    }
    return id;
  }

  function getMembersFilterLabel(id: string): string {
    if (id === "role") {
      return filterMembersRole === "all"
        ? "Papel"
        : (ROLE_LABEL[filterMembersRole as TeamMember["roleInTeam"]] ??
            "Papel");
    }
    return id;
  }

  function removeSearchFilter(filterId: string) {
    setSearchActiveFilters((prev) => prev.filter((f) => f !== filterId));
    if (filterId === "membership") setFilterMembership("all");
    if (filterId === "role") setFilterRole("all");
    if (filterId === "cargo") setFilterCargos([]);
    if (filterId === "time") setFilterTeamIds([]);
  }

  function clearAllSearchFilters() {
    setSearchActiveFilters([]);
    setFilterMembership("all");
    setFilterRole("all");
    setFilterCargos([]);
    setFilterTeamIds([]);
  }

  /* ——— Save / Close ——— */

  function handleSave() {
    if (!formName.trim()) {
      toast.error("Nome do time é obrigatório");
      if (activeTab !== "details") setActiveTab("details");
      return;
    }

    if (
      pendingMembers.length > 0 &&
      !pendingMembers.some((m) => m.roleInTeam === "leader")
    ) {
      toast.error("O time precisa ter pelo menos um líder");
      if (activeTab !== "members") setActiveTab("members");
      return;
    }

    onSave({
      name: formName.trim(),
      description: formDesc.trim(),
      color: formColor,
      members: pendingMembers,
      parentTeamId: formParentTeamId,
    });
  }

  function handleClose() {
    if (
      hasChanges &&
      !window.confirm("Você tem alterações não salvas. Deseja descartar?")
    )
      return;
    onClose();
  }

  /* ——— Tab definitions ——— */

  const mainTabs = [
    { value: "details", label: "Detalhes" },
    {
      value: "members",
      label: "Membros",
      badge:
        pendingMembers.length > 0 ? (
          <Badge color={hasMembersChanges ? "orange" : "neutral"} size="sm">
            {pendingMembers.length}
          </Badge>
        ) : undefined,
    },
  ];

  /* ——— Details panel ——— */

  const detailsPanel = (
    <div className="flex flex-col gap-[var(--sp-md)]">
      <Input
        label="Nome do time"
        placeholder="Ex: Engenharia, Produto..."
        value={formName}
        onChange={(e: React.ChangeEvent<HTMLInputElement>) =>
          setFormName(e.target.value)
        }
      />
      <Textarea
        label="Descrição"
        placeholder="Descreva o propósito deste time..."
        value={formDesc}
        onChange={(e: React.ChangeEvent<HTMLTextAreaElement>) =>
          setFormDesc(e.target.value)
        }
        rows={3}
      />
      <div className="flex flex-col gap-[var(--sp-2xs)]">
        <label
          htmlFor="parent-team-select"
          className="font-[var(--font-label)] font-medium text-[var(--text-sm)] text-[var(--color-neutral-700)]"
        >
          Esse time está relacionado a
        </label>
        <select
          id="parent-team-select"
          value={formParentTeamId ?? ""}
          onChange={(e) => setFormParentTeamId(e.target.value || null)}
          className="h-10 w-full rounded-[var(--radius-sm)] border border-[var(--color-caramel-200)] bg-white px-[var(--sp-sm)] font-[var(--font-body)] text-[var(--text-sm)] text-[var(--color-neutral-900)] outline-none focus:border-[var(--color-orange-400)] focus:ring-2 focus:ring-[var(--color-orange-100)]"
        >
          <option value="">Nenhum</option>
          {teamOptions.map((t) => (
            <option key={t.id} value={t.id}>
              {t.name}
            </option>
          ))}
        </select>
      </div>

      <div className="flex flex-col gap-[var(--sp-2xs)]">
        <span className="font-[var(--font-label)] font-medium text-[var(--text-sm)] text-[var(--color-neutral-700)]">
          Cor do badge
        </span>
        <div className="flex flex-wrap gap-[var(--sp-2xs)]">
          {COLOR_OPTIONS.map((opt) => (
            <button
              key={opt.value}
              type="button"
              className={`flex p-[var(--sp-3xs)] border-2 rounded-[var(--radius-sm)] bg-transparent cursor-pointer transition-[border-color,background-color] duration-[120ms] ease hover:bg-[var(--color-caramel-50)] hover:border-[var(--color-caramel-200)] ${formColor === opt.value ? "border-[var(--color-orange-400)] bg-[var(--color-orange-50)]" : "border-transparent"}`}
              onClick={() => setFormColor(opt.value)}
            >
              <Badge color={opt.value} size="sm">
                {opt.label}
              </Badge>
            </button>
          ))}
        </div>
      </div>
    </div>
  );

  /* ——— Search panel ——— */

  const searchPanel = (
    <div className="flex flex-col h-full gap-[var(--sp-xs)]">
      <Input
        ref={searchInputRef}
        placeholder="Buscar por nome ou cargo..."
        value={search}
        onChange={(e: React.ChangeEvent<HTMLInputElement>) =>
          setSearch(e.target.value)
        }
        leftIcon={MagnifyingGlass}
      />

      <div className="shrink-0">
        <FilterBar
          filters={SEARCH_FILTERS.filter(
            (f) => !searchActiveFilters.includes(f.id),
          )}
          onAddFilter={(id: string) => {
            setSearchActiveFilters((prev) => [...prev, id]);
            requestAnimationFrame(() => setSearchOpenFilter(id));
          }}
          onClearAll={
            searchActiveFilters.length > 0 ? clearAllSearchFilters : undefined
          }
        >
          {searchActiveFilters.map((filterId) => (
            <div
              key={filterId}
              ref={
                filterId === "membership"
                  ? membershipChipRef
                  : filterId === "role"
                    ? roleChipRef
                    : filterId === "cargo"
                      ? cargoChipRef
                      : filterId === "time"
                        ? timeChipRef
                        : undefined
              }
              style={{ display: "inline-flex" }}
            >
              <FilterChip
                label={getSearchFilterLabel(filterId)}
                active={searchOpenFilter === filterId}
                onClick={() =>
                  setSearchOpenFilter(
                    searchOpenFilter === filterId ? null : filterId,
                  )
                }
                onRemove={() => removeSearchFilter(filterId)}
              />
            </div>
          ))}
        </FilterBar>
      </div>

      {/* Situação */}
      <FilterDropdown
        open={searchOpenFilter === "membership"}
        onClose={() => setSearchOpenFilter(null)}
        anchorRef={membershipChipRef}
      >
        <div className={filterDropdownBodyCls}>
          {MEMBERSHIP_OPTIONS.map((opt) => (
            <button
              key={opt.id}
              type="button"
              className={filterActionItemCls(filterMembership === opt.id)}
              onClick={() => {
                setFilterMembership(opt.id);
                setSearchOpenFilter(null);
              }}
            >
              <Radio checked={filterMembership === opt.id} readOnly />
              <span>{opt.label}</span>
            </button>
          ))}
        </div>
      </FilterDropdown>

      {/* Papel */}
      <FilterDropdown
        open={searchOpenFilter === "role"}
        onClose={() => setSearchOpenFilter(null)}
        anchorRef={roleChipRef}
      >
        <div className={filterDropdownBodyCls}>
          <button
            type="button"
            className={filterActionItemCls(filterRole === "all")}
            onClick={() => {
              setFilterRole("all");
              setSearchOpenFilter(null);
            }}
          >
            <Radio checked={filterRole === "all"} readOnly />
            <span>Todos os papéis</span>
          </button>
          {ROLE_OPTIONS.map((opt) => (
            <button
              key={opt.id}
              type="button"
              className={filterActionItemCls(filterRole === opt.id)}
              onClick={() => {
                setFilterRole(opt.id);
                setSearchOpenFilter(null);
              }}
            >
              <Radio checked={filterRole === opt.id} readOnly />
              <span>{opt.label}</span>
            </button>
          ))}
        </div>
      </FilterDropdown>

      {/* Cargo */}
      <FilterDropdown
        open={searchOpenFilter === "cargo"}
        onClose={() => setSearchOpenFilter(null)}
        anchorRef={cargoChipRef}
      >
        <div className={filterDropdownBodyCls}>
          {cargoOptions.map((cargo) => (
            <button
              key={cargo}
              type="button"
              className={filterActionItemCls(filterCargos.includes(cargo))}
              onClick={() => handleToggleCargoFilter(cargo)}
            >
              <Checkbox
                size="sm"
                checked={filterCargos.includes(cargo)}
                readOnly
                tabIndex={-1}
                aria-hidden
              />
              <span>{cargo}</span>
            </button>
          ))}
        </div>
      </FilterDropdown>

      {/* Time */}
      <FilterDropdown
        open={searchOpenFilter === "time"}
        onClose={() => setSearchOpenFilter(null)}
        anchorRef={timeChipRef}
      >
        <div className={filterDropdownBodyCls}>
          {teamOptions.length === 0 ? (
            <div className="px-[var(--sp-sm)] py-[var(--sp-2xs)] font-[var(--font-body)] text-[var(--text-sm)] text-[var(--color-neutral-400)]">
              Nenhum outro time disponível
            </div>
          ) : (
            teamOptions.map((t) => (
              <button
                key={t.id}
                type="button"
                className={filterActionItemCls(filterTeamIds.includes(t.id))}
                onClick={() => handleToggleTeamFilter(t.id)}
              >
                <Checkbox
                  size="sm"
                  checked={filterTeamIds.includes(t.id)}
                  readOnly
                  tabIndex={-1}
                  aria-hidden
                />
                <span>{t.name}</span>
              </button>
            ))
          )}
        </div>
      </FilterDropdown>

      {/* Lista de pessoas */}
      <div
        className="flex-1 overflow-y-auto border border-[var(--color-caramel-200)] rounded-[var(--radius-sm)] min-h-0"
        role="listbox"
        aria-label="Pessoas disponíveis"
      >
        {searchResults.length === 0 ? (
          <div className="flex flex-col items-center gap-[var(--sp-2xs)] py-[var(--sp-2xl)] px-[var(--sp-lg)] text-[var(--color-neutral-400)] text-center">
            <MagnifyingGlass size={20} />
            <span className="font-[var(--font-body)] text-[var(--text-sm)]">
              {search
                ? `Nenhuma pessoa encontrada para "${search}"`
                : "Nenhuma pessoa corresponde aos filtros"}
            </span>
          </div>
        ) : (
          searchResults.map((person) => {
            const isSelected = pendingIds.has(person.id);
            return (
              <button
                key={person.id}
                type="button"
                role="option"
                aria-selected={isSelected}
                className={`flex items-center gap-[var(--sp-xs)] border-b border-[var(--color-caramel-100)] border-l-0 border-r-0 border-t-0 bg-transparent cursor-pointer text-left w-full transition-[background-color] duration-100 outline-none last:border-b-0 focus-visible:shadow-[inset_0_0_0_2px_var(--color-orange-300)] ${isSelected ? "bg-[var(--color-orange-50)] border-l-2 border-l-[var(--color-orange-400)] pl-[calc(var(--sp-sm)-2px)] pr-[var(--sp-sm)] py-[var(--sp-2xs)] hover:bg-[var(--color-orange-100)]" : "px-[var(--sp-sm)] py-[var(--sp-2xs)] hover:bg-[var(--color-caramel-50)]"}`}
                onClick={() => handleTogglePerson(person)}
              >
                <Checkbox
                  size="sm"
                  checked={isSelected}
                  readOnly
                  tabIndex={-1}
                  aria-hidden
                />
                <AvatarLabelGroup
                  size="sm"
                  initials={person.initials}
                  name={person.fullName}
                  supportingText={person.jobTitle}
                />
              </button>
            );
          })
        )}
      </div>

      <div className="font-[var(--font-body)] text-[var(--text-xs)] text-[var(--color-neutral-400)] shrink-0">
        {searchResults.length} de {peoplePool.length} pessoas
        {pendingMembers.length > 0 && ` · ${pendingMembers.length} no time`}
      </div>
    </div>
  );

  /* ——— Members panel (sidePanel) ——— */

  const membersPanel = (
    <div className="flex flex-col h-full bg-[var(--color-neutral-0)] border border-[var(--color-caramel-200)] rounded-[var(--radius-sm)] overflow-hidden max-md:border-0 max-md:rounded-none">
      <div className="flex items-center justify-between px-[var(--sp-md)] py-[var(--sp-sm)] border-b border-[var(--color-caramel-200)] bg-[var(--color-neutral-50)] shrink-0">
        <span className="font-[var(--font-label)] font-medium text-[var(--text-sm)] text-[var(--color-neutral-700)]">
          Membros atuais
        </span>
        <Badge color="neutral" size="sm">
          {pendingMembers.length}
        </Badge>
      </div>

      <div className="px-[var(--sp-sm)] py-[var(--sp-2xs)] border-b border-[var(--color-caramel-100)] shrink-0">
        <FilterBar
          filters={MEMBERS_FILTERS.filter(
            (f) => !membersActiveFilters.includes(f.id),
          )}
          onAddFilter={(id: string) => {
            setMembersActiveFilters((prev) => [...prev, id]);
            requestAnimationFrame(() => setMembersOpenFilter(id));
          }}
          onClearAll={
            membersActiveFilters.length > 0
              ? () => {
                  setMembersActiveFilters([]);
                  setFilterMembersRole("all");
                }
              : undefined
          }
        >
          {membersActiveFilters.map((filterId) => (
            <div
              key={filterId}
              ref={filterId === "role" ? membersRoleChipRef : undefined}
              style={{ display: "inline-flex" }}
            >
              <FilterChip
                label={getMembersFilterLabel(filterId)}
                active={membersOpenFilter === filterId}
                onClick={() =>
                  setMembersOpenFilter(
                    membersOpenFilter === filterId ? null : filterId,
                  )
                }
                onRemove={() => {
                  setMembersActiveFilters((prev) =>
                    prev.filter((f) => f !== filterId),
                  );
                  if (filterId === "role") setFilterMembersRole("all");
                }}
              />
            </div>
          ))}
        </FilterBar>
      </div>

      <FilterDropdown
        open={membersOpenFilter === "role"}
        onClose={() => setMembersOpenFilter(null)}
        anchorRef={membersRoleChipRef}
      >
        <div className={filterDropdownBodyCls}>
          <button
            type="button"
            className={filterActionItemCls(filterMembersRole === "all")}
            onClick={() => {
              setFilterMembersRole("all");
              setMembersOpenFilter(null);
            }}
          >
            <Radio checked={filterMembersRole === "all"} readOnly />
            <span>Todos os papéis</span>
          </button>
          {ROLE_OPTIONS.map((opt) => (
            <button
              key={opt.id}
              type="button"
              className={filterActionItemCls(filterMembersRole === opt.id)}
              onClick={() => {
                setFilterMembersRole(opt.id);
                setMembersOpenFilter(null);
              }}
            >
              <Radio checked={filterMembersRole === opt.id} readOnly />
              <span>{opt.label}</span>
            </button>
          ))}
        </div>
      </FilterDropdown>

      <div className="flex-1 overflow-y-auto min-h-0">
        {pendingMembers.length === 0 ? (
          <div className="flex flex-col items-center gap-[var(--sp-2xs)] py-[var(--sp-2xl)] px-[var(--sp-lg)] text-center text-[var(--color-neutral-400)]">
            <UsersThree size={28} />
            <span className="font-[var(--font-label)] font-medium text-[var(--text-sm)] text-[var(--color-neutral-600)]">
              Nenhum membro ainda
            </span>
            <span className="font-[var(--font-body)] text-[var(--text-xs)] text-[var(--color-neutral-400)]">
              Selecione pessoas na busca ao lado
            </span>
          </div>
        ) : filteredMembers.length === 0 ? (
          <div className="flex flex-col items-center gap-[var(--sp-2xs)] py-[var(--sp-2xl)] px-[var(--sp-lg)] text-center text-[var(--color-neutral-400)]">
            <UsersThree size={28} />
            <span className="font-[var(--font-label)] font-medium text-[var(--text-sm)] text-[var(--color-neutral-600)]">
              Nenhum membro com esse papel
            </span>
          </div>
        ) : (
          filteredMembers.map((member) => {
            const isNew = addedIds.has(member.userId);
            return (
              <div
                key={member.userId}
                className="flex items-center justify-between px-[var(--sp-sm)] py-[var(--sp-2xs)] border-b border-[var(--color-caramel-100)] gap-[var(--sp-2xs)] transition-[background-color] duration-100 last:border-b-0 hover:bg-[var(--color-caramel-50)]"
              >
                <AvatarLabelGroup
                  size="sm"
                  initials={member.user?.initials ?? ""}
                  name={member.user?.fullName ?? ""}
                  supportingText={member.user?.jobTitle ?? ""}
                />
                <div className="flex items-center gap-[var(--sp-2xs)] shrink-0">
                  {isNew && (
                    <Badge color="success" size="sm">
                      Novo
                    </Badge>
                  )}

                  <button
                    ref={(el: HTMLButtonElement | null) => {
                      roleButtonRefs.current[member.userId] = el;
                    }}
                    type="button"
                    className={`inline-flex items-center border-0 bg-transparent cursor-pointer p-0 rounded-[var(--radius-xs)] outline-none transition-opacity duration-100 hover:opacity-80 focus-visible:shadow-[0_0_0_2px_var(--color-orange-400)] ${openRoleFor === member.userId ? "opacity-70" : ""}`}
                    onClick={() =>
                      setOpenRoleFor(
                        openRoleFor === member.userId ? null : member.userId,
                      )
                    }
                    title="Alterar papel no time"
                    aria-haspopup="listbox"
                    aria-expanded={openRoleFor === member.userId}
                  >
                    <Badge
                      color={ROLE_BADGE_COLOR[member.roleInTeam]}
                      size="sm"
                      rightIcon={CaretDown}
                    >
                      {ROLE_LABEL[member.roleInTeam]}
                    </Badge>
                  </button>

                  <FilterDropdown
                    open={openRoleFor === member.userId}
                    onClose={() => setOpenRoleFor(null)}
                    anchorRef={{
                      current: roleButtonRefs.current[member.userId] ?? null,
                    }}
                    noOverlay
                  >
                    <div className={filterDropdownBodyCls}>
                      {ROLE_OPTIONS.map((opt) => (
                        <button
                          key={opt.id}
                          type="button"
                          className={filterActionItemCls(
                            member.roleInTeam === opt.id,
                          )}
                          onClick={() =>
                            handleChangeRole(member.userId, opt.id)
                          }
                        >
                          <Radio
                            checked={member.roleInTeam === opt.id}
                            readOnly
                          />
                          <span>{opt.label}</span>
                        </button>
                      ))}
                    </div>
                  </FilterDropdown>

                  <button
                    type="button"
                    className="flex items-center justify-center w-6 h-6 border-0 bg-transparent text-[var(--color-neutral-400)] cursor-pointer rounded-[var(--radius-xs)] shrink-0 transition-[background-color,color] duration-100 outline-none hover:bg-[var(--color-red-50)] hover:text-[var(--color-red-600)] focus-visible:shadow-[0_0_0_2px_var(--color-red-400)]"
                    onClick={() => handleRemoveMember(member.userId)}
                    aria-label={`Remover ${member.user?.fullName ?? "membro"} do time`}
                  >
                    <X size={14} />
                  </button>
                </div>
              </div>
            );
          })
        )}
      </div>
    </div>
  );

  /* ——— Mobile sub-tabs ——— */

  const mobileMembersTabs = [
    { value: "add", label: "Adicionar" },
    {
      value: "members",
      label: "Membros",
      badge:
        pendingMembers.length > 0 ? (
          <Badge color={hasMembersChanges ? "orange" : "neutral"} size="sm">
            {pendingMembers.length}
          </Badge>
        ) : undefined,
    },
  ];

  const title = isCreating ? "Novo time" : team.name;
  const showSidePanel = activeTab === "members";

  return (
    <Modal
      open={open}
      onClose={handleClose}
      size="lg"
      sidePanel={showSidePanel ? membersPanel : null}
    >
      <ModalHeader title={title} onClose={handleClose}>
        {hasChanges && (
          <Badge color="orange" size="sm">
            Alterado
          </Badge>
        )}
      </ModalHeader>

      <div className="px-[var(--sp-lg)] border-b border-[var(--color-caramel-200)] shrink-0 max-md:px-[var(--sp-sm)]">
        <TabBar
          tabs={mainTabs}
          activeTab={activeTab}
          onTabChange={(v: string) => setActiveTab(v as "details" | "members")}
          id="team-modal"
        />
      </div>

      {activeTab === "members" && (
        <div className="hidden px-[var(--sp-lg)] border-b border-[var(--color-caramel-200)] shrink-0 max-md:block max-md:px-[var(--sp-sm)]">
          <TabBar
            tabs={mobileMembersTabs}
            activeTab={mobileMembersTab}
            onTabChange={(v: string) =>
              setMobileMembersTab(v as "add" | "members")
            }
            id="team-modal-members"
          />
        </div>
      )}

      <ModalBody>
        {!isCreating && !team ? (
          <ConfigErrorState message="Não foi possível carregar os dados do time. Verifique sua conexão e tente novamente." />
        ) : (
          <>
            {activeTab === "details" && detailsPanel}
            {activeTab === "members" && (
              <>
                <div className="flex flex-col h-full max-md:hidden">
                  {searchPanel}
                </div>
                <div className="hidden flex-col h-full max-md:flex">
                  {mobileMembersTab === "add" ? searchPanel : membersPanel}
                </div>
              </>
            )}
          </>
        )}
      </ModalBody>

      <ModalFooter align="between">
        <span className="font-[var(--font-body)] text-[var(--text-sm)] text-[var(--color-neutral-500)]">
          {pendingMembers.length} membro{pendingMembers.length !== 1 ? "s" : ""}
          {hasChanges ? " · alterações pendentes" : ""}
        </span>
        <div className="flex items-center gap-[var(--sp-2xs)]">
          <Button variant="secondary" size="md" onClick={handleClose}>
            Cancelar
          </Button>
          <Button variant="primary" size="md" onClick={handleSave}>
            {isCreating ? "Criar time" : "Salvar"}
          </Button>
        </div>
      </ModalFooter>
    </Modal>
  );
}
