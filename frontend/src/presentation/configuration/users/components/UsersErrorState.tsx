"use client";

import { ConfigErrorState } from "@/components/ConfigErrorState";

export function UsersErrorState() {
  return (
    <ConfigErrorState message="Não foi possível carregar os usuários. Verifique sua conexão e tente novamente." />
  );
}
