export interface Tag {
  id: string;
  organizationId: string;
  name: string;
  color: string;
  createdAt: string;
  updatedAt: string;
  deletedAt: string | null;
  /** Calculado em queries */
  linkedItems?: number;
}
