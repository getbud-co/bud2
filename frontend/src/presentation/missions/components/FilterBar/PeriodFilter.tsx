import type { RefObject } from "react";
import { FilterDropdown, Radio, DatePicker } from "@getbud-co/buds";
import type { CalendarDate } from "@getbud-co/buds";
import { Plus, CaretRight } from "@phosphor-icons/react";

interface PresetPeriod {
  id: string;
  label: string;
  start: CalendarDate;
  end: CalendarDate;
}

interface PeriodFilterProps {
  open: boolean;
  onClose: () => void;
  anchorRef: RefObject<HTMLDivElement | null>;
  ignoreRefs: RefObject<HTMLDivElement | null>[];
  presetPeriods: PresetPeriod[];
  selectedPeriod: [CalendarDate | null, CalendarDate | null];
  setSelectedPeriod: React.Dispatch<
    React.SetStateAction<[CalendarDate | null, CalendarDate | null]>
  >;
  filterPeriodCustom: boolean;
  setFilterPeriodCustom: React.Dispatch<React.SetStateAction<boolean>>;
  filterPeriodCustomBtnRef: RefObject<HTMLButtonElement | null>;
}

export function PeriodFilter({
  open,
  onClose,
  anchorRef,
  ignoreRefs,
  presetPeriods,
  selectedPeriod,
  setSelectedPeriod,
  filterPeriodCustom,
  setFilterPeriodCustom,
  filterPeriodCustomBtnRef,
}: PeriodFilterProps) {
  return (
    <>
      <FilterDropdown
        open={open}
        onClose={() => {
          onClose();
          setFilterPeriodCustom(false);
        }}
        anchorRef={anchorRef}
        ignoreRefs={ignoreRefs}
      >
        <div className="flex flex-col p-1 max-h-80 overflow-y-auto">
          {presetPeriods.map((p) => {
            const isActive =
              selectedPeriod[0]?.year === p.start.year &&
              selectedPeriod[0]?.month === p.start.month &&
              selectedPeriod[0]?.day === p.start.day &&
              selectedPeriod[1]?.year === p.end.year &&
              selectedPeriod[1]?.month === p.end.month &&
              selectedPeriod[1]?.day === p.end.day;
            return (
              <button
                key={p.id}
                type="button"
                className={`flex items-center gap-2 w-full px-2 py-2 border-none bg-transparent rounded-[var(--radius-2xs)] font-[var(--font-label)] font-medium text-[var(--text-xs)] text-[var(--color-neutral-950)] cursor-pointer whitespace-nowrap transition-colors duration-[120ms] text-left hover:bg-[var(--color-caramel-100)] ${isActive ? "bg-[var(--color-caramel-50)]" : ""}`}
                onClick={() => {
                  setSelectedPeriod([p.start, p.end]);
                  onClose();
                  setFilterPeriodCustom(false);
                }}
              >
                <Radio checked={isActive} readOnly />
                <span>{p.label}</span>
              </button>
            );
          })}
        </div>
        <div className="border-t border-[var(--color-caramel-300)] p-1">
          <button
            ref={filterPeriodCustomBtnRef}
            type="button"
            className={`flex items-center gap-2 w-full px-2 py-2 border-none bg-transparent rounded-[var(--radius-2xs)] font-[var(--font-label)] font-medium text-[var(--text-xs)] text-[var(--color-neutral-950)] cursor-pointer whitespace-nowrap transition-colors duration-[120ms] text-left hover:bg-[var(--color-caramel-100)] ${filterPeriodCustom ? "bg-[var(--color-caramel-50)]" : ""}`}
            onClick={() => setFilterPeriodCustom((v) => !v)}
          >
            <Plus size={14} />
            <span>Período personalizado</span>
            <CaretRight
              size={12}
              className="ml-auto text-[var(--color-neutral-400)] flex-shrink-0"
            />
          </button>
        </div>
      </FilterDropdown>

      {/* Sub-panel: custom calendar */}
      <FilterDropdown
        open={open && filterPeriodCustom}
        onClose={() => setFilterPeriodCustom(false)}
        anchorRef={filterPeriodCustomBtnRef}
        placement="right-start"
        noOverlay
      >
        <div className="p-3">
          <DatePicker
            mode="range"
            value={selectedPeriod}
            onChange={(range: [CalendarDate | null, CalendarDate | null]) => {
              setSelectedPeriod(range);
              if (range[0] && range[1]) {
                onClose();
                setFilterPeriodCustom(false);
              }
            }}
          />
        </div>
      </FilterDropdown>
    </>
  );
}
