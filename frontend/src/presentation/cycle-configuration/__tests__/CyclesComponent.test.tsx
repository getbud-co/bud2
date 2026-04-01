import { describe, it, expect, vi, beforeEach } from "vitest";
import { render, screen, fireEvent } from "@testing-library/react";
import { useQuery, useMutation } from "@tanstack/react-query";
import type { Cycle } from "@/types";
import { CyclesComponent } from "../index";

// ─── Mocks de módulos externos ───

vi.mock("@tanstack/react-query", () => ({
  useQuery: vi.fn(),
  useMutation: vi.fn(() => ({
    mutate: vi.fn(),
    mutateAsync: vi.fn(),
    isPending: false,
  })),
  useQueryClient: vi.fn(() => ({ invalidateQueries: vi.fn() })),
}));

vi.mock("@/contexts/OrganizationContext", () => ({
  useOrganization: () => ({ activeOrgId: "org-1" }),
}));

vi.mock("@/hooks/useDataTable", () => ({
  useDataTable: () => ({
    selectedRows: new Set(),
    clearSelection: vi.fn(),
    handleSelectRow: vi.fn(),
    handleSelectAll: vi.fn(),
  }),
}));

vi.mock("@mdonangelo/bud-ds", () => ({
  Table: ({ children }: { children: React.ReactNode }) => <div>{children}</div>,
  TableContent: ({ children }: { children: React.ReactNode }) => (
    <div>{children}</div>
  ),
  TableHead: ({ children }: { children: React.ReactNode }) => (
    <div>{children}</div>
  ),
  TableBody: ({ children }: { children: React.ReactNode }) => (
    <div>{children}</div>
  ),
  TableRow: ({ children }: { children: React.ReactNode }) => (
    <div>{children}</div>
  ),
  TableHeaderCell: ({ children }: { children?: React.ReactNode }) => (
    <div>{children}</div>
  ),
  TableBulkActions: () => null,
  Button: ({
    children,
    onClick,
  }: {
    children: React.ReactNode;
    onClick?: () => void;
  }) => <button onClick={onClick}>{children}</button>,
  Alert: () => null,
  toast: { success: vi.fn(), error: vi.fn() },
}));

vi.mock("@phosphor-icons/react", () => ({ Trash: () => null }));

// ─── Mocks dos sub-componentes ───

vi.mock("../components/CyclesLoadingState", () => ({
  CyclesLoadingState: () => <div data-testid="loading-state" />,
}));

vi.mock("../components/CyclesErrorState", () => ({
  CyclesErrorState: () => <div data-testid="error-state" />,
}));

vi.mock("../components/CyclesTableHeader", () => ({
  CyclesTableHeader: ({
    onSearch,
    onCreateClick,
    count,
  }: {
    onSearch: (v: string) => void;
    onCreateClick: () => void;
    count: number;
    isLoading: boolean;
    search: string;
  }) => (
    <div>
      <input
        data-testid="search-input"
        onChange={(e) => onSearch(e.target.value)}
      />
      <span data-testid="filtered-count">{count}</span>
      <button onClick={onCreateClick}>Novo ciclo</button>
    </div>
  ),
}));

vi.mock("../components/CycleTableRow", () => ({
  CycleTableRow: ({ cycle }: { cycle: Cycle }) => (
    <div data-testid="cycle-row">{cycle.name}</div>
  ),
}));

vi.mock("../components/CycleFormModal", () => ({
  CycleFormModal: () => null,
}));

vi.mock("../components/DeleteConfirmModal", () => ({
  DeleteConfirmModal: () => null,
}));

vi.mock("../../layout/page-header", () => ({
  PageHeader: () => null,
}));

// ─── Helpers ───

const mockUseQuery = vi.mocked(useQuery);
const mockUseMutation = vi.mocked(useMutation);

function makeQueryResult(
  overrides: Partial<{ data: Cycle[]; isLoading: boolean; isError: boolean }>,
) {
  return {
    data: [],
    isLoading: false,
    isError: false,
    ...overrides,
  } as ReturnType<typeof useQuery>;
}

function makeCycle(id: string, name: string): Cycle {
  return {
    id,
    orgId: "org-1",
    name,
    type: "quarterly",
    startDate: "2025-01-01",
    endDate: "2025-03-31",
    status: "active",
    okrDefinitionDeadline: null,
    midReviewDate: null,
    createdAt: "2025-01-01T00:00:00Z",
    updatedAt: "2025-01-01T00:00:00Z",
  };
}

beforeEach(() => {
  vi.clearAllMocks();
  mockUseMutation.mockReturnValue({
    mutate: vi.fn(),
    mutateAsync: vi.fn(),
    isPending: false,
  } as unknown as ReturnType<typeof useMutation>);
});

// ─── Estados de loading e erro ───

describe("CyclesComponent — estados de loading e erro", () => {
  it("renderiza o estado de carregamento quando isLoading=true", () => {
    mockUseQuery.mockReturnValue(makeQueryResult({ isLoading: true }));
    render(<CyclesComponent />);
    expect(screen.getByTestId("loading-state")).toBeInTheDocument();
  });

  it("renderiza o estado de erro quando isError=true", () => {
    mockUseQuery.mockReturnValue(makeQueryResult({ isError: true }));
    render(<CyclesComponent />);
    expect(screen.getByTestId("error-state")).toBeInTheDocument();
  });

  it("não renderiza loading nem error com dados disponíveis", () => {
    mockUseQuery.mockReturnValue(makeQueryResult({ data: [] }));
    render(<CyclesComponent />);
    expect(screen.queryByTestId("loading-state")).not.toBeInTheDocument();
    expect(screen.queryByTestId("error-state")).not.toBeInTheDocument();
  });
});

// ─── Filtragem por search ───

describe("CyclesComponent — filtragem por search", () => {
  const cycles = [
    makeCycle("1", "Q1 2025"),
    makeCycle("2", "Semestre 1"),
    makeCycle("3", "Anual 2025"),
  ];

  beforeEach(() => {
    mockUseQuery.mockReturnValue(makeQueryResult({ data: cycles }));
  });

  it("exibe todos os ciclos quando search está vazio", () => {
    render(<CyclesComponent />);
    expect(screen.getAllByTestId("cycle-row")).toHaveLength(3);
  });

  it("filtra ciclos pelo nome (case-insensitive)", () => {
    render(<CyclesComponent />);
    fireEvent.change(screen.getByTestId("search-input"), {
      target: { value: "2025" },
    });
    const rows = screen.getAllByTestId("cycle-row");
    expect(rows).toHaveLength(2);
    expect(rows[0]).toHaveTextContent("Q1 2025");
    expect(rows[1]).toHaveTextContent("Anual 2025");
  });

  it("filtra de forma case-insensitive", () => {
    render(<CyclesComponent />);
    fireEvent.change(screen.getByTestId("search-input"), {
      target: { value: "semestre" },
    });
    expect(screen.getAllByTestId("cycle-row")).toHaveLength(1);
    expect(screen.getByText("Semestre 1")).toBeInTheDocument();
  });

  it("exibe zero linhas quando search não encontra correspondência", () => {
    render(<CyclesComponent />);
    fireEvent.change(screen.getByTestId("search-input"), {
      target: { value: "xyz-inexistente" },
    });
    expect(screen.queryAllByTestId("cycle-row")).toHaveLength(0);
  });

  it("atualiza o contador de ciclos filtrados", () => {
    render(<CyclesComponent />);
    expect(screen.getByTestId("filtered-count")).toHaveTextContent("3");
    fireEvent.change(screen.getByTestId("search-input"), {
      target: { value: "2025" },
    });
    expect(screen.getByTestId("filtered-count")).toHaveTextContent("2");
  });
});
