import { z } from "zod";

export const OrganizationResponseSchema = z.object({
  id: z.string(),
  name: z.string(),
  cnpj: z.string(),
  iconUrl: z.string().nullable().optional(),
  plan: z.enum(["Free", "Pro", "Enterprise"]),
  contractStatus: z.enum(["ToApproval", "Approved", "Cancelled"]),
  createdAt: z.string(),
});

export type OrganizationResponse = z.infer<typeof OrganizationResponseSchema>;

export const OrganizationListResponseSchema = z.object({
  items: z.array(OrganizationResponseSchema),
});
