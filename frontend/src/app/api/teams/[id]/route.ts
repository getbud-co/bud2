import { getBudToken } from "@/lib/bud-token";
import { NextRequest, NextResponse } from "next/server";
import { capitalize, mapTeam } from "@/lib/api/team-mapper";
import { BackendTeamResponseSchema } from "@/schemas/team";

export async function DELETE(
  request: NextRequest,
  { params }: { params: Promise<{ id: string }> },
) {
  const { id } = await params;
  const apiUrl = process.env.BUD_API_URL;
  const token = await getBudToken();
  const tenantId = request.headers.get("X-Tenant-Id");

  const response = await fetch(`${apiUrl}/api/teams/${id}`, {
    method: "DELETE",
    headers: {
      Authorization: `Bearer ${token}`,
      ...(tenantId ? { "X-Tenant-Id": tenantId } : {}),
    },
  });

  if (!response.ok) {
    const error = await response
      .json()
      .catch(() => ({ detail: "Erro desconhecido" }));
    return NextResponse.json(error, { status: response.status });
  }

  return new NextResponse(null, { status: 204 });
}

export async function PATCH(
  request: NextRequest,
  { params }: { params: Promise<{ id: string }> },
) {
  const { id } = await params;
  const apiUrl = process.env.BUD_API_URL;
  const token = await getBudToken();
  const tenantId = request.headers.get("X-Tenant-Id");
  const body = await request.json();

  const backendBody: Record<string, unknown> = {};
  if (body.name !== undefined) backendBody.name = body.name;
  if (body.description !== undefined)
    backendBody.description = body.description;
  if (body.color !== undefined)
    backendBody.color = capitalize(body.color as string);
  if (body.status !== undefined)
    backendBody.status = capitalize(body.status as string);
  if (body.leaderId !== undefined) backendBody.leaderId = body.leaderId;
  if (body.parentTeamId !== undefined)
    backendBody.parentTeamId = body.parentTeamId;

  const response = await fetch(`${apiUrl}/api/teams/${id}`, {
    method: "PATCH",
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
  return NextResponse.json(mapTeam(parsed.data));
}
