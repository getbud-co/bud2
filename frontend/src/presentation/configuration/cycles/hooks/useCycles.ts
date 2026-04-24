"use client";

import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import type { Cycle, CycleCadence, CycleStatus } from "@/types";

export const CYCLES_QUERY_KEY = "cycles";

async function fetchCycles(orgId: string): Promise<Cycle[]> {
  const res = await fetch("/api/cycles", {
    headers: { "X-Tenant-Id": orgId },
  });
  if (!res.ok) throw new Error(`HTTP ${res.status}`);
  return res.json();
}

async function createCycleApi(
  orgId: string,
  data: {
    name: string;
    cadence: CycleCadence;
    startDate: string;
    endDate: string;
    status: CycleStatus;
  },
): Promise<Cycle> {
  const res = await fetch("/api/cycles", {
    method: "POST",
    headers: {
      "Content-Type": "application/json",
      "X-Tenant-Id": orgId,
    },
    body: JSON.stringify(data),
  });
  if (!res.ok) throw new Error(`HTTP ${res.status}`);
  return res.json();
}

async function updateCycleApi(
  orgId: string,
  id: string,
  data: Partial<
    Pick<Cycle, "name" | "cadence" | "startDate" | "endDate" | "status">
  >,
): Promise<Cycle> {
  const res = await fetch(`/api/cycles/${id}`, {
    method: "PATCH",
    headers: {
      "Content-Type": "application/json",
      "X-Tenant-Id": orgId,
    },
    body: JSON.stringify(data),
  });
  if (!res.ok) throw new Error(`HTTP ${res.status}`);
  return res.json();
}

async function deleteCycleApi(orgId: string, id: string): Promise<void> {
  const res = await fetch(`/api/cycles/${id}`, {
    method: "DELETE",
    headers: { "X-Tenant-Id": orgId },
  });
  if (!res.ok) throw new Error(`HTTP ${res.status}`);
}

export function useCycles(orgId: string | null) {
  return useQuery<Cycle[]>({
    queryKey: [CYCLES_QUERY_KEY, orgId],
    queryFn: () => fetchCycles(orgId!),
    enabled: !!orgId,
  });
}

export function useCreateCycle(orgId: string | null) {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (data: Parameters<typeof createCycleApi>[1]) =>
      createCycleApi(orgId!, data),
    onSuccess: () =>
      queryClient.invalidateQueries({ queryKey: [CYCLES_QUERY_KEY, orgId] }),
    onError: () => {},
  });
}

export function useUpdateCycle(orgId: string | null) {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({
      id,
      data,
    }: {
      id: string;
      data: Parameters<typeof updateCycleApi>[2];
    }) => updateCycleApi(orgId!, id, data),
    onSuccess: () =>
      queryClient.invalidateQueries({ queryKey: [CYCLES_QUERY_KEY, orgId] }),
    onError: () => {},
  });
}

export function useDeleteCycle(orgId: string | null) {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (id: string) => deleteCycleApi(orgId!, id),
    onSuccess: () =>
      queryClient.invalidateQueries({ queryKey: [CYCLES_QUERY_KEY, orgId] }),
    onError: () => {},
  });
}
