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

export async function PATCH(
  request: NextRequest,
  { params }: { params: Promise<{ id: string }> },
) {
  const { id } = await params;
  const apiUrl = process.env.BUD_API_URL;
  const token = await getBudToken();
  const body = await request.json();

  const backendBody: Record<string, unknown> = {};
  if (body.name !== undefined) backendBody.name = body.name;
  if (body.type !== undefined) backendBody.cadence = CADENCE_REVERSE[body.type] ?? 3;
  if (body.startDate !== undefined) backendBody.startDate = body.startDate;
  if (body.endDate !== undefined) backendBody.endDate = body.endDate;
  if (body.status !== undefined) backendBody.status = STATUS_REVERSE[body.status] ?? 0;

  const tenantId = request.headers.get("X-Tenant-Id");

  const response = await fetch(`${apiUrl}/api/cycles/${id}`, {
    method: "PATCH",
    headers: {
      Authorization: `Bearer ${token}`,
      "Content-Type": "application/json",
      ...(tenantId ? { "X-Tenant-Id": tenantId } : {}),
    },
    body: JSON.stringify(backendBody),
  });

  if (!response.ok) {
    const error = await response.json().catch(() => ({ detail: "Unknown error" }));
    return NextResponse.json(error, { status: response.status });
  }

  const updated: Record<string, unknown> = await response.json();
  return NextResponse.json(mapCycle(updated));
}

export async function DELETE(
  request: NextRequest,
  { params }: { params: Promise<{ id: string }> },
) {
  const { id } = await params;
  const apiUrl = process.env.BUD_API_URL;
  const token = await getBudToken();

  const tenantId = request.headers.get("X-Tenant-Id");

  const response = await fetch(`${apiUrl}/api/cycles/${id}`, {
    method: "DELETE",
    headers: {
      Authorization: `Bearer ${token}`,
      ...(tenantId ? { "X-Tenant-Id": tenantId } : {}),
    },
  });

  if (!response.ok) {
    const error = await response.json().catch(() => ({ detail: "Unknown error" }));
    return NextResponse.json(error, { status: response.status });
  }

  return new NextResponse(null, { status: 204 });
}
