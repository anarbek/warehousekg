import { Component, OnInit, inject, signal } from '@angular/core';
import { forkJoin } from 'rxjs';
import { FormArray, FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { MatSelectModule } from '@angular/material/select';
import { ActivatedRoute, Router } from '@angular/router';
import { AdjustmentReason } from '../models/adjustment.model';
import { Warehouse } from '../../inventory/models/warehouse.model';
import { InventoryItem } from '../../inventory/models/inventory-item.model';
import { InventoryService } from '../../inventory/services/inventory.service';
import { AdjustmentsService } from '../services/adjustments.service';

const REASONS = [
  { label: 'Correction', value: 0 },
  { label: 'Damage', value: 1 },
  { label: 'Loss', value: 2 },
  { label: 'Theft', value: 3 },
  { label: 'Found', value: 4 },
  { label: 'Expired', value: 5 },
  { label: 'Other', value: 6 },
];

@Component({selector:'app-adjustment-form',imports:[ReactiveFormsModule,MatCardModule,MatFormFieldModule,MatInputModule,MatSelectModule,MatButtonModule,MatIconModule,MatProgressBarModule],templateUrl:'./adjustment-form.html',styleUrl:'./adjustment-form.scss'})
export class AdjustmentForm implements OnInit {
  private readonly fb=inject(FormBuilder);private readonly svc=inject(AdjustmentsService);private readonly inventorySvc=inject(InventoryService);private readonly router=inject(Router);private readonly route=inject(ActivatedRoute);
  protected readonly saving=signal(false);protected readonly error=signal<string|null>(null);
  protected readonly reasons=REASONS;
  protected readonly warehouses=signal<Warehouse[]>([]);protected readonly items=signal<InventoryItem[]>([]);
  protected readonly form=this.fb.group({number:['',Validators.required],warehouseId:['',Validators.required],reason:[0,Validators.required],notes:[''],lines:this.fb.array([this.mkLine()])});
  get lines():FormArray{return this.form.get('lines') as FormArray}
  private mkLine(){return this.fb.group({inventoryItemId:['',Validators.required],quantityChange:[0,[Validators.required]],notes:['']})}
  ngOnInit():void{forkJoin({w:this.inventorySvc.getWarehouses(),i:this.inventorySvc.getInventoryItems()}).subscribe({next:d=>{this.warehouses.set(d.w);this.items.set(d.i)}})}
  protected addLine():void{this.lines.push(this.mkLine())}
  protected removeLine(i:number):void{this.lines.removeAt(i)}
  protected submit():void{if(this.form.invalid)return;this.saving.set(true);this.error.set(null);const v=this.form.getRawValue();this.svc.createAdjustment({number:v.number!,warehouseId:v.warehouseId!,reason:Number(v.reason),notes:v.notes||null,lines:v.lines.map((l:any)=>({inventoryItemId:l.inventoryItemId!,quantityChange:Number(l.quantityChange),notes:l.notes||null}))}).subscribe({next:()=>{this.saving.set(false);void this.router.navigate(['..'],{relativeTo:this.route})},error:()=>{this.error.set($localize`:@@common.saveError:Не удалось сохранить данные`);this.saving.set(false)}})}
  protected cancel():void{void this.router.navigate(['..'],{relativeTo:this.route})}
}
