export interface LoginRequest {
  /** Sent to the backend as `userName`. */
  email: string;
  password: string;
}

export interface AuthResponse {
  userId: string;
  userName: string;
  email: string | null;
  tenantId: string;
  roles: string[];
  accessToken: string;
  accessTokenExpiresAtUtc: string;
  refreshToken: string;
}

export interface CurrentUser {
  userId: string;
  userName: string;
  email: string | null;
  tenantId: string;
  roles: string[];
}
