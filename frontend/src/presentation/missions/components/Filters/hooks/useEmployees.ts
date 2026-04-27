"use client";

import { useQuery } from "@tanstack/react-query";
import { useOrganization } from "@/contexts/OrganizationContext";

export const EMPLOYEES_QUERY_KEY = "missions/employees";

interface EmployeeLookup {
  id: string;
  fullName: string;
  initials: string;
}

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

async function fetchEmployees(orgId: string): Promise<EmployeeLookup[]> {
  const res = await fetch("/api/employees/lookup", {
    headers: { "X-Tenant-Id": orgId },
  });
  if (!res.ok) throw new Error(`HTTP ${res.status}`);
  const data: { id: string; fullName: string }[] = await res.json();
  return data.map((e) => ({
    id: e.id,
    fullName: e.fullName,
    initials: toInitials(e.fullName),
  }));
}

export function useEmployees() {
  const { activeOrgId } = useOrganization();
  return useQuery<EmployeeLookup[]>({
    queryKey: [EMPLOYEES_QUERY_KEY, activeOrgId],
    queryFn: () => fetchEmployees(activeOrgId!),
    enabled: !!activeOrgId,
    staleTime: 5 * 60 * 1000,
    gcTime: 30 * 60 * 1000,
  });
}
