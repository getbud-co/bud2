"use client";

import { useQuery } from "@tanstack/react-query";
import type { PeopleUserView } from "@/contexts/PeopleDataContext";

export const EMPLOYEES_QUERY_KEY = "employees";

function tenantHeader(orgId: string): Record<string, string> {
  return { "X-Tenant-Id": orgId };
}

async function fetchEmployees(orgId: string): Promise<PeopleUserView[]> {
  const res = await fetch("/api/employees?pageSize=100", {
    headers: tenantHeader(orgId),
  });

  if (!res.ok) throw new Error(`HTTP ${res.status}`);
  const data = await res.json();
  return data;
}

export function useEmployees(activeOrgId: string) {
  return useQuery<PeopleUserView[]>({
    queryKey: [EMPLOYEES_QUERY_KEY, activeOrgId],
    queryFn: () => fetchEmployees(activeOrgId),
    enabled: !!activeOrgId,
  });
}
