import { CalendarDate } from "@getbud-co/buds";

export interface MissionItemData {
  id: string;
  name: string;
  description: string;
  measurementMode: string | null;
  manualType: string | null;
  period: [CalendarDate | null, CalendarDate | null];
  missionValue: string;
  missionValueMin: string;
  missionValueMax: string;
  missionUnit: string;
  children?: MissionItemData[];
}
