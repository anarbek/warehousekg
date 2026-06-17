import { Component, OnInit, inject, signal } from '@angular/core';
import { forkJoin } from 'rxjs';
import { FormArray, FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { DxFormModule, DxSelectBoxModule, DxTextBoxModule, DxButtonModule, DxProgressBarModule } from 'devextreme-angular';
import { ActivatedRoute, Router } from '@angular/router';
import { Warehouse } from '../../inventory/models/warehouse.model';
import { InventoryItem } from '../../inventory/models/inventory-item.model';
import { InventoryService } from '../../inventory/services/inventory.service';
import { StockOperationsService } from '../services/stock-operations.service';

@Component({selector:'app-picking-form',imports:[ReactiveFormsModule,DxFormModule,DxSelectBoxModule,DxTextBoxModule,DxButtonModule,DxProgressBarModule],templateUrl:'./picking-form.html',styleUrl:'./picking-form.scss'})
export class PickingForm implements OnInit {
  private readonly fb=inject(FormBuilder);private readonly svc=inject(StockOperationsService);private readonly inv=inject(InventoryService);private readonly router=inject(Router);private readonly route=inject(ActivatedRoute);
  protected readonly saving=signal(false);protected readonly err=signal<string|null>(null);
  protected readonly items=signal<InventoryItem[]>([]);
  protected readonly headerItems=signal<any[]>([]);
  protected readonly ready=signal(false);
  protected formData:any={number:'',warehouseId:'',reference:'',notes:''};
  protected readonly form=this.fb.group({lines:this.fb.array([this.cl()])});
  get lines():FormArray{return this.form.get('lines') as FormArray}
  private cl(){return this.fb.group({inventoryItemId:['',Validators.required],quantity:[0,[Validators.required,Validators.min(1)]],warehouseLocationId:['']})}
  ngOnInit(){forkJoin({warehouses:this.inv.getWarehouses(),items:this.inv.getInventoryItems()}).subscribe({next:d=>{
    this.items.set(d.items);
    this.headerItems.set([
      {dataField:'number',label:{text:'Номер'},isRequired:true,editorOptions:{placeholder:'PICK-0001',stylingMode:'outlined'}},
      {dataField:'warehouseId',editorType:'dxSelectBox',label:{text:'Склад'},isRequired:true,editorOptions:{dataSource:d.warehouses,displayExpr:'name',valueExpr:'id',stylingMode:'outlined'}},
      {dataField:'reference',label:{text:'Основание (опц.)'},editorOptions:{stylingMode:'outlined'}},
      {dataField:'notes',label:{text:'Примечание'},editorOptions:{stylingMode:'outlined'}},
    ]);
    this.ready.set(true);
  }})}
  addLine(){this.lines.push(this.cl())}
  removeLine(i:number){this.lines.removeAt(i)}
  submit(){this.saving.set(true);this.err.set(null);
    this.svc.createPickOrder({number:this.formData.number,warehouseId:this.formData.warehouseId,reference:this.formData.reference||null,notes:this.formData.notes||null,lines:this.form.getRawValue().lines.map((l:any)=>({inventoryItemId:l.inventoryItemId!,quantity:Number(l.quantity),warehouseLocationId:l.warehouseLocationId||null}))}).subscribe({next:()=>{this.saving.set(false);void this.router.navigate(['..'],{relativeTo:this.route})},error:()=>{this.err.set($localize`:@@common.saveError:Не удалось сохранить данные`);this.saving.set(false)}})}
  cancel(){void this.router.navigate(['..'],{relativeTo:this.route})}
}