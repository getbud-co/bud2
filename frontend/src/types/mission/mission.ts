import type { Tag } from "@/types/tag/tag";
import { Indicator, IndicatorStatus } from "./indicator";
import { MissionTask } from "./mission-task";
import { EmployeeLookup } from "../employee/employee";

export type MissionStatus =
  | "Draft"
  | "Active"
  | "Paused"
  | "Completed"
  | "Cancelled";

export type MissionVisibility = "Public" | "Private";

export type KanbanStatus = "Uncategorized" | "Todo" | "Doing" | "Done";

export type MissionMemberRole = "Owner" | "Supporter" | "Observer";

export type MissionLinkType =
  | "related"
  | "depends_on"
  | "contributes_to"
  | "blocks"
  | "duplicates";

export interface MissionMember {
  missionId: string;
  userId: string;
  role: MissionMemberRole;
  addedAt: string;
  addedBy: string | null;
  user?: {
    id: string;
    fullName: string;
    initials: string | null;
    jobTitle: string | null;
    avatarUrl: string | null;
  };
}

export interface MissionLink {
  id: string;
  sourceMissionId: string;
  targetMissionId: string;
  linkType: MissionLinkType;
  createdBy: string | null;
  createdAt: string;
  target?: {
    id: string;
    title: string;
    status: MissionStatus;
    progress: number;
  };
  source?: {
    id: string;
    title: string;
    status: MissionStatus;
    progress: number;
  };
}

export interface ExternalContribution {
  type: "indicator" | "task";
  id: string;
  title: string;
  progress?: number;
  isDone?: boolean;
  status?: IndicatorStatus;
  owner?: { fullName: string; initials: string | null };
  sourceMission: { id: string; title: string };
}

export interface Mission {
  id: string;
  orgId: string;
  cycleId: string | null;
  parentId: string | null;
  path: string[];
  title: string;
  description: string | null;
  ownerId: string;
  status: MissionStatus;
  visibility: MissionVisibility;
  progress: number;
  kanbanStatus: KanbanStatus;
  sortOrder: string;
  dueDate: string | null;
  completedAt: string | null;
  createdAt: string;
  updatedAt: string;
  deletedAt: string | null;
  owner?: EmployeeLookup;
  indicators?: Indicator[];
  tasks?: MissionTask[];
  children?: Mission[];
  tags?: Tag[];
  members?: MissionMember[];
  outgoingLinks?: MissionLink[];
  incomingLinks?: MissionLink[];
  externalContributions?: ExternalContribution[];
  restrictedSummary?: { indicators: number; tasks: number; children: number };
}

export interface TemplateConfig {
  stepTitle: string;
  namePlaceholder: string;
  descPlaceholder: string;
  addItemLabel: string;
  addItemFormTitle: string;
  editItemFormTitle: string;
  itemTitlePlaceholder: string;
  itemDescPlaceholder: string;
  /** Which measurement modes to show; null = show all */
  allowedModes: string[] | null;
}
