import { Component, inject, signal, computed } from '@angular/core';
import { DxDataGridModule, DxButtonModule, DxProgressBarModule } from 'devextreme-angular';
import { RouterLink } from '@angular/router';
import { PersonnelService } from '../../services/personnel.service';
import { Shift } from '../../models/personnel.model';
import { PermissionsService } from '../../../../core/services/permissions.service';

@Component({
  selector: 'app-shift-list',
  imports: [DxDataGridModule, DxButtonModule, DxProgressBarModule, RouterLink],
  templateUrl: './shift-list.html',
  styleUrl: './shift-list.scss'
})
export class ShiftList {
  private readonly svc = inject(PersonnelService);
  private readonly perms = inject(PermissionsService);
  protected readonly items = signal<Shift[]>([]);
  protected readonly loading = signal(false);
  protected readonly error = signal<string | null>(null);
  protected readonly canDelete = computed(() => this.perms.canDelete('shifts'));
  constructor() { this.load(); }
  private load() {
    this.loading.set(true); this.error.set(null);
    this.svc.getShifts().subscribe({
      next: d => { this.items.set(d); this.loading.set(false); },
      error: () => { this.error.set('Load error'); this.loading.set(false); }
    });
  }
  protected delete(id: string) {
    if (!confirm($localize`:@@common.confirmDelete:Вы уверены, что хотите удалить запись?`)) return;
    this.svc.deleteShift(id).subscribe({ next: () => this.load() });
  }
}
