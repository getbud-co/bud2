"use client";

import { Warning } from "@phosphor-icons/react";
import { Button } from "@getbud-co/buds";

interface ConfigErrorStateProps {
  message: string;
}

export function ConfigErrorState({ message }: ConfigErrorStateProps) {
  return (
    <div className="flex min-h-[280px] w-full items-center justify-center bg-[var(--color-caramel-50)]">
      <div className="flex max-w-xs flex-col items-center gap-[var(--sp-lg)] text-center">
        <div className="flex h-16 w-16 items-center justify-center rounded-full bg-[var(--color-caramel-100)]">
          <Warning
            size={32}
            color="var(--color-neutral-500)"
            weight="regular"
          />
        </div>

        <div className="flex flex-col gap-[var(--sp-xs)]">
          <p className="[font-family:var(--font-heading)] text-[var(--text-md)] font-semibold text-[var(--color-neutral-900)]">
            Falha no servidor
          </p>
          <p className="leading-relaxed [font-family:var(--font-body)] text-[var(--text-sm)] text-[var(--color-neutral-500)]">
            {message}
          </p>
        </div>

        <Button
          variant="secondary"
          size="md"
          onClick={() => window.location.reload()}
        >
          Tentar novamente
        </Button>
      </div>
    </div>
  );
}
