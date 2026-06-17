import { Component, OnInit, inject, signal } from '@angular/core';
import { forkJoin, Observable } from 'rxjs';
import { DatePipe, LowerCasePipe } from '@angular/common';
import { DxDataGridModule, DxButtonModule, DxProgressBarModule } from 'devextreme-angular';
import { ActivatedRoute, Router } from '@angular/router';
import { StockAdjustment } from '../models/adjustment.model';
import { InventoryItem } from '../../inventory/models/inventory-item.model';
import { InventoryService } from '../../inventory/services/inventory.service';
import { AdjustmentsService } from '../services/adjustments.service';

@Component({selector:'app-adjustment-detail',imports:[DatePipe,LowerCasePipe,DxDataGridModule,DxButtonModule,DxProgressBarModule],templateUrl:'./adjustment-detail.html',styleUrl:'./adjustment-detail.scss'})
export class AdjustmentDetail implements OnInit {
  private readonly svc=inject(AdjustmentsService);private readonly inv=inject(InventoryService);private readonly route=inject(ActivatedRoute);private readonly router=inject(Router);
  protected readonly id=this.route.snapshot.paramMap.get('id')!;protected readonly adj=signal<StockAdjustment|null>(null);
  protected readonly loading=signal(false);protected readonly error=signal<string|null>(null);protected readonly items=signal<InventoryItem[]>([]);protected readonly saving=signal(false);
  ngOnInit(){this.load()}
  load(){this.loading.set(true);this.error.set(null);forkJoin({adj:this.svc.getAdjustmentById(this.id),items:this.inv.getInventoryItems()}).subscribe({next:({adj,items})=>{this.adj.set(adj);this.items.set(items);this.loading.set(false)},error:()=>{this.error.set($localize`:@@common.loadError:Не удалось загрузить данные`);this.loading.set(false)}})}
  isDraft(){return this.adj()?.status==='Draft'}
  complete(){this.saving.set(true);this.svc.completeAdjustment(this.id).subscribe({next:()=>{this.saving.set(false);this.load()},error:()=>{this.saving.set(false)}})}
  cancel(){this.saving.set(true);this.svc.cancelAdjustment(this.id).subscribe({next:()=>{this.saving.set(false);this.load()},error:()=>{this.saving.set(false)}})}
  goBack(){void this.router.navigate(['..'],{relativeTo:this.route})}
  getItemName(id:string):string{const it=this.items().find(i=>i.id===id);return it?`${it.sku} — ${it.name}`:id}
  getLines(){const a=this.adj();if(!a)return[];return a.lines.map(l=>({...l,inventoryItemName:this.getItemName(l.inventoryItemId)}))}
}