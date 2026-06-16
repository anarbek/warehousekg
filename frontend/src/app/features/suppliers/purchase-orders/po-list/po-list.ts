import { AfterViewInit, Component, OnInit, ViewChild, inject, signal } from '@angular/core';
import { DatePipe, DecimalPipe, LowerCasePipe } from '@angular/common';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatPaginator, MatPaginatorModule } from '@angular/material/paginator';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { MatSort, MatSortModule } from '@angular/material/sort';
import { MatTableDataSource, MatTableModule } from '@angular/material/table';
import { MatTooltipModule } from '@angular/material/tooltip';
import { RouterLink } from '@angular/router';
import { PurchaseOrderSummary } from '../../models/supplier-po.model';
import { SupplierPoService } from '../../services/supplier-po.service';

@Component({
  selector: 'app-po-list',
  imports: [DatePipe, DecimalPipe, LowerCasePipe, MatTableModule, MatPaginatorModule, MatSortModule, MatButtonModule, MatIconModule, MatTooltipModule, MatProgressBarModule, RouterLink],
  templateUrl: './po-list.html',
  styleUrl: './po-list.scss',
})
export class PoList implements OnInit, AfterViewInit {
  private readonly service = inject(SupplierPoService);
  protected readonly displayedColumns = ['number', 'supplierId', 'status', 'totalAmount', 'lineCount', 'orderDateUtc', 'actions'];
  protected readonly dataSource = new MatTableDataSource<PurchaseOrderSummary>([]);
  protected readonly loading = signal(false);
  protected readonly error = signal<string | null>(null);
  @ViewChild(MatPaginator) paginator!: MatPaginator;
  @ViewChild(MatSort) sort!: MatSort;
  ngOnInit(): void { this.load(); }
  ngAfterViewInit(): void { this.dataSource.paginator = this.paginator; this.dataSource.sort = this.sort; }
  protected load(): void {
    this.loading.set(true); this.error.set(null);
    this.service.getPurchaseOrders().subscribe({
      next: (d) => { this.dataSource.data = d; this.loading.set(false); },
      error: () => { this.error.set($localize`:@@common.loadError:Не удалось загрузить данные`); this.loading.set(false); },
    });
  }
}
