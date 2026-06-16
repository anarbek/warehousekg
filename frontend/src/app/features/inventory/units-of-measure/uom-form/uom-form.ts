import { Component, OnInit, inject, signal } from '@angular/core';
import { Observable } from 'rxjs';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { ActivatedRoute, Router } from '@angular/router';
import { CreateUnitOfMeasureRequest } from '../../models/inventory-item.model';
import { InventoryService } from '../../services/inventory.service';

@Component({
  selector: 'app-uom-form',
  imports: [
    ReactiveFormsModule,
    MatCardModule, MatFormFieldModule, MatInputModule,
    MatButtonModule, MatIconModule, MatProgressBarModule,
  ],
  templateUrl: './uom-form.html',
  styleUrl: './uom-form.scss',
})
export class UomForm implements OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly service = inject(InventoryService);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);

  protected readonly editId = this.route.snapshot.paramMap.get('id');
  protected readonly isEditMode = this.editId !== null;
  protected readonly saving = signal(false);
  protected readonly loading = signal(false);
  protected readonly error = signal<string | null>(null);

  protected readonly title = this.isEditMode
    ? $localize`:@@uom.form.edit.title:Редактировать ед. измерения`
    : $localize`:@@uom.form.create.title:Новая ед. измерения`;

  protected readonly form = this.fb.group({
    code: ['', Validators.required],
    name: ['', Validators.required],
    description: [''],
  });

  ngOnInit(): void {
    if (!this.isEditMode) return;
    this.loading.set(true);
    this.service.getUnitOfMeasureById(this.editId!).subscribe({
      next: (uom) => { this.form.patchValue(uom); this.loading.set(false); },
      error: () => { this.error.set($localize`:@@common.loadError:Не удалось загрузить данные`); this.loading.set(false); },
    });
  }

  protected submit(): void {
    if (this.form.invalid) return;
    this.saving.set(true);
    this.error.set(null);

    const v = this.form.getRawValue();
    const request: CreateUnitOfMeasureRequest = {
      code: v.code!, name: v.name!, description: v.description || null,
    };

    const op: Observable<unknown> = this.isEditMode
      ? this.service.updateUnitOfMeasure(this.editId!, request)
      : this.service.createUnitOfMeasure(request);

    op.subscribe({
      next: () => { this.saving.set(false); void this.router.navigate(['..'], { relativeTo: this.route }); },
      error: () => { this.error.set($localize`:@@common.saveError:Не удалось сохранить данные`); this.saving.set(false); },
    });
  }

  protected cancel(): void { void this.router.navigate(['..'], { relativeTo: this.route }); }
}
