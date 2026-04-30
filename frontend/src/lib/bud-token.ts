import { cookies } from "next/headers";

const BUD_ADMIN_EMAIL = "admin@getbud.co";
export const BUD_TOKEN_COOKIE = "bud_token";
export const BUD_SESSION_META_COOKIE = "bud_session_meta";

// Renew the token 60 seconds before it actually expires
const TOKEN_REFRESH_BUFFER_SECONDS = 60;

export interface SessionMeta {
  email: string;
  displayName: string;
  employeeId: string;
  organizationId: string;
}

interface SessionPayload {
  token: string;
  email: string;
  displayName: string;
  employeeId: string;
  organizationId: string;
}

function getJwtExpiry(token: string): number | null {
  try {
    const payload = token.split(".")[1];
    if (!payload) return null;
    const decoded = JSON.parse(
      Buffer.from(payload, "base64url").toString("utf8"),
    ) as Record<string, unknown>;
    return typeof decoded.exp === "number" ? decoded.exp : null;
  } catch {
    return null;
  }
}

function isTokenValid(token: string): boolean {
  const exp = getJwtExpiry(token);
  if (exp === null) return false;
  return Date.now() / 1000 < exp - TOKEN_REFRESH_BUFFER_SECONDS;
}

async function fetchSession(): Promise<SessionPayload> {
  const apiUrl = process.env.BUD_API_URL;
  const loginResponse = await fetch(`${apiUrl}/api/sessions`, {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify({ email: BUD_ADMIN_EMAIL }),
  });

  if (!loginResponse.ok) {
    throw new Error(
      `Failed to authenticate with Bud API: ${loginResponse.status}`,
    );
  }

  return (await loginResponse.json()) as SessionPayload;
}

function cookieOptions(exp: number | null) {
  return {
    httpOnly: true,
    secure: process.env.NODE_ENV === "production",
    sameSite: "lax" as const,
    path: "/",
    ...(exp ? { expires: new Date(exp * 1000) } : {}),
  };
}

/**
 * Returns a valid Bearer token, reading from the `bud_token` cookie when
 * possible and only hitting /api/sessions when the stored token is missing
 * or about to expire.
 */
export async function getBudToken(): Promise<string> {
  const cookieStore = await cookies();
  const stored = cookieStore.get(BUD_TOKEN_COOKIE)?.value;

  if (stored && isTokenValid(stored)) {
    return stored;
  }

  const session = await fetchSession();
  const exp = getJwtExpiry(session.token);
  const opts = cookieOptions(exp);

  cookieStore.set(BUD_TOKEN_COOKIE, session.token, opts);
  cookieStore.set(
    BUD_SESSION_META_COOKIE,
    JSON.stringify({
      email: session.email,
      displayName: session.displayName,
      employeeId: session.employeeId,
      organizationId: session.organizationId,
    } satisfies SessionMeta),
    opts,
  );

  return session.token;
}

/**
 * Returns the session metadata saved alongside the token.
 * Triggers a token refresh (and meta save) if the cookie is missing.
 */
export async function getBudSessionMeta(): Promise<SessionMeta> {
  const cookieStore = await cookies();
  const raw = cookieStore.get(BUD_SESSION_META_COOKIE)?.value;

  if (raw) {
    return JSON.parse(raw) as SessionMeta;
  }

  // Token might be stale — re-auth to repopulate both cookies.
  await getBudToken();

  const refreshed = cookieStore.get(BUD_SESSION_META_COOKIE)?.value;
  if (!refreshed) {
    throw new Error("Failed to populate session metadata");
  }
  return JSON.parse(refreshed) as SessionMeta;
}
