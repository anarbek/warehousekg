import { AfterViewInit, Component, OnInit, ViewChild, inject, signal } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatPaginator, MatPaginatorModule } from '@angular/material/paginator';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { MatSort, MatSortModule } from '@angular/material/sort';
import { MatTableDataSource, MatTableModule } from '@angular/material/table';
import { MatTooltipModule } from '@angular/material/tooltip';
import { RouterLink } from '@angular/router';
import { Supplier } from '../models/supplier-po.model';
import { SupplierPoService } from '../services/supplier-po.service';

@Component({
  selector: 'app-supplier-list',
  imports: [MatTableModule, MatPaginatorModule, MatSortModule, MatButtonModule, MatIconModule, MatTooltipModule, MatProgressBarModule, RouterLink],
  templateUrl: './supplier-list.html',
  styleUrl: './supplier-list.scss',
})
export class SupplierList implements OnInit, AfterViewInit {
  private readonly service = inject(SupplierPoService);
  protected readonly displayedColumns = ['code', 'name', 'contactName', 'email', 'isActive', 'actions'];
  protected readonly dataSource = new MatTableDataSource<Supplier>([]);
  protected readonly loading = signal(false);
  protected readonly error = signal<string | null>(null);
  protected readonly editLabel = $localize`:@@common.edit:Редактировать`;
  protected readonly deleteLabel = $localize`:@@common.delete:Удалить`;
  @ViewChild(MatPaginator) paginator!: MatPaginator;
  @ViewChild(MatSort) sort!: MatSort;
  ngOnInit(): void { this.load(); }
  ngAfterViewInit(): void { this.dataSource.paginator = this.paginator; this.dataSource.sort = this.sort; }
  protected load(): void {
    this.loading.set(true); this.error.set(null);
    this.service.getSuppliers().subscribe({
      next: (d) => { this.dataSource.data = d; this.loading.set(false); },
      error: () => { this.error.set($localize`:@@common.loadError:Не удалось загрузить данные`); this.loading.set(false); },
    });
  }
  protected deleteItem(id: string): void {
    if (!confirm($localize`:@@common.confirmDelete:Вы уверены, что хотите удалить запись?`)) return;
    this.service.deleteSupplier(id).subscribe({ next: () => this.load() });
  }
}
