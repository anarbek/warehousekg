import { Component, OnInit, inject, signal } from '@angular/core';
import { forkJoin } from 'rxjs';
import { DxDataGridModule, DxButtonModule, DxSelectBoxModule, DxTextBoxModule, DxProgressBarModule, DxPopupModule, DxCheckBoxModule } from 'devextreme-angular';
import { RouterLink } from '@angular/router';
import { UsersService, UserDto } from '../services/users.service';
import { PersonnelService } from '../../personnel/services/personnel.service';
import { ErrorToastService } from '../../../core/services/error-toast.service';

@Component({
  selector: 'app-users',
  standalone: true,
  imports: [DxDataGridModule, DxButtonModule, DxSelectBoxModule, DxTextBoxModule, DxProgressBarModule, DxPopupModule, DxCheckBoxModule, RouterLink],
  templateUrl: './users.html',
  styleUrl: './users.scss',
})
export class UsersComponent implements OnInit {
  private readonly svc = inject(UsersService);
  private readonly personnel = inject(PersonnelService);
  private readonly toast = inject(ErrorToastService);

  protected readonly users = signal<UserDto[]>([]);
  protected readonly roles = signal<string[]>([]);
  protected readonly employees = signal<{ id: string; name: string }[]>([]);
  protected readonly loading = signal(false);
  protected readonly saving = signal(false);
  protected readonly err = signal<string | null>(null);

  // Create form
  protected readonly newUser = signal({ userName: '', email: '', password: '', roles: [] as string[], employeeId: null as string | null });
  protected readonly showCreate = signal(false);

  // Change password
  protected readonly pwdTarget = signal<UserDto | null>(null);
  protected readonly newPassword = signal('');
  protected readonly showPasswordPopup = signal(false);

  ngOnInit(): void {
    this.load();
  }

  load(): void {
    this.loading.set(true);
    forkJoin({
      users: this.svc.getUsers(),
      roles: this.svc.getAvailableRoles(),
      employees: this.personnel.getEmployees(),
    }).subscribe({
      next: ({ users, roles, employees }) => {
        this.users.set(users);
        this.roles.set(roles);
        this.employees.set(employees.map(e => ({ id: e.id, name: `${e.lastName} ${e.firstName}` })));
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
        this.newUser.set({ userName: '', email: '', password: '', roles: [], employeeId: null });
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

  openChangePassword(user: UserDto): void {
    this.pwdTarget.set(user);
    this.newPassword.set('');
    this.showPasswordPopup.set(true);
  }

  closePasswordPopup(): void {
    this.pwdTarget.set(null);
    this.showPasswordPopup.set(false);
  }

  changePassword(): void {
    const user = this.pwdTarget();
    if (!user || !this.newPassword()) return;
    this.saving.set(true);
    this.svc.changePassword(user.id, { newPassword: this.newPassword() }).subscribe({
      next: () => {
        this.saving.set(false);
        this.closePasswordPopup();
      },
      error: (e) => {
        this.toast.showSave(e);
        this.saving.set(false);
      },
    });
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

  toggleUserRole(user: UserDto, role: string): void {
    const idx = user.roles.indexOf(role);
    if (idx >= 0) user.roles.splice(idx, 1);
    else user.roles.push(role);
    this.onRoleChanged(user);
  }

  setNewUserEmployeeId(employeeId: string | null): void {
    const u = this.newUser();
    this.newUser.set({ ...u, employeeId });
  }

  onEmployeeChanged(user: UserDto, employeeId: string | null): void {
    this.svc.linkEmployee(user.id, employeeId).subscribe();
  }
}
