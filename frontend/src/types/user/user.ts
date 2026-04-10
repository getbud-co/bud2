export type UserStatus = "active" | "inactive" | "invited" | "suspended";

export type AuthProvider = "email" | "google" | "microsoft" | "saml";

export type Gender =
  | "feminino"
  | "masculino"
  | "nao-binario"
  | "prefiro-nao-dizer";

export interface User {
  id: string;
  organizationId: string;
  email: string;
  fullName: string;
  nickname: string | null;
  language: string;
  status: UserStatus;
  role: string;
  leaderId: string | null;
  isGlobalAdmin: boolean;
  teams: { id: string; name: string }[];
}
