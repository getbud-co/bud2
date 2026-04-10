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
      { error: "Failed to fetch organization" },
      { status: response.status },
    );
  }

  const data = await response.json();

  const parsed = OrganizationResponseSchema.safeParse(data);
  if (!parsed.success) {
    console.warn(
      "[schema:organization] Type mismatch from backend:",
      parsed.error.issues,
    );
    return NextResponse.json(
      { error: "Unexpected response format from backend" },
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
