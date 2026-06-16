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
import { Customer } from '../../models/customer-so.model';
import { Warehouse } from '../../../inventory/models/warehouse.model';
import { InventoryItem } from '../../../inventory/models/inventory-item.model';
import { InventoryService } from '../../../inventory/services/inventory.service';
import { CustomerSoService } from '../../services/customer-so.service';

@Component({selector:'app-so-form',imports:[ReactiveFormsModule,MatCardModule,MatFormFieldModule,MatInputModule,MatSelectModule,MatButtonModule,MatIconModule,MatProgressBarModule],templateUrl:'./so-form.html',styleUrl:'./so-form.scss'})
export class SoForm implements OnInit {
  private readonly fb=inject(FormBuilder);private readonly svc=inject(CustomerSoService);private readonly inventorySvc=inject(InventoryService);private readonly router=inject(Router);private readonly route=inject(ActivatedRoute);
  protected readonly saving=signal(false);protected readonly error=signal<string|null>(null);
  protected readonly customers=signal<Customer[]>([]);protected readonly warehouses=signal<Warehouse[]>([]);protected readonly items=signal<InventoryItem[]>([]);
  protected readonly form=this.fb.group({number:['',Validators.required],customerId:['',Validators.required],warehouseId:[''],currency:['KGS'],expectedDateUtc:[''],notes:[''],lines:this.fb.array([this.mkLine()])});
  get lines():FormArray{return this.form.get('lines') as FormArray}
  private mkLine(){return this.fb.group({inventoryItemId:['',Validators.required],quantity:[0,[Validators.required,Validators.min(1)]],unitPrice:[0,[Validators.required,Validators.min(0)]]})}
  ngOnInit():void{forkJoin({customers:this.svc.getCustomers(),warehouses:this.inventorySvc.getWarehouses(),items:this.inventorySvc.getInventoryItems()}).subscribe({next:d=>{this.customers.set(d.customers);this.warehouses.set(d.warehouses);this.items.set(d.items)}})}
  protected addLine():void{this.lines.push(this.mkLine())}
  protected removeLine(i:number):void{this.lines.removeAt(i)}
  protected submit():void{if(this.form.invalid)return;this.saving.set(true);this.error.set(null);const v=this.form.getRawValue();this.svc.createSalesOrder({number:v.number!,customerId:v.customerId!,warehouseId:v.warehouseId||null,currency:v.currency||null,expectedDateUtc:this.toUtc(v.expectedDateUtc),notes:v.notes||null,lines:v.lines.map((l:any)=>({inventoryItemId:l.inventoryItemId!,quantity:Number(l.quantity),unitPrice:Number(l.unitPrice)}))}).subscribe({next:()=>{this.saving.set(false);void this.router.navigate(['..'],{relativeTo:this.route})},error:()=>{this.error.set($localize`:@@common.saveError:Не удалось сохранить данные`);this.saving.set(false)}})}
  protected cancel():void{void this.router.navigate(['..'],{relativeTo:this.route})}
  private toUtc(v: string|null|undefined): string|null { if(!v) return null; return v + 'T00:00:00Z'; }
}
