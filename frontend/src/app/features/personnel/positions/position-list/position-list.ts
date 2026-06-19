import { Component, inject, signal } from '@angular/core';
import { DxDataGridModule, DxButtonModule, DxProgressBarModule } from 'devextreme-angular';
import { RouterLink } from '@angular/router';
import { PersonnelService } from '../../services/personnel.service';
import { Position } from '../../models/personnel.model';
import { PermissionsService } from '../../../../core/services/permissions.service';
import { computed } from '@angular/core';

@Component({
  selector: 'app-position-list',
  imports: [DxDataGridModule, DxButtonModule, DxProgressBarModule, RouterLink],
  templateUrl: './position-list.html',
  styleUrl: './position-list.scss'
})
export class PositionList {
  private readonly svc = inject(PersonnelService);
  private readonly perms = inject(PermissionsService);
  protected readonly items = signal<Position[]>([]);
  protected readonly loading = signal(false);
  protected readonly error = signal<string | null>(null);
  protected readonly canDelete = computed(() => this.perms.canDelete('positions'));

  constructor() { this.load(); }

  private load() {
    this.loading.set(true); this.error.set(null);
    this.svc.getPositions().subscribe({
      next: d => { this.items.set(d); this.loading.set(false); },
      error: () => { this.error.set('Load error'); this.loading.set(false); }
    });
  }

  protected delete(id: string) {
    if (!confirm($localize`:@@common.confirmDelete:Вы уверены, что хотите удалить запись?`)) return;
    this.svc.deletePosition(id).subscribe({ next: () => this.load() });
  }
}
