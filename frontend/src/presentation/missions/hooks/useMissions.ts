"use client";

import { useQuery } from "@tanstack/react-query";
import { useOrganization } from "@/contexts/OrganizationContext";
import type { BackendMissionPagedResponse } from "@/schemas/mission";

export const MISSIONS_QUERY_KEY = "missions/list";

export interface MissionsQueryFilter {
  filter?: "Mine" | "MyTeam" | "All";
  search?: string;
  page?: number;
  pageSize?: number;
}

async function fetchMissions(
  orgId: string,
  filters: MissionsQueryFilter,
): Promise<BackendMissionPagedResponse> {
  const params = new URLSearchParams({
    page: String(filters.page ?? 1),
    pageSize: String(filters.pageSize ?? 50),
  });

  if (filters.filter) params.set("filter", filters.filter);
  if (filters.search) params.set("search", filters.search);

  const res = await fetch(`/api/missions?${params}`, {
    headers: { "X-Tenant-Id": orgId },
  });

  if (!res.ok) throw new Error(`HTTP ${res.status}`);

  return res.json() as Promise<BackendMissionPagedResponse>;
}

export function useMissions(filters: MissionsQueryFilter = {}) {
  const { activeOrgId } = useOrganization();

  return useQuery<BackendMissionPagedResponse>({
    queryKey: [MISSIONS_QUERY_KEY, activeOrgId, filters],
    queryFn: () => fetchMissions(activeOrgId!, filters),
    enabled: !!activeOrgId,
    staleTime: 5 * 60 * 1000,
    gcTime: 30 * 60 * 1000,
  });
}
