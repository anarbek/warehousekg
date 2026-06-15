import { Injectable } from '@angular/core';
import { AppSettings } from '../config/app-settings';
import { AuthResponse, CurrentUser } from './auth.models';

const ACCESS_TOKEN_KEY = 'wkg.accessToken';
const ACCESS_EXPIRY_KEY = 'wkg.accessTokenExpiresAt';
const REFRESH_TOKEN_KEY = 'wkg.refreshToken';
const TENANT_KEY = 'wkg.tenantId';
const USER_KEY = 'wkg.user';

/**
 * Thin, dependency-free wrapper over localStorage for auth artifacts. Kept separate from
 * AuthService so the HTTP interceptor can read tokens without depending on HttpClient
 * (which would create a circular dependency in the interceptor chain).
 */
@Injectable({ providedIn: 'root' })
export class TokenStorageService {
  get accessToken(): string | null {
    return localStorage.getItem(ACCESS_TOKEN_KEY);
  }

  get refreshToken(): string | null {
    return localStorage.getItem(REFRESH_TOKEN_KEY);
  }

  /** Tenant id for the `X-Tenant-Id` header; falls back to the configured default. */
  get tenantId(): string {
    return localStorage.getItem(TENANT_KEY) ?? AppSettings.defaultTenantId;
  }

  get user(): CurrentUser | null {
    const raw = localStorage.getItem(USER_KEY);
    return raw ? (JSON.parse(raw) as CurrentUser) : null;
  }

  get isAccessTokenValid(): boolean {
    const token = this.accessToken;
    const expiry = localStorage.getItem(ACCESS_EXPIRY_KEY);
    if (!token || !expiry) {
      return false;
    }
    return new Date(expiry).getTime() > Date.now();
  }

  persist(response: AuthResponse): void {
    localStorage.setItem(ACCESS_TOKEN_KEY, response.accessToken);
    localStorage.setItem(ACCESS_EXPIRY_KEY, response.accessTokenExpiresAtUtc);
    localStorage.setItem(REFRESH_TOKEN_KEY, response.refreshToken);
    localStorage.setItem(TENANT_KEY, response.tenantId);
    localStorage.setItem(
      USER_KEY,
      JSON.stringify({
        userId: response.userId,
        userName: response.userName,
        email: response.email,
        tenantId: response.tenantId,
        roles: response.roles,
      } satisfies CurrentUser),
    );
  }

  clear(): void {
    [ACCESS_TOKEN_KEY, ACCESS_EXPIRY_KEY, REFRESH_TOKEN_KEY, TENANT_KEY, USER_KEY].forEach((key) =>
      localStorage.removeItem(key),
    );
  }
}
