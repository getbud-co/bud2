import type { OrgPersonView } from "@/contexts/PeopleDataContext";
import type { Team } from "@/types";

export interface TeamTreeNode {
  team: Team;
  leader: OrgPersonView | null;
  members: OrgPersonView[];
  subTeams: TeamTreeNode[];
}

export function buildTeamTree(
  teams: Team[],
  people: OrgPersonView[],
): TeamTreeNode[] {
  const peopleById = new Map(people.map((p) => [p.id, p]));

  function buildNode(team: Team, visited: Set<string>): TeamTreeNode {
    const leader = team.leaderId
      ? (peopleById.get(team.leaderId) ?? null)
      : null;

    const members = people.filter(
      (p) => p.teams.some((t) => t.id === team.id) && p.id !== team.leaderId,
    );

    const subTeams = teams
      .filter((t) => t.parentTeamId === team.id && !visited.has(t.id))
      .map((t) => buildNode(t, new Set([...visited, t.id])));

    return { team, leader, members, subTeams };
  }

  return teams
    .filter((t) => !t.parentTeamId)
    .map((t) => buildNode(t, new Set([t.id])));
}

export function flattenTeamTree(nodes: TeamTreeNode[]): TeamTreeNode[] {
  const result: TeamTreeNode[] = [];
  function walk(n: TeamTreeNode) {
    result.push(n);
    n.subTeams.forEach(walk);
  }
  nodes.forEach(walk);
  return result;
}
