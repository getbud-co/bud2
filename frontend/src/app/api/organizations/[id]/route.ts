import { getBudToken } from "@/lib/bud-token";
import { NextResponse, type NextRequest } from "next/server";
import { OrganizationResponseSchema } from "@/schemas/organization";

export async function GET(
  _req: NextRequest,
  { params }: { params: Promise<{ id: string }> },
) {
  const { id } = await params;
  const apiUrl = process.env.BUD_API_URL;
  const token = await getBudToken();

  const response = await fetch(`${apiUrl}/api/organizations/${id}`, {
    headers: { Authorization: `Bearer ${token}` },
  });

  if (!response.ok) {
    return NextResponse.json(
      { error: "Erro ao buscar organização" },
      { status: response.status },
    );
  }

  const data = await response.json();

  const parsed = OrganizationResponseSchema.safeParse(data);
  if (!parsed.success) {
    console.warn(
      "[schema:organization] Divergência de contrato com o backend:",
      parsed.error.issues,
    );
    return NextResponse.json(
      { error: "Formato de resposta inesperado do backend" },
      { status: 400 },
    );
  }

  return NextResponse.json({
    id: parsed.data.id,
    name: parsed.data.name,
    cnpj: parsed.data.cnpj,
    logoUrl: parsed.data.iconUrl ?? null,
    plan: parsed.data.plan,
    contractStatus: parsed.data.contractStatus,
    createdAt: parsed.data.createdAt,
  });
}
