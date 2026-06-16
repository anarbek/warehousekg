/// <reference types="@angular/localize" />

import { loadTranslations } from '@angular/localize';
import { bootstrapApplication } from '@angular/platform-browser';
import { appConfig } from './app/app.config';
import { App } from './app/app';
import { KY_TRANSLATIONS } from './app/core/i18n/ky-translations';

// Load Kyrgyz translations at runtime so language switching works in dev mode.
const storedLocale = localStorage.getItem('wkg.locale');
if (storedLocale === 'ky-KG') {
  loadTranslations(KY_TRANSLATIONS);
  ($localize as any).locale = 'ky-KG';
}

bootstrapApplication(App, appConfig)
  .catch((err) => console.error(err));
