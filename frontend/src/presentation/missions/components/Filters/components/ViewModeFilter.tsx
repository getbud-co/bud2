import { useRef, useState } from "react";
import { Button, FilterDropdown } from "@getbud-co/buds";
import { CaretDown, Kanban, ListBullets, SquaresFour } from "@phosphor-icons/react";

type ViewMode = "list" | "cards" | "kanban";

const OPTIONS = [
  { id: "list" as const, label: "Lista", icon: ListBullets },
  { id: "cards" as const, label: "Cartões", icon: SquaresFour },
  { id: "kanban" as const, label: "Kanban", icon: Kanban },
];

const LABELS: Record<ViewMode, string> = {
  list: "Vendo em lista",
  cards: "Vendo em cartões",
  kanban: "Vendo em kanban",
};

interface ViewModeFilterProps {
  viewMode: ViewMode;
  onChange: (mode: ViewMode) => void;
}

export function ViewModeFilter({ viewMode, onChange }: ViewModeFilterProps) {
  const [open, setOpen] = useState(false);
  const btnRef = useRef<HTMLButtonElement>(null);

  const CurrentIcon =
    viewMode === "list"
      ? ListBullets
      : viewMode === "cards"
        ? SquaresFour
        : Kanban;

  return (
    <>
      <Button
        ref={btnRef}
        variant="secondary"
        size="md"
        leftIcon={CurrentIcon}
        rightIcon={CaretDown}
        onClick={() => setOpen((v) => !v)}
      >
        {LABELS[viewMode]}
      </Button>
      <FilterDropdown
        open={open}
        onClose={() => setOpen(false)}
        anchorRef={btnRef}
        noOverlay
      >
        <div className="flex flex-col p-[var(--sp-3xs)]">
          {OPTIONS.map((opt) => (
            <button
              key={opt.id}
              type="button"
              className={`flex items-center gap-[var(--sp-2xs)] w-full p-[var(--sp-2xs)] border-none bg-transparent rounded-[var(--radius-2xs)] [font-family:var(--font-label)] font-medium text-[var(--text-xs)] text-[var(--color-neutral-950)] cursor-pointer whitespace-nowrap transition-colors duration-[120ms] ease-in-out text-left hover:bg-[var(--color-caramel-100)] ${viewMode === opt.id ? "bg-[var(--color-caramel-50)]" : ""}`}
              onClick={() => {
                onChange(opt.id);
                setOpen(false);
              }}
            >
              <opt.icon size={14} />
              <span>{opt.label}</span>
            </button>
          ))}
        </div>
      </FilterDropdown>
    </>
  );
}
