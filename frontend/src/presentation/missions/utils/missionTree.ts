import type {
  ExternalContribution,
  Indicator,
  Mission,
  MissionTask,
} from "@/types";

export function findParentMission(
  indicatorId: string,
  missionList: Mission[],
): string {
  for (const mission of missionList) {
    for (const indicator of mission.indicators ?? []) {
      if (indicator.id === indicatorId) return mission.title;
      if (indicator.children?.some((sub) => sub.id === indicatorId))
        return `${mission.title} › ${indicator.title}`;
    }

    if (mission.children) {
      const found = findParentMission(indicatorId, mission.children);
      if (found) return found;
    }
  }

  return "";
}

export function findIndicatorById(
  id: string,
  missionList: Mission[],
): Indicator | null {
  function searchInIndicators(indicators: Indicator[]): Indicator | null {
    for (const indicator of indicators) {
      if (indicator.id === id) return indicator;
      if (indicator.children) {
        const child = searchInIndicators(indicator.children);
        if (child) return child;
      }
    }
    return null;
  }

  for (const mission of missionList) {
    const foundInMission = searchInIndicators(mission.indicators ?? []);
    if (foundInMission) return foundInMission;

    if (mission.children) {
      const foundInChildren = findIndicatorById(id, mission.children);
      if (foundInChildren) return foundInChildren;
    }
  }

  return null;
}

export function findTaskById(
  taskId: string,
  missionList: Mission[],
): { task: MissionTask; parentLabel: string } | null {
  for (const mission of missionList) {
    const missionTask = mission.tasks?.find((task) => task.id === taskId);
    if (missionTask) return { task: missionTask, parentLabel: mission.title };

    for (const indicator of mission.indicators ?? []) {
      const indicatorTask = indicator.tasks?.find((task) => task.id === taskId);
      if (indicatorTask)
        return {
          task: indicatorTask,
          parentLabel: `${mission.title} › ${indicator.title}`,
        };

      for (const sub of indicator.children ?? []) {
        const subTask = sub.tasks?.find((task) => task.id === taskId);
        if (subTask) {
          return {
            task: subTask,
            parentLabel: `${mission.title} › ${indicator.title} › ${sub.title}`,
          };
        }
      }
    }

    if (mission.children) {
      const found = findTaskById(taskId, mission.children);
      if (found) return found;
    }
  }

  return null;
}

export function findTaskInMissions(
  taskId: string,
  missionList: Mission[],
): MissionTask | undefined {
  for (const mission of missionList) {
    const missionTask = mission.tasks?.find((task) => task.id === taskId);
    if (missionTask) return missionTask;

    for (const indicator of mission.indicators ?? []) {
      const indicatorTask = indicator.tasks?.find((task) => task.id === taskId);
      if (indicatorTask) return indicatorTask;
    }

    if (mission.children) {
      const childTask = findTaskInMissions(taskId, mission.children);
      if (childTask) return childTask;
    }
  }

  return undefined;
}

export function flattenMissions(
  missions: Mission[],
): { id: string; title: string }[] {
  const list: { id: string; title: string }[] = [];
  for (const mission of missions) {
    list.push({ id: mission.id, title: mission.title });
    if (mission.children) list.push(...flattenMissions(mission.children));
  }
  return list;
}

export function addIndicatorContribution(
  missions: Mission[],
  indicatorId: string,
  target: { id: string; title: string },
): Mission[] {
  return missions.map((mission) => ({
    ...mission,
    indicators: mission.indicators?.map((indicator) =>
      indicator.id === indicatorId
        ? {
            ...indicator,
            contributesTo: indicator.contributesTo?.some(
              (c) => c.missionId === target.id,
            )
              ? indicator.contributesTo
              : [
                  ...(indicator.contributesTo ?? []),
                  { missionId: target.id, missionTitle: target.title },
                ],
          }
        : indicator,
    ),
    children: mission.children
      ? addIndicatorContribution(mission.children, indicatorId, target)
      : undefined,
  }));
}

export function addTaskContribution(
  missions: Mission[],
  taskId: string,
  target: { id: string; title: string },
): Mission[] {
  return missions.map((mission) => ({
    ...mission,
    tasks: mission.tasks?.map((task) =>
      task.id === taskId
        ? {
            ...task,
            contributesTo: task.contributesTo?.some(
              (c) => c.missionId === target.id,
            )
              ? task.contributesTo
              : [
                  ...(task.contributesTo ?? []),
                  { missionId: target.id, missionTitle: target.title },
                ],
          }
        : task,
    ),
    children: mission.children
      ? addTaskContribution(mission.children, taskId, target)
      : undefined,
  }));
}

export function addExternalContrib(
  missions: Mission[],
  targetId: string,
  contrib: ExternalContribution,
): Mission[] {
  return missions.map((mission) => ({
    ...mission,
    externalContributions:
      mission.id === targetId &&
      !mission.externalContributions?.some((ec) => ec.id === contrib.id)
        ? [...(mission.externalContributions ?? []), contrib]
        : mission.externalContributions,
    children: mission.children
      ? addExternalContrib(mission.children, targetId, contrib)
      : undefined,
  }));
}

export function removeIndicatorContribution(
  missions: Mission[],
  indicatorId: string,
  targetMissionId: string,
): Mission[] {
  return missions.map((mission) => ({
    ...mission,
    indicators: mission.indicators?.map((indicator) =>
      indicator.id === indicatorId
        ? {
            ...indicator,
            contributesTo: indicator.contributesTo?.filter(
              (c) => c.missionId !== targetMissionId,
            ),
          }
        : indicator,
    ),
    children: mission.children
      ? removeIndicatorContribution(
          mission.children,
          indicatorId,
          targetMissionId,
        )
      : undefined,
  }));
}

export function removeTaskContribution(
  missions: Mission[],
  taskId: string,
  targetMissionId: string,
): Mission[] {
  return missions.map((mission) => ({
    ...mission,
    tasks: mission.tasks?.map((task) =>
      task.id === taskId
        ? {
            ...task,
            contributesTo: task.contributesTo?.filter(
              (c) => c.missionId !== targetMissionId,
            ),
          }
        : task,
    ),
    children: mission.children
      ? removeTaskContribution(mission.children, taskId, targetMissionId)
      : undefined,
  }));
}

export function removeExternalContrib(
  missions: Mission[],
  targetMissionId: string,
  itemId: string,
): Mission[] {
  return missions.map((mission) => ({
    ...mission,
    externalContributions:
      mission.id === targetMissionId
        ? mission.externalContributions?.filter(
            (external) => external.id !== itemId,
          )
        : mission.externalContributions,
    children: mission.children
      ? removeExternalContrib(mission.children, targetMissionId, itemId)
      : undefined,
  }));
}
