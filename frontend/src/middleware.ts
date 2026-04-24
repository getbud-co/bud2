import { NextResponse } from "next/server";
import type { NextRequest } from "next/server";

// eslint-disable-next-line @typescript-eslint/no-unused-vars
export function middleware(_request: NextRequest) {
  return NextResponse.next();
}

// ── Auth middleware (disabled) ──────────────────────────────────────────────
// import { auth0 } from "./lib/auth0";
/*
export async function middleware(request: NextRequest) {
  
  const authResponse = await auth0.middleware(request);

  if (
    request.nextUrl.pathname.startsWith("/auth") ||
    request.nextUrl.pathname.startsWith("/invite") ||
    request.nextUrl.pathname.startsWith("/api/o/")
  ) {
    return authResponse;
  }

  const session = await auth0.getSession(request);

  if (!session) {
    const loginUrl = new URL("/auth/login", request.url);
    loginUrl.searchParams.set("returnTo", request.nextUrl.pathname);
    return NextResponse.redirect(loginUrl);
  }

  return authResponse;
  
}

export const config = {
  matcher: ["/((?!_next/static|_next/image|favicon.ico|.*\\..*).*)"],
};
*/
