import { Injectable, signal } from '@angular/core';

export interface ResourcePermissions {
  canRead: boolean;
  canWrite: boolean;
  canDelete: boolean;
}

/**
 * Shared service that holds the current user's effective permissions per resource.
 * Populated once on app init by the sidenav (or auth flow) and consumed by any
 * component that needs to conditionally show/hide buttons.
 */
@Injectable({ providedIn: 'root' })
export class PermissionsService {
  /** Map of resource name → permissions, e.g. { warehouses: { canRead: true, canWrite: true, canDelete: false } } */
  readonly resources = signal<Record<string, ResourcePermissions>>({});
  readonly roles = signal<string[]>([]);

  /** Set all permissions at once (called by sidenav on init). */
  setAll(resources: Record<string, ResourcePermissions>, roles: string[]): void {
    this.resources.set(resources);
    this.roles.set(roles);
  }

  /** Convenience: whether the user can delete the given resource. */
  canDelete(resource: string): boolean {
    if (this.roles().includes('Admin')) return true;
    return this.resources()[resource]?.canDelete === true;
  }

  /** Convenience: whether the user can write the given resource. */
  canWrite(resource: string): boolean {
    if (this.roles().includes('Admin')) return true;
    return this.resources()[resource]?.canWrite === true;
  }

  /** Convenience: whether the user can read the given resource. */
  canRead(resource: string): boolean {
    // Admin bypasses all permission checks
    if (this.roles().includes('Admin')) return true;
    return this.resources()[resource]?.canRead === true;
  }
}
