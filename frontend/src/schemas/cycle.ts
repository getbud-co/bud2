import { z } from "zod";

export const CycleResponseSchema = z.object({
  id: z.string(),
  organizationId: z.string(),
  name: z.string(),
  cadence: z.enum(["Quarterly", "SemiAnnual", "Annual", "Custom"]),
  startDate: z.string(),
  endDate: z.string(),
  status: z.enum(["Active", "Ended", "Archived", "Planning", "Review"]),
});

export const CycleListResponseSchema = z.array(CycleResponseSchema);

export type CycleResponse = z.infer<typeof CycleResponseSchema>;
