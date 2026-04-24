import { z } from "zod";

export const EmployeeResponseSchema = z.object({
  id: z.string(),
  fullName: z.string(),
  email: z.string(),
  nickname: z.string().nullable().optional(),
  language: z.enum(["Pt", "En", "Es"]),
  status: z
    .enum(["Active", "Inactive", "Invited", "Suspended"])
    .transform(
      (s) => s.toLowerCase() as "active" | "inactive" | "invited" | "suspended",
    ),
  role: z.enum(["Contributor", "TeamLeader", "HRManager", "OrgAdmin"]),
  organizationId: z.string(),
  leaderId: z.string().nullable().optional(),
  isGlobalAdmin: z.boolean(),
  teams: z.array(z.object({ id: z.string(), name: z.string() })).optional(),
});

export const EmployeeListResponseSchema = z.object({
  items: z.array(EmployeeResponseSchema),
});

export type EmployeeResponse = z.infer<typeof EmployeeResponseSchema>;
