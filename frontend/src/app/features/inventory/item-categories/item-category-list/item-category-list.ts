import { Component, inject, signal } from '@angular/core';
import { DxDataGridModule, DxButtonModule, DxProgressBarModule } from 'devextreme-angular';
import { RouterLink } from '@angular/router';
import { ItemCategory } from '../../models/inventory-item.model';
import { InventoryService } from '../../services/inventory.service';

@Component({ selector: 'app-item-category-list', imports: [DxDataGridModule,DxButtonModule,DxProgressBarModule,RouterLink], templateUrl: './item-category-list.html', styleUrl: './item-category-list.scss' })
export class ItemCategoryList {
  private readonly s = inject(InventoryService);
  protected readonly items = signal<ItemCategory[]>([]);
  protected readonly loading = signal(false);
  protected readonly error = signal<string|null>(null);
  constructor() { this.load(); }
  private load() { this.loading.set(true); this.error.set(null); this.s.getItemCategories().subscribe({ next: (d) => { this.items.set(d); this.loading.set(false); }, error: () => { this.error.set('Load error'); this.loading.set(false); } }); }
}
