import {
  ResponsiveContainer,
  LineChart,
  Line,
  XAxis,
  YAxis,
  CartesianGrid,
  Tooltip as RechartsTooltip,
  ReferenceArea,
  ReferenceLine,
} from "recharts";
import { Badge } from "@getbud-co/buds";
import type { KeyResult, KRStatus, ConfidenceLevel } from "@/types";
import { numVal, getKRStatusLabel, getKRStatusBadge } from "@/lib/missions";
import type { CheckInChartPoint } from "../utils/checkinReadModels";
import styles from "./CheckInEvolutionChart.module.css";

interface CheckInEvolutionChartProps {
  indicator: KeyResult;
  data: CheckInChartPoint[];
  height?: number;
}

const SAFE_FILL = "var(--color-caramel-50)";
const REF_STROKE = "var(--color-neutral-400)";
const TRAJECTORY_STROKE = "var(--color-neutral-400)";
const TODAY_STROKE = "var(--color-neutral-300)";
const LINE_COLOR = "var(--color-neutral-900)";
const AXIS_COLOR = "var(--color-neutral-500)";
const GRID_COLOR = "var(--color-caramel-200)";

const STATUS_DOT_COLOR: Record<KRStatus, string> = {
  on_track: "var(--color-green-500)",
  completed: "var(--color-green-500)",
  attention: "var(--color-yellow-500)",
  off_track: "var(--color-red-500)",
};

const CONFIDENCE_META: Record<ConfidenceLevel, { label: string; color: string }> = {
  high: { label: "Alta", color: "var(--color-green-500)" },
  medium: { label: "Média", color: "var(--color-yellow-500)" },
  low: { label: "Baixa", color: "var(--color-red-500)" },
  barrier: { label: "Bloqueado", color: "var(--color-red-700)" },
  deprioritized: { label: "Despriorizado", color: "var(--color-neutral-500)" },
};

function getUnit(kr: KeyResult): string {
  if (kr.unitLabel) return kr.unitLabel;
  if (kr.unit === "percent") return "%";
  return "";
}

function formatNumber(v: number): string {
  if (Number.isInteger(v)) return v.toLocaleString("pt-BR");
  return v.toFixed(1);
}

function formatValue(v: number, kr: KeyResult): string {
  if (kr.unit === "currency") return `R$ ${formatNumber(v)}`;
  const unit = getUnit(kr);
  const num = formatNumber(v);
  return unit ? `${num} ${unit}` : num;
}

function formatSignedValue(v: number, kr: KeyResult): string {
  const sign = v > 0 ? "+" : "";
  return `${sign}${formatValue(v, kr)}`;
}

function formatDateTick(ts: number): string {
  const d = new Date(ts);
  return `${String(d.getDate()).padStart(2, "0")}/${String(d.getMonth() + 1).padStart(2, "0")}`;
}

function interpolate(ts: number, t0: number, t1: number, v0: number, v1: number): number {
  if (ts <= t0) return v0;
  if (ts >= t1) return v1;
  return v0 + ((v1 - v0) * (ts - t0)) / (t1 - t0);
}

interface MergedPoint {
  x: number;
  value?: number | null;
  trajectory?: number | null;
  date?: string;
  createdAt?: string;
  authorName?: string;
  authorInitials?: string;
  confidence?: ConfidenceLevel | null;
  visualPreviousValue?: number | null;
  isCheckIn?: boolean;
}

interface Insight {
  context: string;
  tone: "good" | "bad" | "neutral";
}

function getInsight(
  indicator: KeyResult,
  currentValue: number,
  todayTs: number,
  startTs: number | null,
  endTs: number | null,
): Insight {
  const target = numVal(indicator.targetValue);
  const startValue = numVal(indicator.startValue);
  const low = numVal(indicator.lowThreshold);
  const high = numVal(indicator.highThreshold);
  const hasTarget = !!indicator.targetValue;
  const hasLow = !!indicator.lowThreshold;
  const hasHigh = !!indicator.highThreshold;

  switch (indicator.goalType) {
    case "reach":
    case "reduce": {
      if (!hasTarget) return { context: "", tone: "neutral" };
      const targetLabel =
        indicator.goalType === "reduce"
          ? `Meta ≤ ${formatValue(target, indicator)}`
          : `Meta ${formatValue(target, indicator)}`;
      if (startTs === null || endTs === null || todayTs < startTs || todayTs > endTs) {
        return { context: targetLabel, tone: "neutral" };
      }
      const idealNow = interpolate(todayTs, startTs, endTs, startValue, target);
      const delta = currentValue - idealNow;
      const ahead = indicator.goalType === "reach" ? delta >= 0 : delta <= 0;
      return {
        context: `${targetLabel} · ${formatSignedValue(delta, indicator)} vs ritmo ideal`,
        tone: Math.abs(delta) < 0.0001 ? "neutral" : ahead ? "good" : "bad",
      };
    }
    case "above": {
      if (!hasLow) return { context: "", tone: "neutral" };
      const slack = currentValue - low;
      return {
        context: `Mín ${formatValue(low, indicator)} · folga ${formatSignedValue(slack, indicator)}`,
        tone: slack >= 0 ? "good" : "bad",
      };
    }
    case "below": {
      if (!hasHigh) return { context: "", tone: "neutral" };
      const slack = high - currentValue;
      return {
        context: `Máx ${formatValue(high, indicator)} · folga ${formatSignedValue(slack, indicator)}`,
        tone: slack >= 0 ? "good" : "bad",
      };
    }
    case "between": {
      if (!hasLow || !hasHigh) return { context: "", tone: "neutral" };
      const inside = currentValue >= low && currentValue <= high;
      const range = `Faixa ${formatValue(low, indicator)}–${formatValue(high, indicator)}`;
      if (inside) return { context: `${range} · dentro`, tone: "good" };
      const out = currentValue < low ? low - currentValue : currentValue - high;
      const dir = currentValue < low ? "abaixo do mín" : "acima do máx";
      return { context: `${range} · ${formatValue(out, indicator)} ${dir}`, tone: "bad" };
    }
    default:
      return { context: "", tone: "neutral" };
  }
}

function formatDateLong(ts: number): string {
  const formatted = new Date(ts).toLocaleDateString("pt-BR", {
    weekday: "short",
    day: "2-digit",
    month: "short",
  });
  // Remove dots from abbreviated parts ("ter., 17 abr" → "ter, 17 abr")
  return formatted.replace(/\./g, "");
}

interface CustomTooltipProps {
  active?: boolean;
  payload?: { payload?: MergedPoint }[];
  indicator: KeyResult;
  startTs: number | null;
  endTs: number | null;
  startValue: number;
  target: number;
  showTrajectory: boolean;
}

function CustomTooltip({
  active,
  payload,
  indicator,
  startTs,
  endTs,
  startValue,
  target,
  showTrajectory,
}: CustomTooltipProps) {
  if (!active || !payload || payload.length === 0) return null;
  const entry = payload.find((p) => p.payload?.isCheckIn)?.payload;
  if (!entry || entry.value == null) return null;

  const currentValue = entry.value;
  const prev = entry.visualPreviousValue ?? null;
  const delta = prev !== null ? currentValue - prev : null;
  const pctDelta =
    delta !== null && prev !== null && prev !== 0 ? (delta / Math.abs(prev)) * 100 : null;
  const confidence = entry.confidence ? CONFIDENCE_META[entry.confidence] : null;

  const dateLabel = entry.createdAt ? formatDateLong(new Date(entry.createdAt).getTime()) : entry.date;

  let idealDelta: number | null = null;
  if (showTrajectory && entry.createdAt && startTs !== null && endTs !== null) {
    const ts = new Date(entry.createdAt).getTime();
    const ideal = interpolate(ts, startTs, endTs, startValue, target);
    idealDelta = currentValue - ideal;
  }

  function deltaColor(d: number): string {
    const gt = indicator.goalType;
    if (gt === "between" || gt === "survey") return "var(--color-neutral-500)";
    const increaseIsGood = gt === "reach" || gt === "above";
    const isGood = increaseIsGood ? d >= 0 : d <= 0;
    return isGood ? "var(--color-green-700)" : "var(--color-red-700)";
  }

  return (
    <div className={styles.tooltip}>
      <div className={styles.tooltipTopRow}>
        <span className={styles.tooltipDate}>{dateLabel}</span>
        {entry.authorInitials && (
          <span className={styles.tooltipAuthorChip}>{entry.authorInitials}</span>
        )}
      </div>

      <div className={styles.tooltipValue}>{formatValue(currentValue, indicator)}</div>

      {delta !== null && (
        <div className={styles.tooltipDeltaRow}>
          <span className={styles.tooltipDeltaLabel}>vs anterior</span>
          <span className={styles.tooltipDeltaValue} style={{ color: deltaColor(delta) }}>
            {delta === 0 ? (
              "sem variação"
            ) : (
              <>
                {formatSignedValue(delta, indicator)}
                {pctDelta !== null && (
                  <span className={styles.tooltipDeltaPct}>
                    {" "}
                    ({pctDelta >= 0 ? "+" : ""}
                    {pctDelta.toFixed(0)}%)
                  </span>
                )}
              </>
            )}
          </span>
        </div>
      )}

      {idealDelta !== null && (
        <div className={styles.tooltipDeltaRow}>
          <span className={styles.tooltipDeltaLabel}>vs ritmo ideal</span>
          <span className={styles.tooltipDeltaValue} style={{ color: deltaColor(idealDelta) }}>
            {Math.abs(idealDelta) < 0.01 ? "no ritmo" : formatSignedValue(idealDelta, indicator)}
          </span>
        </div>
      )}

      {confidence && (
        <div className={styles.tooltipFooter}>
          <span
            className={styles.tooltipConfidenceDot}
            style={{ backgroundColor: confidence.color }}
          />
          <span className={styles.tooltipConfidenceLabel}>
            Confiança {confidence.label.toLowerCase()}
          </span>
        </div>
      )}
    </div>
  );
}

export function CheckInEvolutionChart({ indicator, data, height = 200 }: CheckInEvolutionChartProps) {
  const target = numVal(indicator.targetValue);
  const startValue = numVal(indicator.startValue || indicator.currentValue);
  const low = numVal(indicator.lowThreshold);
  const high = numVal(indicator.highThreshold);
  const hasTarget = !!indicator.targetValue;
  const hasLow = !!indicator.lowThreshold;
  const hasHigh = !!indicator.highThreshold;

  const startTs = indicator.periodStart ? new Date(indicator.periodStart).getTime() : null;
  const endTs = indicator.periodEnd ? new Date(indicator.periodEnd).getTime() : null;
  const todayTs = Date.now();

  const showTrajectory =
    (indicator.goalType === "reach" || indicator.goalType === "reduce") &&
    hasTarget &&
    startTs !== null &&
    endTs !== null;

  // Build merged dataset
  const merged: MergedPoint[] = [];
  if (showTrajectory) {
    merged.push({ x: startTs!, trajectory: startValue });
  }
  for (const p of data) {
    const ts = p.createdAt ? new Date(p.createdAt).getTime() : todayTs;
    merged.push({
      x: ts,
      value: p.value,
      trajectory: showTrajectory ? interpolate(ts, startTs!, endTs!, startValue, target) : null,
      date: p.date,
      createdAt: p.createdAt,
      authorName: p.authorName,
      authorInitials: p.authorInitials,
      confidence: p.confidence,
      visualPreviousValue: p.visualPreviousValue,
      isCheckIn: true,
    });
  }
  if (showTrajectory) {
    merged.push({ x: endTs!, trajectory: target });
  }

  // Empty state: synthesize a start point
  if (data.length === 0 && !showTrajectory) {
    merged.push({
      x: startTs ?? todayTs,
      value: startValue,
      date: "Início",
      isCheckIn: false,
    });
  }

  merged.sort((a, b) => a.x - b.x);

  // Y domain
  const yPool: number[] = [];
  for (const p of merged) {
    if (typeof p.value === "number") yPool.push(p.value);
    if (typeof p.trajectory === "number") yPool.push(p.trajectory);
  }
  if (showTrajectory) yPool.push(startValue, target);
  if (indicator.goalType === "above" && hasLow) yPool.push(low);
  if (indicator.goalType === "below" && hasHigh) yPool.push(high);
  if (indicator.goalType === "between") {
    if (hasLow) yPool.push(low);
    if (hasHigh) yPool.push(high);
  }
  const yMinRaw = yPool.length > 0 ? Math.min(...yPool) : 0;
  const yMaxRaw = yPool.length > 0 ? Math.max(...yPool) : 100;
  const ySpan = yMaxRaw - yMinRaw;
  const yPad = ySpan === 0 ? Math.max(1, Math.abs(yMaxRaw) * 0.1) : ySpan * 0.1;
  const yMin = Math.floor(yMinRaw - yPad);
  const yMax = Math.ceil(yMaxRaw + yPad);

  // X domain — span the cycle when known, otherwise the data range
  const dataXs = merged.map((m) => m.x);
  const xMin = startTs ?? (dataXs[0] ?? todayTs);
  const xMax = endTs ?? (dataXs[dataXs.length - 1] ?? todayTs);

  // Header KPI strip
  const lastReal = data[data.length - 1] ?? null;
  const currentValue = lastReal?.value != null ? lastReal.value : startValue;
  const insight = getInsight(indicator, currentValue, todayTs, startTs, endTs);
  const lastRealTs = lastReal?.createdAt ? new Date(lastReal.createdAt).getTime() : null;
  const statusColor = STATUS_DOT_COLOR[indicator.status] ?? LINE_COLOR;

  function renderDot(props: { cx?: number; cy?: number; payload?: MergedPoint }) {
    const { cx, cy, payload } = props;
    if (cx == null || cy == null) return <g />;
    if (!payload?.isCheckIn) return <g />;
    const isLast = lastRealTs !== null && payload.x === lastRealTs;
    const fill = isLast ? statusColor : LINE_COLOR;
    const r = isLast ? 5 : 3;
    const stroke = isLast ? "var(--color-neutral-0)" : fill;
    const strokeWidth = isLast ? 2 : 0;
    return <circle cx={cx} cy={cy} r={r} fill={fill} stroke={stroke} strokeWidth={strokeWidth} />;
  }

  const insightToneClass =
    insight.tone === "good"
      ? styles.kpiContextGood
      : insight.tone === "bad"
        ? styles.kpiContextBad
        : styles.kpiContextNeutral;

  const showToday = startTs !== null && endTs !== null && todayTs >= startTs && todayTs <= endTs;

  return (
    <div className={styles.wrapper}>
      <div className={styles.kpiRow}>
        <div className={styles.kpiPrimary}>
          <span className={styles.kpiValue}>{formatNumber(currentValue)}</span>
          {getUnit(indicator) && <span className={styles.kpiUnit}>{getUnit(indicator)}</span>}
        </div>
        {insight.context && (
          <span className={`${styles.kpiContext} ${insightToneClass}`}>{insight.context}</span>
        )}
        <Badge color={getKRStatusBadge(indicator.status)} size="sm">
          {getKRStatusLabel(indicator.status)}
        </Badge>
      </div>
      <ResponsiveContainer width="100%" height={height}>
        <LineChart data={merged} margin={{ top: 8, right: 12, bottom: 0, left: -16 }}>
          <CartesianGrid strokeDasharray="3 3" stroke={GRID_COLOR} vertical={false} />
          <XAxis
            type="number"
            dataKey="x"
            domain={[xMin, xMax]}
            tickFormatter={formatDateTick}
            tick={{ fontFamily: "var(--font-label)", fontSize: 11, fill: AXIS_COLOR }}
            axisLine={false}
            tickLine={false}
            scale="time"
          />
          <YAxis
            tick={{ fontFamily: "var(--font-label)", fontSize: 11, fill: AXIS_COLOR }}
            axisLine={false}
            tickLine={false}
            domain={[yMin, yMax]}
            tickFormatter={(v: number) =>
              Number.isInteger(v) ? v.toLocaleString("pt-BR") : v.toFixed(1)
            }
          />
          <RechartsTooltip
            content={
              <CustomTooltip
                indicator={indicator}
                startTs={startTs}
                endTs={endTs}
                startValue={startValue}
                target={target}
                showTrajectory={showTrajectory}
              />
            }
            cursor={{ stroke: "var(--color-neutral-300)", strokeWidth: 1, strokeDasharray: "2 4" }}
            animationDuration={150}
            animationEasing="ease-out"
          />

          {indicator.goalType === "above" && hasLow && (
            <>
              <ReferenceArea
                y1={low}
                y2={yMax}
                fill={SAFE_FILL}
                fillOpacity={0.5}
                ifOverflow="extendDomain"
              />
              <ReferenceLine y={low} stroke={REF_STROKE} strokeDasharray="4 4" />
            </>
          )}
          {indicator.goalType === "below" && hasHigh && (
            <>
              <ReferenceArea
                y1={yMin}
                y2={high}
                fill={SAFE_FILL}
                fillOpacity={0.5}
                ifOverflow="extendDomain"
              />
              <ReferenceLine y={high} stroke={REF_STROKE} strokeDasharray="4 4" />
            </>
          )}
          {indicator.goalType === "between" && hasLow && hasHigh && (
            <>
              <ReferenceArea
                y1={low}
                y2={high}
                fill={SAFE_FILL}
                fillOpacity={0.5}
                ifOverflow="extendDomain"
              />
              <ReferenceLine y={low} stroke={REF_STROKE} strokeDasharray="4 4" />
              <ReferenceLine y={high} stroke={REF_STROKE} strokeDasharray="4 4" />
            </>
          )}

          {showToday && (
            <ReferenceLine x={todayTs} stroke={TODAY_STROKE} strokeDasharray="2 4" strokeWidth={1} />
          )}

          {showTrajectory && (
            <Line
              type="linear"
              dataKey="trajectory"
              stroke={TRAJECTORY_STROKE}
              strokeDasharray="5 5"
              strokeWidth={1.5}
              dot={false}
              activeDot={false}
              connectNulls
              isAnimationActive={false}
              name="Trajetória ideal"
            />
          )}

          <Line
            type="monotone"
            dataKey="value"
            stroke={LINE_COLOR}
            strokeWidth={2}
            dot={renderDot}
            activeDot={{ r: 6, stroke: "var(--color-neutral-0)", strokeWidth: 2 }}
            connectNulls
            isAnimationActive={false}
            name="Valor"
          />
        </LineChart>
      </ResponsiveContainer>
    </div>
  );
}
