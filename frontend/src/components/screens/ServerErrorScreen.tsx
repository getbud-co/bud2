"use client";

import { Warning } from "@phosphor-icons/react";
import { Button } from "@getbud-co/buds";

export function ServerErrorScreen() {
  return (
    <div className="flex h-dvh w-full items-center justify-center bg-[var(--color-caramel-50)]">
      <div className="flex flex-col items-center gap-[var(--sp-lg)] text-center max-w-xs">
        <div className="flex items-center justify-center w-16 h-16 rounded-full bg-[var(--color-caramel-100)]">
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
          <p className="[font-family:var(--font-body)] text-[var(--text-sm)] text-[var(--color-neutral-500)] leading-relaxed">
            Não foi possível carregar os dados da organização. Verifique sua
            conexão e tente novamente.
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
