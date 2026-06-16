import { AfterViewInit, Component, OnInit, ViewChild, inject, signal } from '@angular/core';
import { DatePipe, DecimalPipe, LowerCasePipe } from '@angular/common';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatPaginator, MatPaginatorModule } from '@angular/material/paginator';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { MatSort, MatSortModule } from '@angular/material/sort';
import { MatTableDataSource, MatTableModule } from '@angular/material/table';
import { RouterLink } from '@angular/router';
import { SalesOrderSummary } from '../../models/customer-so.model';
import { CustomerSoService } from '../../services/customer-so.service';
@Component({selector:'app-so-list',imports:[DatePipe,DecimalPipe,LowerCasePipe,MatTableModule,MatPaginatorModule,MatSortModule,MatButtonModule,MatIconModule,MatProgressBarModule,RouterLink],templateUrl:'./so-list.html',styleUrl:'./so-list.scss'})
export class SoList implements OnInit, AfterViewInit {
  private readonly svc=inject(CustomerSoService);
  protected readonly displayedColumns=['number','customerId','status','totalAmount','lineCount','orderDateUtc','actions'];
  protected readonly ds=new MatTableDataSource<SalesOrderSummary>([]);
  protected readonly loading=signal(false);protected readonly error=signal<string|null>(null);
  @ViewChild(MatPaginator) paginator!:MatPaginator;@ViewChild(MatSort) sort!:MatSort;
  ngOnInit():void{this.load()}ngAfterViewInit():void{this.ds.paginator=this.paginator;this.ds.sort=this.sort}
  protected load():void{this.loading.set(true);this.error.set(null);this.svc.getSalesOrders().subscribe({next:d=>{this.ds.data=d;this.loading.set(false)},error:()=>{this.error.set($localize`:@@common.loadError:Не удалось загрузить данные`);this.loading.set(false)}})}
}
