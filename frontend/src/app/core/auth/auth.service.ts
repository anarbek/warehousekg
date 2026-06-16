import { HttpClient } from '@angular/common/http';
import { Injectable, computed, inject, signal } from '@angular/core';
import { Observable, tap } from 'rxjs';
import { AppSettings } from '../config/app-settings';
import { AuthResponse, CurrentUser } from './auth.models';
import { TokenStorageService } from './token-storage.service';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly http = inject(HttpClient);
  private readonly storage = inject(TokenStorageService);

  private readonly authBase = `${AppSettings.apiBaseUrl}/auth`;

  private readonly currentUserSignal = signal<CurrentUser | null>(this.storage.user);

  /** Reactive view of the signed-in user (null when anonymous). */
  readonly currentUser = this.currentUserSignal.asReadonly();

  readonly isAuthenticated = computed(() => this.currentUserSignal() !== null);

  /** Synchronous check used by the route guard (token present and not expired). */
  isLoggedIn(): boolean {
    return this.storage.isAccessTokenValid && this.currentUserSignal() !== null;
  }

  login(email: string, password: string): Observable<AuthResponse> {
    // Backend authenticates by `userName`; the login form collects an email.
    return this.http
      .post<AuthResponse>(`${this.authBase}/login`, { userName: email, password })
      .pipe(tap((response) => this.handleAuthSuccess(response)));
  }

  refresh(): Observable<AuthResponse> {
    return this.http
      .post<AuthResponse>(`${this.authBase}/refresh`, { refreshToken: this.storage.refreshToken })
      .pipe(tap((response) => this.handleAuthSuccess(response)));
  }

  /**
   * Initiates Google OAuth sign-in by navigating the browser to the backend challenge endpoint.
   * The backend redirects to Google, and after approval Google calls the backend callback,
   * which issues a JWT and redirects to /auth/callback in the Angular app.
   */
  loginWithGoogle(): void {
    window.location.href = '/api/v1/auth/google';
  }

  /** Called by the OAuth callback route to hydrate the auth state from external-provider tokens. */
  loginFromExternal(response: AuthResponse): void {
    this.handleAuthSuccess(response);
  }

  logout(): void {
    this.storage.clear();
    this.currentUserSignal.set(null);
  }

  private handleAuthSuccess(response: AuthResponse): void {
    this.storage.persist(response);
    this.currentUserSignal.set({
      userId: response.userId,
      userName: response.userName,
      email: response.email,
      tenantId: response.tenantId,
      roles: response.roles,
    });
  }
}
