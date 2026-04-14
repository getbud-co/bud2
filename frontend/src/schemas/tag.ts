import { z } from "zod";

export const TagResponseSchema = z.object({
  id: z.string(),
  organizationId: z.string(),
  name: z.string(),
  color: z.enum([
    "Neutral",
    "Orange",
    "Wine",
    "Caramel",
    "Success",
    "Warning",
    "Error",
  ]),
  linkedItems: z.number().int().nonnegative(),
  createdAt: z.string(),
  updatedAt: z.string(),
});

export const TagListResponseSchema = z.array(TagResponseSchema);

export type TagResponse = z.infer<typeof TagResponseSchema>;
