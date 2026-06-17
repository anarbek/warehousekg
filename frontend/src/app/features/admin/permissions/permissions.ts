import { Component, OnInit, inject, signal } from '@angular/core';
import { forkJoin } from 'rxjs';
import { DxDataGridModule, DxCheckBoxModule, DxButtonModule, DxProgressBarModule } from 'devextreme-angular';
import { AdminService, TenantPermission } from '../services/admin.service';

interface MatrixRow {
  roleName: string;
  resource: string;
  canRead: boolean;
  canWrite: boolean;
  canDelete: boolean;
  id?: string;
}

@Component({
  selector: 'app-permissions',
  standalone: true,
  imports: [DxDataGridModule, DxCheckBoxModule, DxButtonModule, DxProgressBarModule],
  templateUrl: './permissions.html',
  styleUrl: './permissions.scss',
})
export class Permissions implements OnInit {
  private readonly admin = inject(AdminService);

  protected readonly rows = signal<MatrixRow[]>([]);
  protected readonly roles = signal<string[]>([]);
  protected readonly resources = signal<string[]>([]);
  protected readonly loading = signal(false);
  protected readonly saving = signal(false);
  protected readonly err = signal<string | null>(null);

  ngOnInit(): void {
    this.loading.set(true);
    forkJoin({
      perms: this.admin.getPermissions(),
      roles: this.admin.getRoles(),
      resources: this.admin.getResources(),
    }).subscribe({
      next: ({ perms, roles, resources }) => {
        this.roles.set(roles);
        this.resources.set(resources.map(r => r.key));

        // Build matrix: for each role × resource, use existing perm or default
        const matrix: MatrixRow[] = [];
        for (const role of roles) {
          for (const res of resources.map(r => r.key)) {
            const existing = perms.find(p => p.roleName === role && p.resource === res);
            matrix.push({
              roleName: role,
              resource: res,
              canRead: existing?.canRead ?? true,
              canWrite: existing?.canWrite ?? (role !== 'Viewer'),
              canDelete: existing?.canDelete ?? (role !== 'Viewer'),
              id: existing?.id,
            });
          }
        }
        this.rows.set(matrix);
        this.loading.set(false);
      },
      error: () => {
        this.err.set($localize`:@@common.loadError:Не удалось загрузить данные`);
        this.loading.set(false);
      },
    });
  }

  togglePermission(row: MatrixRow, field: 'canRead' | 'canWrite' | 'canDelete'): void {
    (row as any)[field] = !(row as any)[field];
  }

  save(): void {
    this.saving.set(true);
    this.err.set(null);
    const payload = this.rows().map(r => ({
      id: r.id,
      roleName: r.roleName,
      resource: r.resource,
      canRead: r.canRead,
      canWrite: r.canWrite,
      canDelete: r.canDelete,
    }));
    this.admin.bulkUpsertPermissions(payload).subscribe({
      next: () => this.saving.set(false),
      error: () => {
        this.err.set($localize`:@@common.saveError:Не удалось сохранить данные`);
        this.saving.set(false);
      },
    });
  }
}
