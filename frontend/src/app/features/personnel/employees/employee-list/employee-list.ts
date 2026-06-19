import { Component, inject, signal, computed } from '@angular/core';
import { DxDataGridModule, DxButtonModule, DxProgressBarModule } from 'devextreme-angular';
import { RouterLink } from '@angular/router';
import { PersonnelService } from '../../services/personnel.service';
import { Employee } from '../../models/personnel.model';
import { PermissionsService } from '../../../../core/services/permissions.service';

@Component({
  selector: 'app-employee-list',
  imports: [DxDataGridModule, DxButtonModule, DxProgressBarModule, RouterLink],
  templateUrl: './employee-list.html',
  styleUrl: './employee-list.scss'
})
export class EmployeeList {
  private readonly svc = inject(PersonnelService);
  private readonly perms = inject(PermissionsService);
  protected readonly items = signal<Employee[]>([]);
  protected readonly loading = signal(false);
  protected readonly error = signal<string | null>(null);
  protected readonly canDelete = computed(() => this.perms.canDelete('employees'));
  constructor() { this.load(); }
  private load() {
    this.loading.set(true); this.error.set(null);
    this.svc.getEmployees().subscribe({
      next: d => { this.items.set(d); this.loading.set(false); },
      error: () => { this.error.set('Load error'); this.loading.set(false); }
    });
  }
  protected delete(id: string) {
    if (!confirm($localize`:@@common.confirmDelete:Вы уверены, что хотите удалить запись?`)) return;
    this.svc.deleteEmployee(id).subscribe({ next: () => this.load() });
  }
}
