import { Component, OnInit, inject, signal } from '@angular/core';
import { forkJoin } from 'rxjs';
import { DecimalPipe } from '@angular/common';
import { MatCardModule } from '@angular/material/card';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { DxChartModule } from 'devextreme-angular';
import { InventorySummary, PurchaseSummary, SalesSummary, StockMovementSummary } from '../models/report.model';
import { ReportsService } from '../services/reports.service';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [DecimalPipe, MatCardModule, MatProgressBarModule, DxChartModule],
  templateUrl: './dashboard.html',
  styleUrl: './dashboard.scss',
})
export class Dashboard implements OnInit {
  private readonly svc = inject(ReportsService);
  protected readonly inv = signal<InventorySummary | null>(null);
  protected readonly sales = signal<SalesSummary | null>(null);
  protected readonly purch = signal<PurchaseSummary | null>(null);
  protected readonly mov = signal<StockMovementSummary | null>(null);
  protected readonly loading = signal(false);

  ngOnInit(): void {
    this.loading.set(true);
    forkJoin({
      inv: this.svc.getInventorySummary(),
      sales: this.svc.getSalesSummary(),
      purch: this.svc.getPurchaseSummary(),
      mov: this.svc.getStockMovements(),
    }).subscribe({
      next: (d) => {
        this.inv.set(d.inv);
        this.sales.set(d.sales);
        this.purch.set(d.purch);
        this.mov.set(d.mov);
        this.loading.set(false);
      },
    });
  }
}

