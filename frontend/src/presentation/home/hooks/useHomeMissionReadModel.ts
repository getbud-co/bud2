import { useMemo } from "react";
import { useMissionsData } from "@/contexts/MissionsDataContext";
import { useConfigData } from "@/contexts/ConfigDataContext";
import type { Mission, Indicator, Cycle } from "@/types";

export interface HomeMissionIndicator {
  label: string;
  value: number;
  expected: number;
  target: number;
  owner: string;
  status: "on-track" | "attention" | "off-track";
}

export interface HomeActivityItem {
  title: string;
  subtitle: string;
  urgent?: boolean;
  category: string;
}

export interface HomeCycleInfo {
  id: string;
  label: string;
  value: number;
  expected: number;
  target: number;
  indicators: HomeMissionIndicator[];
}

function flattenMissions(missions: Mission[]): Mission[] {
  const flat: Mission[] = [];
  for (const mission of missions) {
    flat.push(mission);
    if (mission.children?.length) {
      flat.push(...flattenMissions(mission.children));
    }
  }
  return flat;
}

function flattenKrs(missions: Mission[]): Indicator[] {
  const list: Indicator[] = [];
  const allMissions = flattenMissions(missions);
  for (const mission of allMissions) {
    for (const indicator of mission.indicators ?? []) {
      list.push(indicator);
      if (indicator.children?.length) {
        list.push(...indicator.children);
      }
    }
  }
  return list;
}

function ownerName(indicator: Indicator): string {
  if (indicator.owner) {
    return `${indicator.owner.firstName} ${indicator.owner.lastName}`.trim();
  }
  return "Responsável não definido";
}

function getExpectedProgress(cycle: Cycle | null): number {
  if (!cycle) return 50;

  const now = new Date();
  const start = new Date(cycle.startDate);
  const end = new Date(cycle.endDate);

  if (now < start) return 0;
  if (now > end) return 100;

  const totalDays = (end.getTime() - start.getTime()) / (1000 * 60 * 60 * 24);
  const elapsedDays = (now.getTime() - start.getTime()) / (1000 * 60 * 60 * 24);

  return Math.round((elapsedDays / totalDays) * 100);
}

function mapKrToSummary(indicator: Indicator): HomeMissionIndicator {
  return {
    label: indicator.title,
    value: indicator.progress,
    expected: 50, // Will be overridden by cycle-based calculation
    target: Number(indicator.targetValue ?? "100") || 100,
    owner: ownerName(indicator),
    status:
      indicator.status === "off_track"
        ? "off-track"
        : indicator.status === "attention"
          ? "attention"
          : "on-track",
  };
}

function computeAverageProgress(indicators: HomeMissionIndicator[]): number {
  if (indicators.length === 0) return 0;
  return Math.round(
    indicators.reduce((acc, item) => acc + item.value, 0) / indicators.length,
  );
}

export function useHomeMissionReadModel() {
  const { missions } = useMissionsData();
  const { cycles } = useConfigData();

  return useMemo(() => {
    // Find active annual and quarterly cycles
    const annualCycle =
      cycles.find((c) => c.type === "annual" && c.status === "active") ?? null;
    const quarterlyCycle =
      cycles.find((c) => c.type === "quarterly" && c.status === "active") ??
      null;

    const activeMissions = missions.filter(
      (mission) => mission.status !== "cancelled",
    );

    // Filter missions by cycle
    const annualMissions = activeMissions.filter(
      (m) => m.cycleId === annualCycle?.id,
    );
    const quarterlyMissions = activeMissions.filter(
      (m) => m.cycleId === quarterlyCycle?.id,
    );

    // If no missions linked to cycles, use all active missions as fallback
    const annualKrs = flattenKrs(
      annualMissions.length > 0 ? annualMissions : activeMissions,
    );
    const quarterlyKrs = flattenKrs(
      quarterlyMissions.length > 0 ? quarterlyMissions : activeMissions,
    );

    const annualExpected = getExpectedProgress(annualCycle);
    const quarterlyExpected = getExpectedProgress(quarterlyCycle);

    const mappedAnnualKrs: HomeMissionIndicator[] = annualKrs
      .slice(0, 5)
      .map((indicator) => ({
        ...mapKrToSummary(indicator),
        expected: annualExpected,
      }));

    const mappedQuarterlyKrs: HomeMissionIndicator[] = quarterlyKrs
      .slice(0, 5)
      .map((indicator) => ({
        ...mapKrToSummary(indicator),
        expected: quarterlyExpected,
      }));

    const annualProgress = computeAverageProgress(mappedAnnualKrs);
    const quarterlyProgress = computeAverageProgress(mappedQuarterlyKrs);

    // Generate activities from urgent Indicators
    const allKrs = [...mappedAnnualKrs, ...mappedQuarterlyKrs];
    const urgentKrs = allKrs
      .filter((item) => item.status !== "on-track")
      .slice(0, 4);

    const activities: HomeActivityItem[] = urgentKrs.map((indicator) => ({
      title: `Atualizar missão: ${indicator.label}`,
      subtitle: `Progresso atual ${indicator.value}% (esperado ${indicator.expected}%)`,
      urgent: indicator.status === "off-track",
      category: "Missões",
    }));

    return {
      annual: {
        id: annualCycle?.id ?? "annual",
        label: annualCycle?.name ?? "Missões anuais",
        value: annualProgress,
        expected: annualExpected,
        target: 100,
        indicators: mappedAnnualKrs,
      } as HomeCycleInfo,
      quarter: {
        id: quarterlyCycle?.id ?? "quarterly",
        label: quarterlyCycle?.name ?? "Missões trimestrais",
        value: quarterlyProgress,
        expected: quarterlyExpected,
        target: 100,
        indicators: mappedQuarterlyKrs,
      } as HomeCycleInfo,
      activities,
    };
  }, [missions, cycles]);
}
