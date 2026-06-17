import { Component, inject, signal } from '@angular/core';
import { DxDataGridModule, DxButtonModule, DxProgressBarModule } from 'devextreme-angular';
import { RouterLink } from '@angular/router';
import { UnitOfMeasure } from '../../models/inventory-item.model';
import { InventoryService } from '../../services/inventory.service';

@Component({ selector: 'app-uom-list', imports: [DxDataGridModule,DxButtonModule,DxProgressBarModule,RouterLink], templateUrl: './uom-list.html', styleUrl: './uom-list.scss' })
export class UomList {
  private readonly s = inject(InventoryService);
  protected readonly items = signal<UnitOfMeasure[]>([]);
  protected readonly loading = signal(false); protected readonly error = signal<string|null>(null);
  constructor() { this.load(); }
  private load() { this.loading.set(true); this.error.set(null); this.s.getUnitsOfMeasure().subscribe({ next: (d) => { this.items.set(d); this.loading.set(false); }, error: () => { this.error.set('Load error'); this.loading.set(false); } }); }
}
