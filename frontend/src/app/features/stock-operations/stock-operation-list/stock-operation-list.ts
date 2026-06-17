import { Component, OnInit, inject, signal } from '@angular/core';
import { DatePipe } from '@angular/common';
import { Observable } from 'rxjs';
import { DxDataGridModule, DxButtonModule, DxSelectBoxModule, DxProgressBarModule } from 'devextreme-angular';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { OperationType, StockOperationStatus, StockReceipt, PickOrder, PackOrder, StockTransfer } from '../models/stock-operation.model';
import { StockOperationsService } from '../services/stock-operations.service';

type Row = StockReceipt | PickOrder | PackOrder | StockTransfer;

interface ListCfg { title: string; addLabel: string; newRoute: string; showRef: boolean; refLabel: string; }

const CONFIGS: Record<OperationType, ListCfg> = {
  receiving: { title: $localize`:@@stockOp.receiving.list.title:Поступления`, addLabel: $localize`:@@stockOp.receiving.list.add:Новое поступление`, newRoute: 'new', showRef: true, refLabel: $localize`:@@stockOp.column.reference:Ссылка` },
  picking: { title: $localize`:@@stockOp.picking.list.title:Сборка`, addLabel: $localize`:@@stockOp.picking.list.add:Новый заказ на сборку`, newRoute: 'new', showRef: true, refLabel: $localize`:@@stockOp.column.reference:Основание` },
  packing: { title: $localize`:@@stockOp.packing.list.title:Упаковка`, addLabel: $localize`:@@stockOp.packing.list.add:Новый заказ на упаковку`, newRoute: 'new', showRef: false, refLabel: '' },
  transfer: { title: $localize`:@@stockOp.transfer.list.title:Перемещения`, addLabel: $localize`:@@stockOp.transfer.list.add:Новое перемещение`, newRoute: 'new', showRef: false, refLabel: '' },
};

@Component({selector:'app-stock-operation-list',imports:[DatePipe,DxDataGridModule,DxButtonModule,DxSelectBoxModule,DxProgressBarModule,RouterLink],templateUrl:'./stock-operation-list.html',styleUrl:'./stock-operation-list.scss'})
export class StockOperationList implements OnInit {
  private readonly svc=inject(StockOperationsService);private readonly route=inject(ActivatedRoute);
  protected readonly op=this.route.snapshot.data['operationType'] as OperationType;
  protected readonly cfg=signal<ListCfg|null>(null);protected readonly rawData=signal<Row[]>([]);
  protected readonly loading=signal(false);protected readonly err=signal<string|null>(null);
  protected readonly statuses:StockOperationStatus[]=['Draft','Completed','Cancelled'];
  protected readonly selStatus=signal<string>('');
  get filteredData():Row[]{const s=this.selStatus();const d=this.rawData()||[];return s?d.filter(r=>r.status===s):d}
  ngOnInit(){const c=CONFIGS[this.op];this.cfg.set(c);this.load()}
  load(){this.loading.set(true);this.err.set(null);const o=(this.op==='receiving'?this.svc.getStockReceipts():this.op==='picking'?this.svc.getPickOrders():this.op==='packing'?this.svc.getPackOrders():this.svc.getStockTransfers())as Observable<Row[]>;o.subscribe({next:d=>{this.rawData.set(d);this.loading.set(false)},error:()=>{this.err.set($localize`:@@common.loadError:Не удалось загрузить данные`);this.loading.set(false)}})}
  getNewRoute(){return CONFIGS[this.op].newRoute}
  getRef(row:Row):string{return((row as any).supplierReference||(row as any).reference)||'—'}
  getWh(row:Row):string{if((row as any).sourceWarehouseName){return `${(row as StockTransfer).sourceWarehouseName??'—'} → ${(row as StockTransfer).destinationWarehouseName??'—'}`}return(row as any).warehouseName??'—'}
  getDate(row:any):string|null{return row.receivedAtUtc||row.adjustedAtUtc||row.reconciledAtUtc||row.createdAtUtc||null}
}