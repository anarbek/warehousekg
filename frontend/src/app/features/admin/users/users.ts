import { Component, OnInit, inject, signal } from '@angular/core';
import { forkJoin } from 'rxjs';
import { DxDataGridModule, DxButtonModule, DxSelectBoxModule, DxTextBoxModule, DxProgressBarModule } from 'devextreme-angular';
import { RouterLink } from '@angular/router';
import { UsersService, UserDto } from '../services/users.service';
import { ErrorToastService } from '../../../core/services/error-toast.service';

@Component({
  selector: 'app-users',
  standalone: true,
  imports: [DxDataGridModule, DxButtonModule, DxSelectBoxModule, DxTextBoxModule, DxProgressBarModule, RouterLink],
  templateUrl: './users.html',
  styleUrl: './users.scss',
})
export class UsersComponent implements OnInit {
  private readonly svc = inject(UsersService);
  private readonly toast = inject(ErrorToastService);

  protected readonly users = signal<UserDto[]>([]);
  protected readonly roles = signal<string[]>([]);
  protected readonly loading = signal(false);
  protected readonly saving = signal(false);
  protected readonly err = signal<string | null>(null);

  // Create form
  protected readonly newUser = signal({ userName: '', email: '', password: '', roles: [] as string[] });
  protected readonly showCreate = signal(false);

  ngOnInit(): void {
    this.load();
  }

  load(): void {
    this.loading.set(true);
    forkJoin({
      users: this.svc.getUsers(),
      roles: this.svc.getAvailableRoles(),
    }).subscribe({
      next: ({ users, roles }) => {
        this.users.set(users);
        this.roles.set(roles);
        this.loading.set(false);
      },
      error: (e) => {
        this.toast.showLoad(e);
        this.loading.set(false);
      },
    });
  }

  createUser(): void {
    this.saving.set(true);
    this.svc.createUser(this.newUser()).subscribe({
      next: () => {
        this.saving.set(false);
        this.showCreate.set(false);
        this.newUser.set({ userName: '', email: '', password: '', roles: [] });
        this.load();
      },
      error: (e) => {
        this.toast.showSave(e);
        this.saving.set(false);
      },
    });
  }

  onRoleChanged(user: UserDto): void {
    this.svc.updateRoles(user.id, user.roles).subscribe();
  }

  deleteUser(user: UserDto): void {
    if (!confirm($localize`:@@common.confirmDelete:Вы уверены, что хотите удалить запись?`)) return;
    this.svc.deleteUser(user.id).subscribe({ next: () => this.load() });
  }

  toggleRoleFor(role: string): void {
    const roles = this.newUser().roles;
    const idx = roles.indexOf(role);
    if (idx >= 0) roles.splice(idx, 1);
    else roles.push(role);
    this.newUser.set({ ...this.newUser(), roles: [...roles] });
  }

  hasRole(role: string): boolean {
    return this.newUser().roles.includes(role);
  }
}
