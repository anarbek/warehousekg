import { Component, OnInit, inject, signal } from '@angular/core';
import { forkJoin } from 'rxjs';
import { FormArray, FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { DxFormModule, DxSelectBoxModule, DxTextBoxModule, DxButtonModule, DxProgressBarModule } from 'devextreme-angular';
import { ActivatedRoute, Router } from '@angular/router';
import { Warehouse } from '../../inventory/models/warehouse.model';
import { InventoryItem } from '../../inventory/models/inventory-item.model';
import { InventoryService } from '../../inventory/services/inventory.service';
import { AdjustmentsService } from '../services/adjustments.service';

const REASONS = [{label:'Correction',value:0},{label:'Damage',value:1},{label:'Loss',value:2},{label:'Theft',value:3},{label:'Found',value:4},{label:'Expired',value:5},{label:'Other',value:6}];

@Component({selector:'app-adjustment-form',imports:[ReactiveFormsModule,DxFormModule,DxSelectBoxModule,DxTextBoxModule,DxButtonModule,DxProgressBarModule],templateUrl:'./adjustment-form.html',styleUrl:'./adjustment-form.scss'})
export class AdjustmentForm implements OnInit {
  private readonly fb=inject(FormBuilder);private readonly svc=inject(AdjustmentsService);private readonly inv=inject(InventoryService);private readonly router=inject(Router);private readonly route=inject(ActivatedRoute);
  protected readonly saving=signal(false);protected readonly err=signal<string|null>(null);
  protected readonly items=signal<InventoryItem[]>([]);
  protected readonly headerItems=signal<any[]>([]);
  protected readonly ready=signal(false);
  protected formData:any={number:'',warehouseId:'',reason:0,notes:''};
  protected readonly form=this.fb.group({lines:this.fb.array([this.cl()])});
  get lines():FormArray{return this.form.get('lines') as FormArray}
  private cl(){return this.fb.group({inventoryItemId:['',Validators.required],quantityChange:[0,[Validators.required]],notes:['']})}
  ngOnInit(){forkJoin({w:this.inv.getWarehouses(),i:this.inv.getInventoryItems()}).subscribe({next:d=>{
    this.items.set(d.i);
    this.headerItems.set([
      {dataField:'number',label:{text:'Номер'},isRequired:true,editorOptions:{placeholder:'ADJ-0001',stylingMode:'outlined'}},
      {dataField:'warehouseId',editorType:'dxSelectBox',label:{text:'Склад'},isRequired:true,editorOptions:{dataSource:d.w,displayExpr:'name',valueExpr:'id',stylingMode:'outlined'}},
      {dataField:'reason',editorType:'dxSelectBox',label:{text:'Причина'},isRequired:true,editorOptions:{dataSource:REASONS,displayExpr:'label',valueExpr:'value',stylingMode:'outlined'}},
      {dataField:'notes',label:{text:'Примечание'},editorOptions:{stylingMode:'outlined'}},
    ]);
    this.ready.set(true);
  }})}
  addLine(){this.lines.push(this.cl())}
  removeLine(i:number){this.lines.removeAt(i)}
  submit(){this.saving.set(true);this.err.set(null);
    this.svc.createAdjustment({number:this.formData.number,warehouseId:this.formData.warehouseId,reason:Number(this.formData.reason),notes:this.formData.notes||null,lines:this.form.getRawValue().lines.map((l:any)=>({inventoryItemId:l.inventoryItemId!,quantityChange:Number(l.quantityChange),notes:l.notes||null}))}).subscribe({next:()=>{this.saving.set(false);void this.router.navigate(['..'],{relativeTo:this.route})},error:()=>{this.err.set($localize`:@@common.saveError:Не удалось сохранить данные`);this.saving.set(false)}})}
  cancel(){void this.router.navigate(['..'],{relativeTo:this.route})}
}