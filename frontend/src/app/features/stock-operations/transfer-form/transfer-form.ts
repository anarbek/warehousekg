import { Component, OnInit, inject, signal } from '@angular/core';
import { forkJoin } from 'rxjs';
import { FormArray, FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { DxFormModule, DxSelectBoxModule, DxTextBoxModule, DxDateBoxModule, DxButtonModule, DxProgressBarModule } from 'devextreme-angular';
import { ActivatedRoute, Router } from '@angular/router';
import { Warehouse } from '../../inventory/models/warehouse.model';
import { InventoryItem } from '../../inventory/models/inventory-item.model';
import { InventoryService } from '../../inventory/services/inventory.service';
import { StockOperationsService } from '../services/stock-operations.service';
import { ErrorToastService } from '../../../core/services/error-toast.service';
import { PersonnelService } from '../../personnel/services/personnel.service';

@Component({selector:'app-transfer-form',imports:[ReactiveFormsModule,DxFormModule,DxSelectBoxModule,DxTextBoxModule,DxDateBoxModule,DxButtonModule,DxProgressBarModule],templateUrl:'./transfer-form.html',styleUrl:'./transfer-form.scss'})
export class TransferForm implements OnInit {
  private readonly fb=inject(FormBuilder);private readonly svc=inject(StockOperationsService);private readonly inv=inject(InventoryService);private readonly personnel=inject(PersonnelService);private readonly router=inject(Router);private readonly route=inject(ActivatedRoute);private readonly toast=inject(ErrorToastService);
  protected readonly saving=signal(false);protected readonly err=signal<string|null>(null);
  protected readonly items=signal<InventoryItem[]>([]);
  protected readonly headerItems=signal<any[]>([]);
  protected readonly ready=signal(false);
  protected formData:any={number:'',sourceWarehouseId:'',destinationWarehouseId:'',notes:'',transferredAtUtc:new Date(),employeeId:null};
  protected readonly form=this.fb.group({lines:this.fb.array([this.cl()])});
  get lines():FormArray{return this.form.get('lines') as FormArray}
  private cl(){return this.fb.group({inventoryItemId:['',Validators.required],quantity:[0,[Validators.required,Validators.min(1)]]})}
  ngOnInit(){forkJoin({warehouses:this.inv.getWarehouses(),items:this.inv.getInventoryItems(),employees:this.personnel.getEmployees()}).subscribe({next:d=>{
    this.items.set(d.items);
    this.headerItems.set([
      {dataField:'number',label:{text:'Номер'},isRequired:true,editorOptions:{placeholder:'TRF-0001',stylingMode:'outlined'}},
      {dataField:'sourceWarehouseId',editorType:'dxSelectBox',label:{text:'Склад-источник'},isRequired:true,editorOptions:{dataSource:d.warehouses,displayExpr:'name',valueExpr:'id',stylingMode:'outlined'}},
      {dataField:'destinationWarehouseId',editorType:'dxSelectBox',label:{text:'Склад-назначение'},isRequired:true,editorOptions:{dataSource:d.warehouses,displayExpr:'name',valueExpr:'id',stylingMode:'outlined'}},
      {dataField:'employeeId',editorType:'dxSelectBox',label:{text:'Сотрудник (опц.)'},editorOptions:{dataSource:d.employees,displayExpr:'lastName',valueExpr:'id',placeholder:'Не указан',stylingMode:'outlined'}},
      {dataField:'notes',label:{text:'Примечание'},editorOptions:{stylingMode:'outlined'}},
      {dataField:'transferredAtUtc',editorType:'dxDateBox',label:{text:'Дата перемещения'},editorOptions:{type:'date',displayFormat:'dd.MM.yyyy',stylingMode:'outlined'}},
    ]);
    this.ready.set(true);
  }})}
  addLine(){this.lines.push(this.cl())}
  removeLine(i:number){this.lines.removeAt(i)}
  submit(){this.saving.set(true);this.err.set(null);
    this.svc.createStockTransfer({number:this.formData.number,sourceWarehouseId:this.formData.sourceWarehouseId,destinationWarehouseId:this.formData.destinationWarehouseId,notes:this.formData.notes||null,transferredAtUtc:this.formData.transferredAtUtc?new Date(this.formData.transferredAtUtc).toISOString():null,employeeId:this.formData.employeeId||null,lines:this.form.getRawValue().lines.map((l:any)=>({inventoryItemId:l.inventoryItemId!,quantity:Number(l.quantity)}))}).subscribe({next:()=>{this.saving.set(false);void this.router.navigate(['..'],{relativeTo:this.route})},error:(e)=>{this.toast.showSave(e);this.saving.set(false)}})}
  cancel(){void this.router.navigate(['..'],{relativeTo:this.route})}
}