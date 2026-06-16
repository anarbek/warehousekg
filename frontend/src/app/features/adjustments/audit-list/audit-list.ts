import { AfterViewInit, Component, OnInit, ViewChild, inject, signal } from '@angular/core';
import { DatePipe, DecimalPipe, LowerCasePipe } from '@angular/common';
import { MatPaginator, MatPaginatorModule } from '@angular/material/paginator';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { MatSort, MatSortModule } from '@angular/material/sort';
import { MatTableDataSource, MatTableModule } from '@angular/material/table';
import { RouterLink } from '@angular/router';
import { StockAuditSummary } from '../models/adjustment.model';
import { AdjustmentsService } from '../services/adjustments.service';

@Component({selector:'app-audit-list',imports:[DatePipe,DecimalPipe,LowerCasePipe,MatTableModule,MatPaginatorModule,MatSortModule,MatProgressBarModule,RouterLink],templateUrl:'./audit-list.html',styleUrl:'./audit-list.scss'})
export class AuditList implements OnInit, AfterViewInit {
  private readonly svc=inject(AdjustmentsService);
  protected readonly cols=['number','warehouseId','status','lineCount','totalVariance','reconciledAtUtc'];
  protected readonly ds=new MatTableDataSource<StockAuditSummary>([]);
  protected readonly loading=signal(false);protected readonly error=signal<string|null>(null);
  @ViewChild(MatPaginator) paginator!:MatPaginator;@ViewChild(MatSort) sort!:MatSort;
  ngOnInit():void{this.load()}ngAfterViewInit():void{this.ds.paginator=this.paginator;this.ds.sort=this.sort}
  protected load():void{this.loading.set(true);this.error.set(null);this.svc.getAudits().subscribe({next:d=>{this.ds.data=d;this.loading.set(false)},error:()=>{this.error.set($localize`:@@common.loadError:Не удалось загрузить данные`);this.loading.set(false)}})}
}
