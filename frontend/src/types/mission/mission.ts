import type { Tag } from "@/types/tag/tag";
import { Indicator, IndicatorStatus } from "./indicator";
import { MissionTask } from "./mission-task";

export type MissionStatus =
  | "draft"
  | "active"
  | "paused"
  | "completed"
  | "cancelled";

export type MissionVisibility = "public" | "team_only" | "private";

export type KanbanStatus = "uncategorized" | "todo" | "doing" | "done";

export type MissionMemberRole = "owner" | "supporter" | "observer";

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
  /** Preenchido em queries com join */
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
  /** Preenchido em queries com join */
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
  /** Profundidade na árvore: 0 = raiz, 1 = filha, etc. */
  depth: number;
  /** Materialized path: [raiz_id, ..., self_id] para queries eficientes de subárvore */
  path: string[];
  title: string;
  description: string | null;
  ownerId: string;
  teamId: string | null;
  status: MissionStatus;
  visibility: MissionVisibility;
  progress: number;
  kanbanStatus: KanbanStatus;
  sortOrder: number;
  dueDate: string | null;
  completedAt: string | null;
  createdAt: string;
  updatedAt: string;
  deletedAt: string | null;
  /** Preenchido em queries com join */
  owner?: {
    id: string;
    fullName: string;
    initials: string | null;
  };
  team?: { id: string; name: string; color: string };
  indicators?: Indicator[];
  tasks?: MissionTask[];
  children?: Mission[];
  tags?: Tag[];
  /** FUTURE */
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
