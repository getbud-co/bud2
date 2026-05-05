import { getBudToken } from "@/lib/bud-token";
import { NextRequest, NextResponse } from "next/server";
import {
  BackendMissionPagedResponseSchema,
} from "@/schemas/mission";

export async function GET(request: NextRequest) {
  const apiUrl = process.env.BUD_API_URL;
  const token = await getBudToken();
  const tenantId = request.headers.get("X-Tenant-Id");
  const { searchParams } = request.nextUrl;

  const params = new URLSearchParams({
    page: searchParams.get("page") ?? "1",
    pageSize: searchParams.get("pageSize") ?? "50",
  });

  const filter = searchParams.get("filter");
  if (filter) params.set("filter", filter);

  const search = searchParams.get("search");
  if (search) params.set("search", search);

  const response = await fetch(`${apiUrl}/api/missions?${params}`, {
    headers: {
      Authorization: `Bearer ${token}`,
      ...(tenantId ? { "X-Tenant-Id": tenantId } : {}),
    },
  });

  if (!response.ok) {
    return NextResponse.json(
      { error: "Erro ao buscar missões" },
      { status: response.status },
    );
  }

  const data = await response.json();

  const parsed = BackendMissionPagedResponseSchema.safeParse(data);
  if (!parsed.success) {
    console.warn(
      "[schema:mission] Divergência de contrato com o backend:",
      parsed.error.issues,
    );
    return NextResponse.json(
      { error: "Formato de resposta inesperado do backend" },
      { status: 400 },
    );
  }

  return NextResponse.json(parsed.data);
}
