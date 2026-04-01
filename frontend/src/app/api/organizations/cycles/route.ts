import { getBudToken } from "@/lib/bud-token";
import { NextRequest, NextResponse } from "next/server";

const CADENCE_MAP: Record<number, string> = {
  0: "quarterly",
  1: "semi_annual",
  2: "annual",
  3: "custom",
};

const STATUS_MAP: Record<number, string> = {
  0: "active",
  1: "ended",
  2: "archived",
};

const CADENCE_REVERSE: Record<string, number> = {
  quarterly: 0,
  semi_annual: 1,
  annual: 2,
  custom: 3,
};

const STATUS_REVERSE: Record<string, number> = {
  active: 0,
  planning: 0,
  review: 0,
  ended: 1,
  archived: 2,
};

function mapCycle(raw: Record<string, unknown>) {
  return {
    id: raw.id,
    orgId: raw.organizationId,
    name: raw.name,
    type: CADENCE_MAP[raw.cadence as number] ?? "custom",
    startDate: raw.startDate,
    endDate: raw.endDate,
    status: STATUS_MAP[raw.status as number] ?? "active",
    okrDefinitionDeadline: null,
    midReviewDate: null,
    createdAt: raw.createdAt ?? new Date().toISOString(),
    updatedAt: raw.updatedAt ?? new Date().toISOString(),
  };
}

export async function GET(request: NextRequest) {
  const apiUrl = process.env.BUD_API_URL;
  const token = await getBudToken();
  const tenantId = request.headers.get("X-Tenant-Id");

  const response = await fetch(`${apiUrl}/api/cycles`, {
    headers: {
      Authorization: `Bearer ${token}`,
      ...(tenantId ? { "X-Tenant-Id": tenantId } : {}),
    },
  });

  if (!response.ok) {
    return NextResponse.json(
      { error: "Failed to fetch cycles" },
      { status: response.status },
    );
  }

  const items: Record<string, unknown>[] = await response.json();
  return NextResponse.json(items.map(mapCycle));
}

export async function POST(request: NextRequest) {
  const apiUrl = process.env.BUD_API_URL;
  const token = await getBudToken();
  const tenantId = request.headers.get("X-Tenant-Id");
  const body = await request.json();

  const backendBody = {
    name: body.name,
    cadence: CADENCE_REVERSE[body.type] ?? 3,
    startDate: body.startDate,
    endDate: body.endDate,
    status: STATUS_REVERSE[body.status] ?? 0,
  };

  const response = await fetch(`${apiUrl}/api/cycles`, {
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

  const created: Record<string, unknown> = await response.json();
  return NextResponse.json(mapCycle(created), { status: 201 });
}
