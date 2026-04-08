import { getBudToken } from "@/lib/bud-token";
import { NextResponse, type NextRequest } from "next/server";

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

  return NextResponse.json({
    id: data.id as string,
    name: data.name as string,
    logoUrl: (data.iconUrl as string | null) ?? null,
    plan: data.plan as string,
    contractStatus: data.contractStatus as string,
    createdAt: data.createdAt as string,
  });
}
