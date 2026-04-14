"use client";

import { createContext, useContext, useMemo, type ReactNode } from "react";
import { useQuery } from "@tanstack/react-query";

export interface LoggedUser {
  id: string;
  email: string;
  fullName: string;
  initials: string;
  organizationId: string;
}

interface LoggedUserContextValue {
  loggedUser: LoggedUser | null;
  isLoading: boolean;
}

const LoggedUserContext = createContext<LoggedUserContextValue | null>(null);

async function fetchLoggedUser(): Promise<LoggedUser> {
  const res = await fetch("/api/auth/me");
  if (!res.ok) {
    throw new Error("Failed to fetch logged user");
  }
  return res.json() as Promise<LoggedUser>;
}

export function LoggedUserProvider({ children }: { children: ReactNode }) {
  const { data, isLoading } = useQuery<LoggedUser>({
    queryKey: ["auth", "me"],
    queryFn: fetchLoggedUser,
    staleTime: 5 * 60 * 1000,
    retry: 1,
  });

  const value = useMemo<LoggedUserContextValue>(
    () => ({ loggedUser: data ?? null, isLoading }),
    [data, isLoading],
  );

  return (
    <LoggedUserContext.Provider value={value}>
      {children}
    </LoggedUserContext.Provider>
  );
}

export function useLoggedUser(): LoggedUserContextValue {
  const ctx = useContext(LoggedUserContext);
  if (!ctx) {
    throw new Error("useLoggedUser must be used within a LoggedUserProvider");
  }
  return ctx;
}
