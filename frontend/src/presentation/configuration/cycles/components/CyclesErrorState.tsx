"use client";

import { ConfigErrorState } from "@/components/ConfigErrorState";

export function CyclesErrorState() {
  return (
    <ConfigErrorState message="Não foi possível carregar os ciclos. Verifique sua conexão e tente novamente." />
  );
}
