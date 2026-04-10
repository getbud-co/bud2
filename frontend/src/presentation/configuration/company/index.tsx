"use client";

import { useState } from "react";
import { Card, TabBar } from "@mdonangelo/bud-ds";
import { TABS } from "./consts";
import { CompanyInfoTab } from "./components/CompanyInfoTab";
import { CompanyValuesTab } from "./components/CompanyValuesTab";
import { PageHeader } from "@/presentation/layout/page-header";

export function CompanyModule() {
  const [activeTab, setActiveTab] = useState("info");

  return (
    <div className="flex flex-col gap-[var(--sp-2xs)] w-full">
      <PageHeader title="Dados da empresa" />
      <div className="flex flex-col gap-[var(--sp-2xs)] min-w-0">
        <Card padding="none">
          <TabBar
            tabs={TABS}
            activeTab={activeTab}
            onTabChange={setActiveTab}
            ariaLabel="Configurações da empresa"
          />

          {activeTab === "info" && <CompanyInfoTab />}
          {activeTab === "values" && <CompanyValuesTab />}
        </Card>
      </div>
    </div>
  );
}
