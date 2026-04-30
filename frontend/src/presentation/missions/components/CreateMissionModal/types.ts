import type { CalendarDate } from "@getbud-co/buds";

export interface MissionItemData {
  id: string;
  name: string;
  description: string;
  measurementMode: string | null;
  manualType: string | null;
  surveyId: string | null;
  period: [CalendarDate | null, CalendarDate | null];
  goalValue: string;
  goalValueMin: string;
  goalValueMax: string;
  goalUnit: string;
  ownerId: string | null;
  teamId: string | null;
  children?: MissionItemData[];
}
