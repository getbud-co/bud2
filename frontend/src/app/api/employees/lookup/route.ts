import { getBudToken } from "@/lib/bud-token";
import { EmployeeLookupListResponseSchema } from "@/schemas/employee";
import { NextRequest, NextResponse } from "next/server";

export async function GET(request: NextRequest) {
  const apiUrl = process.env.BUD_API_URL;
  const token = await getBudToken();
  const tenantId = request.headers.get("X-Tenant-Id");
  const { searchParams } = request.nextUrl;

  const params = new URLSearchParams();
  const search = searchParams.get("search");
  if (search) params.set("search", search);

  const url = `${apiUrl}/api/employees/lookup${params.size > 0 ? `?${params}` : ""}`;

  const response = await fetch(url, {
    headers: {
      Authorization: `Bearer ${token}`,
      ...(tenantId ? { "X-Tenant-Id": tenantId } : {}),
    },
  });

  if (!response.ok) {
    return NextResponse.json(
      { error: "Erro ao buscar colaboradores" },
      { status: response.status },
    );
  }

  const data = await response.json();

  const parsed = EmployeeLookupListResponseSchema.safeParse(data);
  if (!parsed.success) {
    console.warn(
      "[schema:employee/lookup] Divergência de contrato com o backend:",
      parsed.error.issues,
    );
    return NextResponse.json(
      { error: "Formato de resposta inesperado do backend" },
      { status: 400 },
    );
  }

  return NextResponse.json(parsed.data);
}
