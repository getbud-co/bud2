import { Card, TabBar, Badge, Button, Alert } from "@mdonangelo/bud-ds";
import { DownloadSimple } from "@phosphor-icons/react";
import {
  useParams,
  useRouter,
  usePathname,
  useSearchParams,
} from "next/navigation";
import { useSurveysData } from "@/contexts/SurveysDataContext";
import { buildSurveyResultsFromRecord } from "@/presentation/surveys/utils/localSurveyAdapters";
import { OverviewTab } from "./components/OverviewTab";
import { SummaryTab } from "./components/SummaryTab";
import { CalibrationTab } from "./components/CalibrationTab";
import { SettingsTab } from "./components/SettingsTab";
import styles from "./SurveyResultsPage.module.css";
import { PageHeader } from "@/presentation/layout/page-header";

/* ——— Status config ——— */

const STATUS_CONFIG: Record<
  string,
  {
    label: string;
    color:
      | "neutral"
      | "orange"
      | "success"
      | "warning"
      | "error"
      | "wine"
      | "caramel";
  }
> = {
  draft: { label: "Rascunho", color: "caramel" },
  scheduled: { label: "Agendada", color: "wine" },
  active: { label: "Ativa", color: "success" },
  paused: { label: "Pausada", color: "warning" },
  closed: { label: "Encerrada", color: "neutral" },
  archived: { label: "Arquivada", color: "neutral" },
};

/* ——— Component ——— */

export function SurveyResultsPage() {
  const params = useParams();
  const surveyId =
    typeof params.surveyId === "string" ? params.surveyId : undefined;
  const router = useRouter();
  const pathname = usePathname();
  const searchParams = useSearchParams();
  const { getSurveyRecordById, getSurveySubmissionsBySurveyId } =
    useSurveysData();

  const localRecord = surveyId ? getSurveyRecordById(surveyId) : null;
  const submissions = surveyId ? getSurveySubmissionsBySurveyId(surveyId) : [];
  const data = localRecord
    ? buildSurveyResultsFromRecord(localRecord, { submissions })
    : null;

  const VALID_TABS = ["visao-geral", "resumo", "calibragem", "configuracao"];
  const tabParam = searchParams.get("tab");
  const activeTab =
    tabParam && VALID_TABS.includes(tabParam) ? tabParam : "visao-geral";

  function handleTabChange(tab: string) {
    const nextParams = new URLSearchParams(searchParams.toString());
    if (tab === "visao-geral") {
      nextParams.delete("tab");
    } else {
      nextParams.set("tab", tab);
    }
    const query = nextParams.toString();
    router.replace(query ? `${pathname}?${query}` : pathname);
  }

  if (!data) {
    return (
      <div className={styles.page}>
        <PageHeader title="Pesquisa não encontrada" />
        <p className={styles.emptyText}>
          A pesquisa solicitada não foi encontrada ou não possui resultados.
        </p>
        <Button
          variant="secondary"
          size="md"
          onClick={() => router.push("/surveys")}
        >
          Voltar para pesquisas
        </Button>
      </div>
    );
  }

  const statusConfig = STATUS_CONFIG[data.status] ?? STATUS_CONFIG.draft!;

  const TABS = [
    { value: "visao-geral", label: "Visão geral" },
    { value: "resumo", label: "Resultado por pergunta" },
    ...(data.surveyCategory === "ciclo"
      ? [{ value: "calibragem", label: "Calibragem" }]
      : []),
    { value: "configuracao", label: "Configuração" },
  ];

  return (
    <div className={styles.page}>
      <PageHeader title={data.surveyName}>
        <div className={styles.headerActions}>
          <Badge color={statusConfig.color} size="md">
            {statusConfig.label}
          </Badge>
          {data.period && (
            <span className={styles.periodText}>{data.period}</span>
          )}
          <Button variant="secondary" size="md" leftIcon={DownloadSimple}>
            Exportar
          </Button>
        </div>
      </PageHeader>

      {data.kpis.responses === 0 && (
        <Alert variant="info" title="Sem respostas até o momento">
          Assim que os participantes responderem, os gráficos e análises
          aparecerão aqui.
        </Alert>
      )}

      <Card padding="none">
        <TabBar
          tabs={TABS}
          activeTab={activeTab}
          onTabChange={handleTabChange}
          ariaLabel="Abas de resultados da pesquisa"
        />

        {activeTab === "visao-geral" && <OverviewTab data={data} />}
        {activeTab === "resumo" && <SummaryTab data={data} />}
        {activeTab === "calibragem" && <CalibrationTab data={data} />}
        {activeTab === "configuracao" && <SettingsTab data={data} />}
      </Card>
    </div>
  );
}
