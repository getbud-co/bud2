"use client";

import QueryProvider from "@/contexts/QueryContext";
import { OrganizationProvider } from "@/contexts/OrganizationContext";
import { ConfigDataProvider } from "@/contexts/ConfigDataContext";
import { ActivityDataProvider } from "@/contexts/ActivityDataContext";
import { PeopleDataProvider } from "@/contexts/PeopleDataContext";
import { MissionsDataProvider } from "@/contexts/MissionsDataContext";
import { SettingsDataProvider } from "@/contexts/SettingsDataContext";
import { IntegrationsDataProvider } from "@/contexts/IntegrationsDataContext";
import type { ReactNode } from "react";

interface AppProvidersProps {
  children: ReactNode;
  initialOrgId?: string;
}

export function AppProviders({ children, initialOrgId }: AppProvidersProps) {
  return (
    <QueryProvider>
      <OrganizationProvider initialOrgId={initialOrgId}>
        <ConfigDataProvider>
          <ActivityDataProvider>
            <PeopleDataProvider>
              <MissionsDataProvider>
                <SettingsDataProvider>
                  <IntegrationsDataProvider>
                    {children}
                  </IntegrationsDataProvider>
                </SettingsDataProvider>
              </MissionsDataProvider>
            </PeopleDataProvider>
          </ActivityDataProvider>
        </ConfigDataProvider>
      </OrganizationProvider>
    </QueryProvider>
  );
}
