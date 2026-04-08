import { getBudToken } from "@/lib/bud-token";
import { NextRequest, NextResponse } from "next/server";

const ROLE_MAP: Record<number, string> = {
  0: "colaborador",
  1: "lider",
  2: "admin",
};

function mapEmployee(raw: Record<string, unknown>) {
  const fullName = ((raw.fullName as string) ?? "").trim();
  const parts = fullName.split(" ").filter(Boolean);
  const initials =
    parts
      .map((p) => p[0] ?? "")
      .slice(0, 2)
      .join("")
      .toUpperCase() || null;
  const team = raw.team as { id: string; name: string } | null;

  return {
    id: raw.id,
    orgId: raw.organizationId,
    email: raw.email,
    fullName,
    initials,
    jobTitle: null,
    managerId: raw.leaderId ?? null,
    avatarUrl: null,
    nickname: null,
    birthDate: null,
    gender: null,
    phone: null,
    language: "pt-br",
    status: "active",
    invitedAt: null,
    activatedAt: null,
    lastLoginAt: null,
    authProvider: "email",
    authProviderId: null,
    createdAt: new Date().toISOString(),
    updatedAt: new Date().toISOString(),
    deletedAt: null,
    roleId: null,
    roleType: ROLE_MAP[raw.role as number] ?? "colaborador",
    teams: team ? [team.name] : [],
  };
}

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

  const response = await fetch(`${apiUrl}/api/employees?${params}`, {
    headers: {
      Authorization: `Bearer ${token}`,
      ...(tenantId ? { "X-Tenant-Id": tenantId } : {}),
    },
  });

  if (!response.ok) {
    return NextResponse.json(
      { error: "Failed to fetch employees" },
      { status: response.status },
    );
  }

  const data = await response.json();
  const items: Record<string, unknown>[] = data.items ?? [];
  return NextResponse.json(items.map(mapEmployee));
}
