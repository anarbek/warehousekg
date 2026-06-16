import { Component, OnInit, inject } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { AuthService } from '../../../core/auth/auth.service';
import { AuthResponse } from '../../../core/auth/auth.models';

@Component({
  selector: 'app-auth-callback',
  imports: [],
  template: `<p i18n="@@login.authenticating">Выполняется вход…</p>`,
  styles: [
    `
      :host {
        display: flex;
        justify-content: center;
        align-items: center;
        height: 100vh;
        font-size: 1.1rem;
      }
    `,
  ],
})
export class AuthCallback implements OnInit {
  private readonly route = inject(ActivatedRoute);
  private readonly auth = inject(AuthService);
  private readonly router = inject(Router);

  ngOnInit(): void {
    const p = this.route.snapshot.queryParamMap;

    const accessToken = p.get('accessToken');
    const refreshToken = p.get('refreshToken');
    const userId = p.get('userId');
    const userName = p.get('userName');
    const expiresAt = p.get('expiresAt');
    const tenantId = p.get('tenantId');

    if (!accessToken || !refreshToken || !userId || !userName || !tenantId || !expiresAt) {
      void this.router.navigate(['/login'], {
        queryParams: { error: 'oauth_failed' },
      });
      return;
    }

    const response: AuthResponse = {
      accessToken,
      refreshToken,
      userId,
      userName,
      email: p.get('email') || null,
      tenantId,
      roles: p.get('roles') ? p.get('roles')!.split(',').filter(Boolean) : [],
      accessTokenExpiresAtUtc: expiresAt,
    };

    this.auth.loginFromExternal(response);
    void this.router.navigate(['/dashboard']);
  }
}
