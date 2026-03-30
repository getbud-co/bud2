import { Card, CardBody, MissionProgressBar } from "@mdonangelo/bud-ds";

interface ProgressCardProps {
  value: number;
  expected: number;
}

export function ProgressCard({ value, expected }: ProgressCardProps) {
  return (
    <Card padding="sm">
      <CardBody>
        <div className="flex flex-col gap-2">
          <span className="font-[var(--font-label)] text-[var(--text-xs)] text-[var(--color-neutral-500)]">
            Progresso geral
          </span>
          <MissionProgressBar
            label=""
            value={value}
            target={100}
            expected={expected}
            formattedValue={`${value}%`}
          />
          <span className="font-[var(--font-label)] text-[var(--text-xs)] text-[var(--color-neutral-400)]">
            Esperado {expected}%
          </span>
        </div>
      </CardBody>
    </Card>
  );
}
