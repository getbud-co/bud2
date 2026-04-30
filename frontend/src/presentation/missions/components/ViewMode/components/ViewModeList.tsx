import { Fragment } from "react";
import type { Mission } from "@/types";
import { MissionItem } from "../../MissionItem";

// ── Types ─────────────────────────────────────────────────────────────────────

interface MissionGroup {
  teamName: string;
  teamColor: string;
  missions: Mission[];
}

interface ViewModeListProps {
  displayedMissions: Mission[];
  groupedMissions: MissionGroup[] | null;
  expandedMissions: Set<string>;
  flatMissions: { id: string; title: string }[];
  openRowMenu: string | null;
  setOpenRowMenu: (id: string | null) => void;
  openContributeFor: string | null;
  setOpenContributeFor: (id: string | null) => void;
  contributePickerSearch: string;
  setContributePickerSearch: (s: string) => void;
  rowMenuBtnRefs: React.MutableRefObject<Record<string, HTMLButtonElement | null>>;
  onToggle: (id: string) => void;
  onExpand: (mission: Mission) => void;
  onEdit: (mission: Mission) => void;
  onDelete: (mission: Mission) => void;
  onToggleTask: (taskId: string) => void;
  onToggleSubtask: (taskId: string, subtaskId: string) => void;
}

// ── Internal components ───────────────────────────────────────────────────────

function TeamSectionHeader({
  teamName,
  teamColor,
  missions: groupMissions,
}: {
  teamName: string;
  teamColor: string;
  missions: Mission[];
}) {
  const avgProgress =
    groupMissions.length > 0
      ? Math.round(
          groupMissions.reduce((a, m) => a + m.progress, 0) /
            groupMissions.length,
        )
      : 0;
  return (
    <div className="flex items-center gap-[var(--sp-xs)] pt-[var(--sp-sm)] pb-[var(--sp-2xs)]">
      <span
        className="w-[10px] h-[10px] rounded-full shrink-0"
        style={{ backgroundColor: `var(--color-${teamColor}-500)` }}
      />
      <span className="font-[var(--font-heading)] text-[var(--text-sm)] font-semibold text-[var(--color-neutral-900)] whitespace-nowrap">
        {teamName}
      </span>
      <span className="font-[var(--font-label)] text-[var(--text-xs)] text-[var(--color-neutral-500)] whitespace-nowrap">
        {groupMissions.length}{" "}
        {groupMissions.length === 1 ? "missão" : "missões"} · {avgProgress}%
      </span>
      <div className="flex-1 h-px bg-[var(--color-caramel-200)]" />
    </div>
  );
}

// ── Component ─────────────────────────────────────────────────────────────────

export function ViewModeList({
  displayedMissions,
  groupedMissions,
  expandedMissions,
  flatMissions,
  openRowMenu,
  setOpenRowMenu,
  openContributeFor,
  setOpenContributeFor,
  contributePickerSearch,
  setContributePickerSearch,
  rowMenuBtnRefs,
  onToggle,
  onExpand,
  onEdit,
  onDelete,
  onToggleTask,
  onToggleSubtask,
}: ViewModeListProps) {
  function renderItem(mission: Mission) {
    return (
      <MissionItem
        key={mission.id}
        mission={mission}
        isOpen={expandedMissions.has(mission.id)}
        onToggle={onToggle}
        onExpand={onExpand}
        onEdit={onEdit}
        onDelete={onDelete}
        onToggleTask={onToggleTask}
        expandedMissions={expandedMissions}
        openRowMenu={openRowMenu}
        setOpenRowMenu={setOpenRowMenu}
        openContributeFor={openContributeFor}
        setOpenContributeFor={setOpenContributeFor}
        contributePickerSearch={contributePickerSearch}
        setContributePickerSearch={setContributePickerSearch}
        rowMenuBtnRefs={rowMenuBtnRefs}
        allMissions={flatMissions}
        onToggleSubtask={onToggleSubtask}
      />
    );
  }

  return (
    <div className="flex flex-col gap-[var(--sp-xs)]">
      {groupedMissions
        ? groupedMissions.map((group) => (
            <Fragment key={group.teamName}>
              <TeamSectionHeader
                teamName={group.teamName}
                teamColor={group.teamColor}
                missions={group.missions}
              />
              {group.missions.map((mission) => renderItem(mission))}
            </Fragment>
          ))
        : displayedMissions.map((mission) => renderItem(mission))}
    </div>
  );
}
