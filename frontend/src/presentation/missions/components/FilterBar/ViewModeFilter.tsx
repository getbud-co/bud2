import type { RefObject } from "react";
import { FilterDropdown } from "@mdonangelo/bud-ds";
import { ListBullets, SquaresFour, Kanban } from "@phosphor-icons/react";

type ViewMode = "list" | "cards" | "kanban";

const VIEW_MODE_OPTIONS = [
  { id: "list", label: "Lista", icon: ListBullets },
  { id: "cards", label: "Cartões", icon: SquaresFour },
  { id: "kanban", label: "Kanban", icon: Kanban },
] as const;

interface ViewModeFilterProps {
  open: boolean;
  onClose: () => void;
  anchorRef: RefObject<HTMLButtonElement | null>;
  viewMode: ViewMode;
  setViewMode: React.Dispatch<React.SetStateAction<ViewMode>>;
}

export function ViewModeFilter({
  open,
  onClose,
  anchorRef,
  viewMode,
  setViewMode,
}: ViewModeFilterProps) {
  return (
    <FilterDropdown
      open={open}
      onClose={onClose}
      anchorRef={anchorRef}
      noOverlay
    >
      <div className="flex flex-col p-1 max-h-80 overflow-y-auto">
        {VIEW_MODE_OPTIONS.map((opt) => (
          <button
            key={opt.id}
            type="button"
            className={`flex items-center gap-2 w-full px-2 py-2 border-none bg-transparent rounded-[var(--radius-2xs)] font-[var(--font-label)] font-medium text-[var(--text-xs)] text-[var(--color-neutral-950)] cursor-pointer whitespace-nowrap transition-colors duration-[120ms] text-left hover:bg-[var(--color-caramel-100)] ${viewMode === opt.id ? "bg-[var(--color-caramel-50)]" : ""}`}
            onClick={() => {
              setViewMode(opt.id);
              onClose();
            }}
          >
            <opt.icon size={14} />
            <span>{opt.label}</span>
          </button>
        ))}
      </div>
    </FilterDropdown>
  );
}
