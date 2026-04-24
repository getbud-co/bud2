export type ConfidenceLevel =
  | "high"
  | "medium"
  | "low"
  | "barrier"
  | "deprioritized";

export interface CheckIn {
  id: string;
  indicatorId: string;
  authorId: string;
  value: string;
  previousValue: string | null;
  confidence: ConfidenceLevel | null;
  note: string | null;
  mentions: string[] | null;
  createdAt: string;
  /** Preenchido em queries com join */
  author?: {
    id: string;
    fullName: string;
    initials: string | null;
  };
}

export interface CheckinPayload {
  indicator: Indicator;
  currentValue: number;
  newValue: number;
}
