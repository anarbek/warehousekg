import { Component, inject, signal } from '@angular/core';
import { DxDrawerModule, DxToolbarModule } from 'devextreme-angular';
import { Router, RouterOutlet } from '@angular/router';
import { AuthService } from '../../core/auth/auth.service';
import { Header } from '../header/header';
import { Sidenav } from '../sidenav/sidenav';

@Component({
  selector: 'app-shell',
  imports: [DxDrawerModule, DxToolbarModule, RouterOutlet, Header, Sidenav],
  templateUrl: './shell.html',
  styleUrl: './shell.scss',
})
export class Shell {
  private readonly auth = inject(AuthService);
  private readonly router = inject(Router);

  protected readonly sidenavOpen = signal(true);

  toggleSidenav(): void {
    this.sidenavOpen.update((open) => !open);
  }

  logout(): void {
    this.auth.logout();
    void this.router.navigate(['/login']);
  }
}
