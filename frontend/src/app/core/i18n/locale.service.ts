import { Injectable, LOCALE_ID, inject } from '@angular/core';

export interface AppLocale {
  code: string;
  label: string;
}

const LOCALE_STORAGE_KEY = 'wkg.locale';

/**
 * Manages the active UI locale. With `@angular/localize`, translations are compiled per locale
 * at build time and each locale is served under its own base href (e.g. `/ru-RU/`, `/ky-KG/`),
 * so switching reloads the app at the target locale's path.
 */
@Injectable({ providedIn: 'root' })
export class LocaleService {
  private readonly activeLocale = inject(LOCALE_ID);

  readonly locales: readonly AppLocale[] = [
    { code: 'ru-RU', label: 'Русский' },
    { code: 'ky-KG', label: 'Кыргызча' },
  ];

  get current(): string {
    return this.activeLocale;
  }

  switchTo(code: string): void {
    if (code === this.activeLocale) return;
    localStorage.setItem(LOCALE_STORAGE_KEY, code);
    window.location.reload();
  }
}
