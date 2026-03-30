/**
 * Cascade deletion utilities for maintaining referential integrity across stores.
 * These functions help check for references and clean up when entities are deleted.
 */

import type { Mission } from "@/types";

/**
 * Statistics about user references across the system.
 */
export interface UserReferenceStats {
  ownedMissions: number;
  ownedIndicators: number;
  ownedTasks: number;
  isTeamLeader: boolean;
  isManager: boolean;
}

/**
 * Statistics about team references across the system.
 */
export interface TeamReferenceStats {
  linkedMissions: number;
  hasChildTeams: boolean;
  memberCount: number;
}

/**
 * Result of checking if an entity can be deleted.
 */
export interface DeletionCheck {
  canDelete: boolean;
  canSoftDelete: boolean;
  warnings: string[];
  blockers: string[];
}

/**
 * Get statistics about user references across missions.
 */
export function getUserMissionStats(
  missions: Mission[],
  userId: string,
): { ownedMissions: number; ownedIndicators: number; ownedTasks: number } {
  let ownedMissions = 0;
  let ownedIndicators = 0;
  let ownedTasks = 0;

  function checkMission(mission: Mission) {
    if (mission.ownerId === userId) ownedMissions++;

    mission.indicators?.forEach((indicator) => {
      if (indicator.ownerId === userId) ownedIndicators++;

      indicator.tasks?.forEach((task) => {
        if (task.ownerId === userId) ownedTasks++;
      });
    });

    mission.tasks?.forEach((task) => {
      if (task.ownerId === userId) ownedTasks++;
    });

    mission.children?.forEach(checkMission);
  }

  missions.forEach(checkMission);

  return { ownedMissions, ownedIndicators, ownedTasks };
}

/**
 * Get statistics about team references across missions.
 */
export function getTeamMissionStats(
  missions: Mission[],
  teamId: string,
): { linkedMissions: number } {
  let linkedMissions = 0;

  function checkMission(mission: Mission) {
    if (mission.teamId === teamId) linkedMissions++;
    mission.children?.forEach(checkMission);
  }

  missions.forEach(checkMission);

  return { linkedMissions };
}

/**
 * Check if a user can be safely deleted (soft delete = deactivate).
 * Hard delete is generally not recommended for users with references.
 */
export function checkUserDeletion(
  missions: Mission[],
  userId: string,
  isTeamLeader: boolean,
  isManager: boolean,
): DeletionCheck {
  const stats = getUserMissionStats(missions, userId);
  const warnings: string[] = [];
  const blockers: string[] = [];

  // Count active references
  let activeMissions = 0;
  let activeIndicators = 0;

  function checkActive(mission: Mission) {
    if (mission.ownerId === userId && mission.status === "active") {
      activeMissions++;
    }
    mission.indicators?.forEach((indicator) => {
      if (indicator.ownerId === userId && indicator.status !== "completed") {
        activeIndicators++;
      }
    });
    mission.children?.forEach(checkActive);
  }

  missions.forEach(checkActive);

  // Build warnings
  if (stats.ownedMissions > 0) {
    warnings.push(`${stats.ownedMissions} missão(ões) atribuída(s)`);
  }
  if (stats.ownedIndicators > 0) {
    warnings.push(`${stats.ownedIndicators} key result(s) atribuído(s)`);
  }
  if (stats.ownedTasks > 0) {
    warnings.push(`${stats.ownedTasks} tarefa(s) atribuída(s)`);
  }
  if (isTeamLeader) {
    warnings.push("É líder de time");
  }
  if (isManager) {
    warnings.push("É gestor de outros colaboradores");
  }

  // Blockers for hard delete (but not for soft delete/deactivate)
  if (activeMissions > 0) {
    blockers.push(
      `${activeMissions} missão(ões) ativa(s) - reatribua antes de excluir`,
    );
  }
  if (activeIndicators > 0) {
    blockers.push(
      `${activeIndicators} key result(s) em andamento - reatribua antes de excluir`,
    );
  }

  return {
    canDelete: blockers.length === 0,
    canSoftDelete: true, // Users can always be deactivated (soft delete)
    warnings,
    blockers,
  };
}

/**
 * Check if a team can be safely deleted.
 */
export function checkTeamDeletion(
  missions: Mission[],
  teamId: string,
  hasChildTeams: boolean,
  memberCount: number,
): DeletionCheck {
  const stats = getTeamMissionStats(missions, teamId);
  const warnings: string[] = [];
  const blockers: string[] = [];

  // Build warnings
  if (stats.linkedMissions > 0) {
    warnings.push(`${stats.linkedMissions} missão(ões) vinculada(s)`);
  }
  if (memberCount > 0) {
    warnings.push(`${memberCount} membro(s) no time`);
  }

  // Blockers
  if (hasChildTeams) {
    blockers.push(
      "Time possui subtimes - remova ou reatribua os subtimes primeiro",
    );
  }

  // Active missions linked to team block deletion
  let activeMissions = 0;
  function checkActive(mission: Mission) {
    if (mission.teamId === teamId && mission.status === "active") {
      activeMissions++;
    }
    mission.children?.forEach(checkActive);
  }
  missions.forEach(checkActive);

  if (activeMissions > 0) {
    blockers.push(
      `${activeMissions} missão(ões) ativa(s) vinculada(s) - desvincule antes de excluir`,
    );
  }

  return {
    canDelete: blockers.length === 0,
    canSoftDelete: blockers.length === 0, // Teams can't be soft-deleted if they have blockers
    warnings,
    blockers,
  };
}

/**
 * Transfer ownership of all items from one user to another.
 * Returns the modified missions array.
 */
export function transferUserOwnership(
  missions: Mission[],
  fromUserId: string,
  toUserId: string,
): {
  missions: Mission[];
  transferred: { missions: number; indicators: number; tasks: number };
} {
  const transferred = { missions: 0, indicators: 0, tasks: 0 };

  function transferMission(mission: Mission): Mission {
    const updated = { ...mission };

    if (updated.ownerId === fromUserId) {
      updated.ownerId = toUserId;
      transferred.missions++;
    }

    if (updated.indicators?.length) {
      updated.indicators = updated.indicators.map((indicator) => {
        if (indicator.ownerId === fromUserId) {
          transferred.indicators++;
          return { ...indicator, ownerId: toUserId };
        }
        return indicator;
      });
    }

    if (updated.tasks?.length) {
      updated.tasks = updated.tasks.map((task) => {
        if (task.ownerId === fromUserId) {
          transferred.tasks++;
          return { ...task, ownerId: toUserId };
        }
        return task;
      });
    }

    if (updated.children?.length) {
      updated.children = updated.children.map(transferMission);
    }

    return updated;
  }

  const updatedMissions = missions.map(transferMission);

  return { missions: updatedMissions, transferred };
}

/**
 * Unlink team from all missions (set teamId to null).
 * Returns the modified missions array.
 */
export function unlinkTeamFromMissions(
  missions: Mission[],
  teamId: string,
): { missions: Mission[]; unlinked: number } {
  let unlinked = 0;

  function unlinkMission(mission: Mission): Mission {
    const updated = { ...mission };

    if (updated.teamId === teamId) {
      updated.teamId = null;
      unlinked++;
    }

    if (updated.children?.length) {
      updated.children = updated.children.map(unlinkMission);
    }

    return updated;
  }

  const updatedMissions = missions.map(unlinkMission);

  return { missions: updatedMissions, unlinked };
}
