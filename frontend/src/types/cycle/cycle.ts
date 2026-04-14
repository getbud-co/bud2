export type CycleCadence = "Quarterly" | "SemiAnnual" | "Annual" | "Custom";

export type CycleStatus =
  | "Active"
  | "Ended"
  | "Archived"
  | "Planning"
  | "Review";

export interface Cycle {
  id: string;
  organizationId: string;
  name: string;
  cadence: CycleCadence;
  startDate: string;
  endDate: string;
  status: CycleStatus;
}
