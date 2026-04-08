import { useState } from "react";
import { Card, TabBar } from "@mdonangelo/bud-ds";
import { TABS } from "./consts";
import { CompanyInfoTab } from "./components/CompanyInfoTab";
import { CompanyValuesTab } from "./components/CompanyValuesTab";

export function CompanyModule() {
  const [activeTab, setActiveTab] = useState("info");

  return (
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
  );
}
