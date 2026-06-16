import { Component, OnInit, inject, signal } from '@angular/core';
import { forkJoin } from 'rxjs';
import { DatePipe, DecimalPipe, LowerCasePipe } from '@angular/common';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { MatTableModule } from '@angular/material/table';
import { ActivatedRoute, Router } from '@angular/router';
import { StockAudit } from '../models/adjustment.model';
import { InventoryItem } from '../../inventory/models/inventory-item.model';
import { InventoryService } from '../../inventory/services/inventory.service';
import { AdjustmentsService } from '../services/adjustments.service';

@Component({selector:'app-audit-detail',imports:[DatePipe,DecimalPipe,LowerCasePipe,MatCardModule,MatButtonModule,MatIconModule,MatProgressBarModule,MatTableModule],templateUrl:'./audit-detail.html',styleUrl:'./audit-detail.scss'})
export class AuditDetail implements OnInit {
  private readonly svc=inject(AdjustmentsService);private readonly inventorySvc=inject(InventoryService);private readonly route=inject(ActivatedRoute);private readonly router=inject(Router);
  protected readonly id=this.route.snapshot.paramMap.get('id')!;
  protected readonly aud=signal<StockAudit|null>(null);protected readonly loading=signal(false);protected readonly error=signal<string|null>(null);protected readonly items=signal<InventoryItem[]>([]);
  ngOnInit():void{this.load()}
  protected load():void{this.loading.set(true);this.error.set(null);forkJoin({aud:this.svc.getAuditById(this.id),items:this.inventorySvc.getInventoryItems()}).subscribe({next:({aud,items})=>{this.aud.set(aud);this.items.set(items);this.loading.set(false)},error:()=>{this.error.set($localize`:@@common.loadError:Не удалось загрузить данные`);this.loading.set(false)}})}
  protected goBack():void{void this.router.navigate(['..'],{relativeTo:this.route})}
  protected getItemName(id:string):string{const it=this.items().find(i=>i.id===id);return it?`${it.sku} — ${it.name}`:id}
}
