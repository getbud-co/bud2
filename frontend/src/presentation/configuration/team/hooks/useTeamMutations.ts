"use client";

import { useMutation, useQueryClient } from "@tanstack/react-query";
import type { Team, TeamColor, TeamMember, TeamStatus } from "@/types";
import { TEAMS_QUERY_KEY } from "./useTeams";

export interface CreateTeamPayload {
  name: string;
  description: string | null;
  color: TeamColor;
  organizationId: string;
  leaderId: string;
}

export interface UpdateTeamPayload {
  id: string;
  name?: string;
  description?: string | null;
  color?: TeamColor;
  status?: TeamStatus;
  leaderId?: string | null;
}

async function createTeam(orgId: string, payload: CreateTeamPayload): Promise<Team> {
  const res = await fetch("/api/teams", {
    method: "POST",
    headers: {
      "Content-Type": "application/json",
      "X-Tenant-Id": orgId,
    },
    body: JSON.stringify(payload),
  });
  if (!res.ok) throw new Error(`HTTP ${res.status}`);
  return res.json();
}

async function updateTeam(orgId: string, payload: UpdateTeamPayload): Promise<Team> {
  const { id, ...body } = payload;
  const res = await fetch(`/api/teams/${id}`, {
    method: "PATCH",
    headers: {
      "Content-Type": "application/json",
      "X-Tenant-Id": orgId,
    },
    body: JSON.stringify(body),
  });
  if (!res.ok) throw new Error(`HTTP ${res.status}`);
  return res.json();
}

export function useCreateTeam(orgId: string | null) {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (payload: CreateTeamPayload) => createTeam(orgId!, payload),
    onSuccess: (created) => {
      queryClient.setQueryData<Team[]>([TEAMS_QUERY_KEY, orgId], (prev) =>
        prev ? [...prev, created] : [created],
      );
    },
  });
}

export function useUpdateTeam(orgId: string | null) {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (payload: UpdateTeamPayload) => updateTeam(orgId!, payload),
    onSuccess: (updated) => {
      queryClient.setQueryData<Team[]>([TEAMS_QUERY_KEY, orgId], (prev) =>
        prev?.map((t) => (t.id === updated.id ? { ...t, ...updated } : t)) ?? [],
      );
    },
  });
}

export function useBulkArchiveTeams(orgId: string | null) {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: async (ids: string[]) => {
      const res = await fetch("/api/teams/bulk-archive", {
        method: "POST",
        headers: {
          "Content-Type": "application/json",
          ...(orgId ? { "X-Tenant-Id": orgId } : {}),
        },
        body: JSON.stringify(ids),
      });
      if (!res.ok) throw new Error(`HTTP ${res.status}`);
    },
    onSuccess: (_data, ids) => {
      const idSet = new Set(ids);
      queryClient.setQueryData<Team[]>([TEAMS_QUERY_KEY, orgId], (prev) =>
        prev?.map((t) =>
          idSet.has(t.id) ? { ...t, status: "archived" as const } : t,
        ) ?? [],
      );
    },
  });
}

export function useDeleteTeam(orgId: string | null) {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: async (teamId: string) => {
      const res = await fetch(`/api/teams/${teamId}`, {
        method: "DELETE",
        headers: orgId ? { "X-Tenant-Id": orgId } : {},
      });
      if (!res.ok) throw new Error(`HTTP ${res.status}`);
    },
    onSuccess: (_data, teamId) => {
      queryClient.setQueryData<Team[]>([TEAMS_QUERY_KEY, orgId], (prev) =>
        prev?.filter((t) => t.id !== teamId) ?? [],
      );
    },
  });
}

export function useBulkDeleteTeams(orgId: string | null) {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: async (ids: string[]) => {
      const res = await fetch("/api/teams/bulk-delete", {
        method: "POST",
        headers: {
          "Content-Type": "application/json",
          ...(orgId ? { "X-Tenant-Id": orgId } : {}),
        },
        body: JSON.stringify(ids),
      });
      if (!res.ok) throw new Error(`HTTP ${res.status}`);
    },
    onSuccess: (_data, ids) => {
      const idSet = new Set(ids);
      queryClient.setQueryData<Team[]>([TEAMS_QUERY_KEY, orgId], (prev) =>
        prev?.filter((t) => !idSet.has(t.id)) ?? [],
      );
    },
  });
}

/** Derives the leaderId from the members list (first member with role "leader"). */
export function extractLeaderId(members: TeamMember[]): string | null {
  return members.find((m) => m.roleInTeam === "leader")?.userId ?? null;
}
