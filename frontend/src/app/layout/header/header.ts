import { Component, output } from '@angular/core';
import { DxToolbarModule, DxButtonModule } from 'devextreme-angular';
import { LanguageSwitcher } from '../language-switcher/language-switcher';

@Component({
  selector: 'app-header',
  imports: [DxToolbarModule, DxButtonModule, LanguageSwitcher],
  templateUrl: './header.html',
  styleUrl: './header.scss',
})
export class Header {
  readonly menuToggle = output<void>();
  readonly logout = output<void>();
}
