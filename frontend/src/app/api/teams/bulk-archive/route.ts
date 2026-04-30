import { getBudToken } from "@/lib/bud-token";
import { NextRequest, NextResponse } from "next/server";
import { TeamBulkIdsSchema } from "@/schemas/team";

export async function POST(request: NextRequest) {
  const apiUrl = process.env.BUD_API_URL;
  const token = await getBudToken();
  const tenantId = request.headers.get("X-Tenant-Id");

  const body = await request.json();
  const parsed = TeamBulkIdsSchema.safeParse(body);
  if (!parsed.success) {
    return NextResponse.json({ error: "IDs inválidos" }, { status: 422 });
  }
  const ids = parsed.data;

  const response = await fetch(`${apiUrl}/api/teams/bulk-archive`, {
    method: "POST",
    headers: {
      Authorization: `Bearer ${token}`,
      "Content-Type": "application/json",
      ...(tenantId ? { "X-Tenant-Id": tenantId } : {}),
    },
    body: JSON.stringify(ids),
  });

  if (!response.ok) {
    const error = await response
      .json()
      .catch(() => ({ detail: "Erro desconhecido" }));
    return NextResponse.json(error, { status: response.status });
  }

  return new NextResponse(null, { status: 204 });
}
