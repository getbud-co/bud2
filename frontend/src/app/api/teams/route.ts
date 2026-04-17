import { getBudToken } from "@/lib/bud-token";
import { NextRequest, NextResponse } from "next/server";
import { capitalize, mapTeam } from "@/lib/api/team-mapper";
import {
  BackendTeamListResponseSchema,
  BackendTeamResponseSchema,
} from "@/schemas/team";

export async function GET(request: NextRequest) {
  const apiUrl = process.env.BUD_API_URL;
  const token = await getBudToken();
  const tenantId = request.headers.get("X-Tenant-Id");
  const { searchParams } = request.nextUrl;

  const params = new URLSearchParams({
    page: searchParams.get("page") ?? "1",
    pageSize: searchParams.get("pageSize") ?? "100",
  });

  const search = searchParams.get("search");
  if (search) params.set("search", search);

  const response = await fetch(`${apiUrl}/api/teams?${params}`, {
    headers: {
      Authorization: `Bearer ${token}`,
      ...(tenantId ? { "X-Tenant-Id": tenantId } : {}),
    },
  });

  if (!response.ok) {
    return NextResponse.json(
      { error: "Erro ao buscar times" },
      { status: response.status },
    );
  }

  const data = await response.json();
  const parsed = BackendTeamListResponseSchema.safeParse(data);
  if (!parsed.success) {
    console.warn(
      "[schema:team] Divergência de contrato com o backend:",
      parsed.error.issues,
    );
    return NextResponse.json(
      { error: "Formato de resposta inesperado do backend" },
      { status: 400 },
    );
  }
  return NextResponse.json(parsed.data.items.map(mapTeam));
}

export async function POST(request: NextRequest) {
  const apiUrl = process.env.BUD_API_URL;
  const token = await getBudToken();
  const tenantId = request.headers.get("X-Tenant-Id");
  const body = await request.json();

  const backendBody = {
    name: body.name,
    description: body.description ?? null,
    color: capitalize(body.color as string),
    organizationId: body.organizationId,
    leaderId: body.leaderId,
    parentTeamId: body.parentTeamId ?? null,
  };

  const response = await fetch(`${apiUrl}/api/teams`, {
    method: "POST",
    headers: {
      Authorization: `Bearer ${token}`,
      "Content-Type": "application/json",
      ...(tenantId ? { "X-Tenant-Id": tenantId } : {}),
    },
    body: JSON.stringify(backendBody),
  });

  if (!response.ok) {
    const error = await response
      .json()
      .catch(() => ({ detail: "Erro desconhecido" }));
    return NextResponse.json(error, { status: response.status });
  }

  const data = await response.json();
  const parsed = BackendTeamResponseSchema.safeParse(data);
  if (!parsed.success) {
    console.warn(
      "[schema:team] Divergência de contrato com o backend:",
      parsed.error.issues,
    );
    return NextResponse.json(
      { error: "Formato de resposta inesperado do backend" },
      { status: 400 },
    );
  }
  return NextResponse.json(mapTeam(parsed.data), { status: 201 });
}
