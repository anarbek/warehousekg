/**
 * Static application configuration. In a real deployment these would come from
 * environment files or a runtime config endpoint.
 */
export const AppSettings = {
  /** Base URL of the WarehouseKG API (see backend `/api/v1`). */
  apiBaseUrl: '/api/v1',

  /** Fallback tenant id used for the `X-Tenant-Id` header until multi-tenant onboarding exists. */
  defaultTenantId: '00000000-0000-0000-0000-000000000000',
} as const;
