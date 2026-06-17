import { Component, OnInit, inject, signal } from '@angular/core';
import { DatePipe, LowerCasePipe, DecimalPipe } from '@angular/common';
import { forkJoin, Observable } from 'rxjs';
import { DxDataGridModule, DxButtonModule, DxProgressBarModule } from 'devextreme-angular';
import { ActivatedRoute, Router } from '@angular/router';
import { SalesOrder, SalesOrderStatus } from '../../models/customer-so.model';
import { InventoryItem } from '../../../inventory/models/inventory-item.model';
import { InventoryService } from '../../../inventory/services/inventory.service';
import { CustomerSoService } from '../../services/customer-so.service';

@Component({selector:'app-so-detail',imports:[DatePipe,LowerCasePipe,DecimalPipe,DxDataGridModule,DxButtonModule,DxProgressBarModule],templateUrl:'./so-detail.html',styleUrl:'./so-detail.scss'})
export class SoDetail implements OnInit {
  private readonly svc=inject(CustomerSoService);private readonly inv=inject(InventoryService);private readonly route=inject(ActivatedRoute);private readonly router=inject(Router);
  protected readonly id=this.route.snapshot.paramMap.get('id')!;protected readonly so=signal<SalesOrder|null>(null);
  protected readonly loading=signal(false);protected readonly error=signal<string|null>(null);protected readonly items=signal<InventoryItem[]>([]);protected readonly saving=signal(false);
  ngOnInit(){this.load()}
  load(){this.loading.set(true);this.error.set(null);forkJoin({so:this.svc.getSalesOrderById(this.id),items:this.inv.getInventoryItems()}).subscribe({next:({so,items})=>{this.so.set(so);this.items.set(items);this.loading.set(false)},error:()=>{this.error.set($localize`:@@common.loadError:Не удалось загрузить данные`);this.loading.set(false)}})}
  canTransition(t:SalesOrderStatus):boolean{const s=this.so()?.status;if(!s)return false;if(t==='Confirmed')return s==='Draft';if(t==='Shipped')return s==='Confirmed';if(t==='Cancelled')return s==='Draft'||s==='Confirmed';return false}
  transition(a:'confirm'|'ship'|'cancel'){this.saving.set(true);const op:Observable<void>=a==='confirm'?this.svc.confirmSO(this.id):a==='ship'?this.svc.shipSO(this.id):this.svc.cancelSO(this.id);op.subscribe({next:()=>{this.saving.set(false);this.load()},error:()=>{this.saving.set(false);this.load()}})}
  goBack(){void this.router.navigate(['..'],{relativeTo:this.route})}
  getItemName(id:string):string{const it=this.items().find(i=>i.id===id);return it?`${it.sku} — ${it.name}`:id}
  getLines(){const s=this.so();if(!s)return[];return s.lines.map(l=>({...l,inventoryItemName:this.getItemName(l.inventoryItemId)}))}
}