import { Component, inject, signal } from '@angular/core';
import { DxDataGridModule, DxButtonModule, DxProgressBarModule } from 'devextreme-angular';
import { RouterLink } from '@angular/router';
import { StockAdjustmentSummary } from '../models/adjustment.model';
import { AdjustmentsService } from '../services/adjustments.service';

@Component({ selector: 'app-adjustment-list', imports: [DxDataGridModule,DxButtonModule,DxProgressBarModule,RouterLink], templateUrl: './adjustment-list.html', styleUrl: './adjustment-list.scss' })
export class AdjustmentList {
  private readonly s = inject(AdjustmentsService);
  protected readonly items = signal<StockAdjustmentSummary[]>([]);
  protected readonly loading = signal(false); protected readonly error = signal<string|null>(null);
  constructor() { this.load(); }
  private load() { this.loading.set(true); this.error.set(null); this.s.getAdjustments().subscribe({ next: (d:any) => { this.items.set(d); this.loading.set(false); }, error: () => { this.error.set('Load error'); this.loading.set(false); } }); }
}
