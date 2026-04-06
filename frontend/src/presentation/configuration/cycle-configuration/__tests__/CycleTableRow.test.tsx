import { describe, it, expect, vi, beforeEach } from "vitest";
import { render, screen, fireEvent } from "@testing-library/react";
import type { Cycle } from "@/types";
import { CycleTableRow } from "../components/CycleTableRow";

vi.mock("@mdonangelo/bud-ds", () => ({
  TableRow: ({ children }: { children: React.ReactNode }) => (
    <div data-testid="table-row">{children}</div>
  ),
  TableCell: ({
    children,
    isCheckbox,
  }: {
    children?: React.ReactNode;
    isCheckbox?: boolean;
  }) => (isCheckbox ? null : <div>{children}</div>),
  Badge: ({
    children,
    color,
  }: {
    children: React.ReactNode;
    color: string;
  }) => <span data-color={color}>{children}</span>,
}));

vi.mock("@phosphor-icons/react", () => ({
  PencilSimple: () => null,
  Trash: () => null,
  Play: () => null,
  Stop: () => null,
}));

vi.mock("@/components/table/RowActionsPopover", () => ({
  RowActionsPopover: ({
    items,
  }: {
    items: { id: string; label: string; onClick: () => void }[];
  }) => (
    <ul data-testid="row-actions">
      {items.map((item) => (
        <li key={item.id}>
          <button onClick={item.onClick}>{item.label}</button>
        </li>
      ))}
    </ul>
  ),
}));

vi.mock("../utils", () => ({
  formatDateDisplay: (iso: string) => `display:${iso}`,
}));

const baseProps = {
  isPopoverOpen: true,
  onEdit: vi.fn(),
  onDelete: vi.fn(),
  onToggleStatus: vi.fn(),
  onPopoverToggle: vi.fn(),
  onPopoverClose: vi.fn(),
};

function makeCycle(overrides: Partial<Cycle> = {}): Cycle {
  return {
    id: "c1",
    orgId: "org-1",
    name: "Q1 2025",
    type: "quarterly",
    startDate: "2025-01-01",
    endDate: "2025-03-31",
    status: "active",
    okrDefinitionDeadline: null,
    midReviewDate: null,
    createdAt: "2025-01-01T00:00:00Z",
    updatedAt: "2025-01-01T00:00:00Z",
    ...overrides,
  };
}

beforeEach(() => {
  vi.clearAllMocks();
});

// ─── Ação de toggle de status ───

describe("CycleTableRow — ação de toggle de status", () => {
  it("exibe 'Encerrar' quando status é 'active'", () => {
    render(
      <CycleTableRow {...baseProps} cycle={makeCycle({ status: "active" })} />,
    );
    expect(
      screen.getByRole("button", { name: "Encerrar" }),
    ).toBeInTheDocument();
    expect(
      screen.queryByRole("button", { name: "Ativar" }),
    ).not.toBeInTheDocument();
  });

  it("exibe 'Ativar' quando status é 'ended'", () => {
    render(
      <CycleTableRow {...baseProps} cycle={makeCycle({ status: "ended" })} />,
    );
    expect(screen.getByRole("button", { name: "Ativar" })).toBeInTheDocument();
    expect(
      screen.queryByRole("button", { name: "Encerrar" }),
    ).not.toBeInTheDocument();
  });

  it("exibe 'Ativar' quando status é 'planning'", () => {
    render(
      <CycleTableRow
        {...baseProps}
        cycle={makeCycle({ status: "planning" })}
      />,
    );
    expect(screen.getByRole("button", { name: "Ativar" })).toBeInTheDocument();
  });

  it("chama onToggleStatus com o ciclo ao clicar em 'Encerrar'", () => {
    const cycle = makeCycle({ status: "active" });
    render(<CycleTableRow {...baseProps} cycle={cycle} />);
    fireEvent.click(screen.getByRole("button", { name: "Encerrar" }));
    expect(baseProps.onToggleStatus).toHaveBeenCalledWith(cycle);
  });

  it("chama onToggleStatus com o ciclo ao clicar em 'Ativar'", () => {
    const cycle = makeCycle({ status: "ended" });
    render(<CycleTableRow {...baseProps} cycle={cycle} />);
    fireEvent.click(screen.getByRole("button", { name: "Ativar" }));
    expect(baseProps.onToggleStatus).toHaveBeenCalledWith(cycle);
  });
});

// ─── Ações Editar e Excluir ───

describe("CycleTableRow — ações Editar e Excluir", () => {
  it("chama onEdit com o ciclo ao clicar em 'Editar'", () => {
    const cycle = makeCycle();
    render(<CycleTableRow {...baseProps} cycle={cycle} />);
    fireEvent.click(screen.getByRole("button", { name: "Editar" }));
    expect(baseProps.onEdit).toHaveBeenCalledWith(cycle);
  });

  it("chama onDelete com o ciclo ao clicar em 'Excluir'", () => {
    const cycle = makeCycle();
    render(<CycleTableRow {...baseProps} cycle={cycle} />);
    fireEvent.click(screen.getByRole("button", { name: "Excluir" }));
    expect(baseProps.onDelete).toHaveBeenCalledWith(cycle);
  });
});

// ─── Exibição de dados ───

describe("CycleTableRow — exibição", () => {
  it("exibe o nome do ciclo", () => {
    render(
      <CycleTableRow
        {...baseProps}
        cycle={makeCycle({ name: "Semestre 1" })}
      />,
    );
    expect(screen.getByText("Semestre 1")).toBeInTheDocument();
  });

  it("exibe label do tipo corretamente", () => {
    render(
      <CycleTableRow {...baseProps} cycle={makeCycle({ type: "quarterly" })} />,
    );
    expect(screen.getByText("Trimestral")).toBeInTheDocument();
  });

  it("exibe label do status via badge", () => {
    render(
      <CycleTableRow {...baseProps} cycle={makeCycle({ status: "active" })} />,
    );
    expect(screen.getByText("Ativo")).toBeInTheDocument();
  });

  it("passa as datas por formatDateDisplay", () => {
    render(
      <CycleTableRow
        {...baseProps}
        cycle={makeCycle({ startDate: "2025-01-01", endDate: "2025-03-31" })}
      />,
    );
    expect(screen.getByText("display:2025-01-01")).toBeInTheDocument();
    expect(screen.getByText("display:2025-03-31")).toBeInTheDocument();
  });

  it("badge usa a cor correta para status 'active'", () => {
    render(
      <CycleTableRow {...baseProps} cycle={makeCycle({ status: "active" })} />,
    );
    expect(screen.getByText("Ativo")).toHaveAttribute("data-color", "success");
  });

  it("badge usa a cor correta para status 'ended'", () => {
    render(
      <CycleTableRow {...baseProps} cycle={makeCycle({ status: "ended" })} />,
    );
    expect(screen.getByText("Encerrado")).toHaveAttribute(
      "data-color",
      "neutral",
    );
  });
});
