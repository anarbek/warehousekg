import { Component, inject, signal, computed } from '@angular/core';
import { DxDataGridModule, DxButtonModule, DxProgressBarModule } from 'devextreme-angular';
import { RouterLink } from '@angular/router';
import { PersonnelService } from '../../services/personnel.service';
import { Department } from '../../models/personnel.model';
import { PermissionsService } from '../../../../core/services/permissions.service';

@Component({
  selector: 'app-department-list',
  imports: [DxDataGridModule, DxButtonModule, DxProgressBarModule, RouterLink],
  templateUrl: './department-list.html',
  styleUrl: './department-list.scss'
})
export class DepartmentList {
  private readonly svc = inject(PersonnelService);
  private readonly perms = inject(PermissionsService);
  protected readonly items = signal<Department[]>([]);
  protected readonly loading = signal(false);
  protected readonly error = signal<string | null>(null);
  protected readonly canDelete = computed(() => this.perms.canDelete('departments'));

  constructor() { this.load(); }
  private load() {
    this.loading.set(true); this.error.set(null);
    this.svc.getDepartments().subscribe({
      next: d => { this.items.set(d); this.loading.set(false); },
      error: () => { this.error.set('Load error'); this.loading.set(false); }
    });
  }
  protected delete(id: string) {
    if (!confirm($localize`:@@common.confirmDelete:Вы уверены, что хотите удалить запись?`)) return;
    this.svc.deleteDepartment(id).subscribe({ next: () => this.load() });
  }
}
