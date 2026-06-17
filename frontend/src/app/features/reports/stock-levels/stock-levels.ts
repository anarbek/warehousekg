import { AfterViewInit, Component, OnInit, ViewChild, inject, signal } from '@angular/core';
import { DecimalPipe } from '@angular/common';
import { forkJoin } from 'rxjs';
import { MatPaginator, MatPaginatorModule } from '@angular/material/paginator';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { MatSort, MatSortModule } from '@angular/material/sort';
import { MatTableDataSource, MatTableModule } from '@angular/material/table';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatSelectModule } from '@angular/material/select';
import { MatSlideToggleModule } from '@angular/material/slide-toggle';
import { FormsModule } from '@angular/forms';
import { LowStockItem } from '../models/report.model';
import { InventoryItem, ItemCategory } from '../../inventory/models/inventory-item.model';
import { ReportsService } from '../services/reports.service';
import { InventoryService } from '../../inventory/services/inventory.service';

interface StockRow {
  sku: string;
  name: string;
  categoryName: string;
  quantityOnHand: number;
  reorderLevel: number;
  deficit: number;
}

@Component({
  selector: 'app-stock-levels',
  standalone: true,
  imports: [DecimalPipe, MatTableModule, MatPaginatorModule, MatSortModule, MatProgressBarModule, MatFormFieldModule, MatSelectModule, MatSlideToggleModule, FormsModule],
  templateUrl: './stock-levels.html',
  styleUrl: './stock-levels.scss',
})
export class StockLevels implements OnInit, AfterViewInit {
  private readonly rpt = inject(ReportsService);
  private readonly inv = inject(InventoryService);

  protected readonly cols = ['sku', 'name', 'categoryName', 'quantityOnHand', 'reorderLevel', 'deficit'];
  protected readonly ds = new MatTableDataSource<StockRow>([]);
  protected readonly loading = signal(false);
  protected readonly error = signal<string | null>(null);

  // Filters
  protected readonly categories = signal<ItemCategory[]>([]);
  protected readonly selCategory = signal<string>('');
  protected readonly lowStockOnly = signal(true);

  @ViewChild(MatPaginator) paginator!: MatPaginator;
  @ViewChild(MatSort) sort!: MatSort;

  ngOnInit(): void { this.load(); }
  ngAfterViewInit(): void { this.ds.paginator = this.paginator; this.ds.sort = this.sort; }

  protected load(): void {
    this.loading.set(true);
    this.error.set(null);

    // If low-stock only, use the dedicated endpoint; otherwise fetch all items
    if (this.lowStockOnly()) {
      forkJoin({
        low: this.rpt.getLowStock(),
        categories: this.inv.getItemCategories(),
      }).subscribe({
        next: ({ low, categories }) => {
          this.categories.set(categories);
          this.ds.data = this.mapRows(low, categories);
          this.loading.set(false);
        },
        error: () => {
          this.error.set($localize`:@@common.loadError:Не удалось загрузить данные`);
          this.loading.set(false);
        },
      });
    } else {
      forkJoin({
        items: this.inv.getInventoryItems(),
        categories: this.inv.getItemCategories(),
      }).subscribe({
        next: ({ items, categories }) => {
          this.categories.set(categories);
          this.ds.data = this.mapAllRows(items, categories);
          this.loading.set(false);
        },
        error: () => {
          this.error.set($localize`:@@common.loadError:Не удалось загрузить данные`);
          this.loading.set(false);
        },
      });
    }
  }

  toggleLowStock(): void {
    this.lowStockOnly.update((v) => !v);
    this.load();
  }

  onCategoryChange(): void {
    this.applyFilter();
  }

  private applyFilter(): void {
    const cat = this.selCategory();
    this.ds.filter = cat;
    this.ds.filterPredicate = (row: StockRow, filter: string) => {
      if (!filter) return true;
      return row.categoryName === filter;
    };
  }

  private mapRows(lowItems: LowStockItem[], categories: ItemCategory[]): StockRow[] {
    const catMap = new Map(categories.map((c) => [c.id, c.name]));
    return lowItems.map((l) => ({
      sku: l.sku,
      name: l.name,
      categoryName: catMap.get(l.categoryId) || '—',
      quantityOnHand: l.quantityOnHand,
      reorderLevel: l.reorderLevel,
      deficit: l.deficit,
    }));
  }

  private mapAllRows(items: InventoryItem[], categories: ItemCategory[]): StockRow[] {
    const catMap = new Map(categories.map((c) => [c.id, c.name]));
    return items.map((i) => ({
      sku: i.sku,
      name: i.name,
      categoryName: i.categoryName || catMap.get(i.categoryId) || '—',
      quantityOnHand: i.quantityOnHand,
      reorderLevel: i.reorderLevel,
      deficit: Math.max(0, i.reorderLevel - i.quantityOnHand),
    }));
  }
}

