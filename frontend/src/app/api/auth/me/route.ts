import { getBudSessionMeta } from "@/lib/bud-token";
import { NextResponse } from "next/server";

export async function GET() {
  const meta = await getBudSessionMeta();

  const fullName = meta.displayName.trim();
  const parts = fullName.split(" ").filter(Boolean);
  const initials = parts
    .map((p) => p[0] ?? "")
    .slice(0, 2)
    .join("")
    .toUpperCase();

  return NextResponse.json({
    id: meta.employeeId,
    email: meta.email,
    fullName,
    initials,
    organizationId: meta.organizationId,
  });
}
