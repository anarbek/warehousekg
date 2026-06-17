import { Component, OnInit, inject, signal } from '@angular/core';
import { DatePipe, LowerCasePipe, DecimalPipe } from '@angular/common';
import { forkJoin, Observable } from 'rxjs';
import { DxDataGridModule, DxButtonModule, DxProgressBarModule } from 'devextreme-angular';
import { ActivatedRoute, Router } from '@angular/router';
import { PurchaseOrder, PurchaseOrderStatus } from '../../models/supplier-po.model';
import { InventoryItem } from '../../../inventory/models/inventory-item.model';
import { InventoryService } from '../../../inventory/services/inventory.service';
import { SupplierPoService } from '../../services/supplier-po.service';

@Component({selector:'app-po-detail',imports:[DatePipe,LowerCasePipe,DecimalPipe,DxDataGridModule,DxButtonModule,DxProgressBarModule],templateUrl:'./po-detail.html',styleUrl:'./po-detail.scss'})
export class PoDetail implements OnInit {
  private readonly svc=inject(SupplierPoService);private readonly inv=inject(InventoryService);private readonly route=inject(ActivatedRoute);private readonly router=inject(Router);
  protected readonly id=this.route.snapshot.paramMap.get('id')!;protected readonly po=signal<PurchaseOrder|null>(null);
  protected readonly loading=signal(false);protected readonly error=signal<string|null>(null);protected readonly items=signal<InventoryItem[]>([]);protected readonly saving=signal(false);
  ngOnInit(){this.load()}
  load(){this.loading.set(true);this.error.set(null);forkJoin({po:this.svc.getPurchaseOrderById(this.id),items:this.inv.getInventoryItems()}).subscribe({next:({po,items})=>{this.po.set(po);this.items.set(items);this.loading.set(false)},error:()=>{this.error.set($localize`:@@common.loadError:Не удалось загрузить данные`);this.loading.set(false)}})}
  canTransition(t:PurchaseOrderStatus):boolean{const s=this.po()?.status;if(!s)return false;if(t==='Submitted')return s==='Draft';if(t==='Received')return s==='Submitted';if(t==='Cancelled')return s==='Draft'||s==='Submitted';return false}
  transition(a:'submit'|'receive'|'cancel'){this.saving.set(true);const op:Observable<void>=a==='submit'?this.svc.submitPO(this.id):a==='receive'?this.svc.receivePO(this.id):this.svc.cancelPO(this.id);op.subscribe({next:()=>{this.saving.set(false);this.load()},error:()=>{this.saving.set(false);this.load()}})}
  goBack(){void this.router.navigate(['..'],{relativeTo:this.route})}
  getItemName(id:string):string{const it=this.items().find(i=>i.id===id);return it?`${it.sku} — ${it.name}`:id}
  getLines(){const p=this.po();if(!p)return[];return p.lines.map(l=>({...l,inventoryItemName:this.getItemName(l.inventoryItemId)}))}
}