"use client";

import { useQuery } from "@tanstack/react-query";
import type { Team } from "@/types";

export const TEAMS_QUERY_KEY = "teams";

async function fetchTeams(orgId: string): Promise<Team[]> {
  const res = await fetch("/api/teams?pageSize=100", {
    headers: { "X-Tenant-Id": orgId },
  });
  if (!res.ok) throw new Error(`HTTP ${res.status}`);
  return res.json();
}

export function useTeams(orgId: string | null) {
  return useQuery<Team[]>({
    queryKey: [TEAMS_QUERY_KEY, orgId],
    queryFn: () => fetchTeams(orgId!),
    enabled: !!orgId,
  });
}
