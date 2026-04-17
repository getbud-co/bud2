import { z } from "zod";

const BackendTeamEmployeeSchema = z.object({
  id: z.string(),
  fullName: z.string(),
  email: z.string(),
});

export const BackendTeamResponseSchema = z.object({
  id: z.string(),
  name: z.string(),
  description: z.string().nullable(),
  color: z.string(),
  status: z.string(),
  organizationId: z.string(),
  parentTeamId: z.string().nullable(),
  leaderId: z.string().nullable(),
  createdAt: z.string(),
  updatedAt: z.string(),
  deletedAt: z.string().nullable(),
  employees: z.array(BackendTeamEmployeeSchema),
});

export const BackendTeamListResponseSchema = z.object({
  items: z.array(BackendTeamResponseSchema),
});

export const TeamBulkIdsSchema = z.array(z.string().uuid());

export type BackendTeamResponse = z.infer<typeof BackendTeamResponseSchema>;
