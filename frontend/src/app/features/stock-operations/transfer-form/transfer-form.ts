import { Component, OnInit, inject, signal } from '@angular/core';
import { forkJoin } from 'rxjs';
import { FormArray, FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { MatSelectModule } from '@angular/material/select';
import { ActivatedRoute, Router } from '@angular/router';
import { Warehouse } from '../../inventory/models/warehouse.model';
import { InventoryItem } from '../../inventory/models/inventory-item.model';
import { InventoryService } from '../../inventory/services/inventory.service';
import { StockOperationsService } from '../services/stock-operations.service';

@Component({
  selector: 'app-transfer-form',
  imports: [
    ReactiveFormsModule,
    MatCardModule, MatFormFieldModule, MatInputModule, MatSelectModule,
    MatButtonModule, MatIconModule, MatProgressBarModule,
  ],
  templateUrl: './transfer-form.html',
  styleUrl: './transfer-form.scss',
})
export class TransferForm implements OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly service = inject(StockOperationsService);
  private readonly inventoryService = inject(InventoryService);
  private readonly router = inject(Router);
  private readonly route = inject(ActivatedRoute);

  protected readonly saving = signal(false);
  protected readonly error = signal<string | null>(null);
  protected readonly warehouses = signal<Warehouse[]>([]);
  protected readonly items = signal<InventoryItem[]>([]);

  protected readonly form = this.fb.group({
    number: ['', Validators.required],
    sourceWarehouseId: ['', Validators.required],
    destinationWarehouseId: ['', Validators.required],
    notes: [''],
    lines: this.fb.array([this.createLine()]),
  });

  get lines(): FormArray { return this.form.get('lines') as FormArray; }

  private createLine() {
    return this.fb.group({
      inventoryItemId: ['', Validators.required],
      quantity: [0, [Validators.required, Validators.min(1)]],
    });
  }

  ngOnInit(): void {
    forkJoin({
      warehouses: this.inventoryService.getWarehouses(),
      items: this.inventoryService.getInventoryItems(),
    }).subscribe({
      next: ({ warehouses, items }) => {
        this.warehouses.set(warehouses);
        this.items.set(items);
      },
    });
  }

  protected addLine(): void {
    this.lines.push(this.createLine());
  }

  protected removeLine(index: number): void {
    this.lines.removeAt(index);
  }

  protected submit(): void {
    if (this.form.invalid) return;
    this.saving.set(true);
    this.error.set(null);

    const v = this.form.getRawValue();
    this.service
      .createStockTransfer({
        number: v.number!,
        sourceWarehouseId: v.sourceWarehouseId!,
        destinationWarehouseId: v.destinationWarehouseId!,
        notes: v.notes || null,
        lines: v.lines.map((l) => ({
          inventoryItemId: l.inventoryItemId!,
          quantity: Number(l.quantity),
        })),
      })
      .subscribe({
        next: () => {
          this.saving.set(false);
          void this.router.navigate(['..'], { relativeTo: this.route });
        },
        error: () => {
          this.error.set($localize`:@@common.saveError:Не удалось сохранить данные`);
          this.saving.set(false);
        },
      });
  }

  protected cancel(): void {
    void this.router.navigate(['..'], { relativeTo: this.route });
  }
}
