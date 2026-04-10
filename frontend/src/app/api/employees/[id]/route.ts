import { getBudToken } from "@/lib/bud-token";
import { NextRequest, NextResponse } from "next/server";
import { EmployeeResponseSchema } from "@/schemas/employee";

export async function PATCH(
  request: NextRequest,
  { params }: { params: Promise<{ id: string }> },
) {
  const { id } = await params;
  const apiUrl = process.env.BUD_API_URL;
  const token = await getBudToken();
  const tenantId = request.headers.get("X-Tenant-Id");
  const body = await request.json();

  const response = await fetch(`${apiUrl}/api/employees/${id}`, {
    method: "PATCH",
    headers: {
      "Content-Type": "application/json",
      Authorization: `Bearer ${token}`,
      ...(tenantId ? { "X-Tenant-Id": tenantId } : {}),
    },
    body: JSON.stringify(body),
  });

  if (!response.ok) {
    return NextResponse.json(
      { error: "Failed to update employee" },
      { status: response.status },
    );
  }

  const data = await response.json();

  const parsed = EmployeeResponseSchema.safeParse(data);
  if (!parsed.success) {
    console.warn(
      "EmployeeResponseSchema validation failed",
      parsed.error.issues,
    );
    return NextResponse.json(
      { error: "Invalid response from employees API" },
      { status: 400 },
    );
  }

  return NextResponse.json(parsed.data);
}

export async function DELETE(
  request: NextRequest,
  { params }: { params: Promise<{ id: string }> },
) {
  const { id } = await params;
  const apiUrl = process.env.BUD_API_URL;
  const token = await getBudToken();
  const tenantId = request.headers.get("X-Tenant-Id");

  const response = await fetch(`${apiUrl}/api/employees/${id}`, {
    method: "DELETE",
    headers: {
      Authorization: `Bearer ${token}`,
      ...(tenantId ? { "X-Tenant-Id": tenantId } : {}),
    },
  });

  if (!response.ok) {
    return NextResponse.json(
      { error: "Failed to delete employee" },
      { status: response.status },
    );
  }

  return new NextResponse(null, { status: 204 });
}
