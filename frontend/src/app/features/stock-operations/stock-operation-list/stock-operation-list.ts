import { AfterViewInit, Component, OnInit, ViewChild, inject, signal } from '@angular/core';
import { DatePipe, LowerCasePipe } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Observable } from 'rxjs';
import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatPaginator, MatPaginatorModule } from '@angular/material/paginator';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { MatSelectModule } from '@angular/material/select';
import { MatSort, MatSortModule } from '@angular/material/sort';
import { MatTableDataSource, MatTableModule } from '@angular/material/table';
import { MatTooltipModule } from '@angular/material/tooltip';
import { ActivatedRoute, RouterLink } from '@angular/router';
import {
  OperationType,
  StockOperationStatus,
  StockReceipt,
  PickOrder,
  PackOrder,
  StockTransfer,
} from '../models/stock-operation.model';
import { StockOperationsService } from '../services/stock-operations.service';

type Row = StockReceipt | PickOrder | PackOrder | StockTransfer;

interface ListConfig {
  title: string;
  addLabel: string;
  newRoute: string;
  statusFilterLabel: string;
  statusFilterAll: string;
  columnNumber: string;
  columnWarehouse: string;
  columnStatus: string;
  columnLines: string;
  columnCreated: string;
  columnActions: string;
  showReference: boolean;
  referenceLabel: string;
}

const CONFIGS: Record<OperationType, ListConfig> = {
  receiving: {
    title: $localize`:@@stockOp.receiving.list.title:Поступления`,
    addLabel: $localize`:@@stockOp.receiving.list.add:Новое поступление`,
    newRoute: 'new',
    statusFilterLabel: $localize`:@@stockOp.filter.status:Статус`,
    statusFilterAll: $localize`:@@stockOp.filter.all:Все`,
    columnNumber: $localize`:@@stockOp.column.number:Номер`,
    columnWarehouse: $localize`:@@stockOp.column.warehouse:Склад`,
    columnStatus: $localize`:@@stockOp.column.status:Статус`,
    columnLines: $localize`:@@stockOp.column.lines:Позиций`,
    columnCreated: $localize`:@@stockOp.column.created:Создан`,
    columnActions: $localize`:@@common.actions:Действия`,
    showReference: true,
    referenceLabel: $localize`:@@stockOp.column.reference:Ссылка`,
  },
  picking: {
    title: $localize`:@@stockOp.picking.list.title:Сборка`,
    addLabel: $localize`:@@stockOp.picking.list.add:Новый заказ на сборку`,
    newRoute: 'new',
    statusFilterLabel: $localize`:@@stockOp.filter.status:Статус`,
    statusFilterAll: $localize`:@@stockOp.filter.all:Все`,
    columnNumber: $localize`:@@stockOp.column.number:Номер`,
    columnWarehouse: $localize`:@@stockOp.column.warehouse:Склад`,
    columnStatus: $localize`:@@stockOp.column.status:Статус`,
    columnLines: $localize`:@@stockOp.column.lines:Позиций`,
    columnCreated: $localize`:@@stockOp.column.created:Создан`,
    columnActions: $localize`:@@common.actions:Действия`,
    showReference: true,
    referenceLabel: $localize`:@@stockOp.column.reference:Основание`,
  },
  packing: {
    title: $localize`:@@stockOp.packing.list.title:Упаковка`,
    addLabel: $localize`:@@stockOp.packing.list.add:Новый заказ на упаковку`,
    newRoute: 'new',
    statusFilterLabel: $localize`:@@stockOp.filter.status:Статус`,
    statusFilterAll: $localize`:@@stockOp.filter.all:Все`,
    columnNumber: $localize`:@@stockOp.column.number:Номер`,
    columnWarehouse: $localize`:@@stockOp.column.warehouse:Склад`,
    columnStatus: $localize`:@@stockOp.column.status:Статус`,
    columnLines: $localize`:@@stockOp.column.lines:Позиций`,
    columnCreated: $localize`:@@stockOp.column.created:Создан`,
    columnActions: $localize`:@@common.actions:Действия`,
    showReference: false,
    referenceLabel: '',
  },
  transfer: {
    title: $localize`:@@stockOp.transfer.list.title:Перемещения`,
    addLabel: $localize`:@@stockOp.transfer.list.add:Новое перемещение`,
    newRoute: 'new',
    statusFilterLabel: $localize`:@@stockOp.filter.status:Статус`,
    statusFilterAll: $localize`:@@stockOp.filter.all:Все`,
    columnNumber: $localize`:@@stockOp.column.number:Номер`,
    columnWarehouse: $localize`:@@stockOp.column.warehouse:Склад`,
    columnStatus: $localize`:@@stockOp.column.status:Статус`,
    columnLines: $localize`:@@stockOp.column.lines:Позиций`,
    columnCreated: $localize`:@@stockOp.column.created:Создан`,
    columnActions: $localize`:@@common.actions:Действия`,
    showReference: false,
    referenceLabel: '',
  },
};

@Component({
  selector: 'app-stock-operation-list',
  imports: [
    DatePipe,
    LowerCasePipe,
    FormsModule,
    MatTableModule,
    MatPaginatorModule,
    MatSortModule,
    MatButtonModule,
    MatIconModule,
    MatTooltipModule,
    MatProgressBarModule,
    MatFormFieldModule,
    MatSelectModule,
    RouterLink,
  ],
  templateUrl: './stock-operation-list.html',
  styleUrl: './stock-operation-list.scss',
})
export class StockOperationList implements OnInit, AfterViewInit {
  private readonly service = inject(StockOperationsService);
  private readonly route = inject(ActivatedRoute);

  protected readonly operationType = this.route.snapshot.data['operationType'] as OperationType;
  protected readonly cfg = signal<ListConfig | null>(null);
  protected readonly displayedColumns = signal<string[]>([]);
  protected readonly dataSource = new MatTableDataSource<Row>([]);
  protected readonly loading = signal(false);
  protected readonly error = signal<string | null>(null);

  protected readonly statuses: StockOperationStatus[] = ['Draft', 'Completed', 'Cancelled'];
  protected readonly selectedStatus = signal<string>('');
  protected readonly detailLabel = $localize`:@@common.detail:Просмотр`;

  @ViewChild(MatPaginator) paginator!: MatPaginator;
  @ViewChild(MatSort) sort!: MatSort;

  ngOnInit(): void {
    const cfg = CONFIGS[this.operationType];
    this.cfg.set(cfg);
    const cols = ['number', 'warehouseName'];
    if (cfg.showReference) cols.push('reference');
    cols.push('lineCount', 'status', 'createdAtUtc', 'actions');
    this.displayedColumns.set(cols);
    this.load();
  }

  ngAfterViewInit(): void {
    this.dataSource.paginator = this.paginator;
    this.dataSource.sort = this.sort;
    // Custom filter for status dropdown
    this.dataSource.filterPredicate = (data: Row, filter: string) => {
      if (!filter) return true;
      return data.status === filter;
    };
  }

  protected load(): void {
    this.loading.set(true);
    this.error.set(null);

    const op = this.operationType;
    const obs = (
      op === 'receiving'
        ? this.service.getStockReceipts()
        : op === 'picking'
          ? this.service.getPickOrders()
          : op === 'packing'
            ? this.service.getPackOrders()
            : this.service.getStockTransfers()
    ) as Observable<Row[]>;

    obs.subscribe({
      next: (data: Row[]) => {
        this.dataSource.data = data;
        this.loading.set(false);
      },
      error: () => {
        this.error.set($localize`:@@common.loadError:Не удалось загрузить данные`);
        this.loading.set(false);
      },
    });
  }

  protected applyStatusFilter(status: string): void {
    this.selectedStatus.set(status);
    this.dataSource.filter = status;
  }

  protected getNewRoute(): string {
    return CONFIGS[this.operationType].newRoute;
  }

  /** Returns the best available date field from a row. */
  protected getDate(row: any): string | null {
    return row.receivedAtUtc || row.adjustedAtUtc || row.reconciledAtUtc || row.createdAtUtc || null;
  }

  /** Helper to extract row-specific reference field. */
  protected getReference(row: Row): string | null | undefined {
    if ('supplierReference' in row) return row.supplierReference;
    if ('reference' in row) return row.reference;
    return null;
  }

  /** Helper to extract warehouse name (handles source/dest for transfers). */
  protected getWarehouseName(row: Row): string {
    if ('sourceWarehouseName' in row && row.sourceWarehouseName) {
      const t = row as StockTransfer;
      return `${t.sourceWarehouseName ?? '—'} → ${t.destinationWarehouseName ?? '—'}`;
    }
    return (row as any).warehouseName ?? '—';
  }
}
