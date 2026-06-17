import { Component, inject, signal } from '@angular/core';
import { DxButtonModule, DxProgressBarModule } from 'devextreme-angular';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { InventoryItem } from '../../models/inventory-item.model';
import { InventoryService } from '../../services/inventory.service';

@Component({
  selector: 'app-inventory-item-detail',
  imports: [ DxButtonModule, DxProgressBarModule, RouterLink ],
  templateUrl: './inventory-item-detail.html',
  styleUrl: './inventory-item-detail.scss',
})
export class InventoryItemDetail {
  private readonly service = inject(InventoryService);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  protected readonly id = this.route.snapshot.paramMap.get('id')!;
  protected readonly item = signal<InventoryItem | null>(null);
  protected readonly loading = signal(false);
  protected readonly error = signal<string | null>(null);
  constructor() { this.load(); }
  private load(): void {
    this.loading.set(true);
    this.service.getInventoryItemById(this.id).subscribe({
      next: (it) => { this.item.set(it); this.loading.set(false); },
      error: () => { this.error.set('Load error'); this.loading.set(false); },
    });
  }
  protected delete(): void {
    if (!confirm('Delete?')) return;
    this.service.deleteInventoryItem(this.id).subscribe({
      next: () => void this.router.navigate(['../..'], { relativeTo: this.route }),
    });
  }
}
