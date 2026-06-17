import { Component, OnInit, inject, signal } from '@angular/core';
import { forkJoin } from 'rxjs';
import { FormArray, FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { DxFormModule, DxSelectBoxModule, DxDateBoxModule, DxTextBoxModule, DxButtonModule, DxProgressBarModule } from 'devextreme-angular';
import { ActivatedRoute, Router } from '@angular/router';
import { Customer } from '../../models/customer-so.model';
import { Warehouse } from '../../../inventory/models/warehouse.model';
import { InventoryItem } from '../../../inventory/models/inventory-item.model';
import { InventoryService } from '../../../inventory/services/inventory.service';
import { CustomerSoService } from '../../services/customer-so.service';

@Component({selector:'app-so-form',imports:[ReactiveFormsModule,DxFormModule,DxSelectBoxModule,DxDateBoxModule,DxTextBoxModule,DxButtonModule,DxProgressBarModule],templateUrl:'./so-form.html',styleUrl:'./so-form.scss'})
export class SoForm implements OnInit {
  private readonly fb=inject(FormBuilder);private readonly svc=inject(CustomerSoService);private readonly inv=inject(InventoryService);private readonly router=inject(Router);private readonly route=inject(ActivatedRoute);
  protected readonly saving=signal(false);protected readonly err=signal<string|null>(null);
  protected readonly items=signal<InventoryItem[]>([]);
  protected readonly headerItems=signal<any[]>([]);
  protected readonly ready=signal(false);
  protected formData:any={number:'',customerId:'',warehouseId:'',currency:'KGS',expectedDateUtc:null,notes:''};
  protected readonly form=this.fb.group({lines:this.fb.array([this.cl()])});
  get lines():FormArray{return this.form.get('lines') as FormArray}
  private cl(){return this.fb.group({inventoryItemId:['',Validators.required],quantity:[0,[Validators.required,Validators.min(1)]],unitPrice:[0,[Validators.required,Validators.min(0)]]})}
  ngOnInit(){forkJoin({customers:this.svc.getCustomers(),warehouses:this.inv.getWarehouses(),items:this.inv.getInventoryItems()}).subscribe({next:d=>{
    this.items.set(d.items);
    this.headerItems.set([
      {dataField:'number',label:{text:'Номер'},isRequired:true,editorOptions:{placeholder:'SO-0001',stylingMode:'outlined'}},
      {dataField:'customerId',editorType:'dxSelectBox',label:{text:'Клиент'},isRequired:true,editorOptions:{dataSource:d.customers,displayExpr:'name',valueExpr:'id',stylingMode:'outlined'}},
      {dataField:'warehouseId',editorType:'dxSelectBox',label:{text:'Склад (опц.)'},editorOptions:{dataSource:d.warehouses,displayExpr:'name',valueExpr:'id',stylingMode:'outlined'}},
      {dataField:'currency',label:{text:'Валюта'},editorOptions:{placeholder:'KGS',stylingMode:'outlined'}},
      {dataField:'expectedDateUtc',editorType:'dxDateBox',label:{text:'Ожидаемая дата'},editorOptions:{type:'date',displayFormat:'dd.MM.yyyy',stylingMode:'outlined'}},
      {dataField:'notes',label:{text:'Примечание'},editorOptions:{stylingMode:'outlined'}},
    ]);
    this.ready.set(true);
  }})}
  addLine(){this.lines.push(this.cl())}
  removeLine(i:number){this.lines.removeAt(i)}
  toUtc(v:any):string|null{if(!v)return null;if(v instanceof Date)return v.toISOString();return v+'T00:00:00Z'}
  submit(){this.saving.set(true);this.err.set(null);
    this.svc.createSalesOrder({number:this.formData.number,customerId:this.formData.customerId,warehouseId:this.formData.warehouseId||null,currency:this.formData.currency||null,expectedDateUtc:this.toUtc(this.formData.expectedDateUtc),notes:this.formData.notes||null,lines:this.form.getRawValue().lines.map((l:any)=>({inventoryItemId:l.inventoryItemId!,quantity:Number(l.quantity),unitPrice:Number(l.unitPrice)}))}).subscribe({next:()=>{this.saving.set(false);void this.router.navigate(['..'],{relativeTo:this.route})},error:()=>{this.err.set($localize`:@@common.saveError:Не удалось сохранить данные`);this.saving.set(false)}})}
  cancel(){void this.router.navigate(['..'],{relativeTo:this.route})}
}