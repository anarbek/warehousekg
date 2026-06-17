import { Component, OnInit, inject, signal } from '@angular/core';
import { forkJoin } from 'rxjs';
import { DxFormModule, DxSelectBoxModule, DxButtonModule, DxProgressBarModule } from 'devextreme-angular';
import { ActivatedRoute, Router } from '@angular/router';
import { InventoryService } from '../../services/inventory.service';
import { ErrorToastService } from '../../../../core/services/error-toast.service';

@Component({
  selector: 'app-inventory-item-form',
  standalone: true,
  imports: [DxFormModule, DxSelectBoxModule, DxButtonModule, DxProgressBarModule],
  templateUrl: './inventory-item-form.html',
  styleUrl: './inventory-item-form.scss',
})
export class InventoryItemForm implements OnInit {
  private readonly svc = inject(InventoryService);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly toast = inject(ErrorToastService);

  protected readonly editId = this.route.snapshot.paramMap.get('id');
  protected readonly isEdit = this.editId !== null;
  protected readonly saving = signal(false);
  protected readonly loading = signal(false);
  protected readonly err = signal<string | null>(null);

  protected formData: any = {
    sku: '', name: '', description: '', barcode: '',
    categoryId: '', unitOfMeasureId: '',
    warehouseId: '', initialQuantity: 0,
    reorderLevel: 0, isActive: true,
  };

  protected readonly formItems = signal<any[]>([]);
  protected readonly ready = signal(false);

  ngOnInit() {
    this.loading.set(true);
    forkJoin({
      c: this.svc.getItemCategories(),
      u: this.svc.getUnitsOfMeasure(),
      w: this.svc.getWarehouses(),
    }).subscribe({
      next: (d) => {
        const cats = d.c; const uoms = d.u; const whs = d.w;

        const items: any[] = [
          { dataField: 'sku', label: { text: 'SKU' }, isRequired: true, editorOptions: { stylingMode: 'outlined' } },
          { dataField: 'name', label: { text: 'Название' }, isRequired: true, editorOptions: { stylingMode: 'outlined' } },
          { dataField: 'description', label: { text: 'Описание' }, editorOptions: { stylingMode: 'outlined' } },
          { dataField: 'barcode', label: { text: 'Штрихкод' }, editorOptions: { stylingMode: 'outlined' } },
          { dataField: 'categoryId', editorType: 'dxSelectBox', label: { text: 'Категория' }, isRequired: true, editorOptions: { dataSource: cats, displayExpr: 'name', valueExpr: 'id', stylingMode: 'outlined' } },
          { dataField: 'unitOfMeasureId', editorType: 'dxSelectBox', label: { text: 'Ед. измерения' }, isRequired: true, editorOptions: { dataSource: uoms, displayExpr: 'name', valueExpr: 'id', stylingMode: 'outlined' } },
          { dataField: 'reorderLevel', label: { text: 'Мин. остаток' }, editorOptions: { stylingMode: 'outlined' } },
          { dataField: 'isActive', editorType: 'dxCheckBox', label: { text: 'Активен' } },
        ];

        // Only show warehouse + initial quantity for new items
        if (!this.isEdit) {
          items.push(
            { dataField: 'warehouseId', editorType: 'dxSelectBox', label: { text: 'Склад (начальный)' }, isRequired: true, editorOptions: { dataSource: whs, displayExpr: 'name', valueExpr: 'id', stylingMode: 'outlined' } },
            { dataField: 'initialQuantity', label: { text: 'Начальное кол-во' }, isRequired: true, editorOptions: { stylingMode: 'outlined', min: 1 } },
          );
        }

        this.formItems.set(items);
        this.ready.set(true);
        this.loading.set(false);

        if (this.isEdit) {
          this.svc.getInventoryItemById(this.editId!).subscribe({
            next: (it) => { this.formData = it; },
            error: (e) => { this.toast.showLoad(e); },
          });
        }
      },
      error: (e) => {
        this.toast.showLoad(e);
        this.loading.set(false);
      },
    });
  }

  submit() {
    this.saving.set(true); this.err.set(null);
    const r: any = {
      sku: this.formData.sku, name: this.formData.name,
      description: this.formData.description || null,
      barcode: this.formData.barcode || null,
      categoryId: this.formData.categoryId,
      unitOfMeasureId: this.formData.unitOfMeasureId,
      reorderLevel: Number(this.formData.reorderLevel ?? 0),
      isActive: this.formData.isActive,
    };

    const done = () => { this.saving.set(false); void this.router.navigate(['..'], { relativeTo: this.route }); };
    const fail = (e: any) => { this.toast.showSave(e); this.saving.set(false); };

    if (this.isEdit) {
      this.svc.updateInventoryItem(this.editId!, r).subscribe({ next: done, error: fail });
    } else {
      r.warehouseId = this.formData.warehouseId;
      r.initialQuantity = Number(this.formData.initialQuantity ?? 0);
      this.svc.createInventoryItem(r).subscribe({ next: done, error: fail });
    }
  }

  cancel() { void this.router.navigate(['..'], { relativeTo: this.route }); }
}