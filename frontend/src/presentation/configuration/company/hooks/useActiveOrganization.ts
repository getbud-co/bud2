import { useQuery } from "@tanstack/react-query";

export interface OrganizationDetail {
  id: string;
  name: string;
  logoUrl: string | null;
  plan: string;
  contractStatus: string;
  createdAt: string;
}

async function fetchOrganization(id: string): Promise<OrganizationDetail> {
  const res = await fetch(`/api/organizations/${id}`);
  if (!res.ok) throw new Error(`HTTP ${res.status}`);
  return res.json();
}

export function useActiveOrganization(orgId: string | null) {
  return useQuery<OrganizationDetail>({
    queryKey: ["organization", orgId],
    queryFn: () => fetchOrganization(orgId!),
    enabled: !!orgId,
  });
}
