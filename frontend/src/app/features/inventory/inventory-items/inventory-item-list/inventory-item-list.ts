import { AfterViewInit, Component, OnInit, ViewChild, inject, signal } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatPaginator, MatPaginatorModule } from '@angular/material/paginator';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { MatSort, MatSortModule } from '@angular/material/sort';
import { MatTableDataSource, MatTableModule } from '@angular/material/table';
import { MatTooltipModule } from '@angular/material/tooltip';
import { RouterLink } from '@angular/router';
import { InventoryItem } from '../../models/inventory-item.model';
import { InventoryService } from '../../services/inventory.service';

@Component({
  selector: 'app-inventory-item-list',
  imports: [
    MatTableModule,
    MatPaginatorModule,
    MatSortModule,
    MatButtonModule,
    MatIconModule,
    MatTooltipModule,
    MatProgressBarModule,
    RouterLink,
  ],
  templateUrl: './inventory-item-list.html',
  styleUrl: './inventory-item-list.scss',
})
export class InventoryItemList implements OnInit, AfterViewInit {
  private readonly service = inject(InventoryService);

  protected readonly displayedColumns = [
    'sku',
    'name',
    'barcode',
    'quantityOnHand',
    'reorderLevel',
    'isActive',
    'actions',
  ];
  protected readonly dataSource = new MatTableDataSource<InventoryItem>([]);
  protected readonly loading = signal(false);
  protected readonly error = signal<string | null>(null);

  protected readonly detailLabel = $localize`:@@common.detail:Просмотр`;
  protected readonly editLabel = $localize`:@@common.edit:Редактировать`;
  protected readonly deleteLabel = $localize`:@@common.delete:Удалить`;

  @ViewChild(MatPaginator) paginator!: MatPaginator;
  @ViewChild(MatSort) sort!: MatSort;

  ngOnInit(): void {
    this.load();
  }

  ngAfterViewInit(): void {
    this.dataSource.paginator = this.paginator;
    this.dataSource.sort = this.sort;
  }

  protected load(): void {
    this.loading.set(true);
    this.error.set(null);
    this.service.getInventoryItems().subscribe({
      next: (data) => {
        this.dataSource.data = data;
        this.loading.set(false);
      },
      error: () => {
        this.error.set($localize`:@@common.loadError:Не удалось загрузить данные`);
        this.loading.set(false);
      },
    });
  }

  protected deleteItem(id: string): void {
    if (!confirm($localize`:@@common.confirmDelete:Вы уверены, что хотите удалить запись?`)) return;
    this.service.deleteInventoryItem(id).subscribe({ next: () => this.load() });
  }
}
