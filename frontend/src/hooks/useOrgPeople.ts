"use client";

import { useQuery } from "@tanstack/react-query";
import type { OrgPersonView } from "@/contexts/PeopleDataContext";

export const ORG_PEOPLE_QUERY_KEY = "org-people";

function toInitials(fullName: string): string {
  return fullName
    .trim()
    .split(" ")
    .filter(Boolean)
    .map((p) => p[0] ?? "")
    .slice(0, 2)
    .join("")
    .toUpperCase();
}

async function fetchOrgPeople(orgId: string): Promise<OrgPersonView[]> {
  const res = await fetch("/api/employees?pageSize=100", {
    headers: { "X-Tenant-Id": orgId },
  });
  if (!res.ok) throw new Error(`HTTP ${res.status}`);
  const data: {
    id: string;
    fullName: string;
    leaderId: string | null;
    team: { id: string; name: string } | null;
  }[] = await res.json();

  return data.map((e) => ({
    id: e.id,
    fullName: e.fullName,
    initials: toInitials(e.fullName),
    jobTitle: null,
    leaderId: e.leaderId,
    status: "active" as OrgPersonView["status"],
    teams: e.team ? [e.team] : [],
  }));
}

export function useOrgPeople(orgId: string | null) {
  return useQuery<OrgPersonView[]>({
    queryKey: [ORG_PEOPLE_QUERY_KEY, orgId],
    queryFn: () => fetchOrgPeople(orgId!),
    enabled: !!orgId,
  });
}
