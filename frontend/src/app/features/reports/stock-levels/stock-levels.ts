import { AfterViewInit, Component, OnInit, ViewChild, inject, signal } from '@angular/core';
import { DecimalPipe } from '@angular/common';
import { MatPaginator, MatPaginatorModule } from '@angular/material/paginator';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { MatSort, MatSortModule } from '@angular/material/sort';
import { MatTableDataSource, MatTableModule } from '@angular/material/table';
import { LowStockItem } from '../models/report.model';
import { ReportsService } from '../services/reports.service';

@Component({selector:'app-stock-levels',imports:[DecimalPipe,MatTableModule,MatPaginatorModule,MatSortModule,MatProgressBarModule],templateUrl:'./stock-levels.html',styleUrl:'./stock-levels.scss'})
export class StockLevels implements OnInit, AfterViewInit {
  private readonly svc=inject(ReportsService);
  protected readonly cols=['sku','name','quantityOnHand','reorderLevel','deficit'];
  protected readonly ds=new MatTableDataSource<LowStockItem>([]);
  protected readonly loading=signal(false);protected readonly error=signal<string|null>(null);
  @ViewChild(MatPaginator) paginator!:MatPaginator;@ViewChild(MatSort) sort!:MatSort;
  ngOnInit():void{this.load()}ngAfterViewInit():void{this.ds.paginator=this.paginator;this.ds.sort=this.sort}
  protected load():void{this.loading.set(true);this.error.set(null);this.svc.getLowStock().subscribe({next:d=>{this.ds.data=d;this.loading.set(false)},error:()=>{this.error.set($localize`:@@common.loadError:Не удалось загрузить данные`);this.loading.set(false)}})}
}
