import { Component } from '@angular/core';
import { MatIconModule } from '@angular/material/icon';
import { MatListModule } from '@angular/material/list';
import { RouterLink, RouterLinkActive } from '@angular/router';

interface NavItem {
  route: string;
  icon: string;
  label: string;
}

@Component({
  selector: 'app-sidenav',
  imports: [MatListModule, MatIconModule, RouterLink, RouterLinkActive],
  templateUrl: './sidenav.html',
  styleUrl: './sidenav.scss',
})
export class Sidenav {
  protected readonly items: NavItem[] = [
    { route: '/dashboard', icon: 'dashboard', label: $localize`:@@nav.dashboard:Панель управления` },
    { route: '/inventory', icon: 'inventory_2', label: $localize`:@@nav.inventory:Товары` },
  ];
}
