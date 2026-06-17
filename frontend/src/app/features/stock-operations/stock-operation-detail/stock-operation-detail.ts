import { Component, OnInit, inject, signal } from '@angular/core';
import { DatePipe, LowerCasePipe } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { forkJoin, Observable } from 'rxjs';
import { DxDataGridModule, DxButtonModule, DxProgressBarModule, DxTextBoxModule } from 'devextreme-angular';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { OperationType, StockReceipt, PickOrder, PackOrder, StockTransfer } from '../models/stock-operation.model';
import { InventoryItem } from '../../inventory/models/inventory-item.model';
import { InventoryService } from '../../inventory/services/inventory.service';
import { StockOperationsService } from '../services/stock-operations.service';

type Doc = StockReceipt | PickOrder | PackOrder | StockTransfer;

interface EditableLine { inventoryItemId: string; inventoryItemName: string; quantity: number; warehouseLocationName: string; packageLabel: string; }

@Component({selector:'app-stock-operation-detail',imports:[FormsModule,DatePipe,LowerCasePipe,DxDataGridModule,DxButtonModule,DxProgressBarModule,DxTextBoxModule,RouterLink],templateUrl:'./stock-operation-detail.html',styleUrl:'./stock-operation-detail.scss'})
export class StockOperationDetail implements OnInit {
  private readonly svc=inject(StockOperationsService);private readonly inv=inject(InventoryService);private readonly route=inject(ActivatedRoute);private readonly router=inject(Router);
  protected readonly op=this.route.snapshot.data['operationType'] as OperationType;protected readonly id=this.route.snapshot.paramMap.get('id')!;
  protected readonly doc=signal<Doc|null>(null);protected readonly loading=signal(false);protected readonly saving=signal(false);protected readonly err=signal<string|null>(null);
  protected readonly editableLines=signal<EditableLine[]>([]);
  get titleKey():string{const m:Record<string,string>={receiving:'Поступление',picking:'Сборка',packing:'Упаковка',transfer:'Перемещение'};return m[this.op]||''}
  get warehouseInfo():string{const d=this.doc();if(!d)return'—';const s=(d as any).sourceWarehouseName;if(s)return `${s||'—'} → ${(d as any).destinationWarehouseName||'—'}`;return(d as any).warehouseName||'—'}
  get referenceInfo():string|null{const d=this.doc();if(!d)return null;if(this.op==='receiving')return(d as any).supplierReference||null;if(this.op==='picking')return(d as any).reference||null;return null}
  get pickOrderIdInfo():string|null{const d=this.doc();return d?(d as any).pickOrderId||null:null}
  ngOnInit(){this.load()}
  load(){this.loading.set(true);this.err.set(null);const docObs=(this.op==='receiving'?this.svc.getStockReceiptById(this.id):this.op==='picking'?this.svc.getPickOrderById(this.id):this.op==='packing'?this.svc.getPackOrderById(this.id):this.svc.getStockTransferById(this.id))as Observable<Doc>;forkJoin({doc:docObs,items:this.inv.getInventoryItems()}).subscribe({next:({doc:data,items})=>{const m=new Map(items.map(i=>[i.id,i]));this.doc.set(data);this.editableLines.set(data.lines.map((l:any)=>({inventoryItemId:l.inventoryItemId,inventoryItemName:l.inventoryItemName||(m.get(l.inventoryItemId)?`${m.get(l.inventoryItemId)!.sku} — ${m.get(l.inventoryItemId)!.name}`:l.inventoryItemId),quantity:l.quantity,warehouseLocationName:l.warehouseLocationName||l.warehouseLocationId||'',packageLabel:l.packageLabel||''})));this.loading.set(false)},error:()=>{this.err.set($localize`:@@common.loadError:Не удалось загрузить данные`);this.loading.set(false)}})}
  isDraft(){return this.doc()?.status==='Draft'}
  complete(){this.saving.set(true);const o=(this.op==='receiving'?this.svc.completeStockReceipt(this.id):this.op==='picking'?this.svc.completePickOrder(this.id):this.op==='packing'?this.svc.completePackOrder(this.id):this.svc.completeStockTransfer(this.id))as Observable<void>;o.subscribe({next:()=>{this.saving.set(false);this.load()},error:()=>{this.saving.set(false)}})}
  cancelOp(){this.saving.set(true);const o=(this.op==='receiving'?this.svc.cancelStockReceipt(this.id):this.op==='picking'?this.svc.cancelPickOrder(this.id):this.op==='packing'?this.svc.cancelPackOrder(this.id):this.svc.cancelStockTransfer(this.id))as Observable<void>;o.subscribe({next:()=>{this.saving.set(false);this.load()},error:()=>{this.saving.set(false)}})}
  goBack(){void this.router.navigate(['..'],{relativeTo:this.route})}
  getWarehouseId():string{const d=this.doc();if(!d)return'';return(d as any).warehouseId||(d as any).sourceWarehouseId||''}
}