import { describe, it, expect, vi, beforeEach } from "vitest";
import { render, screen, fireEvent } from "@testing-library/react";
import type { Cycle } from "@/types";
import { CycleFormModal } from "../components/CycleFormModal";

vi.mock("@mdonangelo/bud-ds", () => ({
  Modal: ({ open, children }: { open: boolean; children: React.ReactNode }) =>
    open ? <div data-testid="modal">{children}</div> : null,
  ModalHeader: ({ title, onClose }: { title: string; onClose: () => void }) => (
    <div>
      <h2>{title}</h2>
      <button onClick={onClose}>Fechar</button>
    </div>
  ),
  ModalBody: ({ children }: { children: React.ReactNode }) => (
    <div>{children}</div>
  ),
  ModalFooter: ({ children }: { children: React.ReactNode }) => (
    <div>{children}</div>
  ),
  Button: ({
    children,
    disabled,
    onClick,
  }: {
    children: React.ReactNode;
    disabled?: boolean;
    onClick?: () => void;
  }) => (
    <button disabled={disabled} onClick={onClick}>
      {children}
    </button>
  ),
  Input: ({
    label,
    value,
    onChange,
  }: {
    label: string;
    value: string;
    onChange: React.ChangeEventHandler<HTMLInputElement>;
  }) => <input aria-label={label} value={value} onChange={onChange} />,
  Select: ({
    label,
    value,
    onChange,
    options,
  }: {
    label: string;
    value: string;
    onChange: (v: string) => void;
    options: { value: string; label: string }[];
  }) => (
    <select
      aria-label={label}
      value={value}
      onChange={(e) => onChange(e.target.value)}
    >
      {options.map((o) => (
        <option key={o.value} value={o.value}>
          {o.label}
        </option>
      ))}
    </select>
  ),
  DatePicker: ({
    onChange,
  }: {
    value: unknown;
    onChange: (v: unknown) => void;
  }) => (
    <div data-testid="datepicker">
      <button
        data-testid="set-dates"
        onClick={() =>
          onChange([
            { year: 2025, month: 1, day: 1 },
            { year: 2025, month: 12, day: 31 },
          ])
        }
      >
        Definir datas
      </button>
    </div>
  ),
  ChoiceBox: ({ value, title }: { value: string; title: string }) => (
    <div data-value={value}>{title}</div>
  ),
  ChoiceBoxGroup: ({ children }: { children: React.ReactNode }) => (
    <div>{children}</div>
  ),
}));

const baseProps = {
  open: true,
  editingCycle: null,
  onClose: vi.fn(),
  onSave: vi.fn(),
  isPending: false,
};

const mockCycle: Cycle = {
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
};

beforeEach(() => {
  vi.clearAllMocks();
});

// ─── Modo criação ───

describe("CycleFormModal — modo criação", () => {
  it("exibe título 'Novo ciclo'", () => {
    render(<CycleFormModal {...baseProps} />);
    expect(
      screen.getByRole("heading", { name: "Novo ciclo" }),
    ).toBeInTheDocument();
  });

  it("exibe botão 'Criar ciclo'", () => {
    render(<CycleFormModal {...baseProps} />);
    expect(
      screen.getByRole("button", { name: "Criar ciclo" }),
    ).toBeInTheDocument();
  });

  it("botão salvar desabilitado quando nome está vazio", () => {
    render(<CycleFormModal {...baseProps} />);
    expect(screen.getByRole("button", { name: "Criar ciclo" })).toBeDisabled();
  });

  it("botão salvar desabilitado quando datas não foram definidas", () => {
    render(<CycleFormModal {...baseProps} />);
    fireEvent.change(screen.getByLabelText("Nome do ciclo"), {
      target: { value: "Novo Ciclo" },
    });
    expect(screen.getByRole("button", { name: "Criar ciclo" })).toBeDisabled();
  });

  it("botão salvar habilitado após preencher nome e datas", () => {
    render(<CycleFormModal {...baseProps} />);
    fireEvent.change(screen.getByLabelText("Nome do ciclo"), {
      target: { value: "Novo Ciclo" },
    });
    fireEvent.click(screen.getByTestId("set-dates"));
    expect(
      screen.getByRole("button", { name: "Criar ciclo" }),
    ).not.toBeDisabled();
  });

  it("chama onSave com dados e datas convertidas para ISO", () => {
    render(<CycleFormModal {...baseProps} />);
    fireEvent.change(screen.getByLabelText("Nome do ciclo"), {
      target: { value: "Novo Ciclo" },
    });
    fireEvent.click(screen.getByTestId("set-dates"));
    fireEvent.click(screen.getByRole("button", { name: "Criar ciclo" }));
    expect(baseProps.onSave).toHaveBeenCalledWith(
      expect.objectContaining({
        name: "Novo Ciclo",
        startDate: "2025-01-01",
        endDate: "2025-12-31",
      }),
    );
  });

  it("não renderiza nada quando open=false", () => {
    render(<CycleFormModal {...baseProps} open={false} />);
    expect(screen.queryByTestId("modal")).not.toBeInTheDocument();
  });
});

// ─── Modo edição ───

describe("CycleFormModal — modo edição", () => {
  it("exibe título 'Editar ciclo'", () => {
    render(<CycleFormModal {...baseProps} editingCycle={mockCycle} />);
    expect(
      screen.getByRole("heading", { name: "Editar ciclo" }),
    ).toBeInTheDocument();
  });

  it("exibe botão 'Salvar'", () => {
    render(<CycleFormModal {...baseProps} editingCycle={mockCycle} />);
    expect(screen.getByRole("button", { name: "Salvar" })).toBeInTheDocument();
  });

  it("preenche campo nome com o ciclo sendo editado", () => {
    render(<CycleFormModal {...baseProps} editingCycle={mockCycle} />);
    expect(screen.getByLabelText("Nome do ciclo")).toHaveValue(mockCycle.name);
  });

  it("botão salvar habilitado quando ciclo tem dados válidos", () => {
    render(<CycleFormModal {...baseProps} editingCycle={mockCycle} />);
    expect(screen.getByRole("button", { name: "Salvar" })).not.toBeDisabled();
  });

  it("botão salvar desabilitado quando isPending=true", () => {
    render(
      <CycleFormModal
        {...baseProps}
        editingCycle={mockCycle}
        isPending={true}
      />,
    );
    expect(screen.getByRole("button", { name: "Salvar" })).toBeDisabled();
  });

  it("chama onClose ao clicar em Cancelar", () => {
    render(<CycleFormModal {...baseProps} editingCycle={mockCycle} />);
    fireEvent.click(screen.getByRole("button", { name: "Cancelar" }));
    expect(baseProps.onClose).toHaveBeenCalledOnce();
  });
});
