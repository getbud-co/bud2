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
      { error: "Failed to fetch organizations" },
      { status: response.status },
    );
  }

  const content = await response.json();

  const parsed = OrganizationListResponseSchema.safeParse(content);
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

  return NextResponse.json(parsed.data.items);
}
