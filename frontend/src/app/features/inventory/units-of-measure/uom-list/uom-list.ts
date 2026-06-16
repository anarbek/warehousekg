import { AfterViewInit, Component, OnInit, ViewChild, inject, signal } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatPaginator, MatPaginatorModule } from '@angular/material/paginator';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { MatSort, MatSortModule } from '@angular/material/sort';
import { MatTableDataSource, MatTableModule } from '@angular/material/table';
import { MatTooltipModule } from '@angular/material/tooltip';
import { RouterLink } from '@angular/router';
import { UnitOfMeasure } from '../../models/inventory-item.model';
import { InventoryService } from '../../services/inventory.service';

@Component({
  selector: 'app-uom-list',
  imports: [
    MatTableModule, MatPaginatorModule, MatSortModule,
    MatButtonModule, MatIconModule, MatTooltipModule, MatProgressBarModule,
    RouterLink,
  ],
  templateUrl: './uom-list.html',
  styleUrl: './uom-list.scss',
})
export class UomList implements OnInit, AfterViewInit {
  private readonly service = inject(InventoryService);

  protected readonly displayedColumns = ['code', 'name', 'description', 'actions'];
  protected readonly dataSource = new MatTableDataSource<UnitOfMeasure>([]);
  protected readonly loading = signal(false);
  protected readonly error = signal<string | null>(null);

  protected readonly editLabel = $localize`:@@common.edit:Редактировать`;
  protected readonly deleteLabel = $localize`:@@common.delete:Удалить`;

  @ViewChild(MatPaginator) paginator!: MatPaginator;
  @ViewChild(MatSort) sort!: MatSort;

  ngOnInit(): void { this.load(); }
  ngAfterViewInit(): void {
    this.dataSource.paginator = this.paginator;
    this.dataSource.sort = this.sort;
  }

  protected load(): void {
    this.loading.set(true);
    this.error.set(null);
    this.service.getUnitsOfMeasure().subscribe({
      next: (data) => { this.dataSource.data = data; this.loading.set(false); },
      error: () => { this.error.set($localize`:@@common.loadError:Не удалось загрузить данные`); this.loading.set(false); },
    });
  }

  protected deleteItem(id: string): void {
    if (!confirm($localize`:@@common.confirmDelete:Вы уверены, что хотите удалить запись?`)) return;
    this.service.deleteUnitOfMeasure(id).subscribe({ next: () => this.load() });
  }
}
