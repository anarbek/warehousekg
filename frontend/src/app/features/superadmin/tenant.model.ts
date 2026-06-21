export interface Tenant {
  id: string;
  name: string;
  slug: string;
  contactEmail: string | null;
  contactPhone: string | null;
  defaultCurrency: string;
  status: TenantStatus;
  maxUsers: number;
  enabledModules: string | null;
  createdAt: string;
  userCount: number;
  adminUserId: string | null;
  adminUserName: string | null;
}

export enum TenantStatus {
  Active = 1,
  Suspended = 2,
  Trial = 3,
}

export interface CreateTenantRequest {
  name: string;
  slug: string;
  contactEmail?: string | null;
  contactPhone?: string | null;
  defaultCurrency: string;
  adminUserName: string;
  adminEmail: string;
  adminPassword: string;
  seedDemoData: boolean;
}

export interface UpdateTenantRequest {
  name: string;
  contactEmail?: string | null;
  contactPhone?: string | null;
  defaultCurrency: string;
  maxUsers: number;
  enabledModules?: string | null;
}
