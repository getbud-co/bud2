import { z } from "zod";

const BackendEmployeeMiniSchema = z.object({
  id: z.string(),
  fullName: z.string(),
  email: z.string(),
  nickname: z.string().nullable().optional(),
});

const BackendTagMiniSchema = z.object({
  id: z.string(),
  organizationId: z.string(),
  name: z.string(),
  color: z.string(),
  linkedItems: z.number(),
  createdAt: z.string(),
  updatedAt: z.string(),
});

const BackendIndicatorMiniSchema = z.object({
  id: z.string(),
  organizationId: z.string(),
  missionId: z.string(),
  name: z.string(),
  type: z.string(),
  quantitativeType: z.string().nullable().optional(),
  minValue: z.number().nullable().optional(),
  maxValue: z.number().nullable().optional(),
  unit: z.string().nullable().optional(),
  targetText: z.string().nullable().optional(),
  checkins: z.array(z.unknown()).optional(),
});

const BackendTaskMiniSchema = z.object({
  id: z.string(),
  organizationId: z.string(),
  missionId: z.string(),
  name: z.string(),
  description: z.string().nullable().optional(),
  state: z.string(),
  dueDate: z.string().nullable().optional(),
});

export interface BackendMissionDto {
  id: string;
  name: string;
  description: string | null | undefined;
  dimension: string | null | undefined;
  startDate: string;
  endDate: string;
  status: "Draft" | "Active" | "Completed" | "Paused" | "Cancelled";
  organizationId: string;
  parentId: string | null | undefined;
  employeeId: string | null | undefined;
  employee: z.infer<typeof BackendEmployeeMiniSchema> | null | undefined;
  children: BackendMissionDto[];
  indicators: z.infer<typeof BackendIndicatorMiniSchema>[];
  tasks: z.infer<typeof BackendTaskMiniSchema>[];
  tags: z.infer<typeof BackendTagMiniSchema>[];
}

export const BackendMissionResponseSchema: z.ZodType<BackendMissionDto> = z.lazy(
  () =>
    z.object({
      id: z.string(),
      name: z.string(),
      description: z.string().nullable().optional(),
      dimension: z.string().nullable().optional(),
      startDate: z.string(),
      endDate: z.string(),
      status: z.enum(["Draft", "Active", "Completed", "Paused", "Cancelled"]),
      organizationId: z.string(),
      parentId: z.string().nullable().optional(),
      employeeId: z.string().nullable().optional(),
      employee: BackendEmployeeMiniSchema.nullable().optional(),
      children: z.array(BackendMissionResponseSchema),
      indicators: z.array(BackendIndicatorMiniSchema),
      tasks: z.array(BackendTaskMiniSchema),
      tags: z.array(BackendTagMiniSchema),
    }),
);

export const BackendMissionPagedResponseSchema = z.object({
  items: z.array(BackendMissionResponseSchema),
  total: z.number().int().nonnegative(),
  page: z.number().int().positive(),
  pageSize: z.number().int().positive(),
});

export type BackendMissionPagedResponse = z.infer<
  typeof BackendMissionPagedResponseSchema
>;
