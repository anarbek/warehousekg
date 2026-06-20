import { Component, OnInit, inject, signal } from '@angular/core';
import { forkJoin, firstValueFrom, Observable } from 'rxjs';
import { FormArray, FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { DxFormModule, DxSelectBoxModule, DxDateBoxModule, DxTextBoxModule, DxButtonModule, DxProgressBarModule } from 'devextreme-angular';
import { ActivatedRoute, Router } from '@angular/router';
import { Customer } from '../../models/customer-so.model';
import { Warehouse } from '../../../inventory/models/warehouse.model';
import { InventoryItem } from '../../../inventory/models/inventory-item.model';
import { InventoryService } from '../../../inventory/services/inventory.service';
import { CustomerSoService } from '../../services/customer-so.service';
import { ErrorToastService } from '../../../../core/services/error-toast.service';

@Component({selector:'app-so-form',imports:[ReactiveFormsModule,DxFormModule,DxSelectBoxModule,DxDateBoxModule,DxTextBoxModule,DxButtonModule,DxProgressBarModule],templateUrl:'./so-form.html',styleUrl:'./so-form.scss'})
export class SoForm implements OnInit {
  private readonly fb=inject(FormBuilder);private readonly svc=inject(CustomerSoService);private readonly inv=inject(InventoryService);private readonly router=inject(Router);private readonly route=inject(ActivatedRoute);private readonly toast=inject(ErrorToastService);
  protected readonly saving=signal(false);protected readonly err=signal<string|null>(null);protected readonly editId=signal<string|null>(null);
  protected readonly items=signal<InventoryItem[]>([]);
  protected readonly headerItems=signal<any[]>([]);
  protected readonly ready=signal(false);
  protected formData:any={number:'',customerId:'',warehouseId:'',currency:'KGS',expectedDateUtc:null,notes:''};
  protected readonly form=this.fb.group({lines:this.fb.array([this.cl()])});
  get lines():FormArray{return this.form.get('lines') as FormArray}
  private cl(){return this.fb.group({inventoryItemId:['',Validators.required],quantity:[0,[Validators.required,Validators.min(1)]],unitPrice:[0,[Validators.required,Validators.min(0)]]})}
  ngOnInit(){
    const id = this.route.snapshot.paramMap.get('id');
    this.editId.set(id || null);
    forkJoin({customers:this.svc.getCustomers(),warehouses:this.inv.getWarehouses(),items:this.inv.getInventoryItems()}).subscribe({next:async d=>{
      this.items.set(d.items);
      this.headerItems.set([
        {dataField:'number',label:{text:'Номер'},isRequired:true,editorOptions:{placeholder:'SO-0001',stylingMode:'outlined'}},
        {dataField:'customerId',editorType:'dxSelectBox',label:{text:'Клиент'},isRequired:true,editorOptions:{dataSource:d.customers,displayExpr:'name',valueExpr:'id',stylingMode:'outlined'}},
        {dataField:'warehouseId',editorType:'dxSelectBox',label:{text:'Склад (опц.)'},editorOptions:{dataSource:d.warehouses,displayExpr:'name',valueExpr:'id',stylingMode:'outlined'}},
        {dataField:'currency',label:{text:'Валюта'},editorOptions:{placeholder:'KGS',stylingMode:'outlined'}},
        {dataField:'expectedDateUtc',editorType:'dxDateBox',label:{text:'Ожидаемая дата'},editorOptions:{type:'date',displayFormat:'dd.MM.yyyy',stylingMode:'outlined'}},
        {dataField:'notes',label:{text:'Примечание'},editorOptions:{stylingMode:'outlined'}},
      ]);
      // Load existing order for edit mode
      if (id) {
        try {
          const so = await firstValueFrom(this.svc.getSalesOrderById(id));
          this.formData.number = so.number;
          this.formData.customerId = so.customerId;
          this.formData.warehouseId = so.warehouseId ?? '';
          this.formData.currency = so.currency;
          this.formData.expectedDateUtc = so.expectedDateUtc ?? null;
          this.formData.notes = so.notes ?? '';
          // Populate lines
          if (so.lines && so.lines.length > 0) {
            this.lines.clear();
            for (const l of so.lines) {
              this.lines.push(this.fb.group({
                inventoryItemId: [l.inventoryItemId, Validators.required],
                quantity: [l.quantity, [Validators.required, Validators.min(1)]],
                unitPrice: [l.unitPrice, [Validators.required, Validators.min(0)]]
              }));
            }
          }
        } catch {}
      }
      this.ready.set(true);
    }});
  }
  addLine(){this.lines.push(this.cl())}
  removeLine(i:number){this.lines.removeAt(i)}
  toUtc(v:any):string|null{if(!v)return null;if(v instanceof Date)return v.toISOString();if(typeof v==='string'&&v.includes('T'))return v;return v+'T00:00:00Z'}
  submit(){
    this.saving.set(true); this.err.set(null);
    const body = {
      number: this.formData.number, customerId: this.formData.customerId,
      warehouseId: this.formData.warehouseId || null, currency: this.formData.currency || null,
      expectedDateUtc: this.toUtc(this.formData.expectedDateUtc),
      notes: this.formData.notes || null,
      lines: this.form.getRawValue().lines.map((l: any) => ({
        inventoryItemId: l.inventoryItemId!, quantity: Number(l.quantity), unitPrice: Number(l.unitPrice),
      })),
    };
    const id = this.editId();
    const req: Observable<any> = id ? this.svc.updateSalesOrder(id, body) : this.svc.createSalesOrder(body);
    req.subscribe({
      next: () => { this.saving.set(false); void this.router.navigate(['..'], { relativeTo: this.route }); },
      error: (e: any) => { this.toast.showSave(e); this.saving.set(false); },
    });
  }

  cancel(){ void this.router.navigate(['..'], { relativeTo: this.route }); }
}