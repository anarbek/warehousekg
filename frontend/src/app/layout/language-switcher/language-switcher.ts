import { Component, inject } from '@angular/core';
import { DxSelectBoxModule } from 'devextreme-angular';
import { LocaleService } from '../../core/i18n/locale.service';

@Component({
  selector: 'app-language-switcher',
  imports: [DxSelectBoxModule],
  templateUrl: './language-switcher.html',
  styleUrl: './language-switcher.scss',
})
export class LanguageSwitcher {
  protected readonly locale = inject(LocaleService);

  protected readonly langItems = this.locale.locales.map((l) => ({
    code: l.code,
    text: l.label,
  }));

  protected onLangChange(e: any): void {
    if (e.value) {
      this.locale.switchTo(e.value);
    }
  }
}
