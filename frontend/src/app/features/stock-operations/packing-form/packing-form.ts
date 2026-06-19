import { Component, OnInit, inject, signal } from '@angular/core';
import { forkJoin } from 'rxjs';
import { FormArray, FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { DxFormModule, DxSelectBoxModule, DxTextBoxModule, DxDateBoxModule, DxButtonModule, DxProgressBarModule } from 'devextreme-angular';
import { ActivatedRoute, Router } from '@angular/router';
import { Warehouse } from '../../inventory/models/warehouse.model';
import { InventoryItem } from '../../inventory/models/inventory-item.model';
import { InventoryService } from '../../inventory/services/inventory.service';
import { PickOrder } from '../models/stock-operation.model';
import { StockOperationsService } from '../services/stock-operations.service';
import { ErrorToastService } from '../../../core/services/error-toast.service';
import { PersonnelService } from '../../personnel/services/personnel.service';

@Component({selector:'app-packing-form',imports:[ReactiveFormsModule,DxFormModule,DxSelectBoxModule,DxTextBoxModule,DxDateBoxModule,DxButtonModule,DxProgressBarModule],templateUrl:'./packing-form.html',styleUrl:'./packing-form.scss'})
export class PackingForm implements OnInit {
  private readonly fb=inject(FormBuilder);private readonly svc=inject(StockOperationsService);private readonly inv=inject(InventoryService);private readonly personnel=inject(PersonnelService);private readonly router=inject(Router);private readonly route=inject(ActivatedRoute);private readonly toast=inject(ErrorToastService);
  protected readonly saving=signal(false);protected readonly err=signal<string|null>(null);
  protected readonly items=signal<InventoryItem[]>([]);
  protected readonly headerItems=signal<any[]>([]);
  protected readonly ready=signal(false);
  protected formData:any={number:'',warehouseId:'',pickOrderId:'',notes:'',actualPackDate:null,employeeId:null};
  protected readonly form=this.fb.group({lines:this.fb.array([this.cl()])});
  get lines():FormArray{return this.form.get('lines') as FormArray}
  private cl(){return this.fb.group({inventoryItemId:['',Validators.required],quantity:[0,[Validators.required,Validators.min(1)]],packageLabel:['']})}
  ngOnInit(){forkJoin({warehouses:this.inv.getWarehouses(),items:this.inv.getInventoryItems(),pickOrders:this.svc.getPickOrders(),employees:this.personnel.getEmployees()}).subscribe({next:d=>{
    this.items.set(d.items);
    this.headerItems.set([
      {dataField:'number',label:{text:'Номер'},isRequired:true,editorOptions:{placeholder:'PACK-0001',stylingMode:'outlined'}},
      {dataField:'warehouseId',editorType:'dxSelectBox',label:{text:'Склад'},isRequired:true,editorOptions:{dataSource:d.warehouses,displayExpr:'name',valueExpr:'id',stylingMode:'outlined'}},
      {dataField:'pickOrderId',editorType:'dxSelectBox',label:{text:'ID заказа на сборку'},editorOptions:{dataSource:d.pickOrders,displayExpr:(item:any)=>item?`${item.number} — ${item.warehouseName||item.warehouseId}`:'',valueExpr:'id',searchEnabled:true,searchExpr:'number',placeholder:'Поиск по номеру...',stylingMode:'outlined'}},
      {dataField:'employeeId',editorType:'dxSelectBox',label:{text:'Сотрудник (опц.)'},editorOptions:{dataSource:d.employees,displayExpr:'lastName',valueExpr:'id',placeholder:'Не указан',stylingMode:'outlined'}},
      {dataField:'notes',editorType:'dxTextArea',label:{text:'Примечание'},editorOptions:{stylingMode:'outlined',height:80}},
      {dataField:'actualPackDate',editorType:'dxDateBox',label:{text:'Факт. дата упаковки'},editorOptions:{type:'date',displayFormat:'dd.MM.yyyy',stylingMode:'outlined'}},
    ]);
    this.ready.set(true);
  }})}
  addLine(){this.lines.push(this.cl())}
  removeLine(i:number){this.lines.removeAt(i)}
  submit(){this.saving.set(true);this.err.set(null);
    this.svc.createPackOrder({number:this.formData.number,warehouseId:this.formData.warehouseId,pickOrderId:this.formData.pickOrderId||null,notes:this.formData.notes||null,actualPackDate:this.formData.actualPackDate?new Date(this.formData.actualPackDate).toISOString():null,employeeId:this.formData.employeeId||null,lines:this.form.getRawValue().lines.map((l:any)=>({inventoryItemId:l.inventoryItemId!,quantity:Number(l.quantity),packageLabel:l.packageLabel||null}))}).subscribe({next:()=>{this.saving.set(false);void this.router.navigate(['..'],{relativeTo:this.route})},error:(e)=>{this.toast.showSave(e);this.saving.set(false)}})}
  cancel(){void this.router.navigate(['..'],{relativeTo:this.route})}
}
