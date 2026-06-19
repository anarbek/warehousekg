import { Component, inject, signal, computed } from '@angular/core';
import { DxDataGridModule, DxButtonModule, DxProgressBarModule, DxDateBoxModule } from 'devextreme-angular';
import { RouterLink } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { PersonnelService } from '../../services/personnel.service';
import { AttendanceRecord } from '../../models/personnel.model';
import { PermissionsService } from '../../../../core/services/permissions.service';

@Component({
  selector: 'app-attendance-list',
  imports: [DxDataGridModule, DxButtonModule, DxProgressBarModule, DxDateBoxModule, RouterLink, FormsModule],
  templateUrl: './attendance-list.html',
  styleUrl: './attendance-list.scss'
})
export class AttendanceList {
  private readonly svc = inject(PersonnelService);
  private readonly perms = inject(PermissionsService);
  protected readonly items = signal<AttendanceRecord[]>([]);
  protected readonly loading = signal(false);
  protected readonly error = signal<string | null>(null);
  protected readonly canDelete = computed(() => this.perms.canDelete('attendance'));
  protected dateFrom: Date | null = null;
  protected dateTo: Date | null = null;

  constructor() { this.load(); }

  private load() {
    this.loading.set(true); this.error.set(null);
    const from = this.dateFrom ? this.dateFrom.toISOString().split('T')[0] : undefined;
    const to = this.dateTo ? this.dateTo.toISOString().split('T')[0] : undefined;
    this.svc.getAttendance(from, to).subscribe({
      next: d => { this.items.set(d); this.loading.set(false); },
      error: () => { this.error.set('Load error'); this.loading.set(false); }
    });
  }

  protected applyFilter() { this.load(); }

  protected statusBadge(status: string): string {
    switch(status) {
      case 'Present': return 'Присутствует';
      case 'Absent': return 'Отсутствует';
      case 'Late': return 'Опоздание';
      case 'EarlyDeparture': return 'Ранний уход';
      case 'Overtime': return 'Сверхурочно';
      default: return status;
    }
  }

  protected delete(id: string) {
    if (!confirm($localize`:@@common.confirmDelete:Вы уверены, что хотите удалить запись?`)) return;
    this.svc.deleteAttendance(id).subscribe({ next: () => this.load() });
  }
}
