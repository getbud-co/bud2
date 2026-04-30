export type EmployeeRole =
  | "Contributor"
  | "TeamLeader"
  | "HRManager"
  | "OrgAdmin";

export interface EmployeeLookup {
  id: string;
  fullName: string;
  initials: string | null;
  role: EmployeeRole;
}
