import { DatePicker, FilterDropdown, Radio } from "@getbud-co/buds";
import type { CalendarDate } from "@getbud-co/buds";
import { useRef, useState } from "react";
import { CaretRight, Plus } from "@phosphor-icons/react";
import { useCycles } from "@/presentation/missions/hooks/useCycles";

interface PeriodFilterProps {
  isOpen: boolean;
  periodChipRef: React.RefObject<HTMLDivElement | null>;
  ignoreChipRefs: React.RefObject<HTMLDivElement | null>[];
  selectedPeriod: [CalendarDate | null, CalendarDate | null];
  onClose: () => void;
  onChange: (range: [CalendarDate | null, CalendarDate | null]) => void;
}

export function PeriodFilter({
  isOpen,
  periodChipRef,
  ignoreChipRefs,
  selectedPeriod,
  onClose,
  onChange,
}: PeriodFilterProps) {
  const { data: cycles = [] } = useCycles();
  const [customOpen, setCustomOpen] = useState(false);
  const customBtnRef = useRef<HTMLButtonElement>(null);

  function handleClose() {
    setCustomOpen(false);
    onClose();
  }

  return (
    <>
      <FilterDropdown
        open={isOpen}
        onClose={handleClose}
        anchorRef={periodChipRef}
        ignoreRefs={ignoreChipRefs}
      >
        <div className="flex flex-col max-h-[320px] p-[var(--sp-3xs)] overflow-y-auto">
          {cycles.map((p) => {
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
                className={`flex items-center gap-[var(--sp-2xs)] w-full p-[var(--sp-2xs)] border-none bg-transparent rounded-[var(--radius-2xs)] [font-family:var(--font-label)] font-medium text-[var(--text-xs)] text-[var(--color-neutral-950)] cursor-pointer whitespace-nowrap transition-colors duration-[120ms] ease-in-out text-left hover:bg-[var(--color-caramel-100)] ${isActive ? "bg-[var(--color-caramel-100)]" : ""}`}
                onClick={() => {
                  onChange([p.start, p.end]);
                  handleClose();
                }}
              >
                <Radio checked={isActive} readOnly />
                <span>{p.label}</span>
              </button>
            );
          })}
        </div>
        <div className="border-t border-[var(--color-caramel-300)] p-[var(--sp-3xs)]">
          <button
            ref={customBtnRef}
            type="button"
            className={`flex items-center gap-[var(--sp-2xs)] w-full p-[var(--sp-2xs)] border-none bg-transparent rounded-[var(--radius-2xs)] [font-family:var(--font-label)] font-medium text-[var(--text-xs)] text-[var(--color-neutral-950)] cursor-pointer whitespace-nowrap transition-colors duration-[120ms] ease-in-out text-left hover:bg-[var(--color-caramel-100)] ${customOpen ? "bg-[var(--color-caramel-100)]" : ""}`}
            onClick={() => setCustomOpen((v) => !v)}
          >
            <Plus size={14} />
            <span>Período personalizado</span>
            <CaretRight
              size={12}
              className="ml-auto text-[var(--color-neutral-400)] shrink-0"
            />
          </button>
        </div>
      </FilterDropdown>

      <FilterDropdown
        open={isOpen && customOpen}
        onClose={() => setCustomOpen(false)}
        anchorRef={customBtnRef}
        placement="right-start"
        noOverlay
      >
        <div className="p-[var(--sp-xs)]">
          <DatePicker
            mode="range"
            value={selectedPeriod}
            onChange={(range: [CalendarDate | null, CalendarDate | null]) => {
              onChange(range);
              if (range[0] && range[1]) {
                handleClose();
              }
            }}
          />
        </div>
      </FilterDropdown>
    </>
  );
}
