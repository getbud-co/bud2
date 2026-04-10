"use client";

import { useMutation, useQueryClient } from "@tanstack/react-query";
import { ORG_PEOPLE_QUERY_KEY } from "@/hooks/useOrgPeople";
import type { OrgPersonView } from "@/contexts/PeopleDataContext";

export interface PatchEmployeePayload {
  id: string;
  leaderId?: string | null;
  teams?: { id: string; name: string }[];
}

async function patchEmployee(
  orgId: string,
  { id, ...body }: PatchEmployeePayload,
): Promise<OrgPersonView> {
  const res = await fetch(`/api/employees/${id}`, {
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

export function usePatchEmployee(orgId: string | null) {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (payload: PatchEmployeePayload) =>
      patchEmployee(orgId!, payload),

    onMutate: async (payload) => {
      await queryClient.cancelQueries({
        queryKey: [ORG_PEOPLE_QUERY_KEY, orgId],
      });
      const previous = queryClient.getQueryData<OrgPersonView[]>([
        ORG_PEOPLE_QUERY_KEY,
        orgId,
      ]);
      queryClient.setQueryData<OrgPersonView[]>(
        [ORG_PEOPLE_QUERY_KEY, orgId],
        (prev) =>
          prev?.map((p) =>
            p.id === payload.id ? { ...p, ...payload } : p,
          ) ?? [],
      );
      return { previous };
    },

    onError: (_err, _payload, context) => {
      if (context?.previous) {
        queryClient.setQueryData(
          [ORG_PEOPLE_QUERY_KEY, orgId],
          context.previous,
        );
      }
    },

    onSettled: () => {
      queryClient.invalidateQueries({
        queryKey: [ORG_PEOPLE_QUERY_KEY, orgId],
      });
    },
  });
}
