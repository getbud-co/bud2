import { getBudToken } from "@/lib/bud-token";
import { NextRequest, NextResponse } from "next/server";
import type { Team, TeamMember } from "@/types";

interface BackendEmployee {
  id: string;
  fullName: string;
  email: string;
}

interface BackendTeam {
  id: string;
  name: string;
  description: string | null;
  color: string;
  status: string;
  organizationId: string;
  parentTeamId: string | null;
  leaderId: string | null;
  createdAt: string;
  updatedAt: string;
  deletedAt: string | null;
  employees: BackendEmployee[];
}

function capitalize(value: string): string {
  return value.charAt(0).toUpperCase() + value.slice(1);
}

function toInitials(fullName: string): string {
  return fullName
    .trim()
    .split(" ")
    .filter(Boolean)
    .map((p) => p[0] ?? "")
    .slice(0, 2)
    .join("")
    .toUpperCase();
}

function mapTeam(raw: BackendTeam): Team {
  const members: TeamMember[] = raw.employees.map((emp) => ({
    teamId: raw.id,
    userId: emp.id,
    roleInTeam: emp.id === raw.leaderId ? "leader" : "member",
    joinedAt: raw.createdAt,
    user: {
      id: emp.id,
      fullName: emp.fullName,
      initials: toInitials(emp.fullName),
      jobTitle: null,
      avatarUrl: null,
    },
  }));

  return {
    id: raw.id,
    orgId: raw.organizationId,
    name: raw.name,
    description: raw.description,
    color: raw.color.toLowerCase() as Team["color"],
    status: raw.status.toLowerCase() as Team["status"],
    leaderId: raw.leaderId,
    parentTeamId: raw.parentTeamId,
    createdAt: raw.createdAt,
    updatedAt: raw.updatedAt,
    deletedAt: raw.deletedAt,
    members,
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

  const response = await fetch(`${apiUrl}/api/teams?${params}`, {
    headers: {
      Authorization: `Bearer ${token}`,
      ...(tenantId ? { "X-Tenant-Id": tenantId } : {}),
    },
  });

  if (!response.ok) {
    return NextResponse.json(
      { error: "Failed to fetch teams" },
      { status: response.status },
    );
  }

  const data = await response.json();
  const items: BackendTeam[] = data.items ?? [];
  return NextResponse.json(items.map(mapTeam));
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
      .catch(() => ({ detail: "Unknown error" }));
    return NextResponse.json(error, { status: response.status });
  }

  const created = (await response.json()) as BackendTeam;
  return NextResponse.json(mapTeam(created), { status: 201 });
}
