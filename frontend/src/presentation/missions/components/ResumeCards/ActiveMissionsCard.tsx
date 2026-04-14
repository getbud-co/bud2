import { Card, CardBody, Tooltip } from "@getbud-co/buds";
import { Info } from "@phosphor-icons/react";

interface ActiveMissionsCardProps {
  count: number;
}

export function ActiveMissionsCard({ count }: ActiveMissionsCardProps) {
  return (
    <Card padding="sm">
      <CardBody>
        <div className="flex flex-col gap-2">
          <div className="flex items-baseline gap-2">
            <span className="font-[var(--font-heading)] text-[var(--text-2xl)] font-semibold text-[var(--color-neutral-900)] leading-none">
              {count}
            </span>
            <span className="font-[var(--font-label)] text-[var(--text-xs)] text-[var(--color-neutral-500)]">
              Missões ativas
            </span>
          </div>
          <Tooltip content="Total de missões em andamento no período selecionado">
            <Info
              size={16}
              className="text-[var(--color-neutral-400)] flex-shrink-0 cursor-help"
            />
          </Tooltip>
        </div>
      </CardBody>
    </Card>
  );
}
