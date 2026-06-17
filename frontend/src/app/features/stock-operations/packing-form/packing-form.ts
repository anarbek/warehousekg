import { Component, OnInit, inject, signal } from '@angular/core';
import { forkJoin } from 'rxjs';
import { FormArray, FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatAutocompleteModule } from '@angular/material/autocomplete';
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
import { PickOrder } from '../models/stock-operation.model';
import { StockOperationsService } from '../services/stock-operations.service';
import { ErrorToastService } from '../../../core/services/error-toast.service';

@Component({
  selector: 'app-packing-form',
  imports: [
    ReactiveFormsModule,
    MatAutocompleteModule,
    MatCardModule, MatFormFieldModule, MatInputModule, MatSelectModule,
    MatButtonModule, MatIconModule, MatProgressBarModule,
  ],
  templateUrl: './packing-form.html',
  styleUrl: './packing-form.scss',
})
export class PackingForm implements OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly service = inject(StockOperationsService);
  private readonly inventoryService = inject(InventoryService);
  private readonly router = inject(Router);
  private readonly route = inject(ActivatedRoute);
  private readonly toast = inject(ErrorToastService);

  protected readonly saving = signal(false);
  protected readonly error = signal<string | null>(null);
  protected readonly warehouses = signal<Warehouse[]>([]);
  protected readonly items = signal<InventoryItem[]>([]);
  protected readonly pickOrders = signal<PickOrder[]>([]);

  /** Autocomplete: filtered list updated via valueChanges subscription */
  protected readonly filteredPickOrders = signal<PickOrder[]>([]);

  /** Display function for autocomplete: given an ID, show "number — warehouseName" */
  protected readonly displayPickOrderFn = (id: string | null): string => {
    if (!id) return '';
    const po = this.pickOrders().find((p) => p.id === id);
    if (!po) return id;
    return po.warehouseName
      ? `${po.number} — ${po.warehouseName}`
      : po.number;
  };

  protected readonly form = this.fb.group({
    number: ['', Validators.required],
    warehouseId: ['', Validators.required],
    pickOrderId: [''],
    notes: [''],
    lines: this.fb.array([this.createLine()]),
  });

  get lines(): FormArray { return this.form.get('lines') as FormArray; }

  private createLine() {
    return this.fb.group({
      inventoryItemId: ['', Validators.required],
      quantity: [0, [Validators.required, Validators.min(1)]],
      packageLabel: [''],
    });
  }

  ngOnInit(): void {
    forkJoin({
      warehouses: this.inventoryService.getWarehouses(),
      items: this.inventoryService.getInventoryItems(),
      pickOrders: this.service.getPickOrders(),
    }).subscribe({
      next: ({ warehouses, items, pickOrders }) => {
        this.warehouses.set(warehouses);
        this.items.set(items);
        this.pickOrders.set(pickOrders);
        this.filteredPickOrders.set(pickOrders);
      },
    });

    this.form.controls.pickOrderId.valueChanges.subscribe((val) => {
      const q = (val ?? '').toLowerCase().trim();
      const all = this.pickOrders();
      if (!q) {
        this.filteredPickOrders.set(all);
      } else {
        this.filteredPickOrders.set(
          all.filter((po) => po.number.toLowerCase().includes(q))
        );
      }
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
      .createPackOrder({
        number: v.number!,
        warehouseId: v.warehouseId!,
        pickOrderId: v.pickOrderId || null,
        notes: v.notes || null,
        lines: v.lines.map((l) => ({
          inventoryItemId: l.inventoryItemId!,
          quantity: Number(l.quantity),
          packageLabel: l.packageLabel || null,
        })),
      })
      .subscribe({
        next: () => {
          this.saving.set(false);
          void this.router.navigate(['..'], { relativeTo: this.route });
        },
        error: (e) => {
          this.toast.showSave(e);
          this.saving.set(false);
        },
      });
  }

  protected cancel(): void {
    void this.router.navigate(['..'], { relativeTo: this.route });
  }
}
