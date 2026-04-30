"use client";

import { useQuery } from "@tanstack/react-query";
import { TagListResponseSchema, type TagResponse } from "@/schemas/tag";

export const TAGS_QUERY_KEY = "tags";

async function fetchTags(orgId: string): Promise<TagResponse[]> {
  const res = await fetch("/api/tags", {
    headers: { "X-Tenant-Id": orgId },
  });
  if (!res.ok) throw new Error(`HTTP ${res.status}`);
  return TagListResponseSchema.parse(await res.json());
}

export function useTags(orgId: string | null) {
  return useQuery<TagResponse[]>({
    queryKey: [TAGS_QUERY_KEY, orgId],
    queryFn: () => fetchTags(orgId!),
    enabled: !!orgId,
  });
}
