import { AfterViewInit, Component, OnInit, ViewChild, inject, signal } from '@angular/core';
import { MatPaginator, MatPaginatorModule } from '@angular/material/paginator';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { MatSort, MatSortModule } from '@angular/material/sort';
import { MatTableDataSource, MatTableModule } from '@angular/material/table';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { MatNativeDateModule } from '@angular/material/core';
import { FormsModule } from '@angular/forms';
import { OperationStatusCounts } from '../models/report.model';
import { ReportsService } from '../services/reports.service';

@Component({
  selector: 'app-movement-history',
  standalone: true,
  imports: [MatTableModule, MatPaginatorModule, MatSortModule, MatProgressBarModule, MatFormFieldModule, MatDatepickerModule, MatNativeDateModule, FormsModule],
  templateUrl: './movement-history.html',
  styleUrl: './movement-history.scss',
})
export class MovementHistory implements OnInit, AfterViewInit {
  private readonly svc = inject(ReportsService);
  protected readonly cols = ['operation', 'draft', 'completed', 'cancelled', 'total'];
  protected readonly ds = new MatTableDataSource<OperationStatusCounts>([]);
  protected readonly loading = signal(false);
  protected readonly error = signal<string | null>(null);

  // Date range filters (UI only — backend does not yet support date filtering)
  protected readonly dateFrom = signal<Date | null>(null);
  protected readonly dateTo = signal<Date | null>(null);

  @ViewChild(MatPaginator) paginator!: MatPaginator;
  @ViewChild(MatSort) sort!: MatSort;

  ngOnInit(): void { this.load(); }
  ngAfterViewInit(): void { this.ds.paginator = this.paginator; this.ds.sort = this.sort; }

  protected load(): void {
    this.loading.set(true);
    this.error.set(null);
    this.svc.getStockMovements().subscribe({
      next: (d) => { this.ds.data = d.operations; this.loading.set(false); },
      error: () => { this.error.set($localize`:@@common.loadError:Не удалось загрузить данные`); this.loading.set(false); },
    });
  }
}
