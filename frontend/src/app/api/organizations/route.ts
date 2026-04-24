import { getBudToken } from "@/lib/bud-token";
import { NextResponse } from "next/server";
import { OrganizationListResponseSchema } from "@/schemas/organization";

export async function GET() {
  const apiUrl = process.env.BUD_API_URL;
  const token = await getBudToken();

  const response = await fetch(`${apiUrl}/api/organizations`, {
    headers: { Authorization: `Bearer ${token}` },
  });

  if (!response.ok) {
    return NextResponse.json(
      { error: "Erro ao buscar organizações" },
      { status: response.status },
    );
  }

  const content = await response.json();

  const parsed = OrganizationListResponseSchema.safeParse(content);
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

  return NextResponse.json(
    parsed.data.items.map((item) => ({
      id: item.id,
      name: item.name,
      cnpj: item.cnpj,
      logoUrl: item.iconUrl ?? null,
      plan: item.plan,
      contractStatus: item.contractStatus,
      createdAt: item.createdAt,
    })),
  );
}
