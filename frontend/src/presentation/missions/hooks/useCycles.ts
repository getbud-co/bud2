"use client";

import { useQuery } from "@tanstack/react-query";
import { useOrganization } from "@/contexts/OrganizationContext";
import type { CalendarDate } from "@getbud-co/buds";
import { isoToCalendarDate } from "@/presentation/missions/utils";

export const CYCLES_QUERY_KEY = "missions/cycles";

export interface CyclePreset {
  id: string;
  label: string;
  start: CalendarDate;
  end: CalendarDate;
}

async function fetchCycles(orgId: string): Promise<CyclePreset[]> {
  const res = await fetch("/api/cycles", {
    headers: { "X-Tenant-Id": orgId },
  });
  if (!res.ok) throw new Error(`HTTP ${res.status}`);
  const data: { id: string; name: string; startDate: string; endDate: string }[] =
    await res.json();
  return data.map((c) => ({
    id: c.id,
    label: c.name,
    start: isoToCalendarDate(c.startDate),
    end: isoToCalendarDate(c.endDate),
  }));
}

export function useCycles() {
  const { activeOrgId } = useOrganization();
  return useQuery<CyclePreset[]>({
    queryKey: [CYCLES_QUERY_KEY, activeOrgId],
    queryFn: () => fetchCycles(activeOrgId!),
    enabled: !!activeOrgId,
    staleTime: 5 * 60 * 1000,
    gcTime: 30 * 60 * 1000,
  });
}
