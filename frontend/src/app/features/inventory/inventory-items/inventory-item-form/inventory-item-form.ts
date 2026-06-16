import { Component, OnInit, inject, signal } from '@angular/core';
import { forkJoin, Observable } from 'rxjs';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { MatSelectModule } from '@angular/material/select';
import { ActivatedRoute, Router } from '@angular/router';
import { CreateInventoryItemRequest, ItemCategory, UnitOfMeasure } from '../../models/inventory-item.model';
import { InventoryService } from '../../services/inventory.service';

@Component({
  selector: 'app-inventory-item-form',
  imports: [
    ReactiveFormsModule,
    MatCardModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    MatButtonModule,
    MatCheckboxModule,
    MatIconModule,
    MatProgressBarModule,
  ],
  templateUrl: './inventory-item-form.html',
  styleUrl: './inventory-item-form.scss',
})
export class InventoryItemForm implements OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly service = inject(InventoryService);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);

  protected readonly editId = this.route.snapshot.paramMap.get('id');
  protected readonly isEditMode = this.editId !== null;
  protected readonly saving = signal(false);
  protected readonly loading = signal(false);
  protected readonly error = signal<string | null>(null);

  protected readonly categories = signal<ItemCategory[]>([]);
  protected readonly unitsOfMeasure = signal<UnitOfMeasure[]>([]);

  protected readonly title = this.isEditMode
    ? $localize`:@@item.form.edit.title:Редактировать товар`
    : $localize`:@@item.form.create.title:Новый товар`;

  protected readonly form = this.fb.group({
    sku: ['', Validators.required],
    name: ['', Validators.required],
    description: [''],
    barcode: [''],
    categoryId: ['', Validators.required],
    unitOfMeasureId: ['', Validators.required],
    quantityOnHand: [0, [Validators.required, Validators.min(0)]],
    reorderLevel: [0, [Validators.required, Validators.min(0)]],
    isActive: [true],
  });

  ngOnInit(): void {
    // Always load dropdown data
    forkJoin({
      categories: this.service.getItemCategories(),
      uoms: this.service.getUnitsOfMeasure(),
    }).subscribe({
      next: ({ categories, uoms }) => {
        this.categories.set(categories);
        this.unitsOfMeasure.set(uoms);
      },
    });

    if (!this.isEditMode) return;
    this.loading.set(true);
    this.service.getInventoryItemById(this.editId!).subscribe({
      next: (item) => {
        this.form.patchValue(item);
        this.loading.set(false);
      },
      error: () => {
        this.error.set($localize`:@@common.loadError:Не удалось загрузить данные`);
        this.loading.set(false);
      },
    });
  }

  protected submit(): void {
    if (this.form.invalid) return;
    this.saving.set(true);
    this.error.set(null);

    const v = this.form.getRawValue();
    const request: CreateInventoryItemRequest = {
      sku: v.sku!,
      name: v.name!,
      description: v.description || null,
      barcode: v.barcode || null,
      categoryId: v.categoryId!,
      unitOfMeasureId: v.unitOfMeasureId!,
      quantityOnHand: Number(v.quantityOnHand),
      reorderLevel: Number(v.reorderLevel),
      isActive: v.isActive ?? true,
    };

    const op: Observable<unknown> = this.isEditMode
      ? this.service.updateInventoryItem(this.editId!, request)
      : this.service.createInventoryItem(request);

    op.subscribe({
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
