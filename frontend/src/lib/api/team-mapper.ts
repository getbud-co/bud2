import type { Team, TeamMember } from "@/types";

export interface BackendEmployee {
  id: string;
  fullName: string;
  email: string;
}

export interface BackendTeam {
  id: string;
  name: string;
  description: string | null;
  color: string;
  status: string;
  organizationId: string;
  parentTeamId: string | null;
  leaderId: string | null;
  createdAt: string;
  updatedAt: string;
  deletedAt: string | null;
  employees: BackendEmployee[];
}

export function toInitials(fullName: string): string {
  return fullName
    .trim()
    .split(" ")
    .filter(Boolean)
    .map((p) => p[0] ?? "")
    .slice(0, 2)
    .join("")
    .toUpperCase();
}

/** Converte PascalCase do backend para o formato lowercase esperado pelo frontend. */
export function capitalize(value: string): string {
  return value.charAt(0).toUpperCase() + value.slice(1);
}

export function mapTeam(raw: BackendTeam): Team {
  const members: TeamMember[] = raw.employees.map((emp) => ({
    teamId: raw.id,
    userId: emp.id,
    roleInTeam: emp.id === raw.leaderId ? "leader" : "member",
    joinedAt: raw.createdAt,
    user: {
      id: emp.id,
      fullName: emp.fullName,
      initials: toInitials(emp.fullName),
      jobTitle: null,
      avatarUrl: null,
    },
  }));

  return {
    id: raw.id,
    orgId: raw.organizationId,
    name: raw.name,
    description: raw.description,
    color: raw.color.toLowerCase() as Team["color"],
    status: raw.status.toLowerCase() as Team["status"],
    leaderId: raw.leaderId,
    parentTeamId: raw.parentTeamId,
    createdAt: raw.createdAt,
    updatedAt: raw.updatedAt,
    deletedAt: raw.deletedAt,
    members,
  };
}
