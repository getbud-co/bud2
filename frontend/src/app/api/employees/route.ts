import { getBudToken } from "@/lib/bud-token";
import { NextRequest, NextResponse } from "next/server";
import {
  EmployeeListResponseSchema,
  EmployeeResponseSchema,
} from "@/schemas/employee";

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

  const parsed = EmployeeListResponseSchema.safeParse(data);
  if (!parsed.success) {
    console.warn(
      "EmployeeListResponseSchema validation failed",
      parsed.error.issues,
    );
    return NextResponse.json(
      { error: "Invalid response from employees API" },
      { status: 400 },
    );
  }
  return NextResponse.json(parsed.data.items);
}

export async function POST(request: NextRequest) {
  const apiUrl = process.env.BUD_API_URL;
  const token = await getBudToken();
  const tenantId = request.headers.get("X-Tenant-Id");
  const body = await request.json();

  const response = await fetch(`${apiUrl}/api/employees`, {
    method: "POST",
    headers: {
      "Content-Type": "application/json",
      Authorization: `Bearer ${token}`,
      ...(tenantId ? { "X-Tenant-Id": tenantId } : {}),
    },
    body: JSON.stringify(body),
  });

  if (!response.ok) {
    return NextResponse.json(
      { error: "Failed to create employee" },
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

  return NextResponse.json(parsed.data, { status: 201 });
}
