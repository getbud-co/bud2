"use client";

import {
  createContext,
  useCallback,
  useContext,
  useEffect,
  useMemo,
  useState,
  type ReactNode,
} from "react";
import { useQuery } from "@tanstack/react-query";
import Cookies from "js-cookie";
import type { CompanyProfile } from "@/lib/tempStorage/config-store";
import { LoadingScreen } from "@/components/screens/LoadingScreen";
import { ServerErrorScreen } from "@/components/screens/ServerErrorScreen";

const COOKIE_NAME = "selectedOrgId";
const COOKIE_OPTIONS = {
  secure: true,
  sameSite: "lax" as const,
  path: "/",
  expires: 30,
};

interface OrganizationContextValue {
  organizations: CompanyProfile[];
  activeOrganization: CompanyProfile | null;
  activeOrgId: string | null;
  isLoading: boolean;
  setActiveOrg: (orgId: string) => void;
}

const OrganizationContext = createContext<OrganizationContextValue | null>(
  null,
);

export function OrganizationProvider({
  children,
  initialOrgId,
}: {
  children: ReactNode;
  initialOrgId?: string;
}) {
  const {
    data: organizations = [],
    isLoading,
    isError,
    error,
  } = useQuery<CompanyProfile[]>({
    queryKey: ["organizations"],
    queryFn: async () => {
      const res = await fetch("/api/organizations");
      if (!res.ok) {
        const err = new Error(`HTTP ${res.status}`) as Error & {
          status: number;
        };
        err.status = res.status;
        throw err;
      }
      return res.json() as Promise<CompanyProfile[]>;
    },
  });

  const isServerError =
    isError && ((error as { status?: number })?.status ?? 0) >= 500;

  const [activeOrgId, setActiveOrgId] = useState<string | null>(
    initialOrgId ?? null,
  );

  useEffect(() => {
    if (organizations.length === 0) return;

    const cookieOrgId = Cookies.get(COOKIE_NAME);
    const resolvedId = cookieOrgId ?? activeOrgId;
    const isValid =
      resolvedId && organizations.some((org) => org.id === resolvedId);

    if (isValid) {
      setActiveOrgId(resolvedId!);
    } else {
      const firstId = organizations[0]!.id;
      setActiveOrgId(firstId);
      Cookies.set(COOKIE_NAME, firstId, COOKIE_OPTIONS);
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [organizations]);

  const setActiveOrg = useCallback(
    (orgId: string) => {
      if (!organizations.some((org) => org.id === orgId)) return;
      Cookies.set(COOKIE_NAME, orgId, COOKIE_OPTIONS);
      setActiveOrgId(orgId);
    },
    [organizations],
  );

  const activeOrganization = useMemo(
    () => organizations.find((org) => org.id === activeOrgId) ?? null,
    [organizations, activeOrgId],
  );

  const value = useMemo<OrganizationContextValue>(
    () => ({
      organizations,
      activeOrganization,
      activeOrgId,
      isLoading,
      setActiveOrg,
    }),
    [organizations, activeOrganization, activeOrgId, isLoading, setActiveOrg],
  );

  if (isLoading) return <LoadingScreen />;
  if (isServerError) return <ServerErrorScreen />;

  return (
    <OrganizationContext.Provider value={value}>
      {children}
    </OrganizationContext.Provider>
  );
}

export function useOrganization() {
  const context = useContext(OrganizationContext);
  if (!context) {
    throw new Error("useOrganization must be used within OrganizationProvider");
  }
  return context;
}
