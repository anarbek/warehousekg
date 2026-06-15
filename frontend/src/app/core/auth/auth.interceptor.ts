import { HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { AppSettings } from '../config/app-settings';
import { TokenStorageService } from './token-storage.service';

/**
 * Attaches the JWT bearer token and the `X-Tenant-Id` header to API requests.
 * Only same-origin API calls are decorated so third-party requests stay untouched.
 */
export const authInterceptor: HttpInterceptorFn = (req, next) => {
  const isApiRequest = req.url.startsWith(AppSettings.apiBaseUrl) || req.url.includes('/api/');
  if (!isApiRequest) {
    return next(req);
  }

  const storage = inject(TokenStorageService);
  let headers = req.headers.set('X-Tenant-Id', storage.tenantId);

  const token = storage.accessToken;
  if (token) {
    headers = headers.set('Authorization', `Bearer ${token}`);
  }

  return next(req.clone({ headers }));
};
