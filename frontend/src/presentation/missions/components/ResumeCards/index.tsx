import { CardBody } from "@getbud-co/buds";
import { ProgressCard } from "./ProgressCard";
import { ActiveMissionsCard } from "./ActiveMissionsCard";
import { OutdatedIndicatorsCard } from "./OutdatedIndicatorsCard";

interface ResumeCardsProps {
  totalValue: number;
  totalExpected: number;
  activeMissions: number;
  outdatedIndicators: number;
}

export function ResumeCards({
  totalValue,
  totalExpected,
  activeMissions,
  outdatedIndicators,
}: ResumeCardsProps) {
  return (
    <CardBody>
      <div className="grid grid-cols-3 gap-4">
        <ProgressCard value={totalValue} expected={totalExpected} />
        <ActiveMissionsCard count={activeMissions} />
        <OutdatedIndicatorsCard count={outdatedIndicators} />
      </div>
    </CardBody>
  );
}
