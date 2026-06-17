import { Component, inject, signal } from '@angular/core';
import { DxDataGridModule, DxButtonModule, DxProgressBarModule } from 'devextreme-angular';
import { RouterLink } from '@angular/router';
import { InventoryItem } from '../../models/inventory-item.model';
import { InventoryService } from '../../services/inventory.service';

@Component({
  selector: 'app-inventory-item-list',
  imports: [ DxDataGridModule, DxButtonModule, DxProgressBarModule, RouterLink ],
  templateUrl: './inventory-item-list.html',
  styleUrl: './inventory-item-list.scss',
})
export class InventoryItemList {
  private readonly service = inject(InventoryService);
  protected readonly items = signal<InventoryItem[]>([]);
  protected readonly loading = signal(false);
  protected readonly error = signal<string | null>(null);
  constructor() { this.load(); }
  private load(): void {
    this.loading.set(true); this.error.set(null);
    this.service.getInventoryItems().subscribe({
      next: (data) => { this.items.set(data); this.loading.set(false); },
      error: () => { this.error.set('Load error'); this.loading.set(false); },
    });
  }
}
