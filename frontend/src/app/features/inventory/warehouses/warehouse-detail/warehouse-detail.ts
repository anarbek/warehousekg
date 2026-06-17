import { Component, inject, signal } from '@angular/core';
import { DxButtonModule, DxProgressBarModule } from 'devextreme-angular';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { Warehouse } from '../../models/warehouse.model';
import { InventoryService } from '../../services/inventory.service';

@Component({
  selector: 'app-warehouse-detail',
  imports: [ DxButtonModule, DxProgressBarModule, RouterLink ],
  templateUrl: './warehouse-detail.html',
  styleUrl: './warehouse-detail.scss',
})
export class WarehouseDetail {
  private readonly service = inject(InventoryService);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  protected readonly id = this.route.snapshot.paramMap.get('id')!;
  protected readonly warehouse = signal<Warehouse | null>(null);
  protected readonly loading = signal(false);
  protected readonly error = signal<string | null>(null);
  constructor() { this.load(); }
  private load(): void {
    this.loading.set(true);
    this.service.getWarehouseById(this.id).subscribe({
      next: (wh) => { this.warehouse.set(wh); this.loading.set(false); },
      error: () => { this.error.set('Load error'); this.loading.set(false); },
    });
  }
  protected delete(): void {
    if (!confirm('Delete?')) return;
    this.service.deleteWarehouse(this.id).subscribe({
      next: () => void this.router.navigate(['../..'], { relativeTo: this.route }),
    });
  }
}
