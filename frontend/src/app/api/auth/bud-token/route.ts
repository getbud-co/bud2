import { getBudToken } from "@/lib/bud-token";
import { NextResponse } from "next/server";

export async function POST() {
  const token = await getBudToken();
  return NextResponse.json({ token });
}
